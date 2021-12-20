using Peanut.Libs.Extensions;
using Peanut.Libs.Specialized.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Peanut.Libs.Wpf {
    #region structures
    #region base classes
    /// <summary>
    /// Base class representing an event subscriber.<br/>
    /// </summary>
    public abstract class Subscriber {
        /// <summary>
        /// Gets a value indicating whether the subscriber is still alive.<br/>
        /// </summary>
        protected internal abstract bool IsAlive { get; }

        /// <summary>
        /// Gets the type of the subscriber.<br/>
        /// </summary>
        public Type SubscriberType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscriber"/> class.<br/>
        /// </summary>
        public Subscriber() {
            SubscriberType = Diagnostics.GetTypeOfCallingClass(4);
        }

        /// <inheritdoc/>
        public override string? ToString() {
            return SubscriberType == null ?
                base.ToString() :
                $"{base.ToString()}: {SubscriberType}";
        }
    }

    /// <summary>
    /// Base class for subscribers that do not require a parameter.<br/>
    /// </summary>
    public abstract class ParameterlessSubscriberBase : Subscriber {
        /// <summary>
        /// Publishes the signal to all subscribers.<br/>
        /// </summary>
        protected internal abstract void Publish();

        /// <summary>
        /// Asynchronously Publishes the signal to all subscribers.<br/>
        /// </summary>
        protected internal abstract Task PublishAsync();
    }

    /// <summary>
    /// Subclass of <see cref="ParameterlessSubscriberBase"/>.<br/>
    /// Provides specific implementations for subscribers that provide static delegates.<br/>
    /// </summary>
    internal sealed class StaticParameterlessSubscriber : ParameterlessSubscriberBase {
        private readonly Action action;

        protected internal override bool IsAlive => true;

        /// <summary>
        /// Initializes an instance of the <see cref="StaticParameterlessSubscriber"/> class.<br/>
        /// </summary>
        /// <param name="action">The event delegate of the subscriber.</param>
        internal StaticParameterlessSubscriber(Action action) {
            this.action = action.Method.CreateStaticVoid();
        }

        protected internal override void Publish() {
            action();
        }

        protected internal override async Task PublishAsync() {
            await Task.Run(action).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Subclass of <see cref="ParameterlessSubscriberBase"/>.<br/>
    /// Provides specific implementations for subscribers that provide instance delegates.<br/>
    /// </summary>
    internal sealed class InstanceParameterlessSubscriber : ParameterlessSubscriberBase {
        private readonly WeakReference weakReference;
        private readonly Action<object> action;

        protected internal override bool IsAlive => weakReference.IsAlive;

        /// <summary>
        /// Initializes an instance of the <see cref="InstanceParameterlessSubscriber"/> class.
        /// </summary>
        /// <param name="action">The event delegate of the subscriber.</param>
        internal InstanceParameterlessSubscriber(Action action) {
            weakReference = new(action.Target);
#nullable disable
            this.action = action.Method.CreateInstanceVoid(action.Target.GetType());
#nullable enable
        }

        protected internal override void Publish() {
            if (weakReference.Target is not null) { // equivalent to IsAlive
                action(weakReference.Target);
            }
        }

        protected internal override async Task PublishAsync() {
            if (weakReference.Target is not null) { // equivalent to IsAlive
                action(weakReference.Target);
                await Task.Run(() => action(weakReference.Target)).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Base class for subscribers that require a parameter.<br/>
    /// </summary>
    /// <typeparam name="TParam">The type of the data being published by the event.</typeparam>
    public abstract class ParameteredSubscriberBase<TParam> : Subscriber {
        /// <summary>
        /// Publishes the signal to all subscribers.<br/>
        /// </summary>
        /// <param name="payload">The data being published by the event.</param>
        protected internal abstract void Publish(TParam payload);

        /// <summary>
        /// Asynchronously Publishes the signal to all subscribers.<br/>
        /// </summary>
        /// <param name="payload">The data being published by the event.</param>
        protected internal abstract Task PublishAsync(TParam payload);
    }

    /// <summary>
    /// Subclass of <see cref="ParameteredSubscriberBase{TParam}"/>.<br/>
    /// Provides specific implementations for subscribers that provide static delegates.<br/>
    /// </summary>
    /// <typeparam name="TParam">The type of the data being published by the event.</typeparam>
    internal sealed class StaticParameteredSubscriber<TParam> : ParameteredSubscriberBase<TParam> {
        private readonly Action<TParam> action;

        protected internal override bool IsAlive => true;

        /// <summary>
        /// Initializes an instance of the <see cref="StaticParameteredSubscriber{TParam}"/>
        /// class.<br/>
        /// </summary>
        /// <param name="action">The event delegate of the subscriber.</param>
        internal StaticParameteredSubscriber(Action<TParam> action) {
            this.action = action.Method.CreateStaticVoidParam<TParam>();
        }

        protected internal override void Publish(TParam payload) {
            action(payload);
        }

        protected internal override async Task PublishAsync(TParam payload) {
            await Task.Run(() => action(payload)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Subclass of <see cref="ParameteredSubscriberBase{TParam}"/>.<br/>
    /// Provides specific implementations for subscribers that provide instance delegates.<br/>
    /// </summary>
    /// <typeparam name="TParam">The type of the data being published by the event.</typeparam>
    internal sealed class InstanceParameteredSubscriber<TParam> : ParameteredSubscriberBase<TParam> {
        private readonly WeakReference weakReference;
        private readonly Action<object, TParam> action;

        protected internal override bool IsAlive => weakReference.IsAlive;

        /// <summary>
        /// Initializes an instance of the <see cref="InstanceParameteredSubscriber{TParam}"/> class.
        /// </summary>
        /// <param name="action">The event delegate of the subscriber.</param>
        internal InstanceParameteredSubscriber(Action<TParam> action) {
            weakReference = new(action.Target);
#nullable disable
            this.action = action.Method.CreateInstanceVoidParam<TParam>(action.Target.GetType());
#nullable enable
        }

        protected internal override void Publish(TParam payload) {
            if (weakReference.Target is not null) { // equivalent to IsAlive
                action(weakReference.Target, payload);
            }
        }

        protected internal override async Task PublishAsync(TParam payload) {
            if (weakReference.Target is not null) { // equivalent to IsAlive
                await Task.Run(() => action(weakReference.Target, payload));
            }
        }
    }
    #endregion

    #region factories
    /// <summary>
    /// Base subscriber factory class that provides a specific interfaces for instantiating
    /// parameterless subscribers.<br/>
    /// </summary>
    internal abstract class ParameterlessSubscriberFactoryBase {
        /// <summary>
        /// Instantiates an appropriate kind of subscriber by examining the
        /// <paramref name="action"/>.<br/>
        /// </summary>
        /// <param name="action">The event delegate of the subscriber.</param>
        /// <returns>A <see cref="ParameterlessSubscriberBase"/> instance.</returns>
        internal abstract ParameterlessSubscriberBase Instantiate(Action action);
    }

    /// <summary>
    /// Base subscriber factory class that provides a specific interfaces for instantiating
    /// parametered subscribers.<br/>
    /// </summary>
    internal abstract class ParameteredSubscriberFactoryBase<TParam> {
        /// <summary>
        /// Instantiates an appropriate kind of subscriber by examining the
        /// <paramref name="action"/>.<br/>
        /// </summary>
        /// <param name="action">The event delegate of the subscriber.</param>
        /// <returns>A <see cref="ParameteredSubscriberBase{TParam}"/> instance.</returns>
        internal abstract ParameteredSubscriberBase<TParam> Instantiate(Action<TParam> action);
    }

    /// <summary>
    /// Factory that creates parameterless subscribers.<br/>
    /// This class cannot be inherited.<br/>
    /// </summary>
    internal sealed class ParameterlessSubscriberFactory : ParameterlessSubscriberFactoryBase {
        internal override ParameterlessSubscriberBase Instantiate(Action action) {
            if (action.Target is null) { // static method
                return new StaticParameterlessSubscriber(action);
            }
            else { // instance method
                return new InstanceParameterlessSubscriber(action);
            }
        }
    }

    /// <summary>
    /// Factory that creates parametered subscribers.<br/>
    /// This class cannot be inherited.<br/>
    /// </summary>
    /// <typeparam name="TParam">The type of the data being published by the event.</typeparam>
    internal sealed class ParameteredSubscriberFactory<TParam> :
        ParameteredSubscriberFactoryBase<TParam> {
        internal override ParameteredSubscriberBase<TParam> Instantiate(Action<TParam> action) {
            if (action.Target is null) { // static method
                return new StaticParameteredSubscriber<TParam>(action);
            }
            else { // instance method
                return new InstanceParameteredSubscriber<TParam>(action);
            }
        }
    }
    #endregion

    #region subscription classes
    /// <summary>
    /// PubSub subscription token.<br/>
    /// </summary>
    public abstract class SubscriptionToken : IDisposable {
        /// <summary>
        /// Unsubscribe the event.<br/>
        /// </summary>
        public abstract void Dispose();
    }

    /// <summary>
    /// Subclass of <see cref="SubscriptionToken"/>.<br/>
    /// Provides specific implementations for subscribers that do not require a parameter.<br/>
    /// This class cannot be inherited.<br/>
    /// </summary>
    internal sealed class ParameterlessSubscriptionToken : SubscriptionToken {
        private readonly ParameterlessSubscriberBase subscriber;
        private readonly Action<ParameterlessSubscriberBase> unsubscribeAction;
        private bool disposedValue;

        /// <summary>
        /// Initializes an instance of the <see cref="ParameterlessSubscriptionToken"/>.<br/>
        /// </summary>
        /// <param name="subscriber">The target subscriber.</param>
        /// <param name="unsubscribeAction">
        ///     The action to invoke when the subscriber unsubscribes.
        /// </param>
        internal ParameterlessSubscriptionToken(
            ParameterlessSubscriberBase subscriber,
            Action<ParameterlessSubscriberBase> unsubscribeAction) {
            this.subscriber = subscriber;
            this.unsubscribeAction = unsubscribeAction;
        }

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    unsubscribeAction(subscriber);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ParameterlessSubscriptionToken()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public override void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Subclass of <see cref="SubscriptionToken"/>.<br/>
    /// Provides specific implementations for subscribers that require a parameter.<br/>
    /// This class cannot be inherited.<br/>
    /// </summary>
    /// <typeparam name="TParam">The type of the data being published by the event.</typeparam>
    internal sealed class ParameteredSubscriptionToken<TParam> : SubscriptionToken {
        private readonly ParameteredSubscriberBase<TParam> subscriber;
        private readonly Action<ParameteredSubscriberBase<TParam>> unsubscribeAction;
        private bool disposedValue;

        /// <summary>
        /// Initializes an instance of the <see cref="ParameteredSubscriptionToken{TParam}"/>.<br/>
        /// </summary>
        /// <param name="subscriber">The target subscriber.</param>
        /// <param name="unsubscribeAction">
        ///     The action to invoke when the subscriber unsubscribes.
        /// </param>
        internal ParameteredSubscriptionToken(
            ParameteredSubscriberBase<TParam> subscriber,
            Action<ParameteredSubscriberBase<TParam>> unsubscribeAction) {
            this.subscriber = subscriber;
            this.unsubscribeAction = unsubscribeAction;
        }

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    unsubscribeAction(subscriber);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ParameterlessSubscriptionToken()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public override void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    #endregion
    #endregion

    #region base classes
    /// <summary>
    /// Base class representing a pubsub event.<br/>
    /// </summary>
    public abstract class PubSubEventBase {
        /// <summary>
        /// Gets the total subscribers of this event.<br/>
        /// </summary>
        public abstract int SubscribersCount { get; }

        /// <summary>
        /// Async lock used for ensuring thread-safe.<br/>
        /// </summary>
        protected readonly SemaphoreSlim semaphore = new(1, 1);
    }

    /// <summary>
    /// Subclass of <see cref="PubSubEventBase"/>.<br/>
    /// </summary>
    /// <typeparam name="TSubscriber">The type of the subscriber.</typeparam>
    /// <typeparam name="TAction">
    ///     The type of action to invoke when the event gets triggered.
    /// </typeparam>
    public abstract class PubSubEventBase<TSubscriber, TAction> : PubSubEventBase
        where TSubscriber : Subscriber
        where TAction : Delegate {
        /// <summary>
        /// Collection of all the subscribers of this event.<br/>
        /// </summary>
        protected readonly HashSet<TSubscriber> Subscribers = new();

        /// <inheritdoc/>
        public override int SubscribersCount => Subscribers.Count;

        /// <summary>
        /// Removes all the dead subscribers.<br/>
        /// This method will acquire a lock and call
        /// <see cref="RemoveDeadSubscribers_UnderLock"/>.<br/>
        /// </summary>
        protected internal void RemoveDeadSubscribers() {
            semaphore.Wait();
            RemoveDeadSubscribers_UnderLock();
            semaphore.Release();
        }

        /// <summary>
        /// Removes all the dead subscribers.<br/>
        /// </summary>
        protected void RemoveDeadSubscribers_UnderLock() {
            Subscribers.RemoveWhere(x => !x.IsAlive);
        }

        /// <summary>
        /// Add a subscriber to both the normal list and the enumeration list.<br/>
        /// </summary>
        /// <param name="subscriber">The subscriber to be added.</param>
        protected void AddSubscriber(TSubscriber subscriber) {
            Subscribers.Add(subscriber);
        }

        /// <summary>
        /// Remove a subscriber from both the normal list and the enumeration list.<br/>
        /// </summary>
        /// <param name="subscriber">The subscriber to be removed.</param>
        protected void RemoveSubscriber(TSubscriber subscriber) {
            Subscribers.Remove(subscriber);
        }

        /// <summary>
        /// Subscribes for signal from this event.<br/>
        /// </summary>
        /// <param name="action">The event delegate of the subscriber.</param>
        /// <returns>
        ///     A <see cref="SubscriptionToken"/> that can be used to unsubscribe the event.
        /// </returns>
        public abstract SubscriptionToken Subscribe(TAction action);
    }

    /// <summary>
    /// Subclass of <see cref="PubSubEventBase{TSubscriber, TAction}"/>.<br/>
    /// Provides specific interfaces for events where the publishers and providers do not require
    /// a parameter.<br/>
    /// </summary>
    public abstract class ParameterlessPubSubEventBase :
        PubSubEventBase<ParameterlessSubscriberBase, Action> {
        /// <summary>
        /// Publishes the signal to all subscribers.<br/>
        /// </summary>
        public abstract void Publish();

        /// <summary>
        /// Asynchronously publishes the signal to all subscribers.<br/>
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task PublishAsync();
    }

    /// <summary>
    /// Subclass of <see cref="PubSubEventBase{TSubscriber, TAction}"/>.<br/>
    /// Provides specific interfaces for events where the publishers and providers require
    /// a parameter.<br/>
    /// </summary>
    /// <typeparam name="TParam">The type of the data being published by the event.</typeparam>
    public abstract class ParameteredPubSubEventBase<TParam> :
        PubSubEventBase<ParameteredSubscriberBase<TParam>, Action<TParam>> {
        /// <summary>
        /// Publishes the signal to all subscribers.<br/>
        /// </summary>
        /// <param name="payload">The data to be published.</param>
        public abstract void Publish(TParam payload);

        /// <summary>
        /// Asynchronously publishes the signal to all subscribers.<br/>
        /// </summary>
        /// <param name="payload">The data to be published.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task PublishAsync(TParam payload);
    }
    #endregion

    /// <summary>
    /// A pubsub event where the publishers and the subscribers do not require a parameter.<br/>
    /// </summary>
    public class PubSubEvent : ParameterlessPubSubEventBase {
        private readonly ParameterlessSubscriberFactory subscribersFactory = new();

        /// <inheritdoc/>
        public override SubscriptionToken Subscribe(Action action) {
            semaphore.Wait();
            RemoveDeadSubscribers_UnderLock();
            ParameterlessSubscriberBase subscriber = subscribersFactory.Instantiate(action);
            AddSubscriber(subscriber);
            semaphore.Release();
            return new ParameterlessSubscriptionToken(
                subscriber,
                subscriber => RemoveSubscriber(subscriber));
        }

        /// <inheritdoc/>
        public override void Publish() {
            Publish(new());
        }

        private void Publish(HashSet<ParameterlessSubscriberBase> publishedSubscribers) {
            semaphore.Wait();
            RemoveDeadSubscribers_UnderLock();
            List<ParameterlessSubscriberBase> subscribers;
            if (!publishedSubscribers.Any()) {
                subscribers = new(Subscribers);
            }
            else {
                subscribers = new(Subscribers.Where(x => !publishedSubscribers.Contains(x)));
            }
            int count = subscribers.Count;
            semaphore.Release();
            for (int i = 0; i < count; i++) {
                ParameterlessSubscriberBase subscriber = subscribers[i];
                subscriber.Publish();
                publishedSubscribers.Add(subscriber);

                // There are scenarios when the subscriber unsubscribes one or more tokens.
                // In that case, the for loop variable could run out of the length of the list.
                if (count != subscribers.Count) {
                    Publish(publishedSubscribers);
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public override async Task PublishAsync() {
            await PublishAsync(new()).ConfigureAwait(false);
        }

        private async Task PublishAsync(HashSet<ParameterlessSubscriberBase> publishedSubscribers) {
            semaphore.Wait();
            RemoveDeadSubscribers_UnderLock();
            List<ParameterlessSubscriberBase> subscribers;
            if (!publishedSubscribers.Any()) {
                subscribers = new(Subscribers);
            }
            else {
                subscribers = new(Subscribers.Where(x => !publishedSubscribers.Contains(x)));
            }
            int count = subscribers.Count;
            semaphore.Release();
            for (int i = 0; i < count; i++) {
                ParameterlessSubscriberBase subscriber = subscribers[i];
                await subscriber.PublishAsync().ConfigureAwait(false);
                publishedSubscribers.Add(subscriber);

                // There are scenarios when the subscriber unsubscribes one or more tokens.
                // In that case, the for loop variable could run out of the length of the list.
                if (count != subscribers.Count) {
                    await PublishAsync(publishedSubscribers).ConfigureAwait(false);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// A pubsub event where the publishers and the subscribers require a parameter.<br/>
    /// </summary>
    /// <typeparam name="TParam">The type of the data being published by the event.</typeparam>
    public class PubSubEvent<TParam> : ParameteredPubSubEventBase<TParam> {
        private readonly ParameteredSubscriberFactory<TParam> subscribersFactory = new();

        /// <inheritdoc/>
        public override SubscriptionToken Subscribe(Action<TParam> action) {
            semaphore.Wait();
            RemoveDeadSubscribers_UnderLock();
            ParameteredSubscriberBase<TParam> subscriber = subscribersFactory.Instantiate(action);
            AddSubscriber(subscriber);
            semaphore.Release();
            return new ParameteredSubscriptionToken<TParam>(
                    subscriber,
                    subscriber => RemoveSubscriber(subscriber));
        }

        /// <inheritdoc/>
        public override void Publish(TParam payload) {
            Publish(payload, new());
        }

        private void Publish(
            TParam payload,
            HashSet<ParameteredSubscriberBase<TParam>> publishedSubscribers) {
            semaphore.Wait();
            RemoveDeadSubscribers_UnderLock();
            List<ParameteredSubscriberBase<TParam>> subscribers;
            if (!publishedSubscribers.Any()) {
                subscribers = new(Subscribers);
            }
            else {
                subscribers = new(Subscribers.Where(x => !publishedSubscribers.Contains(x)));
            }
            int count = subscribers.Count;
            semaphore.Release();
            for (int i = 0; i < count; i++) {
                ParameteredSubscriberBase<TParam> subscriber = subscribers[i];
                subscriber.Publish(payload);
                publishedSubscribers.Add(subscriber);

                // There are scenarios when the subscriber unsubscribes one or more tokens.
                // In that case, the for loop variable could run out of the length of the list.
                if (count != subscribers.Count) {
                    Publish(payload, publishedSubscribers);
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public override async Task PublishAsync(TParam payload) {
            await PublishAsync(payload, new()).ConfigureAwait(false);
        }

        private async Task PublishAsync(
            TParam payload,
            HashSet<ParameteredSubscriberBase<TParam>> publishedSubscribers) {
            semaphore.Wait();
            RemoveDeadSubscribers_UnderLock();
            List<ParameteredSubscriberBase<TParam>> subscribers;
            if (!publishedSubscribers.Any()) {
                subscribers = new(Subscribers);
            }
            else {
                subscribers = new(Subscribers.Where(x => !publishedSubscribers.Contains(x)));
            }
            int count = subscribers.Count;
            semaphore.Release();
            for (int i = 0; i < count; i++) {
                ParameteredSubscriberBase<TParam> subscriber = subscribers[i];
                await subscriber.PublishAsync(payload).ConfigureAwait(false);
                publishedSubscribers.Add(subscriber);

                // There are scenarios when the subscriber unsubscribes one or more tokens.
                // In that case, the for loop variable could run out of the length of the list.
                if (count != subscribers.Count) {
                    await PublishAsync(payload, publishedSubscribers).ConfigureAwait(false);
                    return;
                }
            }
        }
    }
}