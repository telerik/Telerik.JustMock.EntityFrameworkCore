using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Telerik.JustMock.EntityFrameworkCore
{
    internal abstract class TestQueryProvider<TEntity> : IOrderedQueryable<TEntity>, IQueryProvider
    {
        private IEnumerable<TEntity> _enumerable;

        protected TestQueryProvider(Expression expression)
        {
            this.Expression = expression;
        }

        protected TestQueryProvider(IEnumerable<TEntity> enumerable)
        {
            _enumerable = enumerable;
            this.Expression = enumerable.AsQueryable().Expression;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression is MethodCallExpression m)
            {
                var resultType = m.Method.ReturnType;
                var tElement = resultType.GetGenericArguments().First();
                return CreateIQueryableInstance<IQueryable>(tElement, expression);
            }

            return CreateQuery<TEntity>(expression);
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
        {
            return CreateIQueryableInstance<IQueryable<TEntity>>(typeof(TEntity), expression);
        }

        private TQueryable CreateIQueryableInstance<TQueryable>(Type elementType, Expression expression) where TQueryable : IQueryable
        {
            var queryType = this.GetType().GetGenericTypeDefinition().MakeGenericType(elementType);
            return (TQueryable)Activator.CreateInstance(queryType, expression);
        }

        public object Execute(Expression expression)
        {
            return CompileExpressionItem<object>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return CompileExpressionItem<TResult>(expression);
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            if (_enumerable == null)
            {
                _enumerable = CompileExpressionItem<IEnumerable<TEntity>>(this.Expression);
            }

            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_enumerable == null)
            {
                _enumerable = CompileExpressionItem<IEnumerable<TEntity>>(this.Expression);
            }

            return _enumerable.GetEnumerator();
        }

        public Type ElementType => typeof(TEntity);

        public Expression Expression { get; }

        public IQueryProvider Provider
        {
            get { return this; }
        }

        private static TResult CompileExpressionItem<TResult>(Expression expression)
        {
            var visitor = new TestExpressionVisitor();
            var body = visitor.Visit(expression);
            var f = Expression.Lambda<Func<TResult>>(body ?? throw new InvalidOperationException($"{nameof(body)} is null"), (IEnumerable<ParameterExpression>)null);
            return f.Compile()();
        }
    }
}