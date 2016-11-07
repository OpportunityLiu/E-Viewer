using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExViewer.Helpers
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
                value(this, AsyncStatus.Completed);
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
}
