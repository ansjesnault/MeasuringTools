using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MeasuringTools.Core
{
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
        /// <see cref="MaxLength"/>.
        /// </summary>
        private int _maxLength = -1;

        /// <summary>
        /// OPTIONAL : \n
        /// Maximum number of measures we can store into our collection list.\n
        /// The list will be rested when it raise this storage size number.\n
        /// By default, no reset occured here and -1 is returned (see <see cref="MinMaxSigmaMeanMeasuresCollection("/>).      
        /// </summary>
        /// <returns></returns>
        public int MaxLength
        {
            get { return _maxLength; }
        }

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
        public MinMaxSigmaMeanMeasuresCollection(string denomination = "") 
            : base(denomination)
        {
            _datas = new List<double>();
            Denomination = denomination;
        }

        /// <summary>
        /// Data copy constructor.
        /// </summary>
        /// <param name="dataCollection">The input measured list to use.</param>
        /// <param name="denomination">The short measure title description.</param>
        public MinMaxSigmaMeanMeasuresCollection(ICollection<double> dataCollection, string denomination = "")
            : base(denomination)
        {
            _datas = dataCollection;
            Denomination = denomination;
        }

        /// <summary>
        /// Constructor recommended to use.\n
        /// By default reset the list on maxLength by a simple clear.
        /// </summary>
        /// <param name="maxLength">The maximum number of measure to took before reseting the list (<see cref="CustomResetCheckAction"/>)</param>
        /// <param name="denomination">The short measure title description.</param>
        public MinMaxSigmaMeanMeasuresCollection(int maxLength, string denomination = "")
            : this(new List<double>(), maxLength, denomination)
        {
        }

        /// <summary>
        /// Custom constructor.\n
        /// By default reset the list on maxLength by a simple clear.
        /// </summary>
        /// <param name="dataCollection">The input measured list to use.</param>
        /// <param name="maxLength">The maximum number of measure to took before reseting the list (<see cref="CustomResetCheckAction"/>)</param>
        /// <param name="denomination">The short measure title description.</param>
        public MinMaxSigmaMeanMeasuresCollection(ICollection<double> dataCollection, int maxLength, string denomination = "")
            : base(denomination)
        {
            _datas = dataCollection;
            _maxLength = maxLength;
            Denomination = denomination;
            CustomResetCheckAction = (MinMaxSigmaMeanMeasures meas) =>
            {
                if (meas.IterationsCount >= _maxLength - 1)
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
            base.Reset();
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
        /// It will print the data collection after the default <see cref="MinMaxSigmaMeanMeasures.ToString()"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + Environment.NewLine + _datas.ToString();
        }

        /// <summary>
        /// Clear the whole data.
        /// </summary>
        public void Clear()
        {
            _datas.Clear();
        }

        /// <summary>
        /// Compute the Min value over the list.\n
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
        /// Compute the Max value over the list.\n
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
        /// Compute the average with our data measures list.\n
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
        public IEnumerable<double> TrimmedListCopy()
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

        /// <summary>
        /// Gives you a Trimmed List of the measured values discarding the noise.\n
        /// \warning : it will use <see cref="TrimmedListCopy"/>, thos, will replace all existing data by new computed ones.
        /// </summary>
        /// <param name="denomination"></param>
        /// <returns>A trimmed data of this <see cref="MinMaxSigmaMeanMeasuresCollection"/> instance.</returns>
        public MinMaxSigmaMeanMeasuresCollection TrimmedList()
        {
            _datas = (ICollection<double>)TrimmedList();
            return this;
        }
    }
}
