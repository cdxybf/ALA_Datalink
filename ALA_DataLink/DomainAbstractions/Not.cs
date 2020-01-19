using ProgrammingParadigms;

namespace DomainAbstractions
{
    /// <summary>
    /// A logic converter, working on IDataFlow<bool>. Converting true to false, or false to true
    /// </summary>
    public class Not : IDataFlow<bool>
    {
        // outputs
        IDataFlow<bool> dataFlowOutput;

        /// <summary>
        /// A not logic which converts boolean IDataFlow true to false, or false to true.
        /// </summary>
        public Not() { }

        // IDataFlow<bool> implementation -------------------------------------------------------
        bool IDataFlow<bool>.Data { set => dataFlowOutput.Data = !value; }
    }
}
