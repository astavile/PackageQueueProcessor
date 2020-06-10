using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageProcessors
{
    public sealed class PackageQueueProcessor<T> : IPackageQueueProcessor<T>
    {
        public PackageQueueProcessor(Action<T> processAction)
        {
            ProcessAction = processAction;
        }

        private ConcurrentQueue<T> messagesQueue;
        private ManualResetEventSlim signalEvent;
        private readonly object signalEventLocker = new object();

        public int Count
        {
            get { return messagesQueue != null ? messagesQueue.Count : 0; }
        }

        public bool IsActive { get; private set; }

        public Action<T> ProcessAction { get; private set; }

        public void Start()
        {
            if (IsActive)
                return;

            if (ProcessAction == null)
            {
                Log.Error("PackageQueueProcessor: ProcessAction is null.");
                return;
            }

            messagesQueue = new ConcurrentQueue<T>();
            signalEvent = new ManualResetEventSlim(false);
            IsActive = true;

            Task.Factory.StartNew(QueueProcessing, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            if (!IsActive)
                return;

            IsActive = false;
            signalEvent.Set();

            lock (signalEventLocker)
            {
                signalEvent.Dispose();
                signalEvent = null;
            }
        }

        public void Add(T dataItem)
        {
            if (dataItem == null || !IsActive)
                return;

            messagesQueue.Enqueue(dataItem);

            signalEvent.Set();
        }

        private void QueueProcessing()
        {
            while (IsActive)
            {
                lock (signalEventLocker)
                {
                    if (signalEvent != null)
                        signalEvent.Reset();
                    else
                        break;
                }

                T dataItem;
                while (messagesQueue.TryDequeue(out dataItem))
                {
                    // Проверка флага обязательна - т.к. могли остановить во время выполнения обработки сообщения, сигнальщик останется в бесконечном ожидании (ошибка).
                    if (!IsActive)
                        break;

                    try
                    {
                        ProcessAction(dataItem);
                    }
                    catch (Exception x)
                    {
                        Log.Exception(x);
                    }
                }

                lock (signalEventLocker)
                {
                    if (signalEvent != null && IsActive)
                        signalEvent.Wait(5000);
                }
            }
        }
    }
}
