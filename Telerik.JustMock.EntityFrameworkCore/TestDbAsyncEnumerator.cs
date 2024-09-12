using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telerik.JustMock.EntityFrameworkCore
{
    internal class TestDbAsyncEnumerator<T> : IAsyncEnumerator<T>
	{
		private readonly IEnumerator<T> _inner;

		public TestDbAsyncEnumerator(IEnumerator<T> inner)
		{
			_inner = inner;
		}

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }

        public T Current
        {
            get { return _inner.Current; }
        }
    }
}
