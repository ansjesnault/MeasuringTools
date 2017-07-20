using MeasuringTools.Core;
using System;

namespace MeasuringTools.Output
{
    /// <summary>
    /// First default implementation of an <see cref="IExecutor{T}"/>
    /// for an <see cref="AbstractMeasures"/>.
    /// </summary>
    public class ConsoleOutputExecutor : IExecutor<AbstractMeasures>
    {
        /// <summary>
        /// Print the recieved measure in the console.
        /// </summary>
        /// <param name="val"></param>
        public void Execute(AbstractMeasures val)
        {
            Console.WriteLine(val);
        }
    }
}
