using System;
using Windows.Foundation;

namespace EhWikiClient.Helpers
{
    public sealed class AsyncWarpper<T> : IAsyncOperation<T>
    {
        public AsyncWarpper()
            : this(default(T)) { }

        public AsyncWarpper(T result)
        {
            this.result = result;
        }

        private readonly T result;

        AsyncOperationCompletedHandler<T> IAsyncOperation<T>.Completed
        {
            get => this.completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Completed);
            }
        }

        private AsyncOperationCompletedHandler<T> completed;

        Exception IAsyncInfo.ErrorCode => null;

        uint IAsyncInfo.Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        void IAsyncInfo.Cancel() { }

        void IAsyncInfo.Close() { }

        public T GetResults() => this.result;
    }

    public sealed class AsyncWarpper : IAsyncAction
    {
        public static AsyncWarpper Create()
            => new AsyncWarpper();

        public static AsyncWarpper<T> Create<T>(T result)
            => new AsyncWarpper<T>(result);

        public static AsyncWarpper<T> Create<T>()
            => new AsyncWarpper<T>();

        public AsyncWarpper() { }

        AsyncActionCompletedHandler IAsyncAction.Completed
        {
            get => this.completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Completed);
            }
        }

        private AsyncActionCompletedHandler completed;

        Exception IAsyncInfo.ErrorCode => null;

        uint IAsyncInfo.Id => uint.MaxValue;

        AsyncStatus IAsyncInfo.Status => AsyncStatus.Completed;

        void IAsyncInfo.Cancel() { }

        void IAsyncInfo.Close() { }

        void IAsyncAction.GetResults() { }
    }
}
