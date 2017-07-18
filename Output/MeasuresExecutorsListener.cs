using MeasuringTools.Core;
using System;
using System.Collections.Generic;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Execute something with the new added measure in another dedicated thread.\n
    /// \note : Do not forget to call <see cref="QueueListener{T}.StartListening"/>.
    /// \note : Do not forget to register at least an <see cref="IExecutor{AbstractMeasures}"/>.
    /// </summary>
    public class MeasuresExecutorsListener : QueueListener<AbstractMeasures>
    {
        /// <summary>
        /// List of data executors which will be processed in this instance thread.
        /// </summary>
        private List<IExecutor<AbstractMeasures>> _executors = new List<IExecutor<AbstractMeasures>>();

        /// <summary>
        /// Helper static creator allowing to register only one <see cref="IExecutor{AbstractMeasures}"/>,
        /// and start automatically the thread.
        /// </summary>
        /// <param name="executor">The consumer callback</param>
        /// <returns></returns>
        public static MeasuresExecutorsListener CreateRegisterAndStart(IExecutor<AbstractMeasures> executor)
        {
            var instance = new MeasuresExecutorsListener();
            instance.RegisterExecutor(executor);
            instance.StartListening();
            return instance;
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
        /// Will be called in the dedicated listener thread (see<see cref="QueueListener{T}"/>).
        /// </summary>
        /// <param name="val"></param>
        protected override void Execute(AbstractMeasures val)
        {
            foreach (var controller in _executors)
            {
                controller.Execute(val);
            }
        }
    }
}
