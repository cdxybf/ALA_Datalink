﻿using System.Collections.Generic;

namespace ProgrammingParadigms
{
    /// <summary>
    /// A dataflow with a single scaler value with a primitive data type and a "OnChanged' event.
    /// OR think of it as an event with data, the receivers are able to read the data at any time.
    /// Or think of it as an implementation of a global variable and an observer pattern, with access to the variable and observer pattern restricted to the line connections on the diagram.
    /// Unidirectional - every line is one direction implying sender(s) and receiver(s).
    /// You can have multiple senders and receivers.
    /// The data is stored in the wire so receivers that don't act on the event can read its value at any time. Receivers cant change the data or send the event. 
    /// </summary>
    /// <typeparam name="T">Generic data type</typeparam>
    public interface IDataFlow<T>
    {
        T Data { set; }
    }

    public delegate void DataChangedDelegate();

    /// <summary>
    /// A reversed IDataFlow, the IDataFlow pushes data to the destination whereas IDataFlowB pulls data from source.
    /// However, the DataChanged event will notify the destination when change happens.
    /// </summary>
    /// <typeparam name="T">Generic data type</typeparam>
    public interface IDataFlowB<T>
    {
        T Data { get; }
        event DataChangedDelegate DataChanged;
    }

    /// <summary>
    /// It fans out data flows by creating a list and assign the data to the element in the list.
    /// Moreover, any IDataFlow and IDataFlowB can be transferred bidirectionally.
    /// </summary>
    /// <typeparam name="T">Generic data type</typeparam>
    public class DataFlowConnector<T> : IDataFlow<T>, IDataFlowB<T>
    {
        // properties
        public string InstanceName;

        // outputs
        private List<IDataFlow<T>> fanoutList = new List<IDataFlow<T>>();

        /// <summary>
        /// Fans out a data flow to mutiple data flows, or connect IDataFlow and IDataFlowB
        /// </summary>
        public DataFlowConnector() { }

        // IDataFlow<T> implementation ---------------------------------
        private T data;
        public T Data
        {
            set
            {
                foreach (var f in fanoutList) f.Data = value;
                data = value;
                DataChanged?.Invoke();
            }
            get => data;
        }

        // IDataFlowB<T> implementation ---------------------------------
        public event DataChangedDelegate DataChanged;
    }
}
