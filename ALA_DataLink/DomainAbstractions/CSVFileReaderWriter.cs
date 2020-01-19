using ProgrammingParadigms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    /// <summary>
    /// Reading text records from a csv file and converts them to the 'ITableDataFlow' type.
    /// Wrting 'ITableDataFlow' type data to a csv file.
    /// It has 3 inputs:
    /// 1. The ITableDataFlow for getting/putting data;
    /// 2. The IDataFlow<string> for input the file full path;
    /// 3. The IDataFlow<int> for input the file format index because there are kinds of file 
    /// format when writing, the abstration will write as the format selected. The formats are 
    /// related to the SaveFileBrowser
    /// </summary>
    public class CSVFileReaderWriter : ITableDataFlow, IDataFlow<string>, IDataFlow<int>
    {
        public string InstanceName;
        public string FilePath { set => filePath = value; }
        public int FileType { set => fileFormatIndex = value; }

        // outputs ----------------------------------------------------------------------------------------
        private IDataFlow<bool> dataFlowOpenOrCloseProgressWindow;
        private IDataFlow<string> dataFlowRecordsTotalCount;
        private IDataFlow<string> dataFlowFilePath;
        private IEvent eventOutputSuccessWindow;
        private IDataFlow<DataTable> dataFlowTableHeader;
        private IDataFlow<string> dataFlowSessionId;
        private IDataFlow<DataTable> dataFlowDataTable;

        // private fields ---------------------------------------------------------------------------------
        private StringBuilder stringBuilder = new StringBuilder();
        private string[] contents;
        private int pageSize = 10;
        private int currentIndex = 0;

        /// <summary>
        /// Reading records from csv file and writing records to csv file. 
        /// The only accepted data type is ITableDataFlow.
        /// </summary>
        public CSVFileReaderWriter() { }

        // ITableDataFlow implementation -------------------------------------------------------------------
        private DataTable dataTable = new DataTable();
        DataTable ITableDataFlow.DataTable => dataTable;
        DataRow ITableDataFlow.CurrentRow { get; set; }

        private List<string> headers = new List<string>();
        async Task ITableDataFlow.GetHeadersFromSourceAsync()
        {
            contents = File.Exists(filePath) ? 
                File.ReadAllLines(filePath) : new string[] { "" };

            bool isXR3000Format = fileFormatIndex == 3;
            currentIndex = isXR3000Format ? 5 : 1;

            if (contents.Length >= currentIndex) // data exists
            {
                if (dataFlowRecordsTotalCount != null) // number of records outputs
                {
                    dataFlowRecordsTotalCount.Data = (contents.Length - currentIndex).ToString();
                }

                // initialization
                headers.Clear();
                dataTable.Rows.Clear();
                dataTable.Columns.Clear();

                var header = isXR3000Format ? contents[4] : contents[0];
                foreach (var h in header.Split(',')) headers.Add(h);

                if (isXR3000Format)
                {
                    string fileNo = contents[0];
                    string fileName = contents[1];
                    string fileDate = contents[2];
                    dataTable.TableName = fileNo + ";" + fileName + ";" + fileDate;

                    dataTable.Columns.Add("EID");
                    dataTable.Columns.Add("Weight");
                    dataTable.Columns.Add("FID");
                }
                else
                {
                    foreach (var h in headers) dataTable.Columns.Add(h);
                }
            }
        }

        async Task<Tuple<int, int>> ITableDataFlow.GetPageFromSourceAsync()
        {
            if (currentIndex >= contents.Length)
            {
                string[] meta = dataTable.TableName.Split(';');
                if (meta.Length >= 3 && dataFlowTableHeader != null)
                {
                    string fileId = meta[0].Length > 8 ? meta[0].Substring(8) : "0";
                    string fileName = meta[1].Length > 6 ? meta[1].Substring(6) : "session-file";
                    fileName = fileName.IndexOf(",") > 0 ? fileName.Substring(0, fileName.IndexOf(",")) : fileName;
                    string date = meta[2].Length > 15 ? meta[2].Substring(6, 10) : DateTime.Now.ToString("dd/MM/yyyy");
                    DataTable dt = new DataTable();
                    dt.Columns.Add("FileId");
                    dt.Columns.Add("FileName");
                    dt.Columns.Add("Date");
                    dt.Columns.Add("Count");
                    DataRow r = dt.NewRow();
                    r["FileId"] = fileId;
                    r["FileName"] = fileName;
                    r["Date"] = date;
                    r["Count"] = dataTable.Rows.Count + "";
                    dt.Rows.Add(r);
                    dataFlowTableHeader.Data = dt;

                    dataFlowSessionId.Data = fileId;
                    dataFlowDataTable.Data = dataTable;
                }

                return new Tuple<int, int>(dataTable.Rows.Count, dataTable.Rows.Count);
            }

            int startRowCount = dataTable.Rows.Count;
            int startIndex = currentIndex;
            for (int i = startIndex; i < startIndex + pageSize; i++)
            {
                if (i >= contents.Length)
                {
                    break;
                }

                currentIndex += 1;

                string[] content = contents[i].Split(',');
                DataRow r = dataTable.NewRow();
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    int index = headers.IndexOf(dataTable.Columns[j].ColumnName);
                    if (index < content.Length && index >= 0)
                    {
                        r[dataTable.Columns[j].ColumnName] = content[index];
                    }
                }
                dataTable.Rows.Add(r);
            }

            return new Tuple<int, int>(startRowCount, dataTable.Rows.Count);
        }

        async Task ITableDataFlow.PutHeaderToDestinationAsync()
        {
            if (dataFlowOpenOrCloseProgressWindow != null)
                dataFlowOpenOrCloseProgressWindow.Data = true;

            stringBuilder.Clear();

            switch (fileFormatIndex)
            {
                case 0: // for appending text
                    foreach (DataColumn c in dataTable.Columns) if (!"hide".Equals(c.Prefix)) stringBuilder.Append(c.ColumnName + ',');
                    stringBuilder.Append(Environment.NewLine);
                    if (File.Exists(filePath))
                    {
                        string[] contents = File.ReadAllLines(filePath);
                        for (int i = 1; i < contents.Length; i++)
                        {
                            stringBuilder.Append(contents[i]);
                            stringBuilder.Append(Environment.NewLine);
                        }
                    }
                    break;
                case 1:  // csv file
                    foreach (DataColumn c in dataTable.Columns) if (!"hide".Equals(c.Prefix)) stringBuilder.Append(c.ColumnName + ',');
                    stringBuilder.Append(Environment.NewLine);
                    break;
                case 2:  // no header - do nothing
                    break;
                case 3:  // 3000 format
                    string[] meta = dataTable.TableName.Split(';');
                    foreach (var m in meta) stringBuilder.Append(m + Environment.NewLine);

                    string header = meta.Length > 1 ? "F11EID(16)isID,DW2Weight(),F01FID(4)notID" : "F01FID(4)notID,F11EID(16)isID";
                    stringBuilder.Append(header + Environment.NewLine);

                    foreach (DataColumn c in dataTable.Columns) if (!"hide".Equals(c.Prefix)) stringBuilder.Append(c.ColumnName + ',');
                    stringBuilder.Append(Environment.NewLine);
                    break;
                case 4:  // minda format
                    string[] metas = dataTable.TableName.Split(';');
                    string headers = metas.Length > 1 ? "Weight,EID,Date" : "EID,Date";
                    if (metas.Length >= 3) if (metas[2].Length > 5) mindaDate = metas[2].Substring(5);
                    stringBuilder.Append(headers + Environment.NewLine);
                    break;
                case 5:  // EID only
                    stringBuilder.Append("EID" + Environment.NewLine);
                    break;
                default:
                    break;
            }
        }

        private string mindaDate;
        async Task ITableDataFlow.PutPageToDestinationAsync(int firstRowIndex, int lastRowIndex, GetNextPageDelegate getNextPage)
        {
            switch (fileFormatIndex)
            {
                case 0:  // appending file
                case 1:  // csv file
                case 2:  // no header
                case 3:  // 3000 format
                    for (var i = firstRowIndex; i < lastRowIndex; i++)
                    {
                        foreach (DataColumn c in dataTable.Columns)
                        {
                            if (!"hide".Equals(c.Prefix))
                            {
                                stringBuilder.Append(dataTable.Rows[i][c.ColumnName].ToString() + ",");
                            }
                        }
                        stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        stringBuilder.Append(Environment.NewLine);
                    }
                    break;
                case 4:  // minda format
                    bool isSession = dataTable.TableName.Split(';').Length > 1;
                    for (var i = firstRowIndex; i < lastRowIndex; i++)
                    {
                        DataRow r = dataTable.Rows[i];
                        stringBuilder.Append(isSession ? r["Weight"].ToString() + "," + r["EID"].ToString() + "," + mindaDate : r["EID"].ToString() + "," + mindaDate);
                        stringBuilder.Append(Environment.NewLine);
                    }
                    break;
                case 5:  // EID only
                    for (var i = firstRowIndex; i < lastRowIndex; i++)
                    {
                        DataRow r = dataTable.Rows[i];
                        stringBuilder.Append(r["EID"].ToString());
                        stringBuilder.Append(Environment.NewLine);
                    }
                    break;
                default:
                    break;
            }
            

            if (firstRowIndex < lastRowIndex && getNextPage != null)
            {
                // load next batch of data
                getNextPage.Invoke();
            }
            else
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // all the data loaded, write them to the file and output the finish signal
                File.WriteAllText(filePath, stringBuilder.ToString());
                stringBuilder.Clear();
                if (dataFlowOpenOrCloseProgressWindow != null)
                    dataFlowOpenOrCloseProgressWindow.Data = false;
                if (dataFlowFilePath != null)
                    dataFlowFilePath.Data = filePath;
                eventOutputSuccessWindow?.Execute();
            }
        }

        // IDataFlow<string> implementation ------------------------------------------------------
        private string filePath;
        string IDataFlow<string>.Data { set => filePath = value; }

        // IDataFlow<int> implmentation ----------------------------------------------------------
        private int fileFormatIndex = 1;
        int IDataFlow<int>.Data { set => fileFormatIndex = value; }

        /* File formats list, the fileFormatIndex will indicate wich format the CSV file will use.
         * Here it ranges from 1 - 5.
            "Datamars CSV file(*.csv)|*.csv|" +
            "Datamars CSV file No Header(*.csv)|*.csv|" +
            "Datamars CSV file 3000 format(*.csv)|*.csv|" +
            "Datamars CSV file Minda format(*.csv)|*.csv|" +
            "Datamars CSV file EID only(*.csv)|*.csv|" +
            "Microsoft Excel 97-2003 Worksheet(*.xls)|*.xls|" +
            "Microsoft Excel Worksheet(*.xlsx)|*.xlsx|" +
            "Microsoft Excel 97-2003 Worksheet from Template File(*.xls)|*.xls|" +
            "Datamars XML file(*.xml)|*.xml|" +
            "All files(*.*)|*.*";
        */
    }
}
