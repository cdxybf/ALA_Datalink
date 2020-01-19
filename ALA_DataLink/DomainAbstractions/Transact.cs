using ProgrammingParadigms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    /// <summary>
    /// Transact ITableDataFlow from source to destination, copy the data in the process
    /// Matches the column names and copies the data in the header and rows.
    /// Error messages are output as rows on an Error Port which is a ITableData.
    /// Error messages can be for a whole columns, a whole row, or a specific cell.
    /// </summary>
    public class Transact : IEvent
    {
        // properties 
        public string InstanceName;
        public bool ClearDestination = false;
        public bool AutoLoadNextBatch = false;

        // outputs
        private ITableDataFlow tableDataFlowSource;
        private ITableDataFlow tableDataFlowDestination;
        private List<IDataFlow<string>> dataFlowsIndex = new List<IDataFlow<string>>();
        private IEvent eventCompleteNoErrors;
        private IDataFlow<bool> dataFlowTransacting;

        // private fields
        private bool transactionInProgress;
        private bool newTransactionPending;

        /// <summary>
        /// Transact data from source to destination which connect to it. The data will be copied in the process.
        /// </summary>
        public Transact() { }

        // -----------------------------------------------------------------------------------------
        // IEvent implementation
        void IEvent.Execute()
        {
            if (transactionInProgress)
            {
                newTransactionPending = true;
            }
            else
            {
                var _fireAndForgot = TransactStartTask(tableDataFlowSource, tableDataFlowDestination);
            }
        }


        // private methods --------------------------------------------------------------------------
        private async Task TransactStartTask(ITableDataFlow source, ITableDataFlow destination)
        {
            transactionInProgress = true;
            
            if (ClearDestination)
            {
                destination.DataTable.Rows.Clear();
                destination.DataTable.Columns.Clear();
                await destination.PutHeaderToDestinationAsync();
            }

            await source.GetHeadersFromSourceAsync();

            // transact column headers and meta datas
            destination.DataTable.TableName = source.DataTable.TableName;
            destination.CurrentRow = source.CurrentRow;
            foreach (DataColumn c in source.DataTable.Columns)
            {
                destination.DataTable.Columns.Add(new DataColumn(c.ColumnName) { Prefix = c.Prefix });
            }

            await destination.PutHeaderToDestinationAsync();

            if (destination.DataTable.Columns.Count > 0)
            {
                await TransferOnePageTask(source, destination);
            }

            transactionInProgress = false;
        }


        // continuation task below
        private async Task TransferOnePageTask(ITableDataFlow source, ITableDataFlow destination)
        {
            // This function will move all the data if the instance configuration, AutoLoadNextBatch is true, other wise only one batch


            transactionInProgress = true;

            do
            {

                Tuple<int, int> tuple = await source.GetPageFromSourceAsync(); 

                if (newTransactionPending)
                {
                    newTransactionPending = false;
                    await TransactStartTask(tableDataFlowSource, tableDataFlowDestination);
                }
                else
                {
                    if (tuple.Item1 < tuple.Item2)  // we actually got some data
                    {
                        TransactOnePage(source, destination, tuple.Item1, tuple.Item2);

                        if (AutoLoadNextBatch)
                        {
                            //if (tuple.Item1 >= tuple.Item2) break;
                            // {
                                // await TransferOnePageTask(source, destination);
                            // }
                        }
                        else
                        {
                            // notify transaction
                            await destination.PutPageToDestinationAsync(tuple.Item1, tuple.Item2, async () =>
                            {
                                if (tuple.Item1 < tuple.Item2)
                                {
                                    await TransferOnePageTask(source, destination);
                                }
                            });
                        }
                    }
                    else
                    {
                        transactionInProgress = false;
                        await destination.PutPageToDestinationAsync(AutoLoadNextBatch ? 0 : tuple.Item1, tuple.Item2, null);
                        eventCompleteNoErrors?.Execute();
                        if (dataFlowTransacting != null) dataFlowTransacting.Data = false;
                        break;
                    }
                }

                transactionInProgress = false;

            }
            while (AutoLoadNextBatch);
        }


        // start to transact the data from source to destination 
        // firstly trasact the headers, then the columns
        // the destination will be filled with the data that it does not have in the source
        private void TransactOnePage(ITableDataFlow source, ITableDataFlow destination, int firstRowIndex, int lastRowIndex)
        {
            var dtSource = source.DataTable;
            var dtDestination = destination.DataTable;

            // --------------------------------------------
            // transact rows
            for (int i = firstRowIndex; i < lastRowIndex; i++)
            {
                dtDestination.ImportRow(dtSource.Rows[i]);
                // the reason of using i+1 is the index of a table starts from 0, the user should see 1 when it's 0.
                foreach (var d in dataFlowsIndex) d.Data = (i+1).ToString();
            }
        }
    }
}
