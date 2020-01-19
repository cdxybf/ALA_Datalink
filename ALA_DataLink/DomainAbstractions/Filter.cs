using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ProgrammingParadigms;

namespace DomainAbstractions
{
    public delegate bool FilterLambdaDelegate(DataRow row);
    public delegate bool FilterLambdaParamDelegate(DataRow row, string param);

    /// <summary>
    /// Generally it is an ITableDataFlow decorator which use a lambda to filter the data
    /// under condition. It goes through all the rows and only stored and returns the row 
    /// which conforms to the condition of the lambda.
    /// </summary>
    public class Filter : ITableDataFlow, IDataFlow<string>
    {
        // properties ---------------------------------------------------------------------
        public string InstanceName;
        public FilterLambdaDelegate FilterDelegate;
        public FilterLambdaParamDelegate FilterLambdaParamDelegate;

        // outputs ------------------------------------------------------------------------
        private ITableDataFlow tableDataFlow;
        private IDataFlow<DataTable> dataFlowDataTableOutput;

        /// <summary>
        /// Filter the rows of data in an ITableDataFlow, 
        /// only keeps the ones comform to the Lambda Delegate.
        /// </summary>
        public Filter() { }

        // IDataFlow implementation ----------------------------------------------
        private string lambdaParam;
        string IDataFlow<string>.Data { set => lambdaParam = value; }

        // ITableDataFlow implmentation ---------------------------------------------------
        private DataTable dataTable = new DataTable();
        DataTable ITableDataFlow.DataTable => dataTable;
        private DataRow currentRow;
        DataRow ITableDataFlow.CurrentRow { get => currentRow; set => currentRow = value; }

        Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync() { throw new NotImplementedException(); }
        Task ITableDataFlow.GetHeadersFromSourceAsync() { throw new NotImplementedException(); }

        async Task ITableDataFlow.PutHeaderToDestinationAsync()
        {
            dataRows.Clear();

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

        private List<DataRow> dataRows = new List<DataRow>();
        async Task ITableDataFlow.PutPageToDestinationAsync(int firstRowIndex, int lastRowIndex, GetNextPageDelegate getNextPage)
        {
            List<DataRow> rowList = new List<DataRow>();

            for (var i = firstRowIndex; i < lastRowIndex; i++)
            {
                DataRow r = dataTable.Rows[i];
                bool condition = FilterDelegate != null ?
                    FilterDelegate(r) : FilterLambdaParamDelegate(r, lambdaParam);

                if (condition) rowList.Add(r);
            }

            if (tableDataFlow == null)
            {
                if (getNextPage != null)
                {
                    dataRows.AddRange(rowList);
                    getNextPage.Invoke();
                }
                else
                {
                    DataTable table = dataTable.Copy();
                    table.Rows.Clear();
                    dataRows.AddRange(rowList);

                    foreach (var r in dataRows) table.ImportRow(r);
                    dataFlowDataTableOutput.Data = table;
                }
            }
            else
            {
                int startIndex = tableDataFlow.DataTable.Rows.Count;
                foreach (var r in rowList) tableDataFlow.DataTable.ImportRow(r);

                if (tableDataFlow.DataTable.Rows.Count == 0 && currentRow != null)
                {
                    tableDataFlow.DataTable.ImportRow(currentRow);
                }

                if (dataFlowDataTableOutput != null)
                    dataFlowDataTableOutput.Data = tableDataFlow.DataTable;

                await tableDataFlow.PutPageToDestinationAsync(startIndex, tableDataFlow.DataTable.Rows.Count, getNextPage);
            }       
        }
    }
}
