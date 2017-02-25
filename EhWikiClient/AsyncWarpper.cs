using System;
using Windows.Foundation;

namespace EhWikiClient.Helpers
{
    public class AsyncWarpper<T> : IAsyncOperation<T>
    {
        public AsyncWarpper()
            : this(default(T)) { }

        public AsyncWarpper(T result)
        {
            this.result = result;
        }

        private T result;

        public AsyncOperationCompletedHandler<T> Completed
        {
            get
            {
                return completed;
            }
            set
            {
                completed = value;
                value?.Invoke(this, AsyncStatus.Completed);
            }
        }


        private AsyncOperationCompletedHandler<T> completed;

        public Exception ErrorCode => null;

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel()
        {
        }

        public void Close()
        {
        }

        public T GetResults() => result;
    }

    public class AsyncWarpper: IAsyncAction
    {
        public AsyncWarpper() { }

        public AsyncActionCompletedHandler Completed
        {
            get
            {
                return completed;
            }
            set
            {
                completed = value;
                value?.Invoke(this, AsyncStatus.Completed);
            }
        }

        private AsyncActionCompletedHandler completed;

        public Exception ErrorCode => null;

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel()
        {
        }

        public void Close()
        {
        }

        public void GetResults()
        {
        }
    }
}
