using System;
using System.Threading;

namespace ClassBackgroundWorker
{
    public class ClassBW
    {
        private readonly object lockObject = new object();
        private Thread workerThread;
        private bool isRunning = false;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        public event EventHandler<ErrorEventArgs> Error;

        public void RunWorkerAsync(Action<object> userFunction, object argument)
        {
            if (isRunning)
                throw new InvalidOperationException("Worker is already running.");

            isRunning = true;
            workerThread = new Thread(() => WorkerThread(userFunction, argument));
            workerThread.Start();
        }

        private void WorkerThread(Action<object> userFunction, object argument)
        {
            try
            {
                userFunction?.Invoke(argument);
            }
            catch (Exception ex)
            {
                OnError(new ErrorEventArgs(ex));
            }
            finally
            {
                isRunning = false;
            }
        }

        public void ReportProgress(int progressPercentage)
        {
            OnProgressChanged(new ProgressChangedEventArgs(progressPercentage));
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            EventHandler<ProgressChangedEventArgs> handler = ProgressChanged;
            if (handler != null)
            {
                lock (lockObject)
                {
                    // Kolejkowanie zdarzenia
                    handler.Invoke(this, e);
                }
            }
        }

        protected virtual void OnError(ErrorEventArgs e)
        {
            EventHandler<ErrorEventArgs> handler = Error;
            if (handler != null)
            {
                lock (lockObject)
                {
                    // Kolejkowanie zdarzenia
                    handler.Invoke(this, e);
                }
            }
        }
    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public int ProgressPercentage { get; }

        public ProgressChangedEventArgs(int progressPercentage)
        {
            ProgressPercentage = progressPercentage;
        }
    }

    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public ErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
