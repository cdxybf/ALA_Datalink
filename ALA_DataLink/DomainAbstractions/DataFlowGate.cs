using ProgrammingParadigms;

namespace DomainAbstractions
{
    // A Gate used to block data flow. Work as similar as an EventGate, when data flow coming in, it will be stored if the gate
    // was turned off. Otherwise, the data will be assigned to the field dataFlow. It actually has 3 inputs:
    // IDataFlow<T> is a generic type of data flow which will be blocked here.
    // IEvent will assign any data stored here to the dataFlow field which only works for turnning on the gate.
    // IDataFlowB<bool> is used to control to turn the gate on or off.
    public class DataFlowGate<T> : IDataFlow<T>, IEvent
    {
        // properties
        public string InstanceName;
        public bool LatchInput = false;

        // outputs
        private IDataFlow<T> dataFlow;
        private IDataFlowB<bool> dataFlowB;

        /// <summary>
        /// Controls the latch of a data flow to block data, and discharges it when recieving a signal.
        /// </summary>
        public DataFlowGate() { }

        // IDataFlow<T> implementation ----------------------
        private T data;
        T IDataFlow<T>.Data {
            set
            {
                if (LatchInput)
                {
                    dataFlow.Data = value;
                }
                else
                {
                    data = value;
                }
            }
        }

        // IEvent implementation -----------------------------------------
        void IEvent.Execute()
        {
            dataFlow.Data = data;
            data = default(T);
        }

        private void PostWiringInitialize()
        {
            if (dataFlowB != null)
            {
                dataFlowB.DataChanged += () =>
                {
                    LatchInput = dataFlowB.Data;
                    if (LatchInput && data != null)
                    {
                        dataFlow.Data = data;
                    }
                };
            }
        }
    }
}
