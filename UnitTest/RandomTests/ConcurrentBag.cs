using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.RandomTests {
    internal class TestConcurrentBag {
        private static ConcurrentBag<int> Bag = new();
        private static readonly object @lock = new();
        [Test]
        public async Task ConcurrentEnumeration() {
            for (int i = 0; i < 20; i++) {
                Bag.Add(i);
            }

            Action actionRemove = () => {
                lock (@lock) {
                    int threadId = Thread.CurrentThread.ManagedThreadId;
                    Console.WriteLine($"Thread inside lock: {threadId}");
                    ConcurrentBag<int> tempBag = Bag;
                    Bag = new();
                    List<int> tempList = new(tempBag.ToArray());
                    tempList.Remove(5);
                    foreach (int item in tempList) {
                        Thread.Sleep(1000);
                        Bag.Add(item);
                    }
                }
            };

            void action1() {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int totalItems = Bag.Count;
                Console.WriteLine($"Thread1 {threadId}: {totalItems} items");
                foreach (int item in Bag) {
                    Console.WriteLine($"Thread1 {threadId}: {item}");
                    Thread.Sleep(1000);
                }
            }

            void action2() {
                Thread.Sleep(3000);
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int totalItems = Bag.Count;
                Console.WriteLine($"Thread2 {threadId}: {totalItems} items");
                foreach (int item in Bag) {
                    Console.WriteLine($"Thread2 {threadId}: {item}");
                    Thread.Sleep(1000);
                }
            }

            void action3() {
                Thread.Sleep(6000);
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int totalItems = Bag.Count;
                Console.WriteLine($"Thread3 {threadId}: {totalItems} items");
                foreach (int item in Bag) {
                    Console.WriteLine($"Thread3 {threadId}: {item}");
                    Thread.Sleep(1000);
                }
            }

            Parallel.Invoke(actionRemove, action1, action2, action3);
            await Task.Delay(20000);
            Assert.IsTrue(true);
        }
    }
}
