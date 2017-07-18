using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Simple interface to implement a <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueueListener<T>
    {
        /// <summary>
        /// Our standard Queue waiting to be processed.
        /// </summary>
        ConcurrentQueue<T> Queue { get; }
    }

    /// <summary>
    /// Represent a listener which run on its own thread
    /// and poll the <see cref="IQueueListener{T}.Queue"/> in order to
    /// manipulate/execute each dequeued value (a consumer).\n
    /// You have to implement <see cref="Execute(T)"/> in derived class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QueueListener<T> : IQueueListener<T>
    {
        /// <summary>
        /// Our standard Queue waiting to be processed.
        /// </summary>
        public ConcurrentQueue<T> Queue { get; protected set; } = new ConcurrentQueue<T>();

        /// <summary>
        /// Used to stop the thread.
        /// </summary>
        private volatile bool _stop = false;

        /// <summary>
        /// Stop the running thread.
        /// </summary>
        public void StopListening()
        {
            _stop = true;
        }

        /// <summary>
        /// Call this to start the thread waiting to dequeue ASAP
        /// there is a new value to <see cref="Execute(T)"/>.
        /// </summary>
        public void StartListening()
        {
            _stop = false;

            var t = new Thread(Listen)
            {
                IsBackground = true
            };
            t.Start();
        }

        /// <summary>
        /// <see cref="Execute(T)"/> a new available value in loop.
        /// </summary>
        private void Listen()
        {
            while (!_stop)
            {
                T val;
                if (Queue.TryDequeue(out val))
                {
                    Execute(val);
                }
            }
        }

        /// <summary>
        /// Have to be implemented in derived class to manipulate the data in this dedicated thread instance.
        /// </summary>
        /// <param name="val"></param>
        protected abstract void Execute(T val);
    }
}
