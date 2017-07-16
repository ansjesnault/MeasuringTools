using System.Collections.Generic;

namespace MeasuringTools.Output
{
    /// <summary>
    /// Simple interface to implement a way to send a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBroadcast<T>
    {
        /// <summary>
        /// Send the value.
        /// </summary>
        /// <param name="value"></param>
        void Broadcast(T value);
    }


    /// <summary>
    /// Represent a way to multicast a <see cref="T"/> value to all registered <see cref="IQueueListener{T}"/>.\n
    /// See <see cref="AbstractQueueListener{T}"/> to see how the data value is consumed.
    /// </summary>
    /// <typeparam name="T">Any data value you want to <see cref="Broadcast(T)"/></typeparam>
    public class QueueMulticaster<T> : IBroadcast<T>
    {
        /// <summary>
        /// A list of all registered <see cref="IQueueListener{T}"/>.
        /// </summary>
        private readonly List<IQueueListener<T>> _subscribers = new List<IQueueListener<T>>();

        /// <summary>
        /// Call it in order to be able to broadcast any value to <see cref="IQueueListener{T}"/>.
        /// </summary>
        /// <param name="subscriber">The instance you want to register.</param>
        public void Subscribe(IQueueListener<T> subscriber)
        {
            _subscribers.Add(subscriber);
        }

        /// <summary>
        /// Call it only if you want a <see cref="IQueueListener{T}"/> to be unregistered.
        /// </summary>
        /// <param name="subscribed">the previously registered instance.</param>
        public void Unsubscribe(IQueueListener<T> subscribed)
        {
            _subscribers.Remove(subscribed);
        }

        /// <summary>
        /// Send the data value in the Queue of all registered <see cref="IQueueListener{T}"/>.
        /// </summary>
        /// <param name="value"></param>
        public void Broadcast(T value)
        {
            foreach (var queueListener in _subscribers)
            {
                queueListener.Queue.Enqueue(value);
            }
        }
    }
}
