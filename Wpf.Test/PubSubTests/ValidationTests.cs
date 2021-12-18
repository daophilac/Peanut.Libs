using NUnit.Framework;
using Peanut.Libs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.Test.PubSubTests {
    internal class InstanceVoidPubSub : PubSubEvent { }
    internal class StaticVoidPubSub : PubSubEvent { }
    internal class InstanceParamPubSub : PubSubEvent<string> { }
    internal class StaticParamPubSub : PubSubEvent<string> { }
    internal class UnsubscribeVoidPubSub : PubSubEvent { }
    internal class UnsubscribeParamPubSub : PubSubEvent<string> { }
    internal class GarbageCollecedPubSub : PubSubEvent { }
    internal class GarbageCollecedStaticPubSub : PubSubEvent { }
    //internal class 
    internal class Class1 {
        private readonly IEventAggregator ea;
        internal bool InstanceVoidInvoked { get; private set; }
        static internal bool StaticVoidInvoked { get; private set; }
        internal bool InstanceParamInvoked { get; private set; }
        static internal bool StaticParamInvoked { get; private set; }

        public Class1(IEventAggregator ea) {
            this.ea = ea;
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<StaticVoidPubSub>().Subscribe(StaticVoid);
            ea.GetEvent<InstanceParamPubSub>().Subscribe(InstanceParam);
            ea.GetEvent<StaticParamPubSub>().Subscribe(StaticParam);
        }

        public void InstanceVoid() {
            Console.WriteLine($"{nameof(Class1)} - {nameof(InstanceVoid)}");
            InstanceVoidInvoked = true;
        }

        public static void StaticVoid() {
            Console.WriteLine($"{nameof(Class1)} - {nameof(StaticVoid)}");
            StaticVoidInvoked = true;
        }

        public void InstanceParam(string message) {
            Console.WriteLine($"{nameof(Class1)} - {nameof(InstanceParam)} - {message}");
            InstanceParamInvoked = true;
        }

        public static void StaticParam(string message) {
            Console.WriteLine($"{nameof(Class1)} - {nameof(StaticParam)} - {message}");
            StaticParamInvoked = true;
        }
    }
    internal class Class2 {
        private readonly IEventAggregator ea;
        internal bool InstanceVoidInvoked { get; private set; }

        public Class2(IEventAggregator ea) {
            this.ea = ea;
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
        }

        public void InstanceVoid() {
            Console.WriteLine($"{nameof(Class2)} - {nameof(InstanceVoid)}");
            InstanceVoidInvoked = true;
        }
    }
    internal class DuplicateClass {
        private readonly IEventAggregator ea;
        internal bool InstanceVoidInvoked { get; private set; }

        public DuplicateClass(IEventAggregator ea) {
            this.ea = ea;
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
            ea.GetEvent<InstanceVoidPubSub>().Subscribe(InstanceVoid);
        }

        public void InstanceVoid() {
            Console.WriteLine($"{nameof(DuplicateClass)} - {nameof(InstanceVoid)}");
            InstanceVoidInvoked = true;
        }
    }
    internal class GarbageCollectedInstanceClass {
        private readonly IEventAggregator ea;
        internal bool InstanceVoidInvoked { get; private set; }

        public GarbageCollectedInstanceClass(IEventAggregator ea) {
            this.ea = ea;
            ea.GetEvent<GarbageCollecedPubSub>().Subscribe(InstanceVoid);
        }

        public void InstanceVoid() {
            Console.WriteLine($"{nameof(GarbageCollectedInstanceClass)} - {nameof(InstanceVoid)}");
            InstanceVoidInvoked = true;
        }
    }

    internal class GarbageCollectedStaticClass {
        private readonly IEventAggregator ea;
        static internal bool StaticVoidInvoked { get; private set; }
        internal SubscriptionToken Token { get; private set; }

        public GarbageCollectedStaticClass(IEventAggregator ea) {
            this.ea = ea;
            int a = 10;
            //Token = ea.GetEvent<GarbageCollecedStaticPubSub>().Subscribe(StaticVoid);
            Token = ea.GetEvent<GarbageCollecedStaticPubSub>().Subscribe(() => { Console.WriteLine(a); });
            Token = ea.GetEvent<GarbageCollecedStaticPubSub>().Subscribe(() => { Console.WriteLine("abc"); });
        }

        public static void StaticVoid() {
            Console.WriteLine($"{nameof(GarbageCollectedInstanceClass)} - {nameof(StaticVoid)}");
            StaticVoidInvoked = true;
        }
    }

    public class ValidationTests {
        private static IEventAggregator ea = new EventAggregator();

        [Test]
        public void CountSubscribers() {
            InstanceVoidPubSub @event = ea.GetEvent<InstanceVoidPubSub>();
            Assert.AreEqual(0, @event.SubscribersCount);
            Class1 class1 = new(ea);
            Assert.AreEqual(1, @event.SubscribersCount);
            Class2 class2 = new(ea);
            Assert.AreEqual(2, @event.SubscribersCount);
            @event.Publish();
            Assert.AreEqual(2, @event.SubscribersCount);
            DuplicateClass duplicateClass = new(ea);
            Assert.AreEqual(10, @event.SubscribersCount);
        }

        [Test]
        public void InstanceVoidDelegate() {
            InstanceVoidPubSub @event = ea.GetEvent<InstanceVoidPubSub>();
            Class1 class1 = new(ea);
            @event.Publish();
            Assert.IsTrue(class1.InstanceVoidInvoked);
        }

        [Test]
        public void StaticVoidDelegate() {
            StaticVoidPubSub @event = ea.GetEvent<StaticVoidPubSub>();
            Class1 class1 = new(ea);
            @event.Publish();
            Assert.IsTrue(Class1.StaticVoidInvoked);
        }

        [Test]
        public async Task PublishAsyncVoid() {
            InstanceVoidPubSub event1 = ea.GetEvent<InstanceVoidPubSub>();
            StaticVoidPubSub event2 = ea.GetEvent<StaticVoidPubSub>();
            Class1 class1 = new(ea);
            await event1.PublishAsync();
            await event2.PublishAsync();
            Assert.IsTrue(class1.InstanceVoidInvoked);
            Assert.IsTrue(Class1.StaticVoidInvoked);
        }

        [Test]
        public void InstanceParamDelegate() {
            InstanceParamPubSub event1 = ea.GetEvent<InstanceParamPubSub>();
            Class1 class1 = new(ea);
            event1.Publish("Hello world");
            Assert.IsTrue(class1.InstanceParamInvoked);
        }

        [Test]
        public void StaticParamDelegate() {
            StaticParamPubSub event1 = ea.GetEvent<StaticParamPubSub>();
            Class1 class1 = new(ea);
            event1.Publish("Hello world");
            Assert.IsTrue(Class1.StaticParamInvoked);
        }

        [Test]
        public async Task PublishAsyncParam() {
            InstanceParamPubSub event1 = ea.GetEvent<InstanceParamPubSub>();
            StaticParamPubSub event2 = ea.GetEvent<StaticParamPubSub>();
            Class1 class1 = new(ea);
            await event1.PublishAsync("Hello world");
            await event2.PublishAsync("Hello world");
            Assert.IsTrue(class1.InstanceParamInvoked);
            Assert.IsTrue(Class1.StaticParamInvoked);
        }

        [Test]
        public void Unsubscribe() {
            UnsubscribeVoidPubSub event1 = ea.GetEvent<UnsubscribeVoidPubSub>();
            UnsubscribeParamPubSub event2 = ea.GetEvent<UnsubscribeParamPubSub>();
            Assert.AreEqual(0, event1.SubscribersCount);
            Assert.AreEqual(0, event2.SubscribersCount);
            SubscriptionToken token1 = event1.Subscribe(() => { Console.WriteLine("Anonymous called!"); });
            SubscriptionToken token2 = event2.Subscribe(message => { Console.WriteLine(message); });
            Assert.AreEqual(1, event1.SubscribersCount);
            Assert.AreEqual(1, event2.SubscribersCount);
            event1.Publish();
            event2.Publish("Helloooooo");
            token1.Dispose();
            token2.Dispose();
            Assert.AreEqual(0, event1.SubscribersCount);
            Assert.AreEqual(0, event2.SubscribersCount);
            event1.Publish();
            event2.Publish("Helloooooo again");
        }

        [Test]
        public void GarbageCollected() {
            GarbageCollecedPubSub event1 = ea.GetEvent<GarbageCollecedPubSub>();
            WeakReference weakReference = null;
            new Action(() => {
                Assert.AreEqual(0, event1.SubscribersCount);
                GarbageCollectedInstanceClass instanceClass = new(ea);
                weakReference = new WeakReference(instanceClass);
                Assert.AreEqual(1, event1.SubscribersCount);
            })();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(weakReference.IsAlive);
            Assert.IsNull(weakReference.Target);
            Assert.AreEqual(1, event1.SubscribersCount);
            event1.Publish();
            Assert.AreEqual(0, event1.SubscribersCount);
        }

        [Test]
        public void GarbageCollectedStatic() {
            GarbageCollecedStaticPubSub event1 = ea.GetEvent<GarbageCollecedStaticPubSub>();
            WeakReference weakReference = null;
            new Action(() => {
                Assert.AreEqual(0, event1.SubscribersCount);
                GarbageCollectedStaticClass instanceClass = new(ea);
                weakReference = new WeakReference(instanceClass);
                Assert.AreEqual(2, event1.SubscribersCount);
            })();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(weakReference.IsAlive);
            Assert.IsNull(weakReference.Target);
            Assert.AreEqual(2, event1.SubscribersCount);
            event1.Publish();
            Assert.AreEqual(1, event1.SubscribersCount); // not a typo
        }
    }
}
