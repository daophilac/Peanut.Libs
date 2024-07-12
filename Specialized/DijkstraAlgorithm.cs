using System.Collections.Generic;

namespace Peanut.Libs.Specialized {
    /// <summary>
    /// The famous Dijkstra algorithm for finding shortest path from a starting vertex to all vertices in a graph.
    /// </summary>
    public sealed class DijkstraAgent<TNode> where TNode : DijkstraNode {
        private readonly CompositeKeyDictionary<TNode, float> _graph = new CompositeKeyDictionary<TNode, float>();
        private readonly HashSet<TNode> _visited = new HashSet<TNode>();
        private readonly HashSet<TNode> _vertices = new HashSet<TNode>();
        private readonly Dictionary<TNode, TNode> _predecessors = new Dictionary<TNode, TNode>();
        private readonly Dictionary<TNode, float> _distances = new Dictionary<TNode, float>();

        /// <summary>
        /// Adds an edge to the graph with the specified weight.
        /// </summary>
        /// <param name="u">The first vertex</param>
        /// <param name="v">The second vertex</param>
        /// <param name="weight">A weight indicating the distance between the two vertices</param>
        public void AddEdge(TNode u, TNode v, float weight) {
            _vertices.Add(u);
            _vertices.Add(v);
            _graph[u, v] = weight;
            _distances[u] = float.MaxValue;
            _distances[v] = float.MaxValue;
        }

        /// <summary>
        /// Finds the shortest path from a starting vertex to a specified vertex.
        /// </summary>
        /// <param name="from">The starting vertex</param>
        /// <param name="to">The ending vertex</param>
        /// <returns>A list indicating the path</returns>
        public (List<TNode> path, float distance)? FindShortestPath(TNode from, TNode to) {
            ClearData();
            _distances[from] = 0;
            foreach (TNode _ in _vertices) {
                TNode? u = null;
                float minDistance = float.MaxValue;
                foreach (TNode vertex in _vertices) {
                    if (!_visited.Contains(vertex) && _distances[vertex] < minDistance) {
                        minDistance = _distances[vertex];
                        u = vertex;
                    }
                }
                if (u == null) {
                    break;
                }

                _visited.Add(u);
                foreach (TNode v in _vertices) {
                    if (_graph.TryGetValue(u, v, out float distance) && !_visited.Contains(v)) {
                        float newDistance = _distances[u] + distance;
                        if (newDistance < _distances[v]) {
                            _distances[v] = newDistance;
                            _predecessors[v] = u;
                        }
                    }
                }

                if (u == to) {
                    List<TNode> result = new List<TNode>();
                    TNode current = to;
                    result.Add(to);
#nullable disable
                    while (_predecessors.TryGetValue(current, out TNode predecessor)) {
#nullable enable
                        result.Add(predecessor);
                        current = predecessor;
                    }
                    result.Reverse();
                    return (result, _distances[to]);
                }
            }

            return null;
        }

        private void ClearData() {
            _visited.Clear();
            _predecessors.Clear();
            foreach (TNode key in _distances.Keys) {
                _distances[key] = float.MaxValue;
            }
        }

        private sealed class CompositeKeyDictionary<TKey, TValue> {
            private readonly Dictionary<(TKey, TKey), TValue> _internalDictionary = new Dictionary<(TKey, TKey), TValue>();
            private readonly List<TKey> _cachedSortedKeys = new List<TKey>(2);

            public bool TryGetValue(TKey first, TKey second, out TValue value) {
                GetKeys(first, second);
#nullable disable
                return _internalDictionary.TryGetValue((_cachedSortedKeys[0], _cachedSortedKeys[1]), out value);
#nullable enable
            }

            private void GetKeys(TKey first, TKey second) {
                _cachedSortedKeys.Clear();
                _cachedSortedKeys.Add(first);
                _cachedSortedKeys.Add(second);
                _cachedSortedKeys.Sort(KeyComparator);
            }

            private static int KeyComparator(TKey first, TKey second) {
#nullable disable
                return first.GetHashCode().CompareTo(second.GetHashCode());
#nullable enable
            }

            public TValue this[TKey first, TKey second] {
                get {
                    GetKeys(first, second);
                    return _internalDictionary[(_cachedSortedKeys[0], _cachedSortedKeys[1])];
                }
                set {
                    GetKeys(first, second);
                    _internalDictionary[(_cachedSortedKeys[0], _cachedSortedKeys[1])] = value;
                }
            }
        }
    }

    /// <summary>
    /// Class that represents a vertex in the Dijkstra's algorithm.
    /// </summary>
    public class DijkstraNode { }
}
