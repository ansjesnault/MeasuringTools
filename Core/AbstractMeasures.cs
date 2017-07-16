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
        /// Litle title description of the measure (generaly where we put the unit too).
        /// </summary>
        /// <returns></returns>
        public abstract string Denomination { get; set; }

        /// <summary>
        /// Add a measure.
        /// </summary>
        /// <param name="val"></param>
        public abstract void AddMeasure(double val);

        /// <summary>
        /// Reset the measures.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Get the current measure.
        /// </summary>
        /// <returns></returns>
        public abstract double Current { get; }

        /// <summary>
        /// Formated string measure representation.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
