using Microsoft.Win32;
using ProgrammingParadigms;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace DomainAbstractions
{
    /// <summary>
    /// The aim of this abstraction is to get a file save path by user selecting the folder manually.
    /// It has an input box for inputing the file name the user wants to save.
    /// It has a save button, when clicked on that, it generates the file name, path and full path for outputing.
    /// It also outputs the file format index
    /// The two inputs are:
    /// 1. IEvent for opening the browser.
    /// 2. IDataFlow<string> for initialization of file name of the browser.
    /// </summary>
    public class SaveFileBrowser : IEvent, IDataFlow<string>
    {
        // outputs
        private List<IDataFlow<string>> dataFlowOutputFileNames = new List<IDataFlow<string>>();
        private List<IDataFlow<string>> dataFlowOutputFilePaths = new List<IDataFlow<string>>();
        private List<IDataFlow<string>> dataFlowOutputFilePathNames = new List<IDataFlow<string>>();
        private IDataFlow<int> dataFlowFileFormatIndex;

        // private fields
        private SaveFileDialog saveFileDialog = new SaveFileDialog();

        /// <summary>
        /// Opening a file browser and save a file or mutiple files to the folder path opened. 
        /// </summary>
        /// <param name="title">the text diaplayed on the window top border</param>
        public SaveFileBrowser(string title = null)
        {
            saveFileDialog.Title = title;
            saveFileDialog.Filter = "Datamars CSV file(*.csv)|*.csv|" +
                                    "Datamars CSV file No Header(*.csv)|*.csv|" +
                                    "Datamars CSV file 3000 format(*.csv)|*.csv|" +
                                    "Datamars CSV file Minda format(*.csv)|*.csv|" +
                                    "Datamars CSV file EID only(*.csv)|*.csv|" +
                                    "Microsoft Excel 97-2003 Worksheet(*.xls)|*.xls|" +
                                    "Microsoft Excel Worksheet(*.xlsx)|*.xlsx|" +
                                    "Microsoft Excel 97-2003 Worksheet from Template File(*.xls)|*.xls|" +
                                    "Datamars XML file(*.xml)|*.xml|" +
                                    "All files(*.*)|*.*";
            //saveFileDialog.FileName = "XR3000 Life data";
            saveFileDialog.FileOk += (object sender, CancelEventArgs e) => {
                if (dataFlowFileFormatIndex != null) dataFlowFileFormatIndex.Data = saveFileDialog.FilterIndex;

                foreach (var i in dataFlowOutputFileNames) i.Data = Path.GetFileName(saveFileDialog.FileName);
                foreach (var i in dataFlowOutputFilePaths) i.Data = Path.GetDirectoryName(saveFileDialog.FileName);
                foreach (var i in dataFlowOutputFilePathNames) i.Data = saveFileDialog.FileName;
            };
        }

        // IDataFlow<string> implementation --------------------------------------------------------
        string IDataFlow<string>.Data { set => saveFileDialog.FileName = value; }

        // IEvent implementation -------------------------------------------------------------------
        void IEvent.Execute()
        {
            saveFileDialog.ShowDialog();
        }
    }
}
