using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Telerik.JustMock.EntityFrameworkCore
{
    /// <summary>
    /// The entry point for creating mock DbContexts for testing.
    /// </summary>
    public static class EntityFrameworkMockCore
    {
        /// <summary>
        /// Creates a mock DbContext and initializes the DbSet and IDbSet properties on the instance to mock DbSets.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
        /// <returns>The mock DbContext.</returns>
        public static TDbContext Create<TDbContext>()
        {
            return PrepareMock(Mock.Create<TDbContext>());
        }

        /// <summary>
        /// Initializes the DbSet and IDbSet properties on the given instance to mock DbSets.
        /// If the DbContext is an interface, the passed instance must have been created using Mock.Create.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
        /// <param name="dbContext">The mock DbContext.</param>
        /// <returns>The mock DbContext.</returns>
        public static TDbContext PrepareMock<TDbContext>(TDbContext dbContext)
        {
            var type = dbContext.GetType();
            var props =
                type.GetProperties()
                .Where(prop => prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(prop =>
                    {
                        var elementType = prop.PropertyType.GetGenericArguments()[0];
                        var mockDbSetType = typeof(MockDbSet<>).MakeGenericType(elementType);
                        var mockDbSet = Mock.Create(mockDbSetType, Behavior.CallOriginal);

                        return new
                        {
                            Property = prop,
                            ElementType = elementType,
                            Value = mockDbSet
                        };
                    })
                .ToArray();

            foreach (var prop in props)
            {
                if (prop.Property.GetSetMethod() != null)
                {
                    prop.Property.SetValue(dbContext, prop.Value);
                }
                else
                {
                    var propLambda = (Expression<Func<object>>)Expression.Lambda(
                        Expression.Convert(
                            Expression.MakeMemberAccess(Expression.Constant(dbContext), prop.Property),
                            typeof(object)));
                    Mock.Arrange(propLambda).Returns(prop.Value);
                }
            }

            if (dbContext is DbContext)
            {
                foreach (var prop in props)
                {
                    var genericSet = typeof(DbContext).GetMethod("Set", new Type[0]).MakeGenericMethod(prop.ElementType);
                    var setLambda = (Expression<Func<object>>)Expression.Lambda(
                        Expression.Convert(
                            Expression.Call(Expression.Constant(dbContext), genericSet),
                            typeof(object)));
                    Mock.Arrange(setLambda).Returns(prop.Value);
                }
            }

            return dbContext;
        }
    }
}
