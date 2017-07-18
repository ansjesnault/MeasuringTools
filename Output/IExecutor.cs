using MeasuringTools.Core;
using System;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Simple interface to handle an execution with a given data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecutor<T>
    {
        /// <summary>
        /// Execute something with data provided by the listener.
        /// </summary>
        /// <param name="val"></param>
        void Execute(T val);
    }
}
