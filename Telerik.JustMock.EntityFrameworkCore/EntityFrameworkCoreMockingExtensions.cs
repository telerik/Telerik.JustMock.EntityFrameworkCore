using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Telerik.JustMock.EntityFrameworkCore
{
    /// <summary>
    /// Extensions for DbContext and IDbSet.
    /// </summary>
    public static class EntityFrameworkCoreMockingExtensions
    {
        /// <summary>
        /// Initializes the DbSet and IDbSet properties on the given DbContext instance to mock DbSets.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the user DbContext.</typeparam>
        /// <param name="dbContext">The user DbContext.</param>
        /// <returns>The user DbContext.</returns>
        public static TDbContext PrepareMock<TDbContext>(this TDbContext dbContext) where TDbContext : DbContext
        {
            return EntityFrameworkMockCore.PrepareMock(dbContext);
        }

        /// <summary>
        /// Binds a mock DbSet to a user-supplied backing collection. Changes to the backing collection are
        /// reflected on the DbSet and vice versa.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The mock DbSet.</param>
        /// <param name="data">The backing collection.</param>
        /// <returns>The mock DbSet.</returns>
        public static DbSet<TEntity> Bind<TEntity>(this DbSet<TEntity> dbSet, ICollection<TEntity> data) where TEntity : class
        {
            ((MockDbSet<TEntity>)dbSet).Data = data;
            return dbSet;
        }

        /// <summary>
        /// Sets the function that can return an entity's ID to the mock DbSet.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity</typeparam>
        /// <param name="dbSet">The mock DbSet.</param>
        /// <param name="getIdFunction">The function that can return an entity's ID.</param>
        /// <returns>The mock DbSet.</returns>
        public static DbSet<TEntity> SetIdFunction<TEntity>(this DbSet<TEntity> dbSet, GetIdFunction<TEntity> getIdFunction) where TEntity : class
        {
            ((MockDbSet<TEntity>)dbSet).GetIdFunction = getIdFunction;
            return dbSet;
        }
    }
}
