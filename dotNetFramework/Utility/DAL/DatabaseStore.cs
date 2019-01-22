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

   public class DatabaseSource {

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
      //protected internal sealed class DatabaseKey : IConvertible {

      //protected internal sealed class DatabaseKey<T> : IConvertible {
      //   // ToDo : Support composite keys
      //   private string name = null;
      //   private SqlDbType sqlDbType = (SqlDbType)0;
      //   private T? _value = null;
      //   public DatabaseKey(string name, T keyValue = default(T)) { this.name = name; this.sqlDbType = dbTypesFromDotNetType[typeof(T)].Item1; _value = keyValue; }
      //   public DatabaseKey(DatabindAttribute attribute) : this(attribute != null ? attribute.ColumnName : null) { }
      //   public string Name { get { return name; } }
      //   public SqlDbType SqlDbType { get { return sqlDbType; } }
      //   public DbType DbType { get { try { return (new SqlParameter(string.Empty, SqlDbType)).DbType; } catch (Exception) { return DbType.Int32; } } }
      //   public T Value { get { return _value.HasValue ? _value.Value : default(T); } }
      //   public T? ValueNull { get { return _value; } }
      //   public string ParameterName { get { return string.Format("@{0}", name); } }
      //   public static DatabaseKey<T> Relate(DatabaseKey<T> fromTypeKey, DatabaseKey<T> toTypeKey) { return (fromTypeKey != null && toTypeKey != null) ? new DatabaseKey(toTypeKey.Name, fromTypeKey.SqlDbType, fromTypeKey.Value) : new DatabaseKey(null); }
      //   public static implicit operator T(DatabaseKey<T> key) { return key.Value; }
      //   public static implicit operator DatabaseKey<T>(T value) { return new DatabaseKey<T>(null, DBTypesFromDotNetType(typeof(T), value)); }
      //   #region IConvertable
      //   public TypeCode GetTypeCode() { return TypeCode.Object; }
      //   public bool ToBoolean(IFormatProvider provider) { return Convert.ToBoolean(_value); }
      //   public byte ToByte(IFormatProvider provider) { return Convert.ToByte(_value); }
      //   public char ToChar(IFormatProvider provider) { return Convert.ToChar(_value); }
      //   public DateTime ToDateTime(IFormatProvider provider) { return Convert.ToDateTime(_value); }
      //   public decimal ToDecimal(IFormatProvider provider) { return Convert.ToDecimal(_value); }
      //   public double ToDouble(IFormatProvider provider) { return Convert.ToDouble(_value); }
      //   public short ToInt16(IFormatProvider provider) { return Convert.ToInt16(_value); }
      //   public int ToInt32(IFormatProvider provider) { return _value; }
      //   public long ToInt64(IFormatProvider provider) { return Convert.ToInt64(_value); }
      //   public sbyte ToSByte(IFormatProvider provider) { return Convert.ToSByte(_value); }
      //   public float ToSingle(IFormatProvider provider) { return Convert.ToSingle(_value); }
      //   public string ToString(IFormatProvider provider) { return Convert.ToString(_value); }
      //   public object ToType(Type conversionType, IFormatProvider provider) { return Convert.ChangeType(_value, conversionType); }
      //   public ushort ToUInt16(IFormatProvider provider) { return Convert.ToUInt16(_value); }
      //   public uint ToUInt32(IFormatProvider provider) { return Convert.ToUInt32(_value); }
      //   public ulong ToUInt64(IFormatProvider provider) { return Convert.ToUInt64(_value); }
      //   #endregion
      //   // I don't see a need yet to support all the type yet, just the likely ones
      //   private static readonly Dictionary<Type, Tuple<SqlDbType, DbType>> dbTypesFromDotNetType = new Dictionary<Type, Tuple<SqlDbType, DbType>> {
      //      { typeof(int), new Tuple<SqlDbType, DbType>(SqlDbType.Int, DbType.Int32)},
      //      { typeof(long), new Tuple<SqlDbType, DbType>(SqlDbType.BigInt, DbType.Int64)},
      //      { typeof(Guid), new Tuple<SqlDbType, DbType>(SqlDbType.UniqueIdentifier, DbType.Guid)},
      //      { typeof(string), new Tuple<SqlDbType, DbType>(SqlDbType.VarChar, DbType.String)},
      //      { typeof(DateTime), new Tuple<SqlDbType, DbType>(SqlDbType.DateTime2, DbType.DateTime2)}
      //   };
      //}
      protected internal sealed class DatabaseKey : IConvertible {
         // ToDo : Support composite keys
         private string name = null;
         private SqlDbType sqlDbType = (SqlDbType)0;
         private DbType dbType = (DbType)0;
         private object _value = null;
         public DatabaseKey(string name, object keyValue = null) {
            this.name = name;
            if (null != keyValue) {
               Tuple<SqlDbType, DbType> dbTypes = dbTypesFromDotNetType[keyValue.GetType()];
               sqlDbType = dbTypes.Item1;
               dbType = dbTypes.Item2;
            }
            _value = keyValue;
         }
         public DatabaseKey(DatabindAttribute attribute) : this(attribute != null ? attribute.ColumnName : null) { }
         public string Name { get { return name; } }
         public SqlDbType SqlDbType { get { return sqlDbType; } }
         public DbType DbType { get { return dbType; } }
         public object Value { get { return _value; } }
         //public int? ValueNull { get { if (_value != -1) return _value; else return null; } }
         public string ParameterName { get { return string.Format("@{0}", name); } }
         public static DatabaseKey Relate(DatabaseKey fromTypeKey, DatabaseKey toTypeKey) { 
            return (fromTypeKey != null && toTypeKey != null) ? new DatabaseKey(toTypeKey.Name, fromTypeKey.Value) : new DatabaseKey(null); 
         }
         //public static implicit operator int(DatabaseKey key) { return key.Value; }
         //public static implicit operator DatabaseKey(int value) { return new DatabaseKey(null, SqlDbType.Int, value); }
         #region IConvertable
         public TypeCode GetTypeCode() { return TypeCode.Object; }
         public bool ToBoolean(IFormatProvider provider) { return Convert.ToBoolean(_value); }
         public byte ToByte(IFormatProvider provider) { return Convert.ToByte(_value); }
         public char ToChar(IFormatProvider provider) { return Convert.ToChar(_value); }
         public DateTime ToDateTime(IFormatProvider provider) { return Convert.ToDateTime(_value); }
         public decimal ToDecimal(IFormatProvider provider) { return Convert.ToDecimal(_value); }
         public double ToDouble(IFormatProvider provider) { return Convert.ToDouble(_value); }
         public short ToInt16(IFormatProvider provider) { return Convert.ToInt16(_value); }
         public int ToInt32(IFormatProvider provider) { return Convert.ToInt32(_value); }
         public long ToInt64(IFormatProvider provider) { return Convert.ToInt64(_value); }
         public sbyte ToSByte(IFormatProvider provider) { return Convert.ToSByte(_value); }
         public float ToSingle(IFormatProvider provider) { return Convert.ToSingle(_value); }
         public string ToString(IFormatProvider provider) { return Convert.ToString(_value); }
         public object ToType(Type conversionType, IFormatProvider provider) { return Convert.ChangeType(_value, conversionType); }
         public ushort ToUInt16(IFormatProvider provider) { return Convert.ToUInt16(_value); }
         public uint ToUInt32(IFormatProvider provider) { return Convert.ToUInt32(_value); }
         public ulong ToUInt64(IFormatProvider provider) { return Convert.ToUInt64(_value); }
         #endregion
         // I don't see a need yet to support all the type yet, just the likely ones
         private static readonly Dictionary<Type, Tuple<SqlDbType, DbType>> dbTypesFromDotNetType = new Dictionary<Type, Tuple<SqlDbType, DbType>> {
               { typeof(int), new Tuple<SqlDbType, DbType>(SqlDbType.Int, DbType.Int32)},
               { typeof(long), new Tuple<SqlDbType, DbType>(SqlDbType.BigInt, DbType.Int64)},
               { typeof(Guid), new Tuple<SqlDbType, DbType>(SqlDbType.UniqueIdentifier, DbType.Guid)},
               { typeof(string), new Tuple<SqlDbType, DbType>(SqlDbType.VarChar, DbType.String)},
               { typeof(DateTime), new Tuple<SqlDbType, DbType>(SqlDbType.DateTime2, DbType.DateTime2)}
            };
      }
      //protected internal sealed class DatabaseRelation<T> {
      //   private DatabaseKey<T> primaryKey = null;
      //   private DatabaseKey<T> foreignKey = null;
      //   internal DatabaseRelation(DatabaseKey<T> primaryKey, DatabaseKey<T> foreignKey) { this.primaryKey = primaryKey; this.foreignKey = foreignKey; }
      //   public DatabaseRelation(int primaryKey) : this(new DatabaseKey(null, SqlDbType.Int, primaryKey), null) { }
      //   internal DatabaseKey<T> PrimaryKey { get { return primaryKey; } }
      //   internal DatabaseKey<T> ForeignKey { get { return foreignKey; } }
      //}
      protected internal sealed class DatabaseRelation {
         private DatabaseKey primaryKey = null;
         private DatabaseKey foreignKey = null;
         internal DatabaseRelation(DatabaseKey primaryKey, DatabaseKey foreignKey) { this.primaryKey = primaryKey; this.foreignKey = foreignKey; }
          public DatabaseRelation(object primaryKey) : this(new DatabaseKey(null, primaryKey), null) { }
         internal DatabaseKey PrimaryKey { get { return primaryKey; } }
         internal DatabaseKey ForeignKey { get { return foreignKey; } }
      }
      protected internal sealed class AttributeFieldInfo {
         private DatabindAttribute fieldAttribute = null;
         private FieldInfo fieldInfo = null;
         private int? readOrdinal = null;
         private int? writeOrdinal = null;
         internal AttributeFieldInfo(DatabindAttribute fieldAttribute, FieldInfo fieldInfo, int? readOrdinal, int? writeOrdinal) {
            this.fieldAttribute = fieldAttribute;
            this.fieldInfo = fieldInfo;
            this.readOrdinal = readOrdinal;
            this.writeOrdinal = writeOrdinal;
         }
         internal DatabindAttribute FieldAttribute { get { return fieldAttribute; } }
         public FieldInfo FieldInfo { get { return fieldInfo; } }
         public int? ReadOrdinal { get { return readOrdinal; } set { readOrdinal = value; } }
         public int? WriteOrdinal { get { return writeOrdinal; } }
      }

      [AttributeUsage(AttributeTargets.Class)]
      protected internal sealed class DatabindPrimaryKeyAttribute : DatabindAttribute {
         public static string FieldName = "primaryKey";
         public DatabindPrimaryKeyAttribute(string columnName, SqlDbType columnType = SqlDbType.Int) : base(columnName, columnType, null, true) { }
      }
      [AttributeUsage(AttributeTargets.Class)]
      protected internal sealed class DatabindForeignKeyAttribute : DatabindAttribute {
         public static string FieldName = "foreignKey";
         public DatabindForeignKeyAttribute(string columnName, SqlDbType columnType = SqlDbType.Int) : base(columnName, columnType, null, true) { }
      }
      [AttributeUsage(AttributeTargets.Class)]
      protected internal sealed class DatabindReadAttribute : DatabindCommandAttribute {
         public DatabindReadAttribute(CommandType commandType, string command) : base(commandType, command) { }
      }
      [AttributeUsage(AttributeTargets.Class)]
      protected internal sealed class DatabindWriteAttribute : DatabindCommandAttribute {
         public DatabindWriteAttribute(CommandType commandType, string command) : base(commandType, command) { }
      }
      [AttributeUsage(AttributeTargets.Field)]
      protected internal sealed class DatabindFieldAttribute : DatabindAttribute {
         public DatabindFieldAttribute(string columnName, SqlDbType columnType, object columnNullValue) : base(columnName, columnType, columnNullValue) { }
      }
      [AttributeUsage(AttributeTargets.Field)]
      protected internal sealed class DatabindScalarAttribute : System.Attribute {
         private string selectProcedure = string.Empty;
         private object nullValue = null;
         private string key = null;
         private FieldInfo fieldInfoCache = null;
         public DatabindScalarAttribute(string selectProcedure, object dbNullValue, string key)
            : base() {
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

      protected internal class DatabindCommandAttribute : System.Attribute {
         private CommandType commandType = CommandType.Text;
         private string command = null;
         public DatabindCommandAttribute(CommandType commandType, string command) : base() { this.commandType = commandType; this.command = command; }
         public string Command { get { return command; } }
         public string StoredProcedure { get { return commandType == CommandType.StoredProcedure ? command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0] : null; } }
         public CommandType CommandType { get { return commandType; } }
      }
      protected internal class DatabindAttribute : System.Attribute {
         private string columnName = null;
         private SqlDbType columnType = SqlDbType.NVarChar;
         private object columnNullValue = null;
         private bool isColumnKey = false;
         private string parameterColumnName = null;
         public DatabindAttribute(string columnName, SqlDbType columnType, object columnNullValue, bool isColumnKey = false, string parameterName = null)
            : base() {
            this.columnName = columnName;
            this.columnType = columnType;
            this.columnNullValue = columnNullValue;
            this.isColumnKey = isColumnKey;
            this.parameterColumnName = string.IsNullOrEmpty(parameterName) ? columnName : parameterName.TrimStart('@');
         }
         public string ColumnName { get { return columnName; } }
         public SqlDbType ColumnType { get { return columnType; } }
         public object ColumnNullValue { get { return columnNullValue; } }
         public bool IsColumnKey { get { return isColumnKey; } set { isColumnKey = value; } }
         public string ParameterName { get { return string.Format("@{0}", parameterColumnName); } set { parameterColumnName = value.TrimStart('@'); } }
      }

      protected internal class Mapping {
         protected AttributeFieldInfo attributeFieldInfo = null;
         private object fieldValue = null;
         public Mapping(AttributeFieldInfo attributeFieldInfo) {
            this.attributeFieldInfo = attributeFieldInfo;
            this.fieldValue = attributeFieldInfo.FieldInfo.FieldType.IsValueType ? Activator.CreateInstance(attributeFieldInfo.FieldInfo.FieldType) : null;
         }
         protected int? ReadParameterOrdinal { get { return attributeFieldInfo.ReadOrdinal; } }
         protected int? WriteParameterOrdinal { get { return attributeFieldInfo.WriteOrdinal; } }
         public string ColumnName { get { return attributeFieldInfo.FieldAttribute.ColumnName; } }
         public bool IsColumnKey { get { return attributeFieldInfo.FieldAttribute.IsColumnKey; } }
         public bool IsInitOnly { get { return attributeFieldInfo.FieldInfo.IsInitOnly; } }
         public FieldInfo FieldInfo { get { return attributeFieldInfo.FieldInfo; } }
         public object FieldValue { get { return fieldValue; } }
      }
      protected internal sealed class ReadMapping : Mapping {
         public ReadMapping(AttributeFieldInfo attributeFieldInfo) : base(attributeFieldInfo) { }
         public int? Ordinal { get { return attributeFieldInfo.ReadOrdinal; } set { attributeFieldInfo.ReadOrdinal = value; } }
         public object NullValue { get { return attributeFieldInfo.FieldAttribute.ColumnNullValue; } }
      }
      protected internal sealed class WriteMapping : Mapping {
         public WriteMapping(AttributeFieldInfo attributeFieldInfo) : base(attributeFieldInfo) { }
         public int? Ordinal { get { return attributeFieldInfo.WriteOrdinal; } }
         public string ParameterName { get { return attributeFieldInfo.FieldAttribute.ParameterName; } }
      }
      protected internal class Map {
         // ToDo: Refactor with base class hide null checks
         private Type type = null;
         private DatabindCommandAttribute readAttribute = null;
         private DatabindCommandAttribute writeAttribute = null;
         private DatabindPrimaryKeyAttribute primaryKeyAttribute = null;
         private DatabindForeignKeyAttribute foreignKeyAttribute = null;
         // ToDo: Support for multiple foreign keys
         // private List<DatabindForeignKeyAttribute> foreignKeyAttributes = null;
         private List<AttributeFieldInfo> attributeFields = new List<AttributeFieldInfo>();
         private List<ReadMapping> readMappings = null;
         private List<WriteMapping> writeMappings = null;
         public Map(Type type) { this.type = type; read(); }
         public virtual void read() {
            readAttribute = (DatabindReadAttribute)Attribute.GetCustomAttribute(type, typeof(DatabindReadAttribute));
            writeAttribute = (DatabindWriteAttribute)Attribute.GetCustomAttribute(type, typeof(DatabindWriteAttribute));
            primaryKeyAttribute = (DatabindPrimaryKeyAttribute)Attribute.GetCustomAttribute(type, typeof(DatabindPrimaryKeyAttribute));
            foreignKeyAttribute = (DatabindForeignKeyAttribute)Attribute.GetCustomAttribute(type, typeof(DatabindForeignKeyAttribute));
            if (primaryKeyAttribute != null && !string.IsNullOrEmpty(primaryKeyAttribute.ColumnName)) attributeFields.Add(new AttributeFieldInfo(primaryKeyAttribute, (FieldInfo)type.BaseType.GetField(DatabindPrimaryKeyAttribute.FieldName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic), readOrdinal(primaryKeyAttribute.ParameterName), writeOrdinal(primaryKeyAttribute.ParameterName)));
            if (foreignKeyAttribute != null && !string.IsNullOrEmpty(foreignKeyAttribute.ColumnName)) attributeFields.Add(new AttributeFieldInfo(foreignKeyAttribute, (FieldInfo)type.BaseType.GetField(DatabindForeignKeyAttribute.FieldName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic), readOrdinal(primaryKeyAttribute.ParameterName), writeOrdinal(primaryKeyAttribute.ParameterName)));
            foreach (FieldInfo fi in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic)) {
               DatabindFieldAttribute dfa = (DatabindFieldAttribute)Attribute.GetCustomAttribute(fi, typeof(DatabindFieldAttribute));
               if (dfa != null) attributeFields.Add(new AttributeFieldInfo(dfa, (FieldInfo)fi, readOrdinal(dfa.ParameterName), writeOrdinal(dfa.ParameterName)));
            }
         }
         internal DatabaseKey PrimaryKey { get { return primaryKeyAttributeIsNull() ? null : new DatabaseKey(primaryKeyAttribute); } }
         internal DatabaseKey ForeignKey { get { return foreignKeyAttributeIsNull() ? null : new DatabaseKey(foreignKeyAttribute); } }
         public string ReadCommandText { get { return readAttributeIsNull() ? null : ReadCommandTextIsSql ? readAttribute.Command : readAttribute.StoredProcedure; } }
         public string WriteCommandText { get { return writeAttributeIsNull() ? null : WriteCommandTextIsSql ? writeAttribute.Command : writeAttribute.StoredProcedure; } }
         public bool ReadCommandTextIsSql { get { return readAttributeIsNull() ? false : readAttribute.CommandType == CommandType.Text; } }
         public bool ReadCommandTextIsStoredProcedure { get { return readAttributeIsNull() ? false : readAttribute.CommandType == CommandType.StoredProcedure; } }
         public bool WriteCommandTextIsSql { get { return writeAttributeIsNull() ? false : writeAttribute.CommandType == CommandType.Text; } }
         public bool WriteCommandTextIsStoredProcedure { get { return writeAttributeIsNull() ? false : writeAttribute.CommandType == CommandType.StoredProcedure; } }
         public string[] ReadUnboundParameterNames() { return unboundParameterNames(readCommandTextRaw); }
         public string[] WriteUnboundParameterNames() { return unboundParameterNames(writeCommandTextRaw); }
         public List<ReadMapping> ReadMap(IDataReader dr) { if (null == readMappings) readMappings = readMap(dr); return readMappings; }
         public List<WriteMapping> WriteMap() { if (null == writeMappings) writeMappings = writeMap(); return writeMappings; }
         private string readCommandTextRaw { get { return readAttributeIsNull() ? null : readAttribute.Command; } }
         private string writeCommandTextRaw { get { return writeAttributeIsNull() ? null : writeAttribute.Command; } }
         private List<ReadMapping> readMap(IDataReader dr) {
            List<ReadMapping> map = new List<ReadMapping>();
            foreach (AttributeFieldInfo afi in attributeFields) map.Add(new ReadMapping(afi));
            return map;
         }
         private List<WriteMapping> writeMap() {
            List<WriteMapping> map = new List<WriteMapping>();
            foreach (AttributeFieldInfo afi in attributeFields) map.Add(new WriteMapping(afi));
            return map;
         }
         private string[] unboundParameterNames(string commandText) {
            List<string> unbound = new List<string>();
            Match[] matches = uniqueParamterNames(commandText);
            foreach (Match match in matches) if (null == attributeFields.Find(delegate(AttributeFieldInfo afi) { return 0 == string.Compare(afi.FieldAttribute.ParameterName, match.Value); })) unbound.Add(match.Value);
            return unbound.ToArray();
         }
         private bool readAttributeIsNull() { return attributeIsNull(readAttribute); }
         private bool writeAttributeIsNull() { return attributeIsNull(writeAttribute); }
         private bool primaryKeyAttributeIsNull() { return attributeIsNull(primaryKeyAttribute); }
         private bool foreignKeyAttributeIsNull() { return attributeIsNull(foreignKeyAttribute); }
         private int? readOrdinal(string parameterName) { return (string.IsNullOrEmpty(parameterName) || string.IsNullOrEmpty(ReadCommandText)) ? null : findParameterOrdinal(readCommandTextRaw, parameterName); }
         private int? writeOrdinal(string parameterName) { return (string.IsNullOrEmpty(parameterName) || string.IsNullOrEmpty(WriteCommandText)) ? null : findParameterOrdinal(writeCommandTextRaw, parameterName); }
         private static bool attributeIsNull(Attribute attribute) { return null == attribute; }
         // ToDo : Handle when a parameter name is used more than once in the command text
         private static int? findParameterOrdinal(string commandText, string parameterName) {
            if (string.IsNullOrEmpty(commandText) || !commandText.Contains("@")) return null;
            Match[] matches = uniqueParamterNames(commandText);
            for (int i = 0; i < matches.Length; ++i) if (0 == string.Compare(parameterName, matches[i].Value, true)) return i;
            return null;
         }
         private static Match[] uniqueParamterNames(string commandText) { return new OrderedSet<Match>(Regex.Matches(commandText, @"@{1}(?<!@@)\w+").Cast<Match>(), new Comparison<Match>(delegate(Match m1, Match m2) { return string.Compare(m1.Value, m2.Value, true); })).ToArray(); }
      }

      protected internal static class Maps {
         private static Dictionary<string, Map> maps = new Dictionary<string, Map>();
         public static List<ReadMapping> ReadMap<T>(IDataReader dr) { string name = typeof(T).Name; if (!maps.ContainsKey(name)) maps[name] = new Map(typeof(T)); return maps[name].ReadMap(dr); }
         public static List<WriteMapping> WriteMap<T>() { string name = typeof(T).Name; if (!maps.ContainsKey(name)) maps[name] = new Map(typeof(T)); return maps[name].WriteMap(); }
         public static Map Map<T>() { string name = typeof(T).Name; if (!maps.ContainsKey(name)) maps[name] = new Map(typeof(T)); return maps[name]; }
      }
      protected internal static T createObject<T>(IDataReader dr, List<ReadMapping> readMapping) where T : class, new() {
         T instanceT = new T();
         foreach (ReadMapping rm in readMapping) {
            if (!rm.IsInitOnly) {
               object value = rm.FieldValue ?? rm.NullValue;
               if (null == rm.Ordinal) rm.Ordinal = dr.GetOrdinal(rm.ColumnName);
               if (!dr.IsDBNull((int)rm.Ordinal)) value = dr.GetValue((int)rm.Ordinal);
               setFieldValue(instanceT, rm.ColumnName, rm.FieldInfo, value);
            }
         }
         return instanceT;
      }
      protected internal static void setFieldValue(object instance, string fieldName, FieldInfo fi, object value) {
         Type type = fi.FieldType;
         if (type.Equals(typeof(DatabaseKey))) fi.SetValue(instance, new DatabaseKey(fieldName, value));
         else if (type.BaseType.Equals(typeof(System.Enum))) fi.SetValue(instance, System.Enum.ToObject(type, value));
         else fi.SetValue(instance, Convert.ChangeType(value, type));
      }
      protected internal static Database database = null;
      protected internal static Database db { get { if (null == database) throw new NullReferenceException("Database uninitialized."); else { database.CreateConnection(); return database; } } } // Hope the pooling is working
      protected internal DatabaseSource() { }
      protected internal DatabaseSource(Database database) { DatabaseSource.database = database; }
   }

   public class DatabaseStore : DatabaseSource, IStorage {
      //careful changing the names of these fields they are reflected on in Map.readFields()
      private DatabaseKey primaryKey = null;
      private DatabaseKey foreignKey = null;

      private static string readCommandSql(string commandText, string keyText) { return string.Format("SELECT * FROM ({0}) subQuery{1}", commandText, keyText != null ? string.Format(" WHERE subQuery.{0} = @{0}", keyText) : string.Empty); }
      private static string writeCommandSql(string commandText, string keyText) { return string.Format("{0}{1}", commandText, keyText != null ? string.Format(" WHERE {0} = @{0}", keyText) : string.Empty); }

      private static DbCommand readCommandType<T>(object[] parameterValues) {
         Map map = Maps.Map<T>();
         DatabaseKey typeKey = map.PrimaryKey;
         return readCommand(map.ReadCommandTextIsSql, map.ReadCommandText, typeKey, map.ReadUnboundParameterNames(), parameterValues);
      }
      private static DbCommand readCommandType<T>(string commandText, string[] parameterNames = null, object[] parameterValues = null) {
          return readCommand(true, commandText, null, parameterNames, parameterValues);
      }

      private static DbCommand readCommandRelatedType<T>(DatabaseKey fromTypeKey) {
         Map map = Maps.Map<T>();
         DatabaseKey toTypeKey = map.ForeignKey;
         return readCommand(map.ReadCommandTextIsSql, map.ReadCommandText, DatabaseKey.Relate(fromTypeKey, toTypeKey));
      }

      private static DbCommand readCommand(bool commandTextIsSql, string commandText, DatabaseKey key, string[] parameterNames = null, object[] parameterValues = null) {
         DbCommand command = null;
         if (commandTextIsSql) {
            command = (key != null && key.Value != null) ? db.GetSqlStringCommand(readCommandSql(commandText, key.Name)) : db.GetSqlStringCommand(commandText);
            if (null != key) db.AddParameter(command, key.ParameterName, key.DbType, ParameterDirection.Input, string.Empty, DataRowVersion.Default, key.Value);
            if (null != parameterValues) mapUnboundParameters(command, parameterNames, parameterValues);
         } else command = db.GetStoredProcCommand(commandText, new object[] { key != null ? key.Value : null });
         logCommand(command);
         return command;
      }

      private static DbCommand writeCommandType<T>(object instance) {
         Map map = Maps.Map<T>();
         DatabaseKey typeKey = map.PrimaryKey;
         if (string.IsNullOrEmpty(map.WriteCommandText)) throw new NullReferenceException(string.Format("No database write defined for type : {0}", typeof(T).Name));
         DbCommand command = null;
         if (map.WriteCommandTextIsSql) {
            command = db.GetSqlStringCommand(writeCommandSql(map.WriteCommandText, typeKey.Name));
            mapBoundParameters(map.WriteMap(), command, instance);
         } else command = db.GetStoredProcCommand(map.WriteCommandText, objectData(map.WriteMap(), instance));
         logCommand(command);
         return command;
      }

      private static void mapBoundParameters(List<WriteMapping> writeMap, DbCommand command, object instance) {
         foreach (WriteMapping wm in writeMap)
            if (wm.IsColumnKey || wm.Ordinal != null) {
               object value = wm.FieldInfo.GetValue(instance);
               //if (wm.FieldInfo.FieldType == typeof(DatabaseKey)) value = (int)(value as DatabaseKey); // Don't like this line at all
               command.Parameters.Add(new SqlParameter(wm.ParameterName, value));
            }
      }

      private static void mapUnboundParameters(DbCommand command, string[] parameterNames, object[] parameterValues) {
         if (parameterNames.Length != parameterValues.Length) throw new ArgumentException("Parameter name to parameter value mismatch.");
         for (int i = 0; i < parameterNames.Length; ++i) command.Parameters.Add(new SqlParameter(parameterNames[i], parameterValues[i]));
      }

      private static void logCommand(DbCommand command) { DAI.Log.DebugFormat("Command Text [{0}] Parameter Count [{1}]", command.CommandText, command.Parameters.Count); }

      private static object[] objectData(List<WriteMapping> writeMap, object instance) {
         List<object> data = new List<object>();
         foreach (WriteMapping wm in writeMap) if (null != wm.Ordinal) data.Add(wm.FieldInfo.GetValue(instance));
         return data.ToArray();
      }

      //      private static DatabindScalarAttribute scalarAttributes(Type type, string scalarFieldName) {
      //         FieldInfo fieldInfo = (FieldInfo)type.GetField(scalarFieldName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
      //         DatabindScalarAttribute dsa = (DatabindScalarAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DatabindScalarAttribute));
      //         dsa.DatabaseFieldInfoCache = fieldInfo;
      //         return dsa;
      //      }
      //      private static DatabindScalarAttribute readQuery(Type type, string scalarFieldName) {
      //         DatabindScalarAttribute dsa = scalarAttributes(type, scalarFieldName);
      //         if (string.IsNullOrEmpty(dsa.SelectProcedure)) throw new NullReferenceException(string.Format("No stored procedure defined for scalar : {0}", dsa.DatabaseFieldInfoCache.Name));
      //         DAI.Log.DebugFormat("{0} {1}", dsa.SelectProcedure, dsa.Key);
      //         return dsa;
      //      }
      //      private static DatabindScalarAttribute writeQuery(Type type, string scalarFieldName) {
      //         DatabindScalarAttribute dsa = scalarAttributes(type, scalarFieldName);
      //         if (string.IsNullOrEmpty(dsa.UpdateProcedure)) throw new NullReferenceException(string.Format("No UDI stored procedure defined for scalar : {0}", dsa.DatabaseFieldInfoCache.Name));
      //         DAI.Log.DebugFormat("{0} {1}", dsa.SelectProcedure, dsa.Key);
      //         return dsa;
      //      }

      private static T readObject<T>(IDataReader dr) where T : class, new() {
         T instanceT = null;
         List<ReadMapping> map = Maps.ReadMap<T>(dr);
         if (dr.Read()) instanceT = createObject<T>(dr, map);
         return instanceT;
      }
		private static C readObjects<T, C, I>(IDataReader dr)
         where T : class, I, new()
         where C : ICollection<I>, new() {
         C collection = new C();
         List<ReadMapping> map = Maps.ReadMap<T>(dr);
         while (dr.Read()) {
            T instanceT = createObject<T>(dr, map);
            collection.Add((I)instanceT);
         }
         return collection;
      }

      public DatabaseStore() { }
      protected internal DatabaseStore(DatabaseRelation dr) { this.primaryKey = dr.PrimaryKey; this.foreignKey = dr.ForeignKey; }
      protected internal delegate T ReadDelegate<T>();
      protected internal static T ReadStoredObject<T>(T obj, ReadDelegate<T> readDelegate) {
         if (null == obj) obj = readDelegate();
         return obj;
      }
      public void Read(string scalarFieldName) {
         //         DatabindScalarAttribute dai = readQuery(GetType(), scalarFieldName);
         //         object o = db.ExecuteScalar(db.GetStoredProcCommand(dai.SelectProcedure, dai.Key));
         //         setFieldValue(this, dai.DatabaseFieldInfoCache, o);
      }
      public C Read<T, C, I>(string commandText, string[] parameterNames = null, object[] parameterValues = null)
          where T : class, I, new()
          where C : ICollection<I>, new() {
        using (IDataReader idr = db.ExecuteReader(readCommandType<T>(commandText, parameterNames, parameterValues))) {
            return readObjects<T, C, I>(idr);
        }
      }

      public T Read<T>(object[] parameterValues = null) where T : class, new() {
         using (IDataReader idr = db.ExecuteReader(readCommandType<T>(parameterValues))) {
            return readObject<T>(idr);
         }
      }
      
       public C Read<T, C, I>(object[] parameterValues = null)
          where T : class, I, new()
          where C : ICollection<I>, new() {
          DatabaseKey key = primaryKey;
			using (IDataReader idr = db.ExecuteReader(readCommandRelatedType<T>(primaryKey))) {
              return readObjects<T, C, I>(idr);
			}
		}

       public C ReadRelated<T, C, I>(object keyValue = null)
         where T : class, I, new()
         where C : ICollection<I>, new() {
         DatabaseKey key = primaryKey;
         if (null != keyValue) key = new DatabaseKey(key.Name, keyValue);
         using (IDataReader idr = db.ExecuteReader(readCommandRelatedType<T>(key))) {
            return readObjects<T, C, I>(idr);
         }
      }
      public void Write(string scalarFieldName) {
         //         DatabindScalarAttribute dai = writeQuery(GetType(), scalarFieldName);
         //         DbCommand dbCommand = db.GetStoredProcCommand(dai.UpdateProcedure, dai.UpdateParameters(null));
         //         if (null != dbCommand) db.ExecuteNonQuery(dbCommand.CommandText, dai.UpdateParameters(this));
      }
      public void Write<T>() {
         DbCommand dbCommand = writeCommandType<T>(this);
         if (null != dbCommand) db.ExecuteNonQuery(dbCommand);
      }
   }
}