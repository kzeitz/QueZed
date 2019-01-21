using System;
using System.Collections;
using System.Collections.Generic;

namespace DAL {
	using System.Reflection;
	using System.ComponentModel;
	using System.Text.RegularExpressions;
	using System.Data;
	using System.Data.Common;
	using System.Data.SqlClient;
	using Microsoft.Practices.EnterpriseLibrary.Data;
	using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
	using Wintellect.PowerCollections;

	// holds a couple of class extensions so IEnumerable in .Net 2.0 acts a little more like IEnumerable in .Net 3.5
	// Remove when Framework upgraded
	public static class Extensions {
		public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source) {
			IEnumerable<TResult> enumerable = source as IEnumerable<TResult>;
			if (enumerable != null) return enumerable;
			if (source == null) throw new ArgumentNullException();
			return CastIterator<TResult>(source);
		}
		private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source) {
			foreach (object current in source) yield return (TResult)current;
			yield break;
		}
	}

   public sealed class DataSourceEnterpriseLibraryConnectionName : IDataSource {
      private string connectionName = string.Empty;
      public DataSourceEnterpriseLibraryConnectionName(string connectionName) {
         this.connectionName = connectionName;
         new DatabaseSource(DatabaseFactory.CreateDatabase(connectionName));
      }
   }

   public sealed class DataSourceEnterpriseLibraryConnectionString : IDataSource {
      private string connectionString = string.Empty;
      public DataSourceEnterpriseLibraryConnectionString(string connectionString) {
         this.connectionString = connectionString;
         new DatabaseSource(new SqlDatabase(connectionString));
      }
   }

	//public class DatabaseKeyConverter : TypeConverter {
	//   public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
	//      if (sourceType == typeof(int) || sourceType == typeof(DatabaseKey)) return true;
	//      return base.CanConvertFrom(context, sourceType);
	//   }
	//   public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
	//      if (destinationType == typeof(int) || destinationType == typeof(DatabaseKey)) return true;
	//      return base.CanConvertTo(context, destinationType);
	//   }
	//   public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
	//      if (value is int) return new DatabaseKey((int)value);
	//      if (value is DatabaseKey) return ((DatabaseKey)value).KeyValue;
	//      return base.ConvertFrom(context, culture, value);
	//   }
	//   public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
	//      if (destinationType == typeof(int))	return ((DatabaseKey)value).KeyValue;
	//      if (destinationType == typeof(int)) return new DatabaseKey((int)value);
	//      return base.ConvertTo(context, culture, value, destinationType);
	//   }
	//}

	//[TypeConverter(typeof(DatabaseKeyConverter))]
	internal sealed class DatabaseKey : IConvertible {
	//internal sealed class DatabaseKey {
      private int key = -1;
      public DatabaseKey(int key) { this.key = key; }
      public int KeyValue { get { return key; } }
      public string KeyString { get { return key != -1 ? key.ToString() : string.Empty; } }
		static public implicit operator int(DatabaseKey key) { return key.KeyValue; }
		static public implicit operator DatabaseKey(int value) { return new DatabaseKey(value); }
		#region IConvertable
		public TypeCode GetTypeCode() { return TypeCode.Object; }
		public bool ToBoolean(IFormatProvider provider) { return Convert.ToBoolean(key); }
		public byte ToByte(IFormatProvider provider) { return Convert.ToByte(key); }
		public char ToChar(IFormatProvider provider) { return Convert.ToChar(key); }
		public DateTime ToDateTime(IFormatProvider provider) { return Convert.ToDateTime(key); }
		public decimal ToDecimal(IFormatProvider provider) { return Convert.ToDecimal(key); }
		public double ToDouble(IFormatProvider provider) { return Convert.ToDouble(key); }
		public short ToInt16(IFormatProvider provider) { return Convert.ToInt16(key); }
		public int ToInt32(IFormatProvider provider) { return key; }
		public long ToInt64(IFormatProvider provider) { return Convert.ToInt64(key); }
		public sbyte ToSByte(IFormatProvider provider) { return Convert.ToSByte(key); }
		public float ToSingle(IFormatProvider provider) { return Convert.ToSingle(key); }
		public string ToString(IFormatProvider provider) { return Convert.ToString(key); }
		public object ToType(Type conversionType, IFormatProvider provider) { return Convert.ChangeType(key, conversionType); }
		public ushort ToUInt16(IFormatProvider provider) { return Convert.ToUInt16(key); }
		public uint ToUInt32(IFormatProvider provider) { return Convert.ToUInt32(key); }
		public ulong ToUInt64(IFormatProvider provider) { return Convert.ToUInt64(key); }
		#endregion
	}

	internal sealed class DatabaseRelation {
		private DatabaseKey primaryKey = null;
		private DatabaseKey foreignKey = null;
      public DatabaseRelation(DatabaseKey primaryKey, DatabaseKey foreignKey) { this.primaryKey = primaryKey; this.foreignKey = foreignKey; }
		public DatabaseRelation(DatabaseKey primaryKey) : this(primaryKey, null) {}
      public DatabaseRelation(int primaryKeyValue, int foreignKeyValue) { this.primaryKey = new DatabaseKey(primaryKeyValue); this.foreignKey = new DatabaseKey(foreignKeyValue); }
		public DatabaseRelation(int primaryKey) : this(new DatabaseKey(primaryKey), null) { }
      public DatabaseKey PrimaryKey { get { return primaryKey; } }
      public DatabaseKey ForeignKey { get { return foreignKey; } }
   }

   public class DatabaseSource {

		internal class Mapping {
			private string name = string.Empty;
			private SqlDbType dbType = SqlDbType.NVarChar;
			private int? ordinal = null;
         private FieldInfo fieldInfo = null;
			private bool isKey = false;
			public Mapping(string name, SqlDbType dbType, int? ordinal, FieldInfo fieldInfo, bool isKey) {
				this.name = name;
				this.dbType = dbType;
				this.ordinal = ordinal;
            this.fieldInfo = fieldInfo;
				this.isKey = isKey;
				if (string.IsNullOrEmpty(name)) { name = fieldInfo.Name; isKey = true; }
         }
			public string Name { get { return name; } }
			public SqlDbType Type { get { return dbType; } }
			public int? Ordinal { get { return ordinal; } }
         public FieldInfo FieldInfo { get { return fieldInfo; } }
			public bool IsKey { get { return isKey; } }
      }
      
		internal class ReadMappping : Mapping {
			private object fieldValue = null;
			public ReadMappping(string fieldName, SqlDbType paramterType, int? parameterOrdinal, FieldInfo fieldInfo, bool isKey) : base(fieldName, paramterType, parameterOrdinal, fieldInfo, isKey) { this.fieldValue = fieldInfo.FieldType.IsValueType ? Activator.CreateInstance(fieldInfo.FieldType) : null; }
			public string FieldName { get { return base.Name; } }
			public int? FieldOrdinal { get { return base.Ordinal; } }
			public object FieldValue { get { return fieldValue; } }
		}

		internal class WriteMapping : Mapping {
			public WriteMapping(string parameterName, SqlDbType paramterType, int? parameterOrdinal, FieldInfo fieldInfo, bool isKey) : base(parameterName, paramterType, parameterOrdinal, fieldInfo, isKey) { }
			public string ParameterName { get { return base.Name; } }
			public SqlDbType ParameterType { get { return base.Type; } }
			public int? ParameterOrdinal { get { return base.Ordinal; } }
      }
      
		internal static T createObject<T>(IDataReader dr, List<ReadMappping> fieldMapList) where T : class, new() {
         T instanceT = new T();
			foreach (ReadMappping fm in fieldMapList) {
            if (!fm.FieldInfo.IsInitOnly) {
               object value = fm.FieldValue;
					if (null != fm.FieldOrdinal && !dr.IsDBNull((int)fm.FieldOrdinal)) value = dr.GetValue((int)fm.FieldOrdinal);
               setFieldValue(instanceT, fm.FieldInfo, value);
            }
         }
         return instanceT;
      }
		internal static void setFieldValue(object instance, FieldInfo fi, object value) {
         Type type = fi.FieldType;
			if (type.Equals(typeof(DatabaseKey))) fi.SetValue(instance, new DatabaseKey((int)value)); // Don't like this line at all
			else if (type.BaseType.Equals(typeof(System.Enum))) fi.SetValue(instance, System.Enum.ToObject(type, value));
         else fi.SetValue(instance, Convert.ChangeType(value, type));
      }
      protected static Database database = null;
      protected static Database db { get { if (null == database) throw new NullReferenceException("Database uninitialized."); else return database; } }
		internal protected DatabaseSource() { }
		internal DatabaseSource(Database database) { DatabaseSource.database = database; }
   }

   [AttributeUsage(AttributeTargets.Field)]
   internal sealed class DatabaseScalarAttribute : System.Attribute {
      private string selectProcedure = string.Empty;
      private object nullValue = null;
      private string key = null;
      private FieldInfo fieldInfoCache = null;
		public DatabaseScalarAttribute(string selectProcedure, object dbNullValue, string key) : base() {
         this.selectProcedure = selectProcedure;
         this.nullValue = dbNullValue;
         this.key = key;
      }
      public string SelectProcedure { get { return selectProcedure; } }
      public object NullValue { get { return nullValue; } }
      public string Key { get { return key; } }
      public string UpdateProcedure { get; set; }
      public object[] UpdateParameters(object instance) {
         if (null == instance) return new object[2]; //key and value = 2
         return new object[] { key, fieldInfoCache.GetValue(instance) };
      }
		public FieldInfo DatabaseFieldInfoCache { get { return fieldInfoCache; } set { fieldInfoCache = value; } }
   }

   public class DatabaseStore : DatabaseSource, IStorage {
		//careful changing the names of these fields they are reflected on in Map.readFields()
		private DatabaseKey primaryKey = null;
		private DatabaseKey foreignKey = null;

      [AttributeUsage(AttributeTargets.Field)]
      protected sealed class DatabaseFieldAttribute : System.Attribute {
			private string resultColumnName = string.Empty;
			private SqlDbType resultColumnType = SqlDbType.NVarChar;
			private object resultColumnNullValue = null;
         private string parameterName = string.Empty;
			int? parameterSelectOrdinal = null;
			int? parameterUpdateOrdinal = null;
			private bool isKeyField = false;
			public DatabaseFieldAttribute(string resultColumnName, SqlDbType resultColumnType, object resultColumnNullValue) : base() {
				this.resultColumnName = resultColumnName;
				this.resultColumnType = resultColumnType;
				this.resultColumnNullValue = resultColumnNullValue;
				this.parameterName = string.Format("@{0}", resultColumnName);
			}
			public string ColumnName { get { return resultColumnName; } }
			public SqlDbType ColumnType { get { return resultColumnType; } }
			public object NullValue { get { return resultColumnNullValue; } }
         public string ParameterName { get { return parameterName; } set { parameterName = value; } }
			public int? ParameterSelectOrdinal { get { return parameterSelectOrdinal; } set { parameterSelectOrdinal = value; } }
			public int? ParameterUpdateOrdinal { get { return parameterUpdateOrdinal; } set { parameterUpdateOrdinal = value; } }
			public bool IsKeyField { get { return isKeyField; } }
			internal static DatabaseFieldAttribute GetDatabaseFieldAttribute(DatabaseRelationAttribute dra, FieldInfo fi, IDataReader dr) {
				DatabaseFieldAttribute dfa = (DatabaseFieldAttribute)Attribute.GetCustomAttribute(fi, typeof(DatabaseFieldAttribute));
				if (null == dfa) dfa = new DatabaseFieldAttribute(dra[fi.Name] ?? fi.Name, SqlDbType.NVarChar, null);
				dfa.parameterSelectOrdinal = dfa.parameterSelectOrdinal ?? dr.GetOrdinal(dfa.ColumnName);
				dfa.isKeyField = DatabaseRelationAttribute.IsKeyField(fi);
				return dfa;
			}
			internal static DatabaseFieldAttribute GetDatabaseFieldAttribute(DatabaseRelationAttribute dra, FieldInfo fi, Type type) {
				DatabaseFieldAttribute dfa = (DatabaseFieldAttribute)Attribute.GetCustomAttribute(fi, typeof(DatabaseFieldAttribute));
				if (null == dfa) dfa = new DatabaseFieldAttribute(dra[fi.Name] ?? fi.Name, SqlDbType.NVarChar, null);
				dfa.parameterUpdateOrdinal = dfa.parameterUpdateOrdinal ?? findUpdateOrdinal(type, dfa.ParameterName);
				dfa.isKeyField = DatabaseRelationAttribute.IsKeyField(fi);
				return dfa;
			}
			private static int? findSelectOrdinal(Type type, string parameterName) { return findOrdinal(((DatabaseCommandAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseSelectAttribute))).Command, parameterName); }
			private static int? findUpdateOrdinal(Type type, string parameterName) { return findOrdinal(((DatabaseCommandAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseUpdateAttribute))).Command, parameterName); }
			private static int? findOrdinal(string command, string parameterName) {
				if (string.IsNullOrEmpty(command) || !command.Contains("@")) return null;
				OrderedSet<Match> uniqueMatches = new OrderedSet<Match>(Regex.Matches(command, @"@{1}\w*").Cast<Match>(), new Comparison<Match>(delegate(Match m1, Match m2) { return string.Compare(m1.Value, m2.Value, true); }));
				Match[] matches = uniqueMatches.ToArray();
				for (int i = 0; i < matches.Length; ++i) if (0 == string.Compare(parameterName, matches[i].Value, true)) return i;
				return null;
			}
      }

      [AttributeUsage(AttributeTargets.Class)]
      protected sealed class DatabaseRelationAttribute : System.Attribute {
			public static string PrimaryKeyName = "primary";
			public static string ForeignKeyName = "foreign";
         private string primaryKey = null;
         private string foreignKey = null;
         private bool primaryKeyInResult = true;
         private bool foreignKeyInResult = true;
         public DatabaseRelationAttribute(string primaryKey, string foreignKey) : base() {
            this.primaryKey = primaryKey;
            this.foreignKey = foreignKey;
         }
         public string PrimaryKey { get { return primaryKey; } }
         public string ForeignKey { get { return foreignKey; } }
         public bool PrimaryKeyInResult { get { return primaryKeyInResult; } set { primaryKeyInResult = value; } }
         public bool ForeignKeyInResult { get { return foreignKeyInResult; } set { foreignKeyInResult = value; } }
         public string this[string fieldName] { 
            get { 
					if (isPrimaryKeyName(fieldName)) return primaryKey;
					if (isForeignKeyName(fieldName)) return foreignKey;
					return null;
				}
            }
			internal static bool IsKeyField(FieldInfo fi) { return (isPrimaryKeyName(fi.Name) || isForeignKeyName(fi.Name)); }
			private static bool isPrimaryKeyName(string fieldName) { return fieldName.StartsWith(PrimaryKeyName, StringComparison.OrdinalIgnoreCase); }
			private static bool isForeignKeyName(string fieldName) { return fieldName.StartsWith(ForeignKeyName, StringComparison.OrdinalIgnoreCase); }
         }

		[AttributeUsage(AttributeTargets.Class)]
		protected sealed class DatabaseSelectAttribute : DatabaseCommandAttribute {
			public DatabaseSelectAttribute(CommandType commandType, string command) : base(commandType, command) { }
      }

      [AttributeUsage(AttributeTargets.Class)]
		protected sealed class DatabaseUpdateAttribute : DatabaseCommandAttribute {
			public DatabaseUpdateAttribute(CommandType commandType, string command) : base(commandType, command) { }
         }

		[AttributeUsage(AttributeTargets.Class)]
		protected class DatabaseCommandAttribute : System.Attribute {
			private CommandType commandType = CommandType.Text;
			private string command = null;
			public DatabaseCommandAttribute(CommandType commandType, string command) : base() { this.commandType = commandType; this.command = command; }
			public string Command { get { return command; } }
			public string StoredProcedure { get { return commandType == System.Data.CommandType.StoredProcedure ? command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0] : null; } }
			public CommandType CommandType { get { return commandType; } }
      }

		protected class _Map {
         private List<FieldInfo> fields = new List<FieldInfo>();
			private List<ReadMappping> readMap = null;
         private List<WriteMapping> writeMap = null;
			private int readParameterCount = -1;
			private int writeParameterCount = -1;
         private Type type = null;
         private DatabaseRelationAttribute databaseRelation = null;
			public _Map(Type type) {
            this.type = type;
				databaseRelation = (DatabaseRelationAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseRelationAttribute)) ?? databaseRelation;
            readFields();
         }

         private void readFields() {
				if (null != databaseRelation) {
					if (databaseRelation.PrimaryKeyInResult && !string.IsNullOrEmpty(databaseRelation.PrimaryKey)) fields.Add((FieldInfo)type.BaseType.GetField("primaryKey", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic));
					if (databaseRelation.ForeignKeyInResult && !string.IsNullOrEmpty(databaseRelation.ForeignKey)) fields.Add((FieldInfo)type.BaseType.GetField("foreignKey", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic));
				}
            foreach (FieldInfo fi in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic)) {
               DatabaseFieldAttribute dfa = (DatabaseFieldAttribute)Attribute.GetCustomAttribute(fi, typeof(DatabaseFieldAttribute));
					if (dfa != null) fields.Add((FieldInfo)fi);
            }
         }
			private List<ReadMappping> getReadMap(IDataReader dr) {
				List<ReadMappping> map = new List<ReadMappping>();
            foreach (FieldInfo fi in fields) {
					DatabaseFieldAttribute dfa = DatabaseFieldAttribute.GetDatabaseFieldAttribute(databaseRelation, fi, dr);
					map.Add(new ReadMappping(dfa.ColumnName, dfa.ColumnType, dfa.ParameterSelectOrdinal, fi, dfa.IsKeyField));
            }
            return map;
         }
			private List<WriteMapping> getWriteMap() {
            List<WriteMapping> map = new List<WriteMapping>();
            foreach (FieldInfo fi in fields) {
					DatabaseFieldAttribute dfa = DatabaseFieldAttribute.GetDatabaseFieldAttribute(databaseRelation, fi, type);
					map.Add(new WriteMapping(dfa.ParameterName, dfa.ColumnType, dfa.ParameterUpdateOrdinal, fi, dfa.IsKeyField));
            }
            return map;
         }
			private int paramterCount(List<Mapping> mappings) {
				int parameterCount = 0;
				foreach (Mapping m in mappings) if (null != m.Ordinal) ++parameterCount;
				return parameterCount;
			} 
			public int FieldCount { get { return fields.Count; } }
			public int WriteParameterCount { 
				get { 
					if (writeParameterCount < 0) writeParameterCount = paramterCount(WriteMap().ConvertAll<Mapping>(new Converter<WriteMapping, Mapping>(delegate(WriteMapping rm) { return (Mapping)rm; })));
					return writeParameterCount;
         }
         }
			internal List<ReadMappping> ReadMap(IDataReader dr) { if (null == readMap) readMap = getReadMap(dr); return readMap; }
			internal List<WriteMapping> WriteMap() { if (null == writeMap) writeMap = getWriteMap();	return writeMap; }
      }

      protected static class Maps {
			private static Dictionary<string, _Map> maps = new Dictionary<string, _Map>();
			internal static List<ReadMappping> Map(Type type, IDataReader dr) {
				if (!maps.ContainsKey(type.Name)) maps[type.Name] = new _Map(type);
            return maps[type.Name].ReadMap(dr);
         }
			internal static List<WriteMapping> Map(Type type) {
				if (!maps.ContainsKey(type.Name)) maps[type.Name] = new _Map(type);
				return maps[type.Name].WriteMap();
         }
			internal static int Count(Type type) {
				if (!maps.ContainsKey(type.Name)) maps[type.Name] = new _Map(type);
				return maps[type.Name].FieldCount;
			}
			internal static int WriteParameterCount(Type type) {
				if (!maps.ContainsKey(type.Name)) maps[type.Name] = new _Map(type);
				return maps[type.Name].WriteParameterCount;
         }
      }

		private static string selectQueryPrimaryKey(Type type, DatabaseKey key) { 
         DatabaseRelationAttribute dra = (DatabaseRelationAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseRelationAttribute));
			return selectQuery(type, dra != null ? dra.PrimaryKey : string.Empty, key);
      }
		private static string selectQueryForeignKey(Type type, DatabaseKey key) {
         DatabaseRelationAttribute dra = (DatabaseRelationAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseRelationAttribute));
			return selectQuery(type, dra != null ? dra.ForeignKey : string.Empty, key);
      } 
		private static string selectQuery(Type type, string keyField, DatabaseKey key) {
			DatabaseCommandAttribute da = (DatabaseCommandAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseSelectAttribute));
			string selectSQL = string.Format("SELECT * FROM ({0}) subQuery {1}", da.Command, (string.IsNullOrEmpty(keyField) || key == null) ? string.Empty : string.Format(" WHERE subQuery.{0} = {1}", keyField, key.KeyValue.ToString()));
         DAI.Log.Debug(selectSQL);
         return selectSQL;
      }
		private static string writeQuery(Type type) {
			DatabaseCommandAttribute da = (DatabaseCommandAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseUpdateAttribute));
			if (string.IsNullOrEmpty(da.Command)) throw new NullReferenceException(string.Format("No UDI stored procedure defined for type : {0}", type.Name));
			DAI.Log.Debug(da.Command);
			return da.Command;
      }

      private static DatabaseScalarAttribute scalarAttributes(Type type, string scalarFieldName) {
			FieldInfo fieldInfo = (FieldInfo)type.GetField(scalarFieldName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
         DatabaseScalarAttribute dsa = (DatabaseScalarAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DatabaseScalarAttribute));
			dsa.DatabaseFieldInfoCache = fieldInfo;
         return dsa;
      }
		private static DatabaseScalarAttribute readQuery(Type type, string scalarFieldName) {
         DatabaseScalarAttribute dsa = scalarAttributes(type, scalarFieldName);
			if (string.IsNullOrEmpty(dsa.SelectProcedure)) throw new NullReferenceException(string.Format("No stored procedure defined for scalar : {0}", dsa.DatabaseFieldInfoCache.Name));
         DAI.Log.DebugFormat("{0} {1}", dsa.SelectProcedure, dsa.Key);
         return dsa;
      }
		private static DatabaseScalarAttribute writeQuery(Type type, string scalarFieldName) {
         DatabaseScalarAttribute dsa = scalarAttributes(type, scalarFieldName);
			if (string.IsNullOrEmpty(dsa.UpdateProcedure)) throw new NullReferenceException(string.Format("No UDI stored procedure defined for scalar : {0}", dsa.DatabaseFieldInfoCache.Name));
         DAI.Log.DebugFormat("{0} {1}", dsa.SelectProcedure, dsa.Key);
         return dsa;
      }

      private static T readObject<T>(Type type, IDataReader dr) where T : class, new() {
         T instanceT = null;
			List<ReadMappping> map = Maps.Map(type, dr);
         if (dr.Read()) instanceT = createObject<T>(dr, map);
         return instanceT;
      }

      private static C readObjects<T, C, I>(Type type, IDataReader dr) where T : class, I, new() where C : ICollection<I>, new() {
         C collection = new C();
			List<ReadMappping> map = Maps.Map(type, dr);
         while (dr.Read()) {
            T instanceT = createObject<T>(dr, map);
            collection.Add((I)instanceT);
         }
         return collection;
      }
		private static object[] objectData(Type type, object instance) {
			List<WriteMapping> map = Maps.Map(type);
			List<object> data = new List<object>();
			foreach (WriteMapping wm in map) if (null != wm.ParameterOrdinal) data.Add(wm.FieldInfo.GetValue(instance)); 
			return data.ToArray();
      }
		private static DbCommand writeCommand(Type type, object instance) {
			DatabaseCommandAttribute da = (DatabaseCommandAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseUpdateAttribute));
			if (string.IsNullOrEmpty(da.Command)) throw new NullReferenceException(string.Format("No database write defined for type : {0}", type.Name));
			DbCommand dbCommand = null;
//			if (CommandType.Text == da.CommandType) dbCommand = mapParameters(type, database.GetSqlStringCommand(writeSqlPrimaryKey(type, da.Command)), instance);
			if (CommandType.Text == da.CommandType) {
				dbCommand = mapParameters(type, database.GetSqlStringCommand(writeSqlPrimaryKey(type, da.Command)), instance);
			} else dbCommand = database.GetStoredProcCommand(da.StoredProcedure, objectData(type, instance));
			DAI.Log.DebugFormat(dbCommand.CommandText);
			return dbCommand;
		}
		private static string writeSqlPrimaryKey(Type type, string command) {
			DatabaseRelationAttribute dra = (DatabaseRelationAttribute)Attribute.GetCustomAttribute(type, typeof(DatabaseRelationAttribute));
			return string.Format("{0}{1}", command, null == dra.PrimaryKey ? string.Empty : string.Format(" WHERE {0} = @{0}", dra.PrimaryKey));
		}
		private static DbCommand mapParameters(Type type, DbCommand command, object instance) {
			foreach (WriteMapping wm in Maps.Map(type)) if (wm.IsKey || wm.ParameterOrdinal != null) {
					object value = wm.FieldInfo.GetValue(instance);
					if (wm.FieldInfo.FieldType == typeof(DatabaseKey)) value = (int)(value as DatabaseKey); // Don't like this line at all
					command.Parameters.Add(new SqlParameter(wm.ParameterName, value));
				}
			return command;
		}
		private static DbCommand mapParameters(Type type, DbCommand command) {
			foreach (WriteMapping wm in Maps.Map(type)) if (wm.IsKey || wm.ParameterOrdinal != null) {
//					object value = wm.DatabaseFieldInfo.GetValue(instance);
//					if (wm.DatabaseFieldInfo.FieldType == typeof(DatabaseKey)) value = (int)(value as DatabaseKey); // Don't like this line at all
					command.Parameters.Add(new SqlParameter(wm.ParameterName, wm.ParameterType));
				}
			return command;
		}
		internal protected DatabaseStore() { }
		internal DatabaseStore(DatabaseRelation dr) { this.primaryKey = dr.PrimaryKey; this.foreignKey = dr.ForeignKey; }
      protected delegate T ReadDelegate<T>();
      protected static T ReadStoredObject<T>(T obj, ReadDelegate<T> readDelegate) {
         if (null == obj) obj = readDelegate();
         return obj;
      }

      public void Read(string scalarFieldName) {
			DatabaseScalarAttribute dai = readQuery(GetType(), scalarFieldName);
         object o = db.ExecuteScalar(db.GetStoredProcCommand(dai.SelectProcedure, dai.Key));
			setFieldValue(this, dai.DatabaseFieldInfoCache, o);
       }
		public T Read<T>(object[] parameterValues = null) where T : class, new() {
			using (IDataReader idr = db.ExecuteReader(db.GetSqlStringCommand(selectQueryPrimaryKey(typeof(T), primaryKey)))) {
            return readObject<T>(typeof(T), idr);
         }
      }
		public C Read<T, C, I>(object[] parameterValues = null) where T : class, I, new() where C : ICollection<I>, new() {
			using (IDataReader idr = db.ExecuteReader(db.GetSqlStringCommand(selectQueryForeignKey(typeof(T), primaryKey)))) {
            return readObjects<T, C, I>(typeof(T), idr); 
         }
      }

      public void Write(string scalarFieldName) {
			DatabaseScalarAttribute dai = writeQuery(GetType(), scalarFieldName);
         DbCommand dbCommand = db.GetStoredProcCommand(dai.UpdateProcedure, dai.UpdateParameters(null));
         if (null != dbCommand) db.ExecuteNonQuery(dbCommand.CommandText, dai.UpdateParameters(this));
      }

      public void Write() {
			DbCommand dbCommand = writeCommand(this.GetType(), this);
			if (null != dbCommand) db.ExecuteNonQuery(dbCommand);
      }
   }
}