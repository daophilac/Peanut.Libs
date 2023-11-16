using System.Collections;
using System.Collections.Generic;

namespace Peanut.Libs.Specialized {
    /// <summary>
    /// A high performance collection that has constant lookup and insert time.<br/>
    /// Duplicate items are not allowed.<br/>
    /// </summary>
    /// <typeparam name="T">Type of items of the collection.</typeparam>
    public class HighPerformanceCollection<T> : IEnumerable<T> where T : notnull {
        private readonly LinkedList<T> linkedList = new();
        private readonly Dictionary<T, LinkedListNode<T>> dictionary = new();

        /// <summary>
        /// Gets the number of items in this collection.<br/>
        /// </summary>
        public int Count => linkedList.Count;

        /// <summary>
        /// Determines whether the <see cref="HighPerformanceCollection{T}"/> contains the specified
        /// key.<br/>
        /// </summary>
        /// <param name="value">
        ///     The value to locate in the <see cref="HighPerformanceCollection{T}"/>.
        ///     The value cannot be null.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="value"/> is found in the
        ///     <see cref="HighPerformanceCollection{T}"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(T value) {
            return dictionary.ContainsKey(value);
        }

        /// <summary>
        /// Adds a new item to the start of the collection.<br/>
        /// </summary>
        /// <param name="value">The item to be added.</param>
        public void AddFirst(T value) {
            LinkedListNode<T> node = linkedList.AddFirst(value);
            dictionary.Add(value, node);
        }

        /// <summary>
        /// Adds a new item to the end of the collection.<br/>
        /// </summary>
        /// <param name="value">The item to be added.</param>
        public void AddLast(T value) {
            LinkedListNode<T> node = linkedList.AddLast(value);
            dictionary.Add(value, node);
        }

        /// <summary>
        /// Adds a new item before an existing item.<br/>
        /// </summary>
        /// <param name="existingItem">The existing item.</param>
        /// <param name="newItem">The item to be added.</param>
        public void AddBefore(T existingItem, T newItem) {
            LinkedListNode<T> existingNode = dictionary[existingItem];
            LinkedListNode<T> newNode = linkedList.AddBefore(existingNode, newItem);
            dictionary.Add(newItem, newNode);
        }

        /// <summary>
        /// Adds a new item after an existing item.<br/>
        /// </summary>
        /// <param name="existingItem">The existing item.</param>
        /// <param name="newItem">The item to be added.</param>
        public void AddAfter(T existingItem, T newItem) {
            LinkedListNode<T> existingNode = dictionary[existingItem];
            LinkedListNode<T> newNode = linkedList.AddAfter(existingNode, newItem);
            dictionary.Add(newItem, newNode);
        }

        /// <summary>
        /// Removes an item from the collection.<br/>
        /// </summary>
        /// <param name="value">The item to be removed.</param>
        public void Remove(T value) {
            LinkedListNode<T> node = dictionary[value];
            dictionary.Remove(value);
            linkedList.Remove(node);
        }

        /// <summary>
        /// Removes all items from the collection.<br/>
        /// </summary>
        public void Clear() {
            dictionary.Clear();
            linkedList.Clear();
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() {
            return linkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
