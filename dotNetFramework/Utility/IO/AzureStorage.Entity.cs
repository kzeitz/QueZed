using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.IO {
	using System.Diagnostics;
	using System.Reflection;
	using System.Collections.Concurrent;
	using System.IO;
	using System.Linq.Expressions;
	using System.Net.Http;
	using Microsoft.WindowsAzure.Storage;
	using Microsoft.WindowsAzure.Storage.Table;
	using Microsoft.WindowsAzure.Storage.Table.DataServices;

//	internal static class AzureStorageEntityUtilities {
//		private static ConcurrentDictionary<Type, Func<object[], object>> compiledActivators = new ConcurrentDictionary<Type, Func<object[], object>>();
//		private static Func<object[], object> Activator(Type type) { return Activator(type, Type.EmptyTypes); }
//		private static Func<object[], object> Activator(Type type, Type[] ctorParamTypes) {
//			ParameterExpression expression = null;
//			ConstructorInfo constructor = type.GetConstructor(ctorParamTypes);
//			if (constructor == null) throw new InvalidOperationException("TableQuery Generic Type must provide a default parameterless constructor.");
//			ParameterInfo[] parameters = constructor.GetParameters();
//			Expression[] arguments = new Expression[parameters.Length];
//			for (int i = 0; i < parameters.Length; i++) {
//				Expression index = Expression.Constant(i);
//				Type parameterType = parameters[i].ParameterType;
//				arguments[i] = Expression.Convert(Expression.ArrayIndex(expression = Expression.Parameter(typeof(object[]), "args"), index), parameterType);
//			}
//			NewExpression body = Expression.New(constructor, arguments);
//			return (Func<object[], object>)Expression.Lambda(typeof(Func<object[], object>), body).Compile();
////			return (Func<object[], object>)Expression.Lambda(typeof(Func<object[], object>), body, new ParameterExpression[] { expression }).Compile();
//		}
//		internal static object CreateEntity(Type type) { return compiledActivators.GetOrAdd(type, new Func<Type, Func<object[], object>>(AzureStorageEntityUtilities.Activator))(null); }
//		internal static DynamicTableEntity ResolveDynamicEntity(string partitionKey, string rowKey, DateTimeOffset timestamp, IDictionary<string, EntityProperty> properties, string etag) {
//			DynamicTableEntity entity = new DynamicTableEntity(partitionKey, rowKey) { Timestamp = timestamp };
//			entity.ReadEntity(properties, null);
//			entity.ETag = etag;
//			return entity;
//		}
//		internal static T ResolveEntity<T>(string partitionKey, string rowKey, DateTimeOffset timestamp, IDictionary<string, EntityProperty> properties, string etag) {
//			ITableEntity entity = (ITableEntity)CreateEntity(typeof(T));
//			entity.PartitionKey = partitionKey;
//			entity.RowKey = rowKey;
//			entity.Timestamp = timestamp;
//			entity.ReadEntity(properties, null);
//			entity.ETag = etag;
//			return (T)entity;
//		}
//	}

	public class AzureStorageEntity : AzureStorageTable {
		// ToDo: We are eating any StorageExceptions here... not sure how I want to handle them, or if I want them to bubble or not.
		// ToDo: We are using the blocking versions of the table storage calls, might have to use asynchronous versions.

		CloudTable table = null;
		public AzureStorageEntity(string tableName) { this.table = Table(tableName); }

		public static T ResolveEntity<T>(T entity, string partitionKey, string rowKey, DateTimeOffset timestamp, IDictionary<string, EntityProperty> properties, string etag) where T : TableEntity {
			entity.PartitionKey = partitionKey;
			entity.RowKey = rowKey;
			entity.Timestamp = timestamp;
			entity.ReadEntity(properties, null);
			entity.ETag = etag;
			return entity;
		}

		#region Create a new entity.
		public T Create<T>(T entity) where T : TableEntity {
			try {
				TableOperation insertOperation = TableOperation.Insert(entity);
				TableResult tableResult = table.Execute(insertOperation);
				Debug.WriteLine("Entity created.");
				return ((T)tableResult.Result);
			} catch (StorageException sex) { Debug.WriteLine(sex.RequestInformation.ExtendedErrorInformation.ErrorMessage); }
			return null;
		}
		#region These were for the EntityController, but are likely going away
		public TableEntity Create(TableEntity entity) { return Create<TableEntity>(entity); }
		public TableEntity Create(Type t, TableEntity entity) {
			MethodInfo method = getMethod<AzureStorageEntity>(e => e.Create<TableEntity>(new TableEntity()));
			return (TableEntity)method.MakeGenericMethod(t).Invoke(this, new object[] { entity });
		}
		#endregion
		#endregion

		#region Read multiple entities. (rows essentially)
		// Read multiple entities. (rows essentially)
		public IEnumerable<T> Read<T>(PartitionKey partitionKey) where T : TableEntity, new() { return Read<T>(partitionKey.EqualFilterCondition); }
		public IEnumerable<T> Read<T>(RowKey rowKey) where T : TableEntity, new() { return Read<T>(rowKey.EqualFilterCondition); }
		public IEnumerable<T> Read<T>(string filterCondtion) where T : TableEntity, new() {
			try {
				TableQuery<T> query = new TableQuery<T>().Where(filterCondtion);
				return table.ExecuteQuery<T>(query);  // this call blocks here.  If this becomes a problem, consider table.ExecuteQuerySegmentedAsync<T>()
			} catch (StorageException sex) { Debug.WriteLine(sex.RequestInformation.ExtendedErrorInformation.ErrorMessage); }
			return null;
		}
		public IEnumerable<T> Read<T>(string filterCondtion, EntityResolver<T> resolver) where T : TableEntity, new() {
			try {
				TableQuery query = new TableQuery().Where(filterCondtion);
				return table.ExecuteQuery<T>(query, resolver);  // this call blocks here.  If this becomes a problem, consider table.ExecuteQuerySegmentedAsync<T>()
			} catch (StorageException sex) { Debug.WriteLine(sex.RequestInformation.ExtendedErrorInformation.ErrorMessage); }
			return null;
		}
		#endregion

		#region Read single entity. (row essentially)
		public T Read<T>(PartitionKey partitionKey, RowKey rowKey) where T : TableEntity {
			try {
				Debug.WriteLine("Storage read start.");
				TableOperation readOperation = TableOperation.Retrieve<T>(partitionKey.Value, rowKey.Value);
				TableResult tableResult = table.Execute(readOperation);
				object result = tableResult.Result;
				Debug.WriteLine("Storage read complete.");
				return ((T)result);
			} catch (StorageException sex) { Debug.WriteLine(sex.RequestInformation.ExtendedErrorInformation.ErrorMessage); }
			return null;
		}
		#region These were for the EntityController, but are likely going away
		public TableEntity Read(PartitionKey partitionKey, RowKey rowKey) { return Read<TableEntity>(partitionKey, rowKey); }
		public TableEntity Read(Type t, PartitionKey partitionKey, RowKey rowKey) {
			MethodInfo method = getMethod<AzureStorageEntity>(e => e.Read<TableEntity>(new PartitionKey(string.Empty), new RowKey(string.Empty)));
			return (TableEntity)method.MakeGenericMethod(t).Invoke(this, new object[] { partitionKey, rowKey });
		}
		#endregion
		#endregion

		#region Update an Entity
		public T Update<T>(T entity) where T : TableEntity {
			try {
				TableOperation replaceOperation = TableOperation.Replace(entity);
				TableResult tableResult = table.Execute(replaceOperation); // this call blocks here.  If this becomes a problem, consider table.ExecuteAsync()
				return ((T)tableResult.Result);
			} catch (StorageException sex) { Debug.WriteLine(sex.RequestInformation.ExtendedErrorInformation.ErrorMessage); }
			return null;
		}
		#region These were for the EntityController, but are likely going away
		public TableEntity Update(TableEntity entity) { return Update<TableEntity>(entity); }
		public TableEntity Update(Type t, TableEntity entity) {
			MethodInfo method = getMethod<AzureStorageEntity>(e => e.Update<TableEntity>(new TableEntity()));
			return (TableEntity)method.MakeGenericMethod(t).Invoke(this, new object[] { entity });
		}
		#endregion
		#endregion

		#region Delete an Entity
		public TableEntity Delete(TableEntity entity) {
			try {
				TableOperation deleteOperation = TableOperation.Delete(entity);
				TableResult tableResult = table.Execute(deleteOperation);
				return ((TableEntity)tableResult.Result);
			} catch (StorageException sex) { Debug.WriteLine(sex.RequestInformation.ExtendedErrorInformation.ErrorMessage); }
			return null;
		}
		#endregion
	}
}
