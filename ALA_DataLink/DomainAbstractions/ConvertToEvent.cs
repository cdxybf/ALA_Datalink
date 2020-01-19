using ProgrammingParadigms;

namespace DomainAbstractions
{
    /// <summary>
    /// Converts the activation event within the IUI interface to an IEvent.
    /// Converts any kind of IDataFlow to an IEvent. 
    /// The Generic Type 'T' should be assigned when it is instantiated.
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    public class ConvertToEvent<T> : IDataFlow<T>
    {
        // properties
        public string InstanceName;

        // outputs 
        private IEvent eventOutput;

        /// <summary>
        /// Converts the activation event of an IUI or an IDataFlow to an IEvent. 
        /// </summary>
        public ConvertToEvent() { }

        // IDataFlow<T> implmentation -----------------------------
        T IDataFlow<T>.Data { set => eventOutput.Execute(); }
    }
}
