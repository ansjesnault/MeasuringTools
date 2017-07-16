using System.Collections;
using System.Collections.Generic;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Represent a way to store many measures together in the same place.\n
    /// We store it in a list by using index accessor.\n
    /// \note : Note that the Measure generic type constraint is not necessary mandatory,
    /// it is an implementation choice just to help / guide the user about how to use this interface.
    /// </summary>
    public class MeasuresAggregator<U_meas> : IMeasuresAggregator<U_meas> where U_meas : AbstractMeasures
    {
        /// <summary>
        /// Our main measures container.
        /// </summary>
        protected List<U_meas> _measuresList = new List<U_meas>();

        /// <summary>
        /// Add a measure to our list.
        /// </summary>
        /// <param name="measure">The measure to add.</param>
        public void RegisterMeasure(U_meas measure)
        {
            _measuresList.Add(measure);
        }

        /// <summary>
        /// Clear the measures.
        /// </summary>
        public virtual void Clear()
        {
            _measuresList.Clear();
        }

        /// <summary>
        /// Return the number of registered measures.
        /// </summary>
        /// <returns></returns>
        public int Count
        {
            get { return _measuresList.Count; }
        }

        /// <summary>
        /// Indexer to get a registered Measures instance.
        /// <exception cref="ArgumentOutOfRangeException"></exception>.
        /// </summary> 
        public U_meas this[int id]
        {
            get
            {
                return _measuresList[id];
            }
        }

        /// <summary>
        /// Implement the IEnumerable interface
        /// </summary>
        /// <returns></returns>
        public IEnumerator<U_meas> GetEnumerator()
        {
            return ((IEnumerable<U_meas>)_measuresList).GetEnumerator();
        }

        /// <summary>
        /// Implement the IEnumerable interface
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<U_meas>)_measuresList).GetEnumerator();
        }
    }
}
