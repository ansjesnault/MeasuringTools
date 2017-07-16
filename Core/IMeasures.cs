namespace MeasuringTools.Core
{
    /// <summary>
    /// Measure interface.
    /// </summary>
    public interface IMeasures<T>
    {
        /// <summary>
        /// Add a measure.
        /// </summary>
        /// <param name="val"></param>
        void AddMeasure(T val);

        /// <summary>
        /// Reset the measures.
        /// </summary>
        void Reset();

        /// <summary>
        /// Get the current measure.
        /// </summary>
        /// <returns></returns>
        T Current { get; }
    }
}
