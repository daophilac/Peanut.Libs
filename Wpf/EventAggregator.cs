using System;
using System.Collections.Generic;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// The interface of any event aggregator.<br/>
    /// </summary>
    public interface IEventAggregator {
        /// <summary>
        /// Gets a <see cref="PubSubEventBase"/> event by specifying the type of the event.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the pubsub event.</typeparam>
        /// <returns>The requested event.</returns>
        T GetEvent<T>() where T : PubSubEventBase, new();
    }

    /// <summary>
    /// Implementation of the <see cref="IEventAggregator"/> interface.<br/>
    /// </summary>
    public sealed class EventAggregator : IEventAggregator {
        private readonly Dictionary<Type, PubSubEventBase> registeredTypes = new();

        /// <inheritdoc/>
        public T GetEvent<T>() where T : PubSubEventBase, new() {
            Type t = typeof(T);
            if (!registeredTypes.ContainsKey(t)) {
                registeredTypes.Add(t, Activator.CreateInstance<T>());
            }
            return (T)registeredTypes[t];
        }
    }
}
