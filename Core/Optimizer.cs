using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;

namespace MeasuringTools.Core
{
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
