using MeasuringTools.Core;
using System;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Represent an easy way to auto register 
    /// an <see cref="AbstractMeasures"/> or a <see cref="MeasuresAggregator{U_meas}"/>
    /// to any changes in order to auto broadcast these changes to all subscribed <see cref="QueueListener{T}"/>.
    /// </summary>
    public class MeasuresMulticaster : QueueMulticaster<AbstractMeasures>
    {
        /// <summary>
        /// Constructor with <see cref="AbstractMeasures"/>.
        /// </summary>
        /// <param name="measure"></param>
        public MeasuresMulticaster(AbstractMeasures measure)
        {
            measure.MeasureAdded += OnMeasureAdded;
        }

        /// <summary>
        /// Constructor with <see cref="MeasuresAggregator<AbstractMeasures>"/>.
        /// </summary>
        /// <param name="measuresList"></param>
        public MeasuresMulticaster(MeasuresAggregator<AbstractMeasures> measuresList)
        {
            foreach (var measure in measuresList)
            {
                measure.MeasureAdded += OnMeasureAdded;
            }
        }

        /// <summary>
        /// The event callback to broadcast the data to <see cref="QueueListener{T}"/> in their own thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMeasureAdded(object sender, EventArgs e)
        {
            this.Broadcast((AbstractMeasures)sender);
        }
    }
}
