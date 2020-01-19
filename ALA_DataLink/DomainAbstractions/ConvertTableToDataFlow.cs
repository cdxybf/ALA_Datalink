using ProgrammingParadigms;
using System.Data;

namespace DomainAbstractions
{
    /// <summary>
    /// Converts DataTable to DataFlow. To be explicit, it picks one cell from the DataTable with the giving 
    /// Column name and Row Index(or Primary Key).
    /// When it's instantiated, the parameter 'Column' is required to pick one column. As for the Row, it can
    /// be assigned programmatically when instantiated or, using the IDataFlow<string> input to assign the 
    /// primary key then to find the right Row.
    /// </summary>
    public class ConvertTableToDataFlow : IDataFlow<DataTable>, IDataFlow<string>
    {
        // properties --------------------------------------------------------------
        public string InstanceName;
        public string Column;
        // -1 means the Row will not be used. Instead, we use the primary key to find the Row we want.
        public int Row = -1;

        // outputs -----------------------------------------------------------------
        private IDataFlow<string> cellOutput;

        // private fields ----------------------------------------------------------
        private DataTable dataTable;


        /// <summary>
        /// Converts DataTable to DataFlow. To be explicit, it picks one cell from the DataTable with 
        /// the giving Column Name and Row Index(or Primary Key).
        /// </summary>
        public ConvertTableToDataFlow() {}


        // IDataFlow<DataTable> implmentation --------------------------------------
        DataTable IDataFlow<DataTable>.Data
        {
            set
            {
                dataTable = value;
                dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["index"] };

                if (Row != -1)
                {
                    cellOutput.Data = dataTable.Rows[Row][Column].ToString();
                }
            }
        }

        // IDataFlow<string> implmentation -----------------------------------------
        string IDataFlow<string>.Data {
            set
            {
                // here value is the primary key
                if (dataTable != null)
                    cellOutput.Data = dataTable.Rows.Find(value)[Column].ToString();
            }
        }
    }
}
