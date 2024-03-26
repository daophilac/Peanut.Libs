using NUnit.Framework;
using Peanut.Libs.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.OS {
    internal class OSTests {
        [Test]
        public void Test1() {
            FileAssiocationInfo.GetAssociatedPrograms("html");
        }
    }
}
