using ProgrammingParadigms;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    /// <summary>
    /// Takes a ITableDataFlow on it RHS port.
    /// When a transaction occurs on this port, it starts iterating through the rows.
    /// It has a output called CurrentRow which is a ITableDataFlow with a single Row and the same columns as the input table.
    /// It generates a Transact operation.
    /// It has a output port called Started of type IEvent.
    /// It has an output port called Index of type IdataFlow of type number which outputs the index of the current row.
    /// It has an input port called Next or type IEvent that causes it to go to the next row of the input table.
    /// It has an output called Complete of type IEvent.
    /// It has an input called Stop or type IEvent, which can be used to stop the iterator before it finishes all the rows.
    /// </summary>
    public class Iterator : ITableDataFlow, IEvent
    {
        // outputs -------------------------------------------------------
        // CurrentRow
        private IDataFlow<DataTable> currentRowOutput;
        // IteratorRunning
        private IDataFlow<bool> iteratorRunningOutput;
        // Complete
        private IEvent eventCompleteOutput;
        // Index 
        private IDataFlow<string> indexOutput;

        // Stop - input (reversal output)
        private IEventB eventBStopInput;


        /// <summary>
        /// When the ITableDataFlow coming in, it iterates them one by one by outputing triggering event
        /// on Transact and recieves event to do next after the previous transaction.
        /// </summary>
        public Iterator() { }


        private void PostWiringInitialize()
        {
            //eventBStopInput.EventHappened += () =>
            //{
            //    // stop iterating
            //};
        }

        // IEvent implmentation ------------------------------------------
        // Next - input
        void IEvent.Execute()
        {
            currentRowIndex += 1;

            if (currentRowIndex >= dataTable.Rows.Count)
            {
                // finish iterating, stop and output signals
                currentRowIndex = 0;
                eventCompleteOutput?.Execute();
                iteratorRunningOutput.Data = false;
            }
            else
            {
                Iterating(currentRowIndex);
            }
        }        

        // ITableDataFlow implmentation ------------------------------------------------
        private DataTable dataTable = new DataTable();
        DataTable ITableDataFlow.DataTable => dataTable;
        DataRow ITableDataFlow.CurrentRow { get; set; }
        Task ITableDataFlow.GetHeadersFromSourceAsync() { throw new NotImplementedException(); }
        Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync() { throw new NotImplementedException(); }

        async Task ITableDataFlow.PutHeaderToDestinationAsync() { }

        async Task ITableDataFlow.PutPageToDestinationAsync(
            int firstRowIndex, 
            int lastRowIndex, 
            GetNextPageDelegate getNextPage)
        {
            Iterating(currentRowIndex);
        }

        private int currentRowIndex = 0;
        private void Iterating(int rowIndex)
        {
            if (rowIndex >= dataTable.Rows.Count) return;

            indexOutput.Data = (rowIndex + 1).ToString();
            iteratorRunningOutput.Data = true;

            var dt = dataTable.Copy();
            dt.Rows.Clear();
            dt.ImportRow(dataTable.Rows[rowIndex]);
            currentRowOutput.Data = dt;
        }
    }
}
