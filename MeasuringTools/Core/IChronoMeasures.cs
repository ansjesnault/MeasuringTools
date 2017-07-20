using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Interface for ChronoMeasures.\n
    /// \note : Note the out U_meas which is a covariant type allowing to give derived class type as generic U_meas param.\n
    /// \note : Note that the Measure generic type constraint is not necessary mandatory,
    /// it is an implementation choice just to help / guide the user about how to use this interface.
    /// </summary>
    public interface IChronoMeasures<out U_meas> where U_meas : AbstractMeasures
    {
        /// <summary>
        /// Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.  
        /// </summary>
        void Restart();

        /// <summary>
        /// Stops measuring an interval and register it to the measures list.
        /// </summary>
        /// <returns>The updated elapsed totalMilliseconds measures.</returns>
        U_meas Stop();

        /// <summary>
        /// Stop measuring
        /// </summary>
        void Interrupt();

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero and force the measures list reset.
        /// </summary>
        void Reset();

        /// <summary>
        /// Get whenever the Chronometer is running or not.
        /// (<see cref="Chronometer.Start"/> or <see cref="Chronometer.Stop"/> was called).
        /// </summary>
        /// <returns></returns>
        bool IsRunning { get; }

        /// <summary>
        /// Return the associated measure to this chrono.
        /// </summary>
        U_meas Measures { get; }
    }
}
