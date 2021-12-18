using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Peanut.Libs.Wpf.Helpers {
    /// <summary>
    /// Helpers related to the <see cref="DataGrid"/> control.<br/>
    /// </summary>
    public static class DataGridHelper {
        /// <summary>
        /// Returns the <see cref="DataGrid"/> that the <paramref name="column"/> belongs
        /// using reflection.<br/>
        /// </summary>
        /// <param name="column">
        ///     The <see cref="DataGridColumn"/> from which get <see cref="DataGrid"/> out.
        /// </param>
        /// <returns>The <see cref="DataGrid"/> that the <paramref name="column"/> belongs.</returns>
        public static DataGrid GetDataGrid(this DataGridColumn column) {
            Type type = column.GetType();
#nullable disable
            PropertyInfo property = type.GetProperty(
                "DataGridOwner",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (DataGrid)property.GetValue(column);
#nullable enable
        }

        /// <summary>
        /// Returns the <see cref="DataGridColumnHeader"/> of the <paramref name="column"/>.<br/>
        /// </summary>
        /// <param name="column">
        ///     The <see cref="DataGridColumn"/> from which get <see cref="DataGridColumnHeader"/>
        ///     out.</param>
        /// <returns>
        /// The <see cref="DataGridColumnHeader"/> that belongs to the <paramref name="column"/>.
        /// If the <paramref name="column"/> does not have a header, <see langword="null"/>
        /// will be returned.
        /// </returns>
        public static DataGridColumnHeader? GetColumnHeader(this DataGridColumn column) {
            DataGrid dataGrid = column.GetDataGrid();
            IEnumerable<DataGridColumnHeader> headers =
                dataGrid.FindVisualChildrenAtTheSameLevel<DataGridColumnHeader>();
            foreach (DataGridColumnHeader header in headers) {
                if (header.Column == column) {
                    return header;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all the <see cref="DataGridColumnHeader"/> of the
        /// <paramref name="dataGrid"/>.<br/>
        /// </summary>
        /// <param name="dataGrid">
        ///     The <see cref="DataGrid"/> that we need to get all the
        ///     <see cref="DataGridColumnHeader"/>.
        /// </param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="DataGridColumnHeader"/>.</returns>
        public static IEnumerable<DataGridColumnHeader> GetColumnHeaders(this DataGrid dataGrid) {
            return dataGrid.FindVisualChildrenAtTheSameLevel<DataGridColumnHeader>();
        }

        /// <summary>
        /// Returns a <see cref="DataGridCell"/> of the <paramref name="grid"/> given a
        /// <paramref name="row"/> and a <paramref name="columnIndex"/>.<br/>
        /// </summary>
        /// <param name="grid">The <see cref="DataGrid"/>.</param>
        /// <param name="row">The <see cref="DataGridRow"/>.</param>
        /// <param name="columnIndex">The column index.</param>
        /// <returns>The found <see cref="DataGridCell"/> object.</returns>
        /// <exception cref="Exception"/>
        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int columnIndex = 0) {
            DataGridCellsPresenter? presenter = row.FindVisualFirstChild<DataGridCellsPresenter>();
            if (presenter == null) {
                throw new Exception($"Could not find the {nameof(DataGridCellsPresenter)} " +
                    $"of the data grid row.");
            }

            if (presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) is DataGridCell cell) {
                return cell;
            }

            // now try to bring into view and retreive the cell
            grid.ScrollIntoView(row, grid.Columns[columnIndex]);
            cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);

            return cell;
        }
    }
}
