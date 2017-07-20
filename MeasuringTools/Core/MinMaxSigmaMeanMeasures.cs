using System;

namespace MeasuringTools.Core
{
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
        /// Get or set the Min value.\n
        ///\warning : It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Get or set the Max value.\n
        /// \warning : It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Get or set the Simple Moving Average value (moving arithmetic mean).\n
        /// \warning : It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double MovingMean
        {
            get { return _movingMean; }
            set { _movingMean = value; }
        }

        /// <summary>
        /// Get or set the Simple Moving sigma (standard deviation) value.\n
        /// Quantify the amount of variation or dispersion of a set of data values.\n
        /// A low standard deviation indicates that the data points tend to be close to the mean (also called the expected value) of the set,
        /// while a high standard deviation indicates that the data points are spread out over a wider range of values.\n
        /// \warning : It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public double MovingSigma
        {
            get { return _movingSigma; }
            set { _movingSigma = value; }
        }

        /// <summary>
        /// Get or set the number of iteration used to compute MovingMean and MovingSigma.\n
        /// \warning : It is not recommended to manually set this value except using in addition with <see cref="CustomResetCheckAction"/>.
        /// </summary>
        /// <returns></returns>
        public int IterationsCount
        {
            get { return (int)_count; }
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
        /// Get the optional Timer instance.\n
        /// Could be used in combinaison with <see cref="CustomResetCheckAction"/> to
        /// reset the measures infos after an Interval (ms) or trigger the Elapsed event.\n
        /// \warning : Do not forget to call <code>mymeasure.Timer.Enabled = true;</code> in order to start the timer.
        /// \todo : maybe should not be part of this class...
        /// </summary>
        public System.Timers.Timer Timer { get; } = new System.Timers.Timer();

        /// <summary>
        /// Optional action to check whenever we need to reset our instance and how to reset it.\n
        /// Could be used to reset after a certain number of iteration count for example.
        /// </summary>
        /// <returns></returns>
        public Action<MinMaxSigmaMeanMeasures> CustomResetCheckAction { get; set; }

        /// <summary>
        /// Optional Func to well format you measure to be printed.\n
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

            RaiseMeasureAdded();
        }

        /// <summary>
        /// Add a value as a new measured data and update MinMax and MovingMean and Sigma,
        /// only if predicate condition function is respected.
        /// </summary>
        /// <param name="val">The value to add.</param>
        /// <param name="predicate">
        /// Could be used to apply a low-pass filter :\n
        /// e.g: abs(thisValue - averageOfLast10Values) > someThreshold
        /// </param>
        public virtual void AddMeasure(double val, Func<bool> predicate)
        {
            if (predicate())
            {
                AddMeasure(val);
            }
        }

        /// <summary>
        /// Get formated string with respect of <see cref="CustomToStringFormater"> if any.\n
        /// Otherwise, default format will be :\n 
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
            _count = _movingMean = _movingSquare = _movingSigma = 0.0d;
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
        /// Update the moving average value (based on the iteration number count).\n
        /// Smooth average function without any shift in the domain (average not computed from a window period),
        /// using a single 'accumulator' based on the difference between the current measured val and the current average.
        /// </summary>
        /// <remarks>Iteration number count (incremented here) will be auto reset to 0 when reach the max value.</remarks>
        /// <param name="val">Our measure to take into account.</param>
        private void UpdateMovingMean(double val)
        {
            // handle _count reset event localy
            if (_count == double.MaxValue)
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
        /// \todo : Add ENUM option to switch average method used.
        /// \todo: handle period which could be iteration number count (by default) or time based.
        /// Update the moving average value.\n
        /// The more recent ticks have a stronger influence, and the effect of old ticks dissipates over time.\n
        /// It is based on the trusting % factor of the new point regarding to the old average.\n
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
}
