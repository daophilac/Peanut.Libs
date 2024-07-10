using NUnit.Framework;
using Peanut.Libs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.RandomTests {
    internal class DijkstraAlgorithm {
        private sealed class MyNode : DijkstraNode {
            public string Name { get; set; }
            public MyNode(string name) {
                Name = name;
            }
            public override string ToString() {
                return Name;
            }
        }

        [Test]
        public void TestGraphOnW3School() {
            DijkstraAgent<MyNode> agent = new DijkstraAgent<MyNode>();
            MyNode a = new("a");
            MyNode b = new("b");
            MyNode c = new("c");
            MyNode d = new("d");
            MyNode e = new("e");
            MyNode f = new("f");
            MyNode g = new("g");
            agent.AddEdge(d, a, 4);
            agent.AddEdge(d, e, 2);
            agent.AddEdge(a, e, 4);
            agent.AddEdge(a, c, 3);
            agent.AddEdge(c, e, 4);
            agent.AddEdge(c, g, 5);
            agent.AddEdge(c, b, 2);
            agent.AddEdge(c, f, 5);
            agent.AddEdge(e, c, 4);
            agent.AddEdge(e, g, 5);
            agent.AddEdge(b, f, 2);
            agent.AddEdge(g, f, 5);

            (List<MyNode> path, float distance)? result = agent.FindShortestPath(d, d);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Value.distance);

            result = agent.FindShortestPath(d, c);
            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Value.distance);
            Assert.AreEqual(3, result.Value.path.Count);
            Assert.AreEqual("d", result.Value.path[0].Name);
            Assert.AreEqual("e", result.Value.path[1].Name);
            Assert.AreEqual("c", result.Value.path[2].Name);

            result = agent.FindShortestPath(d, f);
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Value.distance);
            Assert.AreEqual(5, result.Value.path.Count);
            Assert.AreEqual("d", result.Value.path[0].Name);
            Assert.AreEqual("e", result.Value.path[1].Name);
            Assert.AreEqual("c", result.Value.path[2].Name);
            Assert.AreEqual("b", result.Value.path[3].Name);
            Assert.AreEqual("f", result.Value.path[4].Name);
        }
    }
}
