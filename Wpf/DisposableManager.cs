using System;
using System.Collections.Generic;

namespace Peanut.Libs.Wpf {
    internal static class DisposableManager {
        private static readonly List<IDisposable> list = new();

        internal static void Add(IDisposable disposable) {
            list.Add(disposable);
        }

        internal static bool Remove(IDisposable disposable) {
            return list.Remove(disposable);
        }

        internal static void DisposeAllAndClear(Action? action = null) {
            list.ForEach(disposable => disposable.Dispose());
            list.Clear();
            action?.Invoke();
        }
    }
}
