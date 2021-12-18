// See https://aka.ms/new-console-template for more information
using Benchmark;
using BenchmarkDotNet.Running;

//var summary = BenchmarkRunner.Run(typeof(NormalVsReflectionVsOpenDelegate));
var summary2 = BenchmarkRunner.Run(typeof(FastListVsNormalList));
