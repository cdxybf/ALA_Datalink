using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ProgrammingParadigms;

namespace DomainAbstractions
{
    /// <summary>
    /// Basically, it is an ITableDataFlow decorator. This would happen on a whole table which based on all the
    /// rows has been transacted. Otherwise, after the sorting the firtRowIndex and lastRowIndex will not accurately
    /// indicate the right rows which has been transacted.
    /// </summary>
    public class Sort : ITableDataFlow
    {
        // properties
        public string Column;
        public bool IsDescending;

        // outputs
        private ITableDataFlow tableDataFlow;

        /// <summary>
        /// Sort rows by a giving Column and Order type, Ascending or Descending
        /// </summary>
        public Sort() { }

        // ITableDataFlow implementation -------------------------------------------------------
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
                dataTable.Columns.Add(new DataColumn(c.ColumnName) { Prefix = c.Prefix });
            }
        }

        async Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync()
        {
            Tuple<int, int> tuple = await tableDataFlow.GetPageFromSourceAsync();

            DataTable tempTable = IsDescending ?
                (from row in dataTable.AsEnumerable()
                 let date = Convert.ToDateTime(row.Field<string>(Column))
                 orderby date descending
                 select row).CopyToDataTable() :
                (from row in dataTable.AsEnumerable()
                 let date = Convert.ToDateTime(row.Field<string>(Column))
                 orderby date ascending
                 select row).CopyToDataTable();

            foreach (DataRow r in tempTable.Rows) dataTable.ImportRow(r);

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
                tableDataFlow.DataTable.Columns.Add(new DataColumn(c.ColumnName) { Prefix = c.Prefix });
            }

            await tableDataFlow.PutHeaderToDestinationAsync();
        }

        async Task ITableDataFlow.PutPageToDestinationAsync(int firstRowIndex, int lastRowIndex, GetNextPageDelegate callBack)
        {
            if (tableDataFlow == null)
            {
                callBack?.Invoke();
                return;
            }

            if (dataTable.Rows.Count > 0)
            {

                /* DataTable tempTable = IsDescending ?
                (from row in dataTable.AsEnumerable()
                 let date = Convert.ToDateTime(row.Field<string>(Column))
                 orderby date descending
                 select row).CopyToDataTable() :
                (from row in dataTable.AsEnumerable()
                 let date = Convert.ToDateTime(row.Field<string>(Column))
                 orderby date ascending
                 select row).CopyToDataTable(); */

                foreach (DataRow r in dataTable.Rows) tableDataFlow.DataTable.ImportRow(r);
            }

            await tableDataFlow.PutPageToDestinationAsync(firstRowIndex, lastRowIndex, callBack);
        }
    }
}
