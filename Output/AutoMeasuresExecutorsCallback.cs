using MeasuringTools.Core;
using System;
using System.Collections.Generic;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Represent a callback of all registered controller (<see cref="IExecutor{T}"/>) on the given measure.
    /// </summary>
    public class AutoMeasuresExecutorsCallback : AutoMeasuresCallback
    {
        /// <summary>
        /// List of data executors which will be processed in this instance thread.
        /// </summary>
        private List<IExecutor<AbstractMeasures>> _executors = new List<IExecutor<AbstractMeasures>>();

        /// <summary>
        /// Constructor with one <see cref="AbstractMeasures"/> measure.
        /// </summary>
        /// <param name="measure"></param>
        public AutoMeasuresExecutorsCallback(AbstractMeasures measure)
            : base(measure)
        {
            measure.MeasureAdded += OnMeasureAdded;
        }

        /// <summary>
        /// Constructor with <see cref="MeasuresAggregator<AbstractMeasures>"/> list.
        /// </summary>
        /// <param name="measuresList"></param>
        public AutoMeasuresExecutorsCallback(IMeasuresAggregator<AbstractMeasures> measuresList)
            : base(measuresList)
        {
            foreach (var measure in measuresList)
            {
                measure.MeasureAdded += OnMeasureAdded;
            }
        }

        /// <summary>
        /// Add a controller to manipulate the data in this dedicated instance thread.
        /// </summary>
        /// <param name="executor"></param>
        public void RegisterExecutor(IExecutor<AbstractMeasures> executor)
        {
            _executors.Add(executor);
        }

        /// <summary>
        /// Remove a controller from data manipulate.
        /// </summary>
        /// <param name="executor"></param>
        public void UnRegisterExecutor(IExecutor<AbstractMeasures> executor)
        {
            _executors.Remove(executor);
        }

        /// <summary>
        /// Call of all registered <see cref="IExecutor{T}"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMeasureAdded(object sender, EventArgs e)
        {
            foreach (var controller in _executors)
            {
                controller.Execute((AbstractMeasures)sender);
            }
        }
    }
}
