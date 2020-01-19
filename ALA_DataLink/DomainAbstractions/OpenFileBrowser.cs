using Microsoft.Win32;
using ProgrammingParadigms;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    /// <summary>
    /// It is basically a window which has the same UI style as a general window. Files can be selected
    /// and when the "open" button clicked, it generates an IEvent output as well as a DataFlow<string>
    /// of the count of selected files. The two inputs are:
    /// 1. IEvent which makes the window to show;
    /// 2. ITableDataFlow which generates the selected file paths. e.g. "name" for file name, "path" for 
    /// file path, and "filepath" for the full path of the file.
    /// </summary>
    public class OpenFileBrowser : IEvent, ITableDataFlow
    {
        // outputs
        private IEvent eventFileSelectedOutput;
        private IDataFlow<string> dataFlowSelectedFileCount;

        // private fields
        private OpenFileDialog fileBrowser = new OpenFileDialog();

        /// <summary>
        /// Opening a file browser and selecting or multi-selecting files. 
        /// Selected files should be output through ITableDataFlow.
        /// </summary>
        /// <param name="title">the text diaplayed on the window top border</param>
        public OpenFileBrowser(string title = "")
        {
            fileBrowser.Title = title;
            fileBrowser.Multiselect = true;
            fileBrowser.Filter = "Datamars (*.csv;*.xls;*.xlsx;*.txt;*.xml)|*.csv;*.xls;*.xlsx;*.txt;*.xml|" +
                                 "Datamars CSV file (*.csv)|*.csv|" +
                                 "Microsoft Excel 97-2003 Worksheet (.xls)|*.xls|" +
                                 "Microsoft Excel Worksheet (.xlsx)|*.xlsx|" +
                                 "Datamars XML file (*.xml)|*.xml|" +
                                 "All files (*.*)|*.*";

            fileBrowser.FileOk += (object sender, CancelEventArgs e) => {
                filePaths = fileBrowser.FileNames;
                if (dataFlowSelectedFileCount != null)
                {
                    dataFlowSelectedFileCount.Data = filePaths.Length.ToString();
                }
                eventFileSelectedOutput?.Execute();
            };
        }


        // IEvent implementation ------------------------------------------------------------
        void IEvent.Execute()
        {
            fileBrowser.ShowDialog();
        }


        // ITableDataFlow implmentation -----------------------------------------------------
        private DataTable dataTable = new DataTable();
        DataTable ITableDataFlow.DataTable => dataTable;
        DataRow ITableDataFlow.CurrentRow { get; set; }
        private string[] filePaths;

        private bool calledGetHeaderMethod = false;
        async Task ITableDataFlow.GetHeadersFromSourceAsync()
        {
            dataTable.Rows.Clear();
            dataTable.Columns.Clear();

            dataTable.Columns.Add("name");
            dataTable.Columns.Add("path");
            dataTable.Columns.Add("pathname");

            calledGetHeaderMethod = true;
        }

        async Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync()
        {
            if (filePaths == null || filePaths.Length == 0)
            {
                return new Tuple<int, int>(0, 0);
            }

            if (!calledGetHeaderMethod)
            {
                return new Tuple<int, int>(dataTable.Rows.Count, dataTable.Rows.Count);
            }

            dataTable.Rows.Clear();

            foreach (string p in filePaths)
            {
                DataRow r = dataTable.NewRow();
                r["name"] = Path.GetFileName(p);
                r["path"] = Path.GetDirectoryName(p);
                r["pathname"] = p;

                dataTable.Rows.Add(r);
            }

            calledGetHeaderMethod = false;
            return new Tuple<int, int>(0, dataTable.Rows.Count);
        }

        Task ITableDataFlow.PutHeaderToDestinationAsync() { throw new NotImplementedException(); }
        Task ITableDataFlow.PutPageToDestinationAsync(int firstRowIndex, int lastRowIndex, GetNextPageDelegate callBack) { throw new NotImplementedException(); }
    }
}
