using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Peanut.Libs.Wpf.Helpers {
    /// <summary>
    /// Provides an easy way to register global hotkeys.<br/>
    /// </summary>
    public static class GlobalHotkeyManager {
        private static readonly Dictionary<int, Dictionary<Window, HotkeyHandler>> map = new();

        /// <summary>
        /// Registers a global hotkey.<br/>
        /// </summary>
        /// <param name="window">The window that registers this hotkey.</param>
        /// <param name="hotkeyId">An unique id across all global hotkeys.</param>
        /// <param name="modifierKeys">The modifier keys.</param>
        /// <param name="key">The key.</param>
        /// <param name="onHotkeyPressed">The action to perform when the hotkey is detected.</param>
        /// <param name="nonReapeat">
        ///     <see langword="true"/> will prevent the action from being invoked repeatedly
        ///     if the user holds the hotkey.</param>
        /// <returns>
        /// A <see cref="GlobalHotkeyToken"/> that can be used to unregister the hotkey.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public static GlobalHotkeyToken RegisterGlobalHotkey(
            Window window,
            int hotkeyId,
            ModifierKeys modifierKeys,
            Key key,
            Action onHotkeyPressed,
            bool nonReapeat = false) {
            Dictionary<Window, HotkeyHandler> handlers;
            if (map.ContainsKey(hotkeyId)) {
                handlers = map[hotkeyId];
                if (handlers.ContainsKey(window)) {
                    throw new ArgumentException($"Registering hotkey with the same hotkey id and " +
                        $"window is not allowed.");
                }
            }

            WindowInteropHelper helper = new(window);
            helper.EnsureHandle();
            uint mod = (uint)modifierKeys;
            if (nonReapeat) {
                mod |= 0x4000;
            }
            if (!RegisterHotKey(helper.Handle, hotkeyId, mod,
                (uint)KeyInterop.VirtualKeyFromKey(key))) {
                //throw new Exception("An unexpected exception has occured while registering hotkey.");
            }
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(HwndHook);
            if (!map.ContainsKey(hotkeyId)) {
                map.Add(hotkeyId, new());
            }
            handlers = map[hotkeyId];
            handlers.Add(window, new(source, helper.Handle, onHotkeyPressed));
            GlobalHotkeyToken token = new(UnregisterHotKey, hotkeyId, window);
            return token;
        }

        private static void UnregisterHotKey(int hotkeyId, Window window) {
            if (map.ContainsKey(hotkeyId)) {
                Dictionary<Window, HotkeyHandler> handlers = map[hotkeyId];
                if (handlers.ContainsKey(window)) {
                    HotkeyHandler handler = handlers[window];
                    handlers.Remove(window);
                    handler.hwndSource.RemoveHook(HwndHook);
                    UnregisterHotKey(handler.handle, hotkeyId);
                }
            }
        }

        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY) {
                int hotkeyId = wParam.ToInt32();
                if (map.ContainsKey(hotkeyId)) {
                    Dictionary<Window, HotkeyHandler> handlers = map[hotkeyId];
                    foreach (KeyValuePair<Window, HotkeyHandler> entry in handlers) {
                        entry.Value.onHotkeyPressed();
                    }
                }
            }
            return IntPtr.Zero;
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private class HotkeyHandler {
            internal readonly HwndSource hwndSource;
            internal readonly IntPtr handle;
            internal readonly Action onHotkeyPressed;

            internal HotkeyHandler(HwndSource hwndSource, IntPtr handle, Action onHotkeyPressed) {
                this.hwndSource = hwndSource;
                this.handle = handle;
                this.onHotkeyPressed = onHotkeyPressed;
            }
        }
    }

    /// <summary>
    /// A global hotkey token that can be used to unregister a global hotkey.<br/>
    /// </summary>
    public sealed class GlobalHotkeyToken : IDisposable {
        private readonly Action<int, Window> unregisterAction;
        private readonly int hotkeyId;
        private readonly Window window;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalHotkeyToken"/> class.<br/>
        /// </summary>
        /// <param name="unregisterAction">
        ///     The action responsible for unregistering the hotkey.
        /// </param>
        /// <param name="hotkeyId">The hotkey id.</param>
        /// <param name="window">The window that registers the hotkey.</param>
        internal GlobalHotkeyToken(Action<int, Window> unregisterAction, int hotkeyId, Window window) {
            this.unregisterAction = unregisterAction;
            this.hotkeyId = hotkeyId;
            this.window = window;
        }

        #region IDisposable implementation
        private bool disposedValue;

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    unregisterAction(hotkeyId, window);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GlobalHotkeyToken()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Unregisters the global hotkey.<br/>
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
