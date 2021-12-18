using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.RandomTests {
    public class TestWeakReference {
        [Test]
        public void MyTest() {
            WeakReference reference = null;
            new Action(() =>
            {
                var service = new List<string>();
                // Do things with service that might cause a memory leak...

                reference = new WeakReference(service, true);
            })();

            // Service should have gone out of scope about now, 
            // so the garbage collector can clean it up
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsNull(reference.Target);
        }

        [Test]
        public void TestNull() {
            WeakReference reference = new(null);
            Assert.IsNull(reference.Target);
            Assert.IsTrue(true);
        }
    }
}
