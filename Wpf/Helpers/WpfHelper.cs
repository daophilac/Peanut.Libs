using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Peanut.Libs.Wpf.Helpers {
    /// <summary>
    /// Helpers related to WPF controls in general.<br/>
    /// </summary>
    public static class WpfHelper {
        /// <summary>
        /// Finds all the children that have the <typeparamref name="T"/> type and have the same
        /// level in the visual tree.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the children that need to be found.</typeparam>
        /// <param name="parent">The parent of the children.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of all the found children.</returns>
        public static IEnumerable<T> FindVisualChildrenAtTheSameLevel<T>(
            this DependencyObject parent) where T : DependencyObject {
            T? firstChild = parent.FindVisualFirstChild<T>();
            if (firstChild is not null) {
                parent = VisualTreeHelper.GetParent(firstChild);
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                    DependencyObject otherChild = VisualTreeHelper.GetChild(parent, i);
                    if (otherChild is T t) {
                        yield return t;
                    }
                }
            }
        }

        /// <summary>
        /// Finds all the children that have the <typeparamref name="T"/> type.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the children that need to be found.</typeparam>
        /// <param name="parent">The parent of the children.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of all the found children.</returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent)
            where T : DependencyObject {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) {
                    yield return t;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child)) {
                    yield return childOfChild;
                }
            }
        }

        /// <summary>
        /// Conditionally finds all the children that have the <typeparamref name="T"/> type.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the children that need to be found.</typeparam>
        /// <param name="parent">The parent of the children.</param>
        /// <param name="predicate">A filter.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of all the found children.</returns>
        public static IEnumerable<T> FindVisualChildren<T>(
            this DependencyObject parent,
            Func<T, bool> predicate) where T : DependencyObject {
            foreach (T child in FindVisualChildren<T>(parent)) {
                if (predicate(child)) {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Finds the first child of the <paramref name="parent"/> that has the
        /// <typeparamref name="T"/> type.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the child that needs to be found.</typeparam>
        /// <param name="parent">The parent of the child.</param>
        /// <returns>The found child. <see langword="null"/> if no such child was found.</returns>
        public static T? FindVisualFirstChild<T>(this DependencyObject parent)
            where T : DependencyObject {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                T? child = FindVisualFirstChildRecursive<T>(VisualTreeHelper.GetChild(parent, i));
                if (child is not null) {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the parent of a child.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the parent that needs to be found.</typeparam>
        /// <param name="child">The child of the parent.</param>
        /// <returns>The found parent. <see langword="null"/> if no such parent was found.</returns>
        public static T? FindVisualParent<T>(this DependencyObject child) where T : DependencyObject {
            // get parent item
            DependencyObject? parent = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parent == null) {
                return null;
            }

            // check if the parent matches the type we’re looking for
            if (parent is T tParent) {
                return tParent;
            }
            else {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parent);
            }
        }

        /// <summary>
        /// Finds an object in the visual tree that has the <typeparamref name="T"/> type
        /// from a point.<br/>
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the <see cref="DependencyObject"/> that needs to be found.
        /// </typeparam>
        /// <param name="reference">The <see cref="UIElement"/> that contains the the object.</param>
        /// <param name="point">
        ///     The point to find relative to the <paramref name="reference"/>.
        /// </param>
        /// <returns></returns>
        public static T? TryFindFromPoint<T>(this UIElement reference, Point point)
            where T : DependencyObject {
            if (reference.InputHitTest(point) is not DependencyObject element) {
                return null;
            }
            if (element is T t) {
                return t;
            }
            return FindVisualParent<T>(element);
        }

        private static T? FindVisualFirstChildRecursive<T>(this DependencyObject parent)
            where T : DependencyObject {
            if (parent is T t) {
                return t;
            }
            else {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                    T? child = FindVisualFirstChildRecursive<T>(VisualTreeHelper.GetChild(parent, i));
                    if (child is not null) {
                        return child;
                    }
                }
            }
            return null;
        }
    }
}
