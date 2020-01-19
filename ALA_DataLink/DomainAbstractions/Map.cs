using ProgrammingParadigms;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    // This delegate is used to get the value of the determined column of the provided DataRow. 
    // The selection is a parameter that help the delegate to make a decision and this delegate is only used here.
    public delegate object MapDelegate(DataRow row, string selection);

    /// <summary>
    /// An ITableDataFlow decorator, working on a specific column given by the public property. The value of the 
    /// Column comes from the Delegate which will be customized by any user. An extra parameter was offered to the 
    /// Delegate as well by the input port of the Map so the Delegate will handle more possibilities. 
    /// The 2 inputs are:
    /// 1. ITableDataFlow, decorating the field tableDataFlow, let data goes through but copy the data as well.
    /// A variation is to map all the columns into a new set of columns (like a C# RX Select statement can map to 
    /// a new result class using fields from the source class.)
    /// 2. IDataFlow<string>, offering the parameter which will be used by the Delegate.
    /// </summary>
    public class Map : ITableDataFlow, IDataFlow<string>, IEvent
    {
        // properties
        public string Column;
        public MapDelegate MapDelegate;

        // outputs
        private IEvent eventOutput; // wire to Transact
        private ITableDataFlow tableDataFlow;

        // private fields
        private bool calledGetHeaderMethod = false;

        /// <summary>
        /// Set the value of a column of an ITableDataFlow by the given Delegate and a string parameter.
        /// </summary>
        public Map() { }


        // IDataFlow<string> implementation -------------------------------------------------------------------
        string IDataFlow<string>.Data
        {
            set
            {
                foreach (DataRow r in dataTable.Rows) r[Column] = MapDelegate(r, value);
                eventOutput?.Execute();
            }
        }

        // IEvent implementation ---------------------------------------
        void IEvent.Execute()
        {
            dataTable.Rows.Clear();
            dataTable.Columns.Clear();
        }

        // ITableDataFlow implementation ------------------------------------------------------------------------
        private DataTable dataTable = new DataTable();
        DataTable ITableDataFlow.DataTable => dataTable;

        DataRow ITableDataFlow.CurrentRow { get; set; }

        async Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync()
        {
            if (calledGetHeaderMethod)
            {
                calledGetHeaderMethod = false;
                return new Tuple<int, int>(0, dataTable.Rows.Count);
            }
            else
            {
                return new Tuple<int, int>(0, 0);
            }
        }

        async Task ITableDataFlow.GetHeadersFromSourceAsync()
        {
            calledGetHeaderMethod = true;
        }

        async Task ITableDataFlow.PutHeaderToDestinationAsync()
        {
            if (tableDataFlow == null) return;

            tableDataFlow.DataTable.Rows.Clear();
            tableDataFlow.DataTable.Columns.Clear();
            tableDataFlow.DataTable.TableName = dataTable.TableName;

            foreach (DataColumn c in dataTable.Columns)
            {
                tableDataFlow.DataTable.Columns.Add(new DataColumn(c.ColumnName) { Prefix = c.Prefix });
            }

            await tableDataFlow.PutHeaderToDestinationAsync();
        }

        async Task ITableDataFlow.PutPageToDestinationAsync(
            int firstRowIndex, 
            int lastRowIndex, 
            GetNextPageDelegate getNextPage)
        {
            if (tableDataFlow == null)
            {
                getNextPage?.Invoke();
                return;
            }

            for (var i = firstRowIndex; i < lastRowIndex; i++)
            {
                tableDataFlow.DataTable.ImportRow(dataTable.Rows[i]);
            }

            await tableDataFlow.PutPageToDestinationAsync(firstRowIndex, lastRowIndex, getNextPage);
        }
    }
}
