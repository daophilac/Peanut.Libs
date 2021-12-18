using System.Windows;
using System.Windows.Controls;

namespace Peanut.Libs.Wpf.Controls {
    /// <summary>
    /// A <see cref="DataGrid"/> that uses <see cref="GenericDataGridRow{T}"/> to make working
    /// with data rows easier.<br/>
    /// </summary>
    /// <typeparam name="T">The type of item that the data rows represent.</typeparam>
    public abstract class GenericDataGrid<T> : DataGrid where T : class {
        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride() {
            return new GenericDataGridRow<T>();
        }
    }
}
