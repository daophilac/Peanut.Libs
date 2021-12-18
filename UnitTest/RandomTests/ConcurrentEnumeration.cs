using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.RandomTests {
    public interface IMyInterface {
        public void Test();
    }
    public interface IMyOtherInterface : IMyInterface {

    }
    public class TestConcurrentEnumeration {
        private static List<int> List = new();

        [Test]
        public async Task ConcurrentEnumeration() {
            for (int i = 0; i < 20; i++) {
                List.Add(i);
            }

            Action actionRemove = () => {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine($"Thread inside lock: {threadId}");
                List.Remove(5);
            };

            void action1() {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int totalItems = List.Count;
                Console.WriteLine($"Thread1 {threadId}: {totalItems} items");
                for (int i = 0; i < List.Count; i++) {
                    Console.WriteLine($"Thread1 {threadId}: {List[i]}");
                    Thread.Sleep(1000);
                }
            }

            void action2() {
                Thread.Sleep(3000);
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int totalItems = List.Count;
                Console.WriteLine($"Thread2 {threadId}: {totalItems} items");
                for (int i = 0; i < List.Count; i++) {
                    Console.WriteLine($"Thread2 {threadId}: {List[i]}");
                    Thread.Sleep(1000);
                }
            }

            void action3() {
                Thread.Sleep(6000);
                int threadId = Thread.CurrentThread.ManagedThreadId;
                int totalItems = List.Count;
                Console.WriteLine($"Thread3 {threadId}: {totalItems} items");
                for (int i = 0; i < List.Count; i++) {
                    Console.WriteLine($"Thread3 {threadId}: {List[i]}");
                    Thread.Sleep(1000);
                }
            }

            Parallel.Invoke(actionRemove, action1, action2, action3);
            await Task.Delay(20000);
            Assert.IsTrue(true);
        }
    }
}
