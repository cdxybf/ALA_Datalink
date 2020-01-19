using ProgrammingParadigms;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DomainAbstractions
{
    // This is a UI abstraction that displays the data in the style of a data grid.
    // Currently the only supported input data source is a DataTable, and we assume that every
    // DataTable binded to this grid will have the column PrimaryKey which was assigned when instantiated, so when any row selected, 
    // we can find the PrimaryKey value as the primary key of the selection and generate an IDataFlow<string> as an output.
    // The case here of the output is a list of IDataFlow<string>.
    // ------------------------------------------------------------------------------------------------------------------
    // Notice: 
    // 1. If there is a checkbox column, please use the key word "checkbox" as the column name.
    // 2. The visibility of a column was configured in the DataTable (DataColumn.Prefix = 'hide').
    // ------------------------------------------------------------------------------------------------------------------
    // Ports
    // IUI: Connection from the containing UI element
    // ITableDataFlow: mainTableDataFlow, the main data input/output for the grid
    // IEvent: Clear the grid input
    // IDataFlow<bool>: Visible input
    // IEvent eventRowSelected outputs an event when the currently selected row changes (by the user click or the default) TBD deprecate this, use dataFlowSelectedPrimaryKey instead
    // IDataFlow<string> dataFlowSelectedPrimaryKey outputs the primary key selected (by the user click or the default)
    // IDataFlow<string> dataFlowNumberOfRecords outputs the total number of records displayed
    // IDataFlow<bool> dataFlowShowRecordStateTitle TBD ask Rosman
    // IDataFlow<bool> dataFlowShowDownloadingStateTitle TBD ask Rosman
    public class Grid : IUI, ITableDataFlow, IEvent, IDataFlow<bool>
    {
        // properties 
        public string InstanceName;
        public bool ShowHeader = true;
        public double RowHeight = 22;
        public Thickness Margin;

        public string PrimaryKey;

        // ports
        private IEvent eventRowSelected;  
        private IDataFlow<string> dataFlowSelectedPrimaryKey;
        private IDataFlow<string> dataFlowNumberOfRecords;

        private IDataFlow<bool> dataFlowShowRecordStateTitle;
        private IDataFlow<bool> dataFlowShowDownloadingStateTitle;

        // private fields
        private DataGrid dataGrid;
        private int selectedIndex = 0;

        /// <summary>
        /// An WPF Data Grid which used for displaying the data of an ITableDataFlow.
        /// It keeps a copy of the data itself.
        /// </summary>
        public Grid()
        {
            dataGrid = new DataGrid() { AutoGenerateColumns = false };
            dataGrid.HorizontalGridLinesBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150));
            dataGrid.VerticalGridLinesBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150));
            dataGrid.ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Star);
            dataGrid.IsReadOnly = true;
            dataGrid.CellStyle = GetCellStyle();
            dataGrid.SelectionChanged += RowSelectionChanged;
            dataGrid.LoadingRow += GridRowloaded;
            dataGrid.Background = Brushes.White;
        }

        // IUI implementation
        UIElement IUI.GetWPFElement()
        {
            dataGrid.Margin = Margin;
            dataGrid.RowHeight = RowHeight;
            dataGrid.HeadersVisibility = ShowHeader ? DataGridHeadersVisibility.All : DataGridHeadersVisibility.None;
            return dataGrid;
        }

        // IEvent implementation
        void IEvent.Execute()
        {
            dataTable.Rows.Clear();
            dataTable.Columns.Clear();
            dataGrid.Columns.Clear();
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        // IDataFlow<bool> implementation
        bool IDataFlow<bool>.Data { set => dataGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }

        // ITableDataFlow implementation
        GetNextPageDelegate getNextPageCalback;
        private DataTable dataTable = new DataTable();
        DataTable ITableDataFlow.DataTable => dataTable;
        DataRow ITableDataFlow.CurrentRow
        {
            get
            {
                if (dataGrid.SelectedIndex >= 0)
                    return dataTable.Rows[dataGrid.SelectedIndex];
                return null;
            }
            set
            {
                if (value != null)
                {
                    dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns[PrimaryKey] };
                    int index = dataTable.Rows.IndexOf(dataTable.Rows.Find(value[PrimaryKey]));
                    dataGrid.SelectedIndex = index;
                }
            }
        }


        private bool calledGetHeaderMethod = false;
        async Task ITableDataFlow.GetHeadersFromSourceAsync()
        {
            calledGetHeaderMethod = true;
        }

        async Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync()
        {
            if (!calledGetHeaderMethod)
            {
                return new Tuple<int, int>(0, 0);
            }
            else
            {
                calledGetHeaderMethod = false;
                return new Tuple<int, int>(0, dataTable.Rows.Count);
            }
        }

        async Task ITableDataFlow.PutHeaderToDestinationAsync()
        {
            dataGrid.Columns.Clear();
            // generates data grid headers
            foreach (DataColumn c in dataTable.Columns)
            {
                dataGrid.Columns.Add(GetInitializedColumn(c));
            }

            if (dataFlowShowRecordStateTitle != null && dataFlowShowDownloadingStateTitle != null)
            {
                dataFlowShowDownloadingStateTitle.Data = true;
                dataFlowShowRecordStateTitle.Data = false;
            }
        }

        async Task ITableDataFlow.PutPageToDestinationAsync(
            int firstRowIndex, 
            int lastRowIndex, 
            GetNextPageDelegate getNextPage)
        {
            // assign the data table source the grid
            dataGrid.ItemsSource = dataTable.DefaultView;
       
            // output the number of rows
            if (dataFlowNumberOfRecords != null)
            {
                dataFlowNumberOfRecords.Data = dataTable.Rows.Count.ToString();
            }

            if (firstRowIndex >= lastRowIndex)
            {
                getNextPage?.Invoke();
            }
            else
            {
                getNextPageCalback = getNextPage;
            }

            if (dataFlowShowRecordStateTitle != null && dataFlowShowDownloadingStateTitle != null)
            {
                dataFlowShowDownloadingStateTitle.Data = false;
                dataFlowShowRecordStateTitle.Data = true;
            }
        }


        // private methods ---------------------------------------------------------------------------------
        // data grid event - when a row is going to displaying
        private void GridRowloaded(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.GetIndex() == selectedIndex)
            {
                dataGrid.SelectedIndex = selectedIndex;
            }

            if (e.Row.GetIndex() == dataTable.Rows.Count - 1)
            {
                if (dataFlowShowRecordStateTitle != null && dataFlowShowDownloadingStateTitle != null)
                {
                    dataFlowShowDownloadingStateTitle.Data = true;
                    dataFlowShowRecordStateTitle.Data = false;
                }
                getNextPageCalback?.Invoke();
            }            
        }

        // data grid row selection event - select a row manually or programactically
        private void RowSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedIndex >= 0)
            {
                selectedIndex = dataGrid.SelectedIndex;
                // output the primary key of the selected row
                if (dataFlowSelectedPrimaryKey != null)
                { 
                    dataFlowSelectedPrimaryKey.Data = dataTable.Rows[dataGrid.SelectedIndex][PrimaryKey].ToString();
                }
                eventRowSelected?.Execute();
            }
        }


        // cell initialized style
        private Style GetCellStyle()
        {
            var cellStyle = new Style();
            cellStyle.Setters.Add(new Setter() { Property = Control.FontSizeProperty, Value = 14.0 });
            cellStyle.Setters.Add(new Setter() { Property = FrameworkElement.VerticalAlignmentProperty, Value = VerticalAlignment.Stretch });
            cellStyle.Setters.Add(new Setter() { Property = FrameworkElement.HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });

            // cell selection style
            cellStyle.Resources.Add(SystemColors.InactiveSelectionHighlightBrushKey, Brushes.DodgerBlue);
            var trigger = new Trigger() { Property = DataGridCell.IsSelectedProperty, Value = true };
            trigger.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White)); // Transparent
            trigger.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)))); // Transparent
            trigger.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            trigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.DodgerBlue));
            cellStyle.Triggers.Add(trigger);

            return cellStyle;
        }


        private DataGridColumn GetInitializedColumn(DataColumn column)
        {
            return column.ColumnName.Equals("checkbox") ?
            // checkbox column
            (DataGridColumn)new DataGridCheckBoxColumn()
            {
                Header = column.ColumnName,
                Width = 50,
                Binding = new Binding(column.ColumnName)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                },
                ElementStyle = new Style()
                {
                    Setters = {
                        new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center),
                        new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center)
                    }
                }
            } :
            // text column
            new DataGridTextColumn()
            {
                Header = column.ColumnName,
                Binding = new Binding(column.ColumnName),
                ElementStyle = new Style()
                {
                    Setters = {
                        new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center),
                        new Setter(FrameworkElement.MarginProperty, new Thickness(5, 0, 0, 0))
                    }
                },
                Visibility = "hide".Equals(column.Prefix) ? Visibility.Collapsed : Visibility.Visible
            };
        }
    }
}
