using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        /// Gets all the nodes in this linked list.<br/>
        /// </summary>
        public ReadOnlyCollection<CircularLinkedListNode<T>> Nodes { get; }

        private readonly T[] internalValues;

        /// <summary>
        /// Get the list containing all values. Do NOT modify this list.<br/>
        /// </summary>
        public List<T?> Values { get; }

        /// <summary>
        /// Initializes an instance of the <see cref="CircularLinkedList{T}"/> class with
        /// a capacity.<br/>
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CircularLinkedList(int capacity) {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
            nodes = new CircularLinkedListNode<T>[capacity];
            internalValues = new T[capacity];
            Values = new List<T?>(capacity);
            for (int i = 0; i < capacity; i++) {
                nodes[i] = new CircularLinkedListNode<T>(this, i);
                Values.Add(default);
            }
            for (int i = 1; i < capacity - 1; i++) {
                nodes[i].Previous = nodes[i - 1];
                nodes[i].Next = nodes[i + 1];
            }
            Nodes = nodes.AsReadOnly();
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
            for (int i = 0, count = nodes.Length; i < count; i++) {
                nodes[i].Value = value;
                internalValues[i] = value;
                Values[i] = value;
            }
        }

        /// <summary>
        /// Fills all the nodes with values created from a creator.<br/>
        /// </summary>
        /// <param name="creator"></param>
        public void FillWith(Func<T> creator) {
            for (int i = 0, count = nodes.Length; i < count; i++) {
                T value = creator();
                nodes[i].Value = value;
                internalValues[i] = value;
                Values[i] = value;
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
            internalValues[Current.InternalIndex] = value;
            Values[Current.InternalIndex] = value;
        }

        /// <summary>
        /// Sets value for the previous node and also advance the current node to the previous
        /// node.<br/>
        /// </summary>
        /// <param name="value">The value being set.</param>
        public void SetPreviousAndAdvance(T value) {
            Backward();
            Current.Value = value;
            internalValues[Current.InternalIndex] = value;
            Values[Current.InternalIndex] = value;
        }
    }

    /// <summary>
    /// Underlying node machenism, powering the <see cref="CircularLinkedList{T}"/>.<br/>
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public sealed class CircularLinkedListNode<T> {
        /// <summary>
        /// Gets the index of the node in the underlying array.<br/>
        /// </summary>
        public int InternalIndex { get; private set; }

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
        /// <param name="internalIndex">The index of the node in the underlying array.</param>
#nullable disable
        internal CircularLinkedListNode(CircularLinkedList<T> list, int internalIndex) {
#nullable enable
            List = list;
            InternalIndex = internalIndex;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="CircularLinkedListNode{T}"/> class.<br/>
        /// </summary>
        /// <param name="list">The list that contains this node.</param>
        /// <param name="value">The value of the node.</param>
        /// <param name="internalIndex">The index of the node in the underlying array.</param>
        internal CircularLinkedListNode(CircularLinkedList<T> list, T value, int internalIndex) : this(list, internalIndex) {
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
