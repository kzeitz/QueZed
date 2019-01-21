using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.IO {
	using System.IO;
	using System.Net.Http;
	using Microsoft.WindowsAzure.Storage.Table;

	//Azure table namespace restrictions "^[A-Za-z][A-Za-z0-9]{2,62}$"
	public class AzureStorageTable : AzureStorage {
		private const string partitionKey = "PartitionKey";
		private const string rowKey = "RowKey";

		public class ColumnValue<T> {
//			private KeyValuePair<string, string> kvp;
			private KeyValuePair<string, T> kvp;
			public ColumnValue(string keyColumn, T value) { kvp = new KeyValuePair<string, T>(keyColumn, value); }
			public T Value { get { return kvp.Value; } }
			public string EqualFilterCondition { 
				get {
					if (typeof(T) == typeof(bool)) return TableQuery.GenerateFilterConditionForBool(kvp.Key, QueryComparisons.Equal, (bool)(object)kvp.Value);
					if (typeof(T) == typeof(string)) return TableQuery.GenerateFilterCondition(kvp.Key, QueryComparisons.Equal, (string)(object)kvp.Value);
					throw new InvalidCastException();
				} 
			}
			public string GreaterThanFilterCondition { 
				get {
					if (typeof(T) == typeof(string)) return TableQuery.GenerateFilterCondition(kvp.Key, QueryComparisons.GreaterThan, (string)(object)kvp.Value);
					throw new InvalidCastException();
				} 
			}
			public string GreaterThanOrEqualFilterCondition {
				get {
					if (typeof(T) == typeof(string)) return TableQuery.GenerateFilterCondition(kvp.Key, QueryComparisons.GreaterThanOrEqual, (string)(object)kvp.Value);
					throw new InvalidCastException();
				}
			}
			public string LessThanFilterCondition {
				get {
					if (typeof(T) == typeof(string)) return TableQuery.GenerateFilterCondition(kvp.Key, QueryComparisons.LessThan, (string)(object)kvp.Value);
					throw new InvalidCastException();
				}
			}
			public string LessThanOrEqualFilterCondition {
				get {
					if (typeof(T) == typeof(string)) return TableQuery.GenerateFilterCondition(kvp.Key, QueryComparisons.LessThanOrEqual, (string)(object)kvp.Value);
					throw new InvalidCastException();
				}
			}
			public string NotEqualFilterCondition {
				get {
					if (typeof(T) == typeof(string)) return TableQuery.GenerateFilterCondition(kvp.Key, QueryComparisons.NotEqual, (string)(object)kvp.Value);
					throw new InvalidCastException();
				}
			}
		}
		public class PartitionKey : ColumnValue<string> { public PartitionKey(string value) : base(partitionKey, value) { } }
		public class RowKey : ColumnValue<string> { public RowKey(string value) : base(rowKey, value) { } }

		protected static CloudTable Table(string tableName, bool createIfNotExists = true) {
			CloudTable table = tableClient.GetTableReference(tableName.ToLower());
			if (createIfNotExists) table.CreateIfNotExists(); // The Azure storage REST API will throw a 404 (not found) error before creating the table. Not sure I like that.

			// we might want to do something with this...
			TablePermissions permissions = table.GetPermissions();

			return table;
		}

		protected static List<CloudTable> Tables(string prefix = null) { return new List<CloudTable>(tableClient.ListTables()); }

	}
}
