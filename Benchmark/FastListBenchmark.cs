using BenchmarkDotNet.Attributes;
using Peanut.Libs.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark {
    public class FastListVsNormalList {
        private List<int> normalList = new();
        private FastList<int> fastList = new();
        public FastListVsNormalList() {
            for (int i = 0; i < 1000000; i++) {
                normalList.Add(i);
                fastList.Add(i);
            }
        }

        [Benchmark]
        public int RemoveNormal() {
            normalList.RemoveAt(0);
            return 1;
        }

        [Benchmark]
        public int RemoveFast() {
            fastList.RemoveAt(0);
            return 1;
        }
    }
}
