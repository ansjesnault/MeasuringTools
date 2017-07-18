using MeasuringTools.Core;
using System;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Represent a way to implement a measure callback in derived class
    /// by subscribing on the <see cref="AbstractMeasures.MeasureAdded"/> event.
    /// </summary>
    public abstract class AutoMeasuresCallback
    {
        /// <summary>
        /// Constructor with one <see cref="AbstractMeasures"/> measure.
        /// </summary>
        /// <param name="measure"></param>
        public AutoMeasuresCallback(AbstractMeasures measure)
        {
            measure.MeasureAdded += OnMeasureAdded;
        }

        /// <summary>
        /// Constructor with <see cref="MeasuresAggregator<AbstractMeasures>"/> list.
        /// </summary>
        /// <param name="measuresList"></param>
        public AutoMeasuresCallback(IMeasuresAggregator<AbstractMeasures> measuresList)
        {
            foreach (var measure in measuresList)
            {
                measure.MeasureAdded += OnMeasureAdded;
            }
        }

        /// <summary>
        /// Have to be implmented in derived class.
        /// </summary>
        /// <param name="sender">The measure itself</param>
        /// <param name="e"></param>
        protected abstract void OnMeasureAdded(object sender, EventArgs e);
    }
}
