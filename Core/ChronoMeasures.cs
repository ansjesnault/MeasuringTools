using System;
using System.Diagnostics;

namespace MeasuringTools.Core
{
    /// <summary>
    /// Represent a timer to measure spent time of pieces of code using double precision with a <see cref="Stopwatch"/>.\n
    /// 
    /// \note : 
    /// * pros: It has the hightest possible resolution (about microseconds).
    /// * cons: This mechanism is a bit slower than the a diff between two <see cref="DateTime.UtcNow"/> (about 10 times slower),
    ///   so for short intervals this may have an impact on the result.\n
    /// * cons: This can be unreliable on a PC with multiple processors as Start() and Stop() must be executed
    ///   on the same processor to get a correct result (BIOS bug?).\n
    /// * cons: This is unreliable on processors that do not have a constant clock speed 
    ///   (most processors can reduce the clock speed to conserve energy).\n
    ///   You could get reliable results if you run it on a single-processor machine with power-saving disabled.\n
    ///   
    /// \warning : When doing timing or benchmarking of code, you should do a number of runs and take the average time
    /// (<see cref="Measure{T}(Action, int, ChronoMeasures{U_meas}.OptimizerOptions)"/>),
    /// because of other processes running under Windows, how much swapping to disk is occurring etc,
    /// the values between two runs may vary.
    /// 
    /// \note : Note that the Measure generic type constraint is not necessary mandatory,
    /// it is an implementation choice just to help / guide the user about how to use this interface.
    /// </summary>
    public class ChronoMeasures<U_meas> : IChronoMeasures<U_meas> where U_meas : AbstractMeasures
    {
        /// <summary>
        /// We decided to wrap stopwatch class here.
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Our associated measure.
        /// </summary>
        private readonly U_meas _measures;

        /// <summary>
        /// Get the registered measures for this chrono.
        /// </summary>
        public U_meas Measures { get { return _measures; } }

        /// <summary>
        /// Optional optimizer tool too avoid interferences measure.
        /// </summary>
        private readonly Optimizer _optim = new Optimizer();

        /// <summary>
        /// Optimizer Options which could be cumulated with '|' flags.
        /// </summary>
        [Flags]
        public enum OptimizerOptions : byte
        {
            /// <summary>
            /// No optimizations.
            /// </summary>
            None = 0x00,

            /// <summary>
            /// CleanGarbage.
            /// </summary>
            CleanGarbage = 0x01,

            /// <summary>
            /// Optimize GCLatency.
            /// </summary>
            GCLatency = 0x02,

            /// <summary>
            /// Optimize current thread.
            /// </summary>
            Thread = 0x04,

            /// <summary>
            /// Optimize current process,
            /// to be realTime.
            /// </summary>
            Process = 0x08,

            /// <summary>
            /// Optimize current process,
            /// to be realTime and executed 
            /// within only 1 processor.
            /// </summary>
            Process1 = 0x10,

            /// <summary>
            /// Optimize current process,
            /// to be realTime and executed 
            /// within only 2 processors.
            /// </summary>
            Process2 = 0x20,

            /// <summary>
            /// Optimize current process,
            /// to be realTime and executed 
            /// within only 4 processors.
            /// </summary>
            Process4 = 0x40,

            /// <summary>
            /// Optimize current process,
            /// to be realTime and executed 
            /// within only 8 processors.
            /// </summary>
            Process8 = 0x80,
        }

        /// <summary>
        /// Launch appropriate optimizations if needed.
        /// </summary>
        private static void HandleOptims(Optimizer optimHandle, OptimizerOptions optimOpt)
        {
            var checkTypeValues = Enum.GetValues(typeof(OptimizerOptions));
            foreach (OptimizerOptions value in checkTypeValues)
            {
                if ((optimOpt & value) == value)
                {
                    switch (value)
                    {
                        case OptimizerOptions.CleanGarbage: optimHandle.CleanGarbage(); break;
                        case OptimizerOptions.GCLatency: optimHandle.OptimizeGCLatency(); break;
                        case OptimizerOptions.Thread: optimHandle.OptimizedCurrentThread(); break;
                        case OptimizerOptions.Process: optimHandle.OptimizedCurrentProcess(); break;
                        case OptimizerOptions.Process1: optimHandle.OptimizedCurrentProcess(1); break;
                        case OptimizerOptions.Process2: optimHandle.OptimizedCurrentProcess(3); break;
                        case OptimizerOptions.Process4: optimHandle.OptimizedCurrentProcess(15); break;
                        case OptimizerOptions.Process8: optimHandle.OptimizedCurrentProcess(255); break;
                        case OptimizerOptions.None:
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Launch appropriate optimizations if needed.
        /// </summary>
        private static void RestoreAllOptims(Optimizer optimHandle)
        {
            optimHandle.RestoreGCLatency();
            optimHandle.RestoreCurrentThreadOptim();
            optimHandle.RestoreCurrentProcessOptim();
        }

        /// <summary>
        /// Get whenever the Chronometer is running or not.
        /// (<see cref="Chronometer.Start"/> or <see cref="Chronometer.Stop"/> was called).
        /// </summary>
        /// <returns></returns>
        public bool IsRunning
        {
            get { return _stopwatch.IsRunning; }
        }

        /// <summary>
        /// Constructor.
        /// <exception cref="NotSupportedException">
        /// Throws NotSupportedException if the hardware doesn't support high resolution counter. 
        /// </exception>
        /// </summary>
        public ChronoMeasures(U_meas measure)
        {
            if (!Stopwatch.IsHighResolution)
            {
                throw new NotSupportedException("Your hardware doesn't support high resolution counter.");
            }
            _measures = measure;
        }

        /// <summary>
        /// Launch appropriate optimizations if needed.
        /// </summary>
        private void HandleOptims(OptimizerOptions optimOpt)
        {
            HandleOptims(_optim, optimOpt);
        }

        /// <summary>
        /// Restore all optimizations set before if any.
        /// </summary>
        private void RestoreAllOptims()
        {
            RestoreAllOptims(_optim);
        }

        /// <summary>
        /// Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        public void Start()
        {
            _stopwatch.Start();
        }

        /// <summary>
        /// Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        public void Start(OptimizerOptions optimOpt = OptimizerOptions.None)
        {
            HandleOptims(optimOpt);
            _stopwatch.Start();
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.  
        /// </summary>
        public void Restart()
        {
            _stopwatch.Restart();
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.  
        /// </summary>
        public void Restart(OptimizerOptions optimOpt = OptimizerOptions.None)
        {
            HandleOptims(optimOpt);
            _stopwatch.Restart();
        }

        /// <summary>
        /// Stops measuring elapsed time (ms) for an interval and register it to the measures list.
        /// </summary>
        /// <returns>The updated elapsed totalMilliseconds measures.</returns>
        public U_meas Stop()
        {
            _stopwatch.Stop();
            _measures.AddMeasure(_stopwatch.Elapsed.TotalMilliseconds);
            RestoreAllOptims();
            return _measures;
        }

        /// <summary>
        /// Stop / cancel the chrono if is running and do not register the associated measure.
        /// </summary>
        public void Interrupt(bool retorOptims = false)
        {
            _stopwatch.Stop();
            if (retorOptims)
            {
                RestoreAllOptims();
            }
        }

        /// <summary>
        /// Stop / cancel the chrono if is running and do not register the associated measure.
        /// </summary>
        public void Interrupt()
        {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero and force the measures list reset.
        /// </summary>
        public void Reset(bool retorOptims = false)
        {
            _stopwatch.Reset();
            _measures.Reset();
            if (retorOptims)
            {
                RestoreAllOptims();
            }
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero and force the measures list reset.
        /// </summary>
        public void Reset()
        {
            _stopwatch.Reset();
            _measures.Reset();
        }

        /// <summary>
        /// Measure the timing of the action method execution according to the numberOfIterations. 
        /// NOTE: This is another way to measure a piece of code, creating and returning a completly new Measures instance.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="numberOfIterations"></param>
        public static T Measure<T>(Action action, int numberOfIterations = 1, OptimizerOptions optimOpt = OptimizerOptions.None) where T : AbstractMeasures, new()
        {
            var timings = new T();
            var optim = new Optimizer();
            HandleOptims(optim, optimOpt);
            for (var i = 0; i < numberOfIterations; i++)
            {
                Stopwatch chrono = Stopwatch.StartNew();
                action();
                chrono.Stop();
                timings.AddMeasure(chrono.Elapsed.TotalMilliseconds);
            }
            RestoreAllOptims(optim);
            return timings;
        }
    }
}
