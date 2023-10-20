using System;
using System.Collections.Generic;
using System.Linq;

namespace Peanut.Libs.Specialized {
    /// <summary>
    /// A special linked list that is circular.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public class CircularLinkedList<T> {
        /// <summary>
        /// Gets the current <see cref="CircularLinkedListNode{T}"/>.<br/>
        /// </summary>
        public CircularLinkedListNode<T> Current { get; private set; }

        /// <summary>
        /// Gets the total nodes of this linked list.<br/>
        /// </summary>
        public int Count => nodes.Length;

        private readonly CircularLinkedListNode<T>[] nodes;

        /// <summary>
        /// Initializes an instance of the <see cref="CircularLinkedList{T}"/> class with
        /// a capacity.<br/>
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CircularLinkedList(int capacity) {
            if (capacity <= 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            nodes = new CircularLinkedListNode<T>[capacity];
            for (int i = 0; i < capacity; i++) {
                nodes[i] = new CircularLinkedListNode<T>(this);
            }
            for (int i = 1; i < capacity - 1; i++) {
                nodes[i].Previous = nodes[i - 1];
                nodes[i].Next = nodes[i + 1];
            }
            nodes[0].Previous = nodes[capacity - 1];
            nodes[0].Next = nodes[1];
            nodes[capacity - 1].Previous = nodes[capacity - 2];
            nodes[capacity - 1].Next = nodes[0];
            Current = nodes[0];
        }

        /// <summary>
        /// Fills all the nodes with a value.<br/>
        /// </summary>
        /// <param name="value">The value being filled.</param>
        public void FillWith(T value) {
            foreach (CircularLinkedListNode<T> node in nodes) {
                node.Value = value;
            }
        }

        /// <summary>
        /// Fills all the nodes with values created from a creator.<br/>
        /// </summary>
        /// <param name="creator"></param>
        public void FillWith(Func<T> creator) {
            foreach (CircularLinkedListNode<T> node in nodes) {
                node.Value = creator();
            }
        }

        /// <summary>
        /// Sets the current node to the next node.
        /// </summary>
        public void Forward() {
            Current = Current.Next;
        }

        /// <summary>
        /// Sets the current node to the previous node.
        /// </summary>
        public void Backward() {
            Current = Current.Previous;
        }
        

        /// <summary>
        /// Sets value for the next node and also advance the current node to the next node.<br/>
        /// </summary>
        /// <param name="value">The value being set.</param>
        public void SetNextAndAdvance(T value) {
            Forward();
            Current.Value = value;
        }

        /// <summary>
        /// Sets value for the previous node and also advance the current node to the previous
        /// node.<br/>
        /// </summary>
        /// <param name="value">The value being set.</param>
        public void SetPreviousAndAdvance(T value) {
            Backward();
            Current.Value = value;
        }

        /// <summary>
        /// Creates a <see cref="List{T}"/> containing all the
        /// <see cref="CircularLinkedListNode{T}"/>.<br/>
        /// </summary>
        /// <returns>
        /// A <see cref="List{T}"/> containing all the <see cref="CircularLinkedListNode{T}"/>.
        /// </returns>
        public List<CircularLinkedListNode<T>> ToListNodes() {
            return nodes.ToList();
        }

        /// <summary>
        /// Creates a <see cref="List{T}"/> containing all the values.<br/>
        /// </summary>
        /// <returns>A <see cref="List{T}"/> containing all the values.</returns>
        public List<T?> ToListValues() {
            return nodes.Select(x => x.Value).ToList();
        }
    }

    /// <summary>
    /// Underlying node machenism, powering the <see cref="CircularLinkedList{T}"/>.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public sealed class CircularLinkedListNode<T> {
        /// <summary>
        /// Gets the value of this node.<br/>
        /// </summary>
        public T? Value { get; internal set; }

        /// <summary>
        /// Gets the list that contains this node.<br/>
        /// </summary>
        public CircularLinkedList<T> List { get; private set; }

        /// <summary>
        /// Gets the previous node.<br/>
        /// </summary>
        public CircularLinkedListNode<T> Previous { get; internal set; }

        /// <summary>
        /// Gets the next node.<br/>
        /// </summary>
        public CircularLinkedListNode<T> Next { get; internal set; }

        /// <summary>
        /// Initializes an instance of the <see cref="CircularLinkedListNode{T}"/> class.<br/>
        /// </summary>
        /// <param name="list">The list that contains this node.</param>
#nullable disable
        internal CircularLinkedListNode(CircularLinkedList<T> list) {
#nullable enable
            List = list;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="CircularLinkedListNode{T}"/> class.<br/>
        /// </summary>
        /// <param name="list">The list that contains this node.</param>
        /// <param name="value">The value of the node.</param>
        internal CircularLinkedListNode(CircularLinkedList<T> list, T value) : this(list) {
            Value = value;
        }

        /// <summary>
        /// Calculate the distance between the source and the target node.
        /// The source node will run forward to the target node.<br/>
        /// </summary>
        /// <param name="target">target node.</param>
        /// <param name="countZeroDistance">
        ///     Indicates whether the method should return 0 if the source and the target node is
        ///     the same.
        /// </param>
        /// <returns>The distance between the source and the target node.</returns>
        public int DistanceForward(CircularLinkedListNode<T> target, bool countZeroDistance = true) {
            if (List != target.List) {
                throw new Exception("The target node is not in the same list as the source node.");
            }
            if (this == target) {
                if (countZeroDistance) {
                    return 0;
                }
                else {
                    return List.Count;
                }
            }
            else {
                int distance = 0;
                CircularLinkedListNode<T> running = this;
                while (running != target) {
                    distance++;
                    running = running.Next;
                }
                return distance;
            }
        }

        /// <summary>
        /// Calculate the distance between the source and the target node.
        /// The source node will run backward to the target node.<br/>
        /// </summary>
        /// <param name="other">The target node.</param>
        /// <param name="countZeroDistance">
        ///     Indicates whether the method should return 0 if the source and the target node is
        ///     the same.
        /// </param>
        /// <returns>The distance between the source and the target node.</returns>
        public int DistanceBackward(CircularLinkedListNode<T> other, bool countZeroDistance = true) {
            if (List != other.List) {
                throw new Exception("The target node is not in the same list as the source node.");
            }
            if (this == other) {
                if (countZeroDistance) {
                    return 0;
                }
                else {
                    return List.Count;
                }
            }
            else {
                int distance = 0;
                CircularLinkedListNode<T> running = this;
                while (running != other) {
                    distance++;
                    running = running.Previous;
                }
                return distance;
            }
        }

        /// <inheritdoc/>
        public override string? ToString() {
            return Value?.ToString();
        }
    }
}
