using System;
using System.Collections.Generic;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Represent a way to store (easier / efficient way to get it back) :\n
    /// * many measures (max 10 slots available)\n
    /// * many chronos (max 10 slots available)\n
    /// As a <see cref="List{T}"/> is less efficient than <see cref="Dictionary{TKey, TValue}"/> to lookup and get a value,
    /// we use a <see cref="MeasuresStore"/> and <see cref="ChronoStore"/> flags to avoid storing by index 
    /// (which may introduce conflicts in case of moving register code lines order).\n
    /// \note : Note that the Measure generic type constraint is not necessary mandatory,
    /// it is an implementation choice just to help / guide the user about how to use this interface.
    /// </summary>
    public class MeasuresAggregatorStores<U_meas> : MeasuresAggregator<U_meas> where U_meas : AbstractMeasures
    {
        /// <summary>
        /// Storage measures container.
        /// </summary>
        protected Dictionary<MeasuresStore, U_meas> _measuresStore
            = new Dictionary<MeasuresStore, U_meas>();

        /// <summary>
        /// Storage chronos container.
        /// </summary>
        protected Dictionary<ChronoStore, IChronoMeasures<U_meas>> _chronoStore
            = new Dictionary<ChronoStore, IChronoMeasures<U_meas>>();

        /// <summary>
        /// Register a measure.
        /// </summary>
        /// <param name="measure"></param>
        public void RegisterMeasure(MeasuresStore id, U_meas measure)
        {
            _measuresStore.Add(id, measure);
            _measuresList.Add(measure);
        }

        /// <summary>
        /// Add a chrono.
        /// </summary>
        /// <param name="id">Have to be shorter as possible for efficient lookup.</param>
        /// <param name="chrono">The chronometer measure.</param>
        /// <exception cref="ArgumentOutOfRangeException">Entry already exist.</exception>
        public void RegisterMeasure(ChronoStore id, IChronoMeasures<U_meas> chrono)
        {
            if (!_chronoStore.ContainsKey(id))
            {
                _chronoStore.Add(id, chrono);
                _measuresList.Add(chrono.Measures);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Chrono with the same name ({id}) already exist!");
            }
        }

        /// <summary>
        /// Clear the measures.
        /// </summary>
        public override void Clear()
        {
            _measuresStore.Clear();
            _chronoStore.Clear();
            base.Clear();
        }

        /// <summary>
        /// Indexer to get a registered Measures instance.
        /// <exception cref="ArgumentOutOfRangeException"></exception>.
        /// </summary> 
        public U_meas this[MeasuresStore id]
        {
            get
            {
                return _measuresStore[id];
            }
        }

        /// <summary>
        /// Indexer to get a registered ChronoMeasures instance.
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// </summary>
        public IChronoMeasures<U_meas> this[ChronoStore id]
        {
            get
            {
                return _chronoStore[id];
            }
        }

        /// <summary>
        /// Try to get a Measures instance from its id.\n
        /// NOTE: Use this method to avoid exception when you know you want to handle null ref in case 
        /// you explicitly not registered any Measures (due to an external activation bool flag).
        /// </summary>
        /// <code>measuresAggreg.TryGet(MeasuresStore.Slot1)?.AddMeasure(myVal)</code>
        /// <param name="id">The identifier of the registered Measures instance.</param>
        /// <returns>Measures instance if exist or null otherwise.</returns>
        public U_meas TryGet(MeasuresStore id)
        {
            return _measuresStore.ContainsKey(id) ? _measuresStore[id] : null;
        }

        /// <summary>
        /// Try to get a chronoMeasure instance from its id.\n
        /// NOTE: Use this method to avoid exception when you know you want to handle null ref in case 
        /// you explicitly not registered any Measures (due to an external activation bool flag).
        /// </summary>
        /// <code>measuresChronosAggreg.TryGet(ChronoStore.FirstChrono)?.Stop()</code>
        /// <param name="id">The identifier of the registered chrono instance.</param>
        /// <returns>chrono instance if exist or null otherwise.</returns>
        public IChronoMeasures<U_meas> TryGet(ChronoStore id)
        {
            return _chronoStore.ContainsKey(id) ? _chronoStore[id] : null;
        }
    }
}
