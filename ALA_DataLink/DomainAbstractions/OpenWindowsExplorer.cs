using ProgrammingParadigms;
using System.Diagnostics;
using System.IO;

namespace DomainAbstractions
{
    /// <summary>
    /// It is basically a window which has the same UI style as a general window. All the files in the given
    /// file path will be listed in the window but only the given file name will be highlighted. 
    /// The two inputs are:
    /// 1. IDataFlow<string> which inputs the file full path;
    /// 2. IEvent which open the file window when it is triggered.
    /// </summary>
    public class OpenWindowsExplorer : IDataFlow<string>, IEvent
    {
        // private fields
        private string filePath;

        /// <summary>
        /// A window explorer which aims to diaplay a specfic file in a file list with a given file full path.
        /// </summary>
        public OpenWindowsExplorer() { }

        // IDataFlow<string> implmentation -------------------------------------------------
        string IDataFlow<string>.Data { set => filePath = value; }


        // IEvent implementation -----------------------------------------------------------
        void IEvent.Execute() => Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + filePath));
    }
}
