using MeasuringTools.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringTools.Output
{
    class MeasuresCSVOutputListener : QueueListener<AbstractMeasures>
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
