using ProgrammingParadigms;

namespace DomainAbstractions
{
    /// <summary>
    /// Using StringFormat with nothing connected to the list of inputs
    /// </summary>
    public class LiteralString : IDataFlow<bool>, IEvent
    {
        // properties
        public string InstanceName;

        // outputs
        private IDataFlow<string> dataFlowOutput;

        // private fields
        private string literalString;

        /// <summary>
        /// Using StringFormat with nothing connected to the list of inputs
        /// </summary>
        /// <param name="liter">the literal string</param>
        public LiteralString(string liter)
        {
            literalString = liter;
        }

        // IDataFlow<bool> implementation -------------------------------
        bool IDataFlow<bool>.Data { set { if (value) dataFlowOutput.Data = literalString; } }

        // IEvent implementation --------------------------------------
        void IEvent.Execute() => dataFlowOutput.Data = literalString;
    }
}
