using System.Windows.Controls;

namespace Peanut.Libs.Wpf.Controls {
    /// <summary>
    /// Makes working with <see cref="DataGridRow"/> easier.<br/>
    /// </summary>
    /// <typeparam name="T">The type of item that the data row represents.</typeparam>
    internal class GenericDataGridRow<T> : DataGridRow where T : class {
        /// <summary>
        /// Gets or sets the data item that the row represents.<br/>
        /// </summary>
        internal T? GenericItem {
            // Since DataGridRow does not provide us a way to affect the Item property,
            // we are going to box and unbox the item instead.
            get => (T?)Item;
            set => Item = value;
        }
    }
}
