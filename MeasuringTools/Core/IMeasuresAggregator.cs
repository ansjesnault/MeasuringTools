using System.Collections.Generic;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Interface needed to specify that the generic U_meas type is covariant.\n
    /// \note : Note that the Measure generic type constraint is not necessary mandatory,
    /// it is an implementation choice just to help / guide the user about how to use this interface.
    /// </summary>
    /// <typeparam name="T">Type used by our Measures.</typeparam>
    /// <typeparam name="U_meas">Our Measure type.</typeparam>
    public interface IMeasuresAggregator<out U_meas> : IEnumerable<U_meas> where U_meas : AbstractMeasures
    {
        /// <summary>
        /// Indexer to get a registered Measures instance.
        /// <exception cref="ArgumentOutOfRangeException"></exception>.
        /// </summary> 
        U_meas this[int id]
        {
            get;
        }

        /// <summary>
        /// Clear the measures.
        /// </summary>
        void Clear();

        /// <summary>
        /// Return the number of registered measures.
        /// </summary>
        /// <returns></returns>
        int Count
        {
            get;
        }
    }
}
