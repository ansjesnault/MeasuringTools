using MeasuringTools.Core;
using System;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Execute something with the new added measure in another dedicated thread.
    /// </summary>
    public class MeasureConsoleOutputListener : QueueListener<AbstractMeasures>
    {
        /// <summary>
        /// By default write the data into the console.
        /// </summary>
        /// <param name="val"></param>
        public override void Execute(AbstractMeasures val)
        {
            Console.WriteLine(val);
        }
    }
}
