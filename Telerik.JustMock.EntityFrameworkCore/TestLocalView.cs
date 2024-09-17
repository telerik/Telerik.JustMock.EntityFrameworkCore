
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Telerik.JustMock.EntityFrameworkCore
{
    internal class TestLocalView<TEntity> : LocalView<TEntity> where TEntity : class
    {
        public ObservableCollection<TEntity> Data;

        public TestLocalView(DbSet<TEntity> set, ObservableCollection<TEntity> data)
            : base(set)
        {
            this.Data = data;
        }

        public override int Count
        {
            get { return this.Data.Count; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

#if NET6_0
        public override event NotifyCollectionChangedEventHandler CollectionChanged
#elif NET7_0_OR_GREATER
        public new event NotifyCollectionChangedEventHandler CollectionChanged
#endif
        {
            add { this.Data.CollectionChanged += value; }
            remove { this.Data.CollectionChanged -= value; }
        }

        public override void Add(TEntity item)
        {
            this.Data.Add(item);
        }

        public override void Clear()
        {
            this.Data.Clear();
        }

        public override bool Contains(TEntity item)
        {
            return this.Data.Contains(item);
        }

        public override void CopyTo(TEntity[] array, int arrayIndex)
        {
            this.Data.CopyTo(array, arrayIndex);
        }

        public override bool Equals(object obj)
        {
            return this.Data.Equals(obj);
        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() + this.Data.GetHashCode();
        }

        public override bool Remove(TEntity item)
        {
            return this.Data.Remove(item);
        }

        public override BindingList<TEntity> ToBindingList()
        {
            return this.Data.ToBindingList();
        }

        public override ObservableCollection<TEntity> ToObservableCollection()
        {
            return this.Data;
        }

        public override string ToString()
        {
            return this.Data.ToString();
        }
    }
}