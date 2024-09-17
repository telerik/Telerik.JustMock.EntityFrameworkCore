using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Telerik.JustMock.EntityFrameworkCore
{
    internal class TestDbAsyncQueryProvider<TEntity> : TestQueryProvider<TEntity>, IAsyncEnumerable<TEntity>, IAsyncQueryProvider
    {
        public TestDbAsyncQueryProvider(Expression expression)
            : base(expression)
        {
        }

        public TestDbAsyncQueryProvider(IEnumerable<TEntity> enumerable)
            : base(enumerable)
        {
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .Single(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new object[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult });
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestDbAsyncEnumerator<TEntity>(this.AsEnumerable().GetEnumerator());
        }
    }
}