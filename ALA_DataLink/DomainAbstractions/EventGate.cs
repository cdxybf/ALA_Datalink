using ProgrammingParadigms;

namespace DomainAbstractions
{
    // The EventGate is used to block and store an IEvent, when an IDataFlow<bool> event coming in, the gate will be turned on
    // or off due to the value. It has two inputs:
    // 1. The IEvent decorates the event, when the IEvent inputs coming in, it does not go to the eventOutput directly, instead, it
    // checks the LatchInput (the gate is on or off), then decides to let the event go or just store here.
    // 2. The IDataFlow<bool> input, which is used to control the LatchInput, when it is true and there is an event recieved and stored,
    // the eventOutput will be executed, other wise it just shut the gate down.
    public class EventGate : IEvent, IDataFlow<bool>
    {
        // properties
        public bool LatchInput = false;
        public string InstanceName;

        // outputs
        private IEvent eventOutput;

        /// <summary>
        /// Controls the latch of the gate to let an event go through or not.
        /// </summary>
        public EventGate() { }

        // IDataFlow<bool> implementation ------------------------------------------
        bool IDataFlow<bool>.Data
        {
            set
            {
                LatchInput = value;

                if (value && eventRecieved)
                {
                    eventRecieved = false;
                    eventOutput?.Execute();
                }                
            }
        }

        // IEvent implementation --------------------------------------------------
        private bool eventRecieved = false;
        void IEvent.Execute()
        {
            if (LatchInput)
            {
                eventOutput?.Execute();
            }
            else
            {
                eventRecieved = true;
            }
        }
    }
}
