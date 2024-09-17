using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Telerik.JustMock.EntityFrameworkCore
{
    /// <summary>
    /// A user-provided delegate that can return the value of the primary key of the entity.
    /// The return value should be either:
    /// * the value of the primary key if is non-composite
    /// * an IEnumerable with the values of the key columns, if the key is composite
    /// </summary>
    public delegate object GetIdFunction<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// An in-memory mock DbSet. DbContext instances created by <see cref="EntityFrameworkMockCore.Create"/> are
    /// initialized with instances of this class.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class MockDbSet<TEntity> : DbSet<TEntity>, IAsyncEnumerable<TEntity>, IQueryable<TEntity> where TEntity : class
    {
        private ICollection<TEntity> data;
        private IQueryable<TEntity> asQueryable;

        /// <summary>
        /// The backing collection for this DbSet. All operations on the DbSet are made
        /// against this collection. The collection should be an instance of
        /// ObservableCollection&lt;T&gt; for the <see cref="Local"/> property to work.
        /// </summary>
        public ICollection<TEntity> Data
        {
            get { return this.data; }
            set
            {
                this.data = value;
                this.asQueryable = value.AsQueryable();
            }
        }

        /// <summary>
        /// A user-provided delegate that can return the value of the primary key of the entity.
        /// 
        /// This delegate is called by the <see cref="Find"/> method. If the entity's primary key property
        /// is called "Id" or "%Entity%Id" (where %Entity% is the name of the entity class), then a
        /// function is generated automatically to return the value of that property.
        /// 
        /// The return value should be either:
        /// * the value of the primary key if is non-composite
        /// * an IEnumerable with the values of the key columns, if the key is composite
        /// </summary>
        public GetIdFunction<TEntity> GetIdFunction { get; set; }

        /// <summary>
        /// Creates a DbSet with an empty ObservableCollection for its backing store.
        /// </summary>
        public MockDbSet()
        {
            this.Data = new ObservableCollection<TEntity>();
        }

        private void InitializeDefaultGetIdFunction()
        {
            var idProp = typeof(TEntity).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                ?? typeof(TEntity).GetProperty(typeof(TEntity).Name + "Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (idProp == null)
            {
                throw new InvalidOperationException("Couldn't automatically determine the entity key. Specify a 'get ID' function before using Find() using SetIdFunction() extension method.");
            }

            var entityParam = Expression.Parameter(typeof(TEntity));
            var getIdLambda = Expression.Lambda(typeof(GetIdFunction<TEntity>),
                Expression.Convert(Expression.MakeMemberAccess(entityParam, idProp), typeof(object)),
                entityParam);
            this.GetIdFunction = (GetIdFunction<TEntity>)getIdLambda.Compile();
        }

        private static EntityEntry<TEntity> MockEntityEnty(TEntity entity)
        {
            var entryMock = Mock.Create<EntityEntry<TEntity>>();
            Mock.Arrange(() => entryMock.Entity).Returns(entity);
            Mock.Arrange(() => entryMock.State).Returns(EntityState.Detached);
            return entryMock;
        }

        public override EntityEntry<TEntity> Add(TEntity entity)
        {
            this.Data.Add(entity);
            return MockEntityEnty(entity);
        }

        public override void AddRange(IEnumerable<TEntity> entities)
        {
            AddRange(entities);
        }

        public override void AddRange(params TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                this.Data.Add(entity);
            }
        }

        public override EntityEntry<TEntity> Attach(TEntity entity)
        {
            this.Data.Add(entity);
            return MockEntityEnty(entity);
        }

        public override TEntity? Find(params object[] keyValues)
        {
            if (this.GetIdFunction == null)
            {
                InitializeDefaultGetIdFunction();
            }

            foreach (var entity in this.Data)
            {
                var keys = GetIdFunction(entity);
                var keyCollection = keys as ICollection;
                if (keyCollection != null)
                {
                    if (keyValues.Length != keyCollection.Count)
                    {
                        throw new InvalidOperationException("Number of keys passed to Find() is not equal to the number of keys on the entity.");
                    }
                    if (keyValues.SequenceEqual(keyCollection.Cast<object>()))
                    {
                        return entity;
                    }
                }
                else
                {
                    if (keyValues.Length != 1)
                    {
                        throw new InvalidOperationException("Number of keys passed to Find() is not equal to the number of keys on the entity.");
                    }
                    if (Object.Equals(keyValues[0], keys))
                    {
                        return entity;
                    }
                }
            }

            return null;
        }

        public override EntityEntry<TEntity> Remove(TEntity entity)
        {
            this.Data.Remove(entity);
            return MockEntityEnty(entity);
        }

        public override void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.Data.Remove(entity);
            }
        }

        public override IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestDbAsyncEnumerator<TEntity>(this.data.GetEnumerator());
        }

        public virtual Type ElementType
        {
            get { return this.asQueryable.ElementType; }
        }

        public virtual Expression Expression
        {
            get { return this.asQueryable.Expression; }
        }

        public virtual IQueryProvider Provider
        {
            get { return new TestDbAsyncQueryProvider<TEntity>(this.asQueryable.Provider); }
        }

        public override IEntityType EntityType
        {
            get { return Mock.Create<IEntityType>(); }
        }

#if NET6_0
        public override LocalView<TEntity> Local
        {
            get 
            {
                var mockLocalView = Mock.Create<TestLocalView<TEntity>>(Constructor.Mocked, Behavior.CallOriginal);
                mockLocalView.Data = (ObservableCollection<TEntity>)this.Data;
                return mockLocalView;
            }
        }
#endif

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
    }
}
