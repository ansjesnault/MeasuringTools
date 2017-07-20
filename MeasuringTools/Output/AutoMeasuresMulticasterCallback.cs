using MeasuringTools.Core;
using System;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Represent an easy way to auto register 
    /// an <see cref="AbstractMeasures"/> or a <see cref="MeasuresAggregator{AbstractMeasures}"/>
    /// to any changes in order to auto broadcast these changes to all subscribed <see cref="MeasuresExecutorsListener"/>.
    /// </summary>
    public class AutoMeasuresMulticasterCallback : AutoMeasuresCallback
    {
        /// <summary>
        /// Our multicaster which will broadcast any measure changes to all its listeners.
        /// </summary>
        private QueueMulticaster<AbstractMeasures> _measureMulticaster = new QueueMulticaster<AbstractMeasures>();

        /// <summary>
        /// Helper creator to directly register one listener associated to the measure.
        /// </summary>
        /// <param name="measure"></param>
        public static AutoMeasuresMulticasterCallback CreateAndRegister(
            AbstractMeasures measure, MeasuresExecutorsListener listerner)
        {
            var instance = new AutoMeasuresMulticasterCallback(measure);
            instance.Subscribe(listerner);
            return instance;
        }

        /// <summary>
        /// Helper creator to directly register one listener associated to the measure.
        /// </summary>
        /// <param name="measuresList"></param>
        public static AutoMeasuresMulticasterCallback CreateAndRegister(
            IMeasuresAggregator<AbstractMeasures> measuresList, MeasuresExecutorsListener listerner)
        {
            var instance = new AutoMeasuresMulticasterCallback(measuresList);
            instance.Subscribe(listerner);
            return instance;
        }

        /// <summary>
        /// Constructor with <see cref="AbstractMeasures"/>.
        /// </summary>
        /// <param name="measure"></param>
        public AutoMeasuresMulticasterCallback(AbstractMeasures measure)
            : base(measure)
        {
        }

        /// <summary>
        /// Constructor with <see cref="MeasuresAggregator<AbstractMeasures>"/>.
        /// </summary>
        /// <param name="measuresList"></param>
        public AutoMeasuresMulticasterCallback(IMeasuresAggregator<AbstractMeasures> measuresList)
            : base(measuresList)
        {
        }

        /// <summary>
        /// Add a listener to listen for new measure to execute in its own thread.
        /// </summary>
        /// <param name="listerner"></param>
        public void Subscribe(MeasuresExecutorsListener listerner)
        {
            _measureMulticaster.Subscribe(listerner);
        }

        /// <summary>
        /// Remove a listener.
        /// </summary>
        /// <param name="listerner"></param>
        public void Unsubscribe(MeasuresExecutorsListener listerner)
        {
            _measureMulticaster.Unsubscribe(listerner);
        }

        /// <summary>
        /// The event callback to broadcast the data to <see cref="QueueListener{T}"/> in their own thread.
        /// \warning : The registered <see cref="QueueListener{T}"/> 
        /// will execute/process the referenced data <see cref="AbstractMeasures"/> in another thread, be aware of that 'shared data'.
        /// \todo : Maybe we want to pass a concrete measure, or even a copy to avoid consurrent access/modification of the same measure instance...
        /// We also may want to pass a valueType like a struct in order to auto broadcast a measure snapshot...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMeasureAdded(object sender, EventArgs e)
        {
            _measureMulticaster.Broadcast((AbstractMeasures)sender);
        }
    }
}
