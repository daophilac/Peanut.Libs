using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Peanut.Libs.Wpf.Helpers {
    // https://agsmith.wordpress.com/2008/04/07/propertydescriptor-addvaluechanged-alternative/
    /// <summary>
    /// Provides a safe way to register the changed event from a
    /// <see cref="DependencyProperty"/>.<br/>
    /// This approach prevents memory leak.<br/>
    /// Note that this class stops working when it gets out of scope. So please keep it alive
    /// somehow to get notified when the property changes.<br/>
    /// This class cannot be inherited.<br/>
    /// </summary>
    /// <typeparam name="T">A <see cref="DependencyObject"/> subclass.</typeparam>
    public sealed class PropertyChangeNotifier<T> : DependencyObject, IDisposable
        where T : DependencyObject {
        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(object),
                typeof(PropertyChangeNotifier<T>),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the value of the property.<br/>
        /// </summary>
        /// <seealso cref="ValueProperty"/>
        [Description("Returns/sets the value of the property")]
        [Category("Behavior")]
        [Bindable(true)]
        public object Value {
            get {
                return GetValue(ValueProperty);
            }
            set {
                SetValue(ValueProperty, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="DependencyObject"/> that the target property belongs to.<br/>
        /// </summary>
        public T? PropertySource {
            get {
                try {
                    // note, it is possible that accessing the target property
                    // will result in an exception so i've wrapped this check
                    // in a try catch
                    return propertySourceRef.IsAlive ? propertySourceRef.Target as T : null;
                }
                catch {
                    return null;
                }
            }
        }

        /// <summary>
        /// Occurs when the value of the target property changed.<br/>
        /// </summary>
        public event EventHandler? ValueChanged;

        private readonly WeakReference propertySourceRef;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangeNotifier{T}"/> class.<br/>
        /// </summary>
        /// <param name="propertySource">
        ///     The <see cref="DependencyObject"/> that the property belongs to.
        /// </param>
        /// <param name="propertyPath">The path to the target property.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PropertyChangeNotifier(DependencyObject propertySource, PropertyPath propertyPath) {
            propertySourceRef = new WeakReference(propertySource);
            Binding binding = new();
            binding.Source = propertySource ?? throw new ArgumentNullException(nameof(propertySource));
            binding.Path = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
            binding.Mode = BindingMode.OneWay;
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangeNotifier{T}"/> class.<br/>
        /// </summary>
        /// <param name="propertySource">
        ///     The <see cref="DependencyObject"/> that the property belongs to.
        /// </param>
        /// <param name="property">The target property.</param>
        public PropertyChangeNotifier(DependencyObject propertySource, DependencyProperty property)
            : this(propertySource, new PropertyPath(property)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangeNotifier{T}"/> class.<br/>
        /// </summary>
        /// <param name="propertySource">
        ///     The <see cref="DependencyObject"/> that the property belongs to.
        /// </param>
        /// <param name="path">The path to the target property.</param>
        public PropertyChangeNotifier(DependencyObject propertySource, string path)
            : this(propertySource, new PropertyPath(path)) { }

        private static void OnPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PropertyChangeNotifier<T> notifier = (PropertyChangeNotifier<T>)d;
            notifier.ValueChanged?.Invoke(notifier, EventArgs.Empty);
        }

        #region IDisposable implementation
        private bool disposedValue;

        private void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    BindingOperations.ClearBinding(this, ValueProperty);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PropertyChangeNotifier()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <inheritdoc/>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
