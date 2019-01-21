using System;
using System.Collections.Generic;

namespace DAL {
	using System.ComponentModel;
	using System.Reflection;

	public static class GenericComparerFactory {
		private readonly static Dictionary<Type, Dictionary<String, RuntimeMethodHandle>> comparers = new Dictionary<Type, Dictionary<string, RuntimeMethodHandle>>();
		public static GenericIComparer<T> GetComparer<T>(string propertyName, ListSortDirection direction) {
			//Check if the type array for this comparer was created.
			if (!comparers.ContainsKey(typeof(T))) comparers.Add(typeof(T), new Dictionary<string, RuntimeMethodHandle>());
			if (!comparers[typeof(T)].ContainsKey(propertyName)) comparers[typeof(T)].Add(propertyName, typeof(T).GetProperty(propertyName).GetGetMethod().MethodHandle);
			return (GenericIComparer<T>)new GenericIComparer<T>(MethodInfo.GetMethodFromHandle(comparers[typeof(T)][propertyName]), direction);
		}
		public static GenericIComparer<T> GetComparer<T>(string propertyName) { return GetComparer<T>(propertyName, ListSortDirection.Ascending); }
	}

	public class GenericIComparer<T> : IComparer<T> {
		private readonly MethodBase methodInfo;
		private ListSortDirection sortDirection = ListSortDirection.Ascending;

		internal GenericIComparer(MethodBase methodInfo, ListSortDirection direction) { this.methodInfo = methodInfo; sortDirection = direction; }
		public GenericIComparer(string propertyName) : this(propertyName, ListSortDirection.Ascending) { }
		public GenericIComparer(string propertyName, ListSortDirection direction) : this(typeof(T).GetProperty(propertyName).GetGetMethod(), direction) { }
		#region IComparer<T> members
		public int Compare(T x, T y) {
			IComparable obj1 = (IComparable)methodInfo.Invoke(x, null);
			IComparable obj2 = (IComparable)methodInfo.Invoke(y, null);
			Int32 result = (obj1.CompareTo(obj2));
			return sortDirection == ListSortDirection.Ascending ? result : -result;
		}
		#endregion
		public static PropertyDescriptor GetPropertyDescriptor(string propertyName, Type type) {
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(type);
			foreach (PropertyDescriptor pd in pdc) if (pd.Name == propertyName) return pd;
			return null;
		}
	}

	public class FindableSortableBindingList<T> : BindingList<T> {
		#region Find members
		protected override bool SupportsSearchingCore { get { return true; } }
		protected override int FindCore(PropertyDescriptor property, object key) {
			if (!(key is T)) throw new ArgumentException("Incompatible object type", "key");
			GenericIComparer<T> comparer = GenericComparerFactory.GetComparer<T>(property.Name);
			for (Int32 index = 0; index < Items.Count; ++index) if (comparer.Compare(Items[index], (T)key) == 0) return index;
			return -1;
		}
		#endregion
		#region Sort members
		private bool isSorted = false;
		protected override bool IsSortedCore { get { return isSorted; } }
		protected override void RemoveSortCore() { isSorted = false; }
		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction) {
			List<T> items = Items as List<T>;
			if (items != null) {
				GenericIComparer<T> comparer = GenericComparerFactory.GetComparer<T>(property.Name, direction);
				items.Sort(comparer);
				isSorted = true;
			}
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
		#endregion
		protected PropertyDescriptor getPropertyDescriptor(string propertyName) { return GenericIComparer<T>.GetPropertyDescriptor(propertyName, typeof(T)); }
	}

	// I believe this class and GenericIComparer<> can be combined at some point
	public class PropertyComparer<T> : IComparer<T> {
		private PropertyDescriptor propertyDescriptor;
		private ListSortDirection sortDirection;
	 
		public PropertyComparer(PropertyDescriptor property, ListSortDirection direction) {
			propertyDescriptor = property;
			sortDirection = direction;
		}
		public int Compare(T xWord, T yWord) {
			object xValue = GetPropertyValue(xWord, propertyDescriptor.Name);
			object yValue = GetPropertyValue(yWord, propertyDescriptor.Name);
			if (sortDirection == ListSortDirection.Ascending) return CompareAscending(xValue, yValue);
			else return CompareDescending(xValue, yValue);
		}
		public bool Equals(T xWord, T yWord) { return xWord.Equals(yWord); }
		public int GetHashCode(T obj) { return obj.GetHashCode(); }
		private int CompareAscending(object xValue, object yValue) {
			if (xValue is IComparable) return ((IComparable)xValue).CompareTo(yValue);
			else if (xValue.Equals(yValue)) return 0;
			else return xValue.ToString().CompareTo(yValue.ToString());
		}
		private int CompareDescending(object xValue, object yValue) { return CompareAscending(xValue, yValue) * -1; }
		private object GetPropertyValue(T value, string property) {
			PropertyInfo propertyInfo = value.GetType().GetProperty(property);
			return propertyInfo.GetValue(value, null);
		}
		public static PropertyDescriptor GetPropertyDescriptor(string propertyName, Type type) {
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(type);
			foreach (PropertyDescriptor pd in pdc) if (pd.Name == propertyName) return pd;
			return null;
		}
	}
	#region Save Code
	// This class did not work out as well as I wanted.  Quite a lot of work to just delete it so lets keep it here for now
	public class AggregateBindingList<T> : IList<T>, System.Collections.IList, IBindingList {
		protected FindableSortableBindingList<T> bindingList = new FindableSortableBindingList<T>();
		#region IList<T> Members
		int IList<T>.IndexOf(T item) { return ((IList<T>)bindingList).IndexOf(item); }
		void IList<T>.Insert(int index, T item) { ((IList<T>)bindingList).Insert(index, item); }
		void IList<T>.RemoveAt(int index) { ((IList<T>)bindingList).RemoveAt(index); }
		T IList<T>.this[int index] { get { return ((IList<T>)bindingList)[index]; } set { ((IList<T>)bindingList)[index] = value; } }
		#endregion
		#region ICollection<T> Members
		void ICollection<T>.Add(T item) { ((ICollection<T>)bindingList).Add(item); }
		void ICollection<T>.Clear() { ((ICollection<T>)bindingList).Clear(); }
		bool ICollection<T>.Contains(T item) { return ((ICollection<T>)bindingList).Contains(item); }
		void ICollection<T>.CopyTo(T[] array, int arrayIndex) { ((ICollection<T>)bindingList).CopyTo(array, arrayIndex); }
		int ICollection<T>.Count { get { return ((ICollection<T>)bindingList).Count; } }
		bool ICollection<T>.IsReadOnly { get { return ((ICollection<T>)bindingList).IsReadOnly; } }
		bool ICollection<T>.Remove(T item) { return ((ICollection<T>)bindingList).Remove(item); }
		#endregion
		#region IEnumerable<T> Members
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return ((IEnumerable<T>)bindingList).GetEnumerator(); }
		#endregion
		#region IBindingList Members
		void IBindingList.AddIndex(PropertyDescriptor property) { ((IBindingList)bindingList).AddIndex(property); }
		object IBindingList.AddNew() { return ((IBindingList)bindingList).AddNew(); }
		bool IBindingList.AllowEdit { get { return ((IBindingList)bindingList).AllowEdit; } }
		bool IBindingList.AllowNew { get { return ((IBindingList)bindingList).AllowNew; } }
		bool IBindingList.AllowRemove { get { return ((IBindingList)bindingList).AllowRemove; } }
		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) { ((IBindingList)bindingList).ApplySort(property, direction); }
		int IBindingList.Find(PropertyDescriptor property, object key) { return ((IBindingList)bindingList).Find(property, key); }
		bool IBindingList.IsSorted { get { return ((IBindingList)bindingList).IsSorted; } }
		event ListChangedEventHandler IBindingList.ListChanged { add { lock (bindingList) ((IBindingList)bindingList).ListChanged += value; } remove { lock (bindingList) ((IBindingList)bindingList).ListChanged -= value; } }
		void IBindingList.RemoveIndex(PropertyDescriptor property) { ((IBindingList)bindingList).RemoveIndex(property); }
		void IBindingList.RemoveSort() { ((IBindingList)bindingList).RemoveSort(); }
		ListSortDirection IBindingList.SortDirection { get { return ((IBindingList)bindingList).SortDirection; } }
		PropertyDescriptor IBindingList.SortProperty { get { return ((IBindingList)bindingList).SortProperty; } }
		bool IBindingList.SupportsChangeNotification { get { return ((IBindingList)bindingList).SupportsChangeNotification; } }
		bool IBindingList.SupportsSearching { get { return ((IBindingList)bindingList).SupportsSearching; } }
		bool IBindingList.SupportsSorting { get { return ((IBindingList)bindingList).SupportsSorting; } }
		#endregion
		#region IList Members
		int System.Collections.IList.Add(object value) { return ((System.Collections.IList)bindingList).Add(value); }
		void System.Collections.IList.Clear() { ((System.Collections.IList)bindingList).Clear(); }
		bool System.Collections.IList.Contains(object value) { return ((System.Collections.IList)bindingList).Contains(value); }
		int System.Collections.IList.IndexOf(object value) { return ((System.Collections.IList)bindingList).IndexOf(value); }
		void System.Collections.IList.Insert(int index, object value) { ((System.Collections.IList)bindingList).Insert(index, value); }
		bool System.Collections.IList.IsFixedSize { get { return ((System.Collections.IList)bindingList).IsFixedSize; } }
		bool System.Collections.IList.IsReadOnly { get { return ((System.Collections.IList)bindingList).IsReadOnly; } }
		void System.Collections.IList.Remove(object value) { ((System.Collections.IList)bindingList).Remove(value); }
		void System.Collections.IList.RemoveAt(int index) { ((System.Collections.IList)bindingList).RemoveAt(index); }
		object System.Collections.IList.this[int index] { get { return ((System.Collections.IList)bindingList)[index]; } set { ((System.Collections.IList)bindingList)[index] = value; } }
		#endregion
		#region ICollection Members
		void System.Collections.ICollection.CopyTo(Array array, int index) { ((System.Collections.ICollection)bindingList).CopyTo(array, index); }
		int System.Collections.ICollection.Count { get { return ((System.Collections.ICollection)bindingList).Count; } }
		bool System.Collections.ICollection.IsSynchronized { get { return ((System.Collections.ICollection)bindingList).IsSynchronized; } }
		object System.Collections.ICollection.SyncRoot { get { return ((System.Collections.ICollection)bindingList).SyncRoot; } }
		#endregion
		#region IEnumerable Members
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((System.Collections.IEnumerable)bindingList).GetEnumerator(); }
		#endregion
	}
	#endregion
}
