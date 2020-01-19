using ProgrammingParadigms;

namespace DomainAbstractions
{
    /// <summary>
    /// It compares a stored value with an input value, and emits an IDataFlow<bool> to indicate the comparison result.
    /// The input port IDataFlow<T> is for the input value.
    /// </summary>
    /// <typeparam name="T">Generic Type of any kind of comparing data</typeparam>
    public class Equals<T> : IDataFlow<T>
    {
        // properties
        public string InstanceName;

        // outputs 
        private IDataFlow<bool> dataFlowOutput;
        
        // private fields 
        private T compareData;

        /// <summary>
        /// Works similar as the object.Equals() method. 
        /// </summary>
        public Equals(T compareData)
        {
            this.compareData = compareData;
        }

        // IDataFlow<T> implmentation ---------------------------------------
        T IDataFlow<T>.Data
        {
            set
            {
                if (compareData == null)
                {
                    dataFlowOutput.Data = null == value;
                }
                else
                {
                    dataFlowOutput.Data = compareData.Equals(value);
                }
            }
        }
    }
}
