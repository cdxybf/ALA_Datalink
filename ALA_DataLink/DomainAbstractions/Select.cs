using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ProgrammingParadigms;

namespace DomainAbstractions
{
    /// <summary>
    /// A ITableDataFlow decorator, which selects certain columns by the giving property 'Columns', and remove 
    /// other columns.
    /// </summary>
    public class Select : ITableDataFlow, IDataFlow<DataTable>
    {
        // properties
        public string[] Columns;

        // outputs
        private ITableDataFlow tableDataFlow;
        private IDataFlow<DataTable> dataFlowDataTable;

        /// <summary>
        /// Takes an ITableDataFlow and keeps certain columns.
        /// </summary>
        public Select() { }

        // IDataFlow<DataTable> implementation
        private DataTable dataTableHeader;
        DataTable IDataFlow<DataTable>.Data { set => dataTableHeader = value; }      

        // ITableDataFlow implementation ---------------------------------------------------
        private DataTable dataTable = new DataTable();
        DataTable ITableDataFlow.DataTable => dataTable;

        DataRow ITableDataFlow.CurrentRow { get; set; }

        async Task ITableDataFlow.GetHeadersFromSourceAsync()
        {
            await tableDataFlow.GetHeadersFromSourceAsync();

            dataTable.Rows.Clear();
            dataTable.Columns.Clear();
            dataTable.TableName = tableDataFlow.DataTable.TableName;

            foreach (DataColumn c in tableDataFlow.DataTable.Columns)
            {
                if (Columns.Contains(c.ColumnName))
                {
                    dataTable.Columns.Add(new DataColumn(c.ColumnName) { Prefix = c.Prefix });
                }
            }
        }

        async Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync()
        {
            Tuple<int, int> tuple = await tableDataFlow.GetPageFromSourceAsync();
            foreach (DataRow r in tableDataFlow.DataTable.Rows) dataTable.ImportRow(r);

            return tuple;
        }

        async Task ITableDataFlow.PutHeaderToDestinationAsync()
        {
            if (tableDataFlow == null) return;

            tableDataFlow.DataTable.Rows.Clear();
            tableDataFlow.DataTable.Columns.Clear();
            tableDataFlow.DataTable.TableName = dataTable.TableName;

            foreach (DataColumn c in dataTable.Columns)
            {
                if (Columns.Contains(c.ColumnName))
                {
                    tableDataFlow.DataTable.Columns.Add(new DataColumn(c.ColumnName) { Prefix = c.Prefix });
                }
            }

            await tableDataFlow.PutHeaderToDestinationAsync();
        }

        async Task ITableDataFlow.PutPageToDestinationAsync(int firstRowIndex, int lastRowIndex, GetNextPageDelegate getNextPage)
        {
            if (tableDataFlow == null) return;

            for (var i = firstRowIndex; i < lastRowIndex; i++)
            {
                tableDataFlow.DataTable.ImportRow(dataTable.Rows[i]);
            }

            // output the data table
            dataFlowDataTable.Data = dataTable;

            await tableDataFlow.PutPageToDestinationAsync(firstRowIndex, lastRowIndex, getNextPage);
        }
    }
}
