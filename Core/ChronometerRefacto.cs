using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;

namespace Measures
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

    /// <summary>
    /// Represent an efficient way to get Min, Max, moving average and standard deviation over a series of measures.\n
    /// It uses Welford's method to perform the computations incrementally on the fly.\n
    /// It therefore doesn't store the individual values and is reasonably fast.\n
    /// </summary>
    public class MinMaxSigmaMeanMeasures : AbstractMeasures
    {
        /// <summary>
        /// Minimum of our measures values.
        /// </summary>
        private double _min = 0.0d;

        /// <summary>
        /// Maximum of our measures values.
        /// </summary>
        private double _max = 0.0d;

        /// <summary>
        /// Current moving average of our measures values.
        /// </summary>
        private double _movingMean = 0.0d;

        /// <summary>
        /// Number of iteration (measures added) used to compute moving average.
        /// </summary>
        private double _count = 0.0d;

        /// <summary>
        /// Current moving square of our measures values needed to compute sigma (standard deviation).
        /// </summary>
        private double _movingSquare = 0.0d;

        /// <summary>
        /// Current moving sigma (standard deviation) of our measures values.
        /// </summary>
        private double _movingSigma = 0.0d;

        /// <summary>
        /// Current measure value.
        /// </summary>
        private double _current = 0.0d;

        /// <summary>
        /// Get or set the Min value.
        /// NOTE: It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Get or set the Max value.
        /// NOTE: It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Get or set the Simple Moving Average value (moving arithmetic mean).
        /// NOTE: It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double MovingMean
        {
            get { return _movingMean; }
            set { _movingMean = value; }
        }

        /// <summary>
        /// Get or set the Simple Moving sigma (standard deviation) value.
        /// Quantify the amount of variation or dispersion of a set of data values.
        /// A low standard deviation indicates that the data points tend to be close to the mean (also called the expected value) of the set, 
        /// while a high standard deviation indicates that the data points are spread out over a wider range of values.
        /// NOTE: It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double MovingSigma
        {
            get { return _movingSigma; }
            set { _movingSigma = value; }
        }

        /// <summary>
        /// Get or set the number of iteration used to compute MovingMean and MovingSigma.
        /// NOTE: It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public int IterationsCount
        {
            get { return (int)_count;}
            set { _count = value; }
        } 

        /// <summary>
        /// Get the last registered value.
        /// </summary>
        /// <returns></returns>
        public override double Current
        {
            get { return _current; }
        }

        /// <summary>
        /// A short title name to describe this measure.
        /// </summary>
        public override string Denomination { get; set; } = string.Empty;

        /// <summary>
        /// Get the optional Timer instance.
        /// Could be used in combinaison with <see cref="CustomResetCheckAction"/> to
        /// reset the measures infos after an Interval (ms) or trigger the Elapsed event.
        /// NOTE: Do not forget to call <code>mymeasure.Timer.Enabled = true;</code> in order to start the timer.
        /// </summary>
        public System.Timers.Timer Timer { get; } = new System.Timers.Timer();

        /// <summary>
        /// Optional action to check whenever we need to reset our instance and how to reset it.
        /// Could be used to reset after a certain number of iteration count for example.
        /// </summary>
        /// <returns></returns>
        public Action<MinMaxSigmaMeanMeasures> CustomResetCheckAction { get; set; }

        /// <summary>
        /// Optional Func to well format you measure to be printed.
        /// Could be used to personalize the ToString() implementation.
        /// </summary>
        /// <returns></returns>
        public Func<MinMaxSigmaMeanMeasures, string> CustomToStringFormater { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="denomination">The short measure title description.</param>
        public MinMaxSigmaMeanMeasures()
        {
            GC.KeepAlive(Timer);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="denomination">The short measure title description.</param>
        public MinMaxSigmaMeanMeasures(string denomination = "")
        {
            Denomination = denomination;
            GC.KeepAlive(Timer);
        }

        /// <summary>
        /// Add a value as a new measured data and update MinMax and MovingMean and Sigma,
        /// after having checked whenever to reset the list (<see cref="CustomResetCheckAction"/>).
        /// </summary>
        /// <param name="val">The value to add.</param>
        public override void AddMeasure(double val)
        {
            CustomResetCheckAction?.Invoke(this);
            _current = val;
            MinMax(val);
            UpdateMovingMean(val);
            UpdateMovingSigma(val);
        }

        /// <summary>
        /// Add a value as a new measured data and update MinMax and MovingMean and Sigma,
        /// only if predicate condition function is respected.
        /// </summary>
        /// <param name="val">The value to add.</param>
        /// <param name="predicate">
        /// Could be used to apply a low-pass filter :
        /// (e.g: abs(thisValue - averageOfLast10Values) > someThreshold )
        /// </param>
        public virtual void AddMeasure(double val, Func<bool> predicate)
        {
            if (predicate())
            {
                AddMeasure(val);
            }
        }

        /// <summary>
        /// Get formated string with respect of <see cref="CustomToStringFormater"> if any. 
        /// Otherwise, default format will be : 
        /// min('minVal') max('maxVal') mean('meanVal') +/-('sigmaVal') curr('currentVal'),
        /// with val displaying 2 last rounded decimal.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return CustomToStringFormater != null ? 
                CustomToStringFormater.Invoke(this) : 
                String.Format("{0, -20} {1, -15} {2, -15} {3, -15} {4, -15} {5, -15}",
                    Denomination,
                    $"min({_min.ToString("00.00")})",
                    $"max({_max.ToString("00.00")})",
                    $"mean({_movingMean.ToString("00.00")})",
                    $"+/-({_movingSigma.ToString("00.00")})",
                    $"curr({_current.ToString("00.00")})"
                );
        }

        /// <summary>
        /// Clear the whole measures datas.
        /// </summary>
        public override void Reset()
        {
            _current = 0.0d;
            _min = _max = 0.0d;
            _count =_movingMean = _movingSquare = _movingSigma = 0.0d;
        }

        /// <summary>
        /// Update the min and max values.
        /// </summary>
        /// <param name="val">Our measure to take into account.</param>
        private void MinMax(double val)
        {
            _max = Math.Max(_max, val);
            _min = Math.Min(_min, val);
        }

        /// <summary>
        /// Update the moving average value (based on the iteration number count).
        /// Smooth average function without any shift in the domain (average not computed from a window period),
        /// using a single 'accumulator' based on the difference between the current measured val and the current average.
        /// </summary>
        /// <remarks>Iteration number count (incremented here) will be auto reset to 0 when reach the max value.</remarks>
        /// <param name="val">Our measure to take into account.</param>
        private void UpdateMovingMean(double val)
        {
            // handle _count reset event localy
            if(_count == double.MaxValue)
            {
                _count = 0.0d;
            }
            
            var deltaMean = val - _movingMean;
            _movingMean = _movingMean + deltaMean / ++_count;

            var deltaSquare = val - _movingSquare;
            _movingSquare = _movingSquare + deltaSquare * deltaMean;
        }

        /// <summary>
        /// Update the moving sigma (standard deviation) 
        /// based on iteration number count and moving square provided by <see cref="UpdateMovingMean">. 
        /// </summary>
        /// <param name="val"></param>
        private void UpdateMovingSigma(double val)
        {
            _movingSigma = Math.Sqrt(_movingSquare / _count);
        }

        /// <summary>
        /// TODO: Add ENUM option to switch average method used.
        /// TODO: handle period which could be iteration number count (by default) or time based.
        /// Update the moving average value.
        /// The more recent ticks have a stronger influence, and the effect of old ticks dissipates over time.
        /// It is based on the trusting % factor of the new point regarding to the old average.
        /// New average is computed according to the weight we put on the new point and on the previous average.
        /// </summary>
        /// <param name="val">Our measure to take into account.</param>
        /// <param name="period">The window of data measures to compute the trusting / smoothingFactor.</param>
        private void MovingWeightedWindowedAverage(double val, double period)
        {
            var smoothingFactor = 1.0f - (1.0f / period);
            _movingMean = val * smoothingFactor + _movingMean * (1.0f - smoothingFactor);
        }
    }


    /// <summary>
    /// Represent a way to store measures data into a list and manipulate them.\n
    /// \note : Note that it provide features of MinMaxSigmaMeanMeasures class for efficiency but
    /// store datas on each iteration, providing additional manipulation tools (with the associated performance cost).
    /// </summary>
    public class MinMaxSigmaMeanMeasuresCollection : MinMaxSigmaMeanMeasures, IEnumerable
    {
        /// <summary>
        /// Our internal collection measured data list.
        /// </summary>
        private ICollection<double> _datas;

        /// <summary>
        /// OPTIONAL : \n
        /// Maximum number of measures we can store into our collection list.\n
        /// The list will be rested when it raise this storage size number.\n
        /// By default, no reset occured here.
        /// </summary>
        private int _maxLength = 0;

        /// <summary>
        /// Return the current number of registered measures.
        /// </summary>
        /// <returns></returns>
        public int Count
        {
            get { return _datas.Count; }
        }

        /// <summary>
        /// Default constructor.
        /// By default use a List.
        /// </summary>
        public MinMaxSigmaMeanMeasuresCollection()
        {
            _datas = new List<double>();
        }

        /// <summary>
        /// Constructor recommended to use.
        /// By default reset the list on maxLength by a simple clear.
        /// </summary>
        /// <param name="maxLength">The maximum number of measure to took before reseting the list (<see cref="CustomResetCheckAction"/>)</param>
        /// <param name="denomination">The short measure title description.</param>
        public MinMaxSigmaMeanMeasuresCollection(int maxLength, string denomination = "")
            : this(new List<double>(), maxLength, denomination)
        {
        }

        /// <summary>
        /// Custom constructor.
        /// By default reset the list on maxLength by a simple clear.
        /// </summary>
        /// <param name="dataCollection">The input measured list to use.</param>
        /// <param name="maxLength">The maximum number of measure to took before reseting the list (<see cref="CustomResetCheckAction"/>)</param>
        /// <param name="denomination">The short measure title description.</param>
        public MinMaxSigmaMeanMeasuresCollection(ICollection<double> dataCollection, int maxLength, string denomination = "")
        {
            _datas = dataCollection;
            _maxLength = maxLength;
            Denomination = denomination;
            CustomResetCheckAction = (MinMaxSigmaMeanMeasures meas) =>
            {
                if (meas.IterationsCount >= maxLength - 1)
                {
                    _datas.Clear();
                }
            };
        }

        /// <summary>
        /// Implement the IEnumerable interface
        /// </summary>
        /// <returns></returns>
        public IEnumerator<double> GetEnumerator()
        {
            return ((IEnumerable<double>)_datas).GetEnumerator();
        }

        /// <summary>
        /// Implement the IEnumerable interface
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<double>)_datas).GetEnumerator();
        }

        /// <summary>
        /// Add a value as a new measured data to our measures list 
        /// after having checked whenever to reset the list (<see cref="CustomResetCheckAction"/>).
        /// </summary>
        /// <param name="val">The value to add.</param>
        public override void AddMeasure(double val)
        {
            base.AddMeasure(val);
            _datas.Add(val);
        }

        /// <summary>
        /// Reset action on the measure list if any <see cref="CustomResetCheckAction"/> were registered,
        /// or clear the whole data.
        /// </summary>
        public override void Reset()
        {
            if (CustomResetCheckAction != null)
            {
                CustomResetCheckAction(this);
            }
            else
            {
                _datas.Clear();
            }
        }

        /// <summary>
        /// Clear the whole data.
        /// </summary>
        public void Clear()
        {
            _datas.Clear();
        }

        /// <summary>
        /// Compute the Min value over the list.
        /// May return double.NaN value.
        /// </summary>
        /// <returns></returns>
        public double ComputeMin()
        {
            double min = double.NaN;
            if (_datas.Any())
            {
                min = _datas.Min();
            }
            return min;
        }

        /// <summary>
        /// Compute the Max value over the list.
        /// May return double.NaN value.
        /// </summary>
        /// <returns></returns>
        public double ComputeMax()
        {
            double max = double.NaN;
            if (_datas.Any())
            {
                max = _datas.Max();
            }
            return max;
        }

        /// <summary>
        /// Compute the average with our data measures list.
        /// May return double.NaN value.
        /// </summary>
        /// <returns></returns>
        public double ComputeAverage()
        {
            double average = double.NaN;
            if (_datas.Any())
            {
                average = _datas.Average();
            }
            return average;
        }

        /// <summary>
        /// Compute a new list of averages using neighbors averages values. 
        /// </summary>
        /// <param name="period">Number of neighbors to use.</param>
        /// <returns></returns>
        public IEnumerable<double> ComputeMovingMean(int period)
        {
            return Enumerable
                    .Range(0, _datas.Count - period)
                    .Select(n => _datas.Skip(n).Take(period).Average());
        }

        /// <summary>
        /// Gives you a Trimmed List of the measured values discarding the noise.\n
        /// The deviation of each value from the actual average is calculated 
        /// and then the values which are farer from the mean of deviation (called absolute deviation) are discarded.\n 
        /// For example if values are { 1, 2, 3, 2, 100 }, it discards 100, and returns { 1, 2, 3, 2 }.\n
        /// Then you could use Min, Max, or even Average on this newly filtered list. 
        /// </summary>
        /// <exception cref="ArgumentException">If measured list is empty.</exception>
        /// <returns>TrimmedAverage</returns>
        public IEnumerable<double> TrimmedList()
        {
            if (_datas.Any())
            {
                var deviations = _datas.Select(value => Tuple.Create(value, _datas.Average() - value)).ToList();
                var meanDeviation = deviations.Sum(deviation => System.Math.Abs(deviation.Item2)) / _datas.Count;
                var response = deviations.Where(tuple => tuple.Item2 > 0 || System.Math.Abs(tuple.Item2) <= meanDeviation);
                return response.Select(deviation => deviation.Item1);
            }
            else
            {
                throw new ArgumentException("Measured list is empty!");
            }
        }
    }






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


    /// <summary>
    /// Max Ten Measures available per instance.
    /// See <see cref="MeasuresAggregatorStores{U_meas}"/>.
    /// </summary>
    public enum MeasuresStore : byte
    {
        /// <summary>
        /// 1st Measures flag id.
        /// </summary>
        MeasuresSlot0 = 0x00,

        /// <summary>
        /// 2nd Measures flag id.
        /// </summary>
        MeasuresSlot1 = 0x01,

        /// <summary>
        /// 3rd Measures flag id.
        /// </summary>
        MeasuresSlot2 = 0x02,

        /// <summary>
        /// 4th Measures flag id.
        /// </summary>
        MeasuresSlot3 = 0x04,

        /// <summary>
        /// 5th Measures flag id.
        /// </summary>
        MeasuresSlot4 = 0x08,

        /// <summary>
        /// 6th Measures flag id.
        /// </summary>
        MeasuresSlot5 = 0x10,

        /// <summary>
        /// 7th Measures flag id.
        /// </summary>
        MeasuresSlot6 = 0x20,

        /// <summary>
        /// 8th Measures flag id.
        /// </summary>
        MeasuresSlot7 = 0x40,

        /// <summary>
        /// 9th Measures flag id.
        /// </summary>
        MeasuresSlot8 = 0x80,

        /// <summary>
        /// 10th Measures flag id.
        /// </summary>
        MeasuresSlot9 = 0x0A,
    }

    /// <summary>
    /// Max Ten chronos available per instance.\n
    /// See <see cref="MeasuresAggregatorStores{U_meas}"/>.
    /// </summary>
    public enum ChronoStore : byte
    {
        /// <summary>
        /// 1st chrono flag id.
        /// </summary>
        ChronoSlot0 = 0x00,

        /// <summary>
        /// 2nd chrono flag id.
        /// </summary>
        ChronoSlot1 = 0x01,

        /// <summary>
        /// 3rd chrono flag id.
        /// </summary>
        ChronoSlot2 = 0x02,

        /// <summary>
        /// 4th chrono flag id.
        /// </summary>
        ChronoSlot3 = 0x04,

        /// <summary>
        /// 5th chrono flag id.
        /// </summary>
        ChronoSlot4 = 0x08,

        /// <summary>
        /// 6th chrono flag id.
        /// </summary>
        ChronoSlot5 = 0x10,

        /// <summary>
        /// 7th chrono flag id.
        /// </summary>
        ChronoSlot6 = 0x20,

        /// <summary>
        /// 8th chrono flag id.
        /// </summary>
        ChronoSlot7 = 0x40,

        /// <summary>
        /// 9th chrono flag id.
        /// </summary>
        ChronoSlot8 = 0x80,

        /// <summary>
        /// 10th chrono flag id.
        /// </summary>
        ChronoSlot9 = 0x0A,
    }

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


    class Test
    {
        Test()
        {
            // power of generic !
            MeasuresAggregatorStores<MinMaxSigmaMeanMeasures> mca = new MeasuresAggregatorStores<MinMaxSigmaMeanMeasures>();
            MinMaxSigmaMeanMeasures measure = new MinMaxSigmaMeanMeasures("test0");
            var average = measure.MovingMean;
            var chrono = new ChronoMeasures<MinMaxSigmaMeanMeasures>(measure);
            var sameaverage = chrono.Measures.MovingMean;
            mca.RegisterMeasure(ChronoStore.ChronoSlot1, chrono);
            foreach (var measures in mca)
            {
                var sameAgainAverage = measures.MovingMean;
            }

            // simple one
            IMeasuresAggregator<AbstractMeasures> mcas = new MeasuresAggregatorStores<MinMaxSigmaMeanMeasures>();
            var cast = mcas as MeasuresAggregatorStores<MinMaxSigmaMeanMeasures>;
            cast.RegisterMeasure(ChronoStore.ChronoSlot2, new ChronoMeasures<MinMaxSigmaMeanMeasures>(new MinMaxSigmaMeanMeasures("test1")));
            cast.RegisterMeasure(new MinMaxSigmaMeanMeasuresCollection(1000));
            cast.RegisterMeasure(MeasuresStore.MeasuresSlot1, new MinMaxSigmaMeanMeasures("anotherOne"));
            foreach (var measures in mcas)
            {
                var measureStringRepresentation = measures.ToString();
                var sameAgainAverage = ((MinMaxSigmaMeanMeasures)(measures))?.MovingMean;
            }
        }
    }


    /// <summary>
    /// Represent a utility class to optimize the application temporarily.
    /// </summary>
    public class Optimizer
    {
        private GCLatencyMode _oldGarbageCollectorLatencyMode;
        private ThreadPriority _oldCurrentThreadPriority;
        private ProcessPriorityClass _oldCurrentProcessPriorityClass;
        private IntPtr _oldCurrentProcessProcessorAffinity;

        /// <summary>
        /// Get whenever GCLatency is Optimized or not.
        /// </summary>
        public bool IsGCLatencyOptimized { get; private set; }

        /// <summary>
        /// Get whenever CurrentThread is Optimized or not.
        /// </summary>
        public bool IsCurrentThreadOptimized { get; private set; }

        /// <summary>
        /// Get whenever CurrentProcess is Optimized or not.
        /// </summary>
        public bool IsCurrentProcessOptimized { get; private set; }

        /// <summary>
        /// Frees resources and performs other cleanup operations before it is reclaimed by garbage collection. 
        /// </summary>
        ~Optimizer()
        {
            // If the optimizations were never reverted back
            RestoreGCLatency();
            RestoreCurrentThreadOptim();
            RestoreCurrentProcessOptim();
        }

        /// <summary>
        /// Creates a time-frame during which the performance of the current process is optimized.
        /// </summary>
        /// <remarks>
        /// Why use SustainedLowLatency instead of LowLatency: http://www.infoq.com/news/2012/03/Net-403.
        /// </remarks>
        public void OptimizeGCLatency()
        {
            IsGCLatencyOptimized = true;
            _oldGarbageCollectorLatencyMode = GCSettings.LatencyMode;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        /// <summary>
        /// Reverts back all the optimizations done by <see cref="OptimizeGCLatency"/>.
        /// </summary>
        public void RestoreGCLatency()
        {
            if (IsGCLatencyOptimized)
            {
                IsGCLatencyOptimized = false;
                GCSettings.LatencyMode = _oldGarbageCollectorLatencyMode;
            }
        }

        /// <summary>
        /// Prevent "Normal" threads from interrupting this thread (set priority as Highest).\n
        /// Use the second Core/Processor for the ideal measure processor thread affinity.
        /// </summary>
        public void OptimizedCurrentThread()
        {
            IsCurrentThreadOptimized = true;

            _oldCurrentThreadPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            foreach (ProcessThread t in Process.GetCurrentProcess().Threads)
            {
                if (t.Id == Thread.CurrentThread.ManagedThreadId)
                {
                    t.IdealProcessor = 1;
                }
            }
        }

        /// <summary>
        /// Reverts back all the optimizations done by <see cref="OptimizedCurrentThread"/>.
        /// </summary>
        public void RestoreCurrentThreadOptim()
        {
            if (IsCurrentThreadOptimized)
            {
                IsCurrentThreadOptimized = false;

                Thread.CurrentThread.Priority = _oldCurrentThreadPriority;

                foreach (ProcessThread t in Process.GetCurrentProcess().Threads)
                {
                    if (t.Id == Thread.CurrentThread.ManagedThreadId)
                    {
                        t.ResetIdealProcessor();
                    }
                }
            }
        }

        /// <summary>
        /// Prevent "Normal" processes from interrupting threads as this one have to be real-time.
        /// </summary>
        /// <param name="mask">
        /// Change eligibles processors on which some threads of the process could be executed.\n
        /// See <see cref="Process.ProcessorAffinity"/>.\n
        /// Examples (heler website here : <see cref="http://www.binaryhexconverter.com/binary-to-decimal-converter"/>):\n
        /// * 3  (0x0003 ; 00000000 00000011 ; only 2 processors 1 and 2 will be used)\n
        /// * 9  (0x0009 ; 00000000 00001001 ; only 2 processors 1 AND 4 will be used)\n
        /// * 15 (0x000F ; 00000000 00001111 ; only 4 processors 1 TO 4 will be used)\n
        /// By default negative (or 0) value indicate that all available processors will be used.
        /// </param>
        public void OptimizedCurrentProcess(int mask = -1)
        {
            IsCurrentProcessOptimized = true;

            _oldCurrentProcessProcessorAffinity = Process.GetCurrentProcess().ProcessorAffinity;
            Process.GetCurrentProcess().ProcessorAffinity = mask > 0 ? new IntPtr(mask) : _oldCurrentProcessProcessorAffinity;

            _oldCurrentProcessPriorityClass = Process.GetCurrentProcess().PriorityClass;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        }

        /// <summary>
        /// Reverts back all the optimizations done by <see cref="OptimizedCurrentProcess"/>.
        /// </summary>
        public void RestoreCurrentProcessOptim()
        {
            if (IsCurrentProcessOptimized)
            {
                IsCurrentProcessOptimized = false;
                Process.GetCurrentProcess().ProcessorAffinity = _oldCurrentProcessProcessorAffinity;
                Process.GetCurrentProcess().PriorityClass = _oldCurrentProcessPriorityClass;
            }
        }

        /// <summary>
        /// Wait for the finalizer queue to be empty.
        /// </summary>
        public void CleanGarbage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}