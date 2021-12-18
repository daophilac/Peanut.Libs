using BenchmarkDotNet.Attributes;
using Peanut.Libs.Specialized.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark {
    public class CustomClass {
        public object CustomMethod() {
            return 1;
        }
    }

    [MarkdownExporter, AsciiDocExporter, HtmlExporter, CsvMeasurementsExporter, RPlotExporter]
    public class NormalVsReflectionVsOpenDelegate {
        private readonly CustomClass customClass = new();
        private readonly MethodInfo method;
        private readonly Func<CustomClass, object> strongOpenDelegate;
        private readonly Func<object, object> weakOpenDelegate;
        public NormalVsReflectionVsOpenDelegate() {
            method = typeof(CustomClass).GetMethod("CustomMethod");
            strongOpenDelegate = method.CreateInstanceReturn<CustomClass, object>();
            weakOpenDelegate = method.CreateInstanceReturn<object>(typeof(CustomClass));
        }

        [Benchmark(Baseline = true)]
        public object Normal() {
            return customClass.CustomMethod();
        }

        [Benchmark]
        public object Reflection() {
            return method.Invoke(customClass, null);
        }

        [Benchmark]
        public object StrongOpenDelegate() {
            return strongOpenDelegate(customClass);
        }

        [Benchmark]
        public object WeakOpenDelegate() {
            return weakOpenDelegate(customClass);
        }
    }
}
