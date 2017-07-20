using System;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Measure base class.\n
    /// \note : To implement a derived class it is recommended 
    /// to have a default parameterless constructor in order
    /// to use classes such as <see cref="MeasuresAggregator"/>, <see cref="MeasuresChronosAggregator"/>
    /// and <see cref="ChronoMeasures"/>. 
    /// </summary>
    public abstract class AbstractMeasures : IMeasures<double>
    {
        /// <summary>
        /// Will be raised on <see cref="AddMeasure(double)"/>.
        /// </summary>
        public event EventHandler MeasureAdded;

        /// <summary>
        /// Litle title description of the measure (generaly where we put the unit too).
        /// </summary>
        /// <returns></returns>
        public abstract string Denomination { get; set; }

        /// <summary>
        /// Get the current measure.
        /// </summary>
        /// <returns></returns>
        public abstract double Current { get; }

        /// <summary>
        /// Add a measure.
        /// </summary>
        /// <param name="val"></param>
        public abstract void AddMeasure(double val);

        /// <summary>
        /// Raise the event (useful for child classes).
        /// </summary>
        protected void RaiseMeasureAdded()
        {
            MeasureAdded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Reset the measures.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Formated string measure representation.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
