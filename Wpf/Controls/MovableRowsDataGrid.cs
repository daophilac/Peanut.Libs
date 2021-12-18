using Peanut.Libs.Wpf.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Peanut.Libs.Wpf.Controls {
    /// <summary>
    /// A <see cref="GenericDataGrid{T}"/> subclass that can enable moving row by dragging
    /// the rows around.<br/>
    /// Note that it is not possible to drag rows if the grid is having its default view sorted.<br/>
    /// So please register the <see cref="ActualSorting"/> event and actually sort the underlying
    /// items source.<br/>
    /// This class is thread-safe.<br/>
    /// </summary>
    /// <typeparam name="T">The type of item that the data rows represent.</typeparam>
    public abstract class MovableRowsDataGrid<T> : GenericDataGrid<T> where T : class {
        private readonly object locker = new();

        /// <summary>
        /// Gets or sets whether to allow moving rows or not.<br/>
        /// The <see cref="ItemsControl.ItemsSource"/> property has to be at least inherited from
        /// the <see cref="IList"/> interface in order to be able to set this property to true.
        /// Otherwise, setting this property will not do anything.<br/>
        /// However, you can still set this property to true if the
        /// <see cref="ItemsControl.ItemsSource"/> property is currently null.
        /// </summary>
        public bool EnableRowsMove {
            get {
                lock (locker) {
                    return enableRowsMove;
                }
            }
            set {
                lock (locker) {
                    // If the value is false, we don't care about anything,
                    // we will just set the property to false and unregister event handlers.
                    if (!value) {
                        enableRowsMove = false;
                        isDragging = false;
                        draggedItem = null;
                        PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                        PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
                        PreviewMouseMove -= OnMouseMove;
                        EnableRowsMoveChanged?.Invoke(this, EventArgs.Empty);
                    }
                    // However, if the value is true, it's complicated.
                    // Generally, we want to enable rows move only when the ItemsSource property
                    // inherits from at least IList.
                    // But we cannot do that in a simple way due to the fact that user code can
                    // assign this property to true in design time. And when the control gets
                    // initialized, the ItemsSource property would be null.
                    // So, in case the ItemsSource property is null, we will just assign true.
                    // Then we have to catch the ItemsSourceChanged event to validate the
                    // ItemsSource property again.
                    else {
                        if (ItemsSource is null) {
                            enableRowsMove = true;
                        }
                        else {
                            if (ItemsSource is not IList) {
                                enableRowsMove = false;
                            }
                            else {
                                PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                                PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                                PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
                                PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
                                PreviewMouseMove -= OnMouseMove;
                                PreviewMouseMove += OnMouseMove;
                                enableRowsMove = true;
                            }
                        }
                        EnableRowsMoveChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
        private bool enableRowsMove;

        private T? draggedItem;
        private bool isDragging;
        private DataGridColumn? sortedColumn;

        /// <summary>
        /// Occurs when the <see cref="EnableRowsMove"/> property has changed.<br/>
        /// </summary>
        public event EventHandler? EnableRowsMoveChanged;

        /// <summary>
        /// Occurs when the data grid needs to perform an actual sort.<br/>
        /// </summary>
        public event EventHandler<MovableRowsDataGridSortingEventArgs>? ActualSorting;

        /// <summary>
        /// Triggers after a sort has been completed.<br/>
        /// </summary>
        public event EventHandler<DataGridSortingChangedEventArgs>? SortingChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MovableRowsDataGrid{T}"/> class.<br/>
        /// </summary>
        public MovableRowsDataGrid() {
            Sorting += MovableRowsDataGrid_Sorting;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            lock (locker) {
                if (!EnableRowsMove) {
                    return;
                }

                // find datagrid row by mouse point position
                GenericDataGridRow<T>? row = this.TryFindFromPoint<GenericDataGridRow<T>>(
                    e.GetPosition(sender as MovableRowsDataGrid<T>));
                if (row == null || row.IsEditing || row.Item is not T) {
                    return;
                }

                // cannot drag rows if default view is sorted
                ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
                if (view.SortDescriptions.Count != 0) {
                    Cursor = Cursors.No;
                    return;
                }

                draggedItem = row.GenericItem;
                Cursor = Cursors.Hand;
                isDragging = true;
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            lock (locker) {
                if (!EnableRowsMove) {
                    return;
                }

                draggedItem = null;
                Cursor = Cursors.Arrow;
                isDragging = false;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            lock (locker) {
                if (!EnableRowsMove) {
                    return;
                }

                if (e.LeftButton is not MouseButtonState.Pressed) {
                    return;
                }

                if (isDragging is false) {
                    return;
                }

                GenericDataGridRow<T>? row = this.TryFindFromPoint<GenericDataGridRow<T>>(
                    e.GetPosition(this));
                if (row == null ||
                    row.IsEditing ||
                    row.Item is not T ||
                    draggedItem == row.GenericItem) {
                    return;
                }

                if (sortedColumn != null) {
                    sortedColumn.SortDirection = null;
                }

                ExchangeItems(row.GenericItem);
            }
        }

        private void ExchangeItems(T? targetItem) {
            if (!ReferenceEquals(draggedItem, targetItem)) {

                IList list = (IList)ItemsSource;
                int targetIndex = list.IndexOf(targetItem);
                if (targetIndex < 0) { // the new row placeholder
                    return;
                }

                // remove the source from the list
                list.Remove(draggedItem);

                // move source at the target's location
                list.Insert(targetIndex, draggedItem);
            }
        }

        private void MovableRowsDataGrid_Sorting(object sender, DataGridSortingEventArgs e) {
            ListSortDirection sortDirection = ListSortDirection.Ascending;
            if (e.Column.SortDirection == ListSortDirection.Ascending) {
                sortDirection = ListSortDirection.Descending;
            }
            ActualSorting?.Invoke(this, new(e.Column, sortDirection));
            sortedColumn = e.Column;
            sortedColumn.SortDirection = sortDirection;
            e.Handled = true;
        }

        /// <inheritdoc/>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
            lock (locker) {
                base.OnItemsSourceChanged(oldValue, newValue);
                if (!EnableRowsMove) {
                    return;
                }

                // If the EnableRowsMove propery is false, we already don't do anything.
                // But if it's true, we will just try to set the EnableRowsMove to true
                // and the setter will do all the validation checks.
                EnableRowsMove = true;

                SetupSortEventHandlers();
            }
        }

        private readonly List<PropertyChangeNotifier<DataGridColumn>> propertyChangeNotifiers = new();

        /// <summary>
        /// Here I use an overkill method to listen to the sorting event to perform useless tasks.
        /// Because I'm just that jacked.
        /// </summary>
        private void SetupSortEventHandlers() {
            propertyChangeNotifiers.Clear();
            foreach (var columnHeader in this.FindVisualChildren<DataGridColumnHeader>(
                    x => VisualTreeHelper.GetParent(x) is DataGridCellsPanel)) {
                columnHeader.Click += ColumnHeader_Click;
                PropertyChangeNotifier<DataGridColumn> propertyChangeNotifier =
                    new(columnHeader.Column, DataGridColumn.SortDirectionProperty);
                propertyChangeNotifier.ValueChanged += SortDirection_Changed;
                propertyChangeNotifiers.Add(propertyChangeNotifier);
            }
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e) { }

        private void SortDirection_Changed(object? sender, EventArgs e) {
            if (sender == null) {
                return;
            }

            PropertyChangeNotifier<DataGridColumn> notifier =
                (PropertyChangeNotifier<DataGridColumn>)sender;
            if (notifier.PropertySource is DataGridColumn dataGridColumn) {
                string? header = dataGridColumn.Header.ToString();
                ListSortDirection? sortDirection = dataGridColumn.SortDirection;
                Console.WriteLine($"{header}: {sortDirection}");
                SortingChanged?.Invoke(this, new(dataGridColumn));
            }
        }
    }

    /// <summary>
    /// Event arguments for the <see cref="MovableRowsDataGrid{T}.ActualSorting"/> event.<br/>
    /// </summary>
    public class MovableRowsDataGridSortingEventArgs : EventArgs {
        /// <summary>
        /// Gets the column being sorted. Do not rely on its <see cref="DataGridColumn.SortDirection"/>
        /// property to determine the sort direction. Use the property
        /// <see cref="DesiredSortDirection"/> instead.
        /// </summary>
        public DataGridColumn DataGridColumn { get; }

        /// <summary>
        /// Gets the sort direction that this column should perform.
        /// </summary>
        public ListSortDirection DesiredSortDirection { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovableRowsDataGridSortingEventArgs"/>
        /// class.<br/>
        /// </summary>
        /// <param name="dataGridColumn"></param>
        /// <param name="sortDirection"></param>
        internal MovableRowsDataGridSortingEventArgs(
            DataGridColumn dataGridColumn, ListSortDirection sortDirection) {
            DataGridColumn = dataGridColumn;
            DesiredSortDirection = sortDirection;
        }
    }

    /// <summary>
    /// Event arguments for the <see cref="MovableRowsDataGrid{T}.EnableRowsMoveChanged"/>
    /// event.<br/>
    /// </summary>
    public class DataGridSortingChangedEventArgs : EventArgs {
        /// <summary>
        /// Gets the column that has been sorted. Examine its
        /// <see cref="DataGridColumn.SortDirection"/> to see the sort direction.
        /// </summary>
        public DataGridColumn DataGridColumn { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridBeginningEditEventArgs"/>
        /// class.<br/>
        /// </summary>
        /// <param name="dataGridColumn">The <see cref="DataGridColumn"/> that was sorted.</param>
        internal DataGridSortingChangedEventArgs(DataGridColumn dataGridColumn) {
            DataGridColumn = dataGridColumn;
        }
    }
}
