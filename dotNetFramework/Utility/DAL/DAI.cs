using System;
using System.Collections.Generic;

namespace DAL {
	using System.Runtime.Serialization;
	using System.Security.Permissions;
	using System.Security.Principal;
	using System.Threading;
	using log4net;

	public interface IDataSource { }

	public interface IStorage {
		void Read(string key);
		T Read<T>(object[] parameterValues = null) where T : class, new();
		C Read<T, C, I>(object[] parameterValues = null) where T : class, I, new() where C : ICollection<I>, new();
		void Write(string key);
		//void Write(object[] parameterValues = null);
		void Write<T>();
	}

	public abstract class DAI : IDisposable {
		private object sync = new object();
		private static DAI dai = null;
		private IDataSource ds = null;
		private Connection connection = null;
		public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		[Serializable]
		public class DAIException : Exception, ISerializable {
			private void logException(string message, Exception ex) { Log.Error(message, ex); }
			public DAIException() : base() { logException(string.Empty, this); }
			public DAIException(string message) : base(message) { logException(message, this); }
			public DAIException(string message, Exception innerException) : base(message, innerException) { { logException(message, this); } }
			public DAIException(SerializationInfo info, StreamingContext context) : base(info, context) { }
			[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
			public override void GetObjectData(SerializationInfo info, StreamingContext context) { base.GetObjectData(info, context); }
		}

		// These are broken out so it's easy to make them virtual if need be
		private void attach(IDataSource source) { dai = this; ds = source; }
		private void connect(Security principal) { connection = new DatabaseStore().Read<Connection>(); connection.AssumePrincipal(principal); }

			// create a top-level DAI objects, there shouldn't be too many.
		public virtual void Initialize() { return; }

		public DAI(string connectionString, ApplicationSecurity principal = null) : this(new DataSourceEnterpriseLibraryConnectionString(connectionString), principal) { }
		public DAI(IDataSource source, ApplicationSecurity principal = null) {
			attach(source);
			connect(principal);
			Initialize();
			OnRaiseDAIChangedEvent(new DAIChangedEventArgs());
		}
		protected static DAI DA { get { if (null == dai) throw new NullReferenceException("Data access interface is null."); return dai; } }
		public static bool Connected { get { return dai != null; } }
		public IConnection Connection { get { return connection; } }

		public class DAIChangedEventArgs : EventArgs { }
		public static event EventHandler<DAIChangedEventArgs> DAIChangedEvent;
		protected static void OnRaiseDAIChangedEvent(DAIChangedEventArgs dcea) {
			EventHandler<DAIChangedEventArgs> handler = DAIChangedEvent;
			if (handler != null) { handler(null, dcea); }
		}

		~DAI() { Dispose(false); }
		public virtual void Close() { Dispose(); }
		public void Dispose() { GC.SuppressFinalize(this); Dispose(true); }
		private void Dispose(bool disposing) { lock (sync) { if (disposing) connection.RevertPrincipal(); } }
	}

	public interface IConnection {
		string ServerName { get; }
		string DatabaseName { get; }
		string HostName { get; }
		string UserName { get; }
		bool IsSecurityGroupPermission(string securityGroupName, string permissionName);
		bool IsWindowsUserGroup(string userName, string groupName);
	}

	[DatabindRead(System.Data.CommandType.Text, "SELECT @@SERVERNAME AS Server, DB_NAME() AS [Database], HOST_NAME() AS [Host], SUSER_NAME() AS [User]")]
	class Connection : DatabaseStore, IConnection {
		private IPrincipal originalPrincipal = null;

		[DatabindField("Server", System.Data.SqlDbType.NVarChar, null)]
		private string serverName = string.Empty;
		[DatabindField("Database", System.Data.SqlDbType.NVarChar, null)]
		private string databaseName = string.Empty;
		[DatabindField("Host", System.Data.SqlDbType.NVarChar, null)]
		private string hostName = string.Empty;
		[DatabindField("User", System.Data.SqlDbType.NVarChar, null)]
		private string userName = string.Empty;

		public Connection() : base() { }

		public string ServerName { get { return serverName; } }
		public string DatabaseName { get { return databaseName; } }
		public string HostName { get { return hostName; } }
		public string UserName { get { return userName; } }

		internal void AssumePrincipal(Security principal) {
			if (!(Thread.CurrentPrincipal is Security)) {
 				if (null == principal) principal = new Security(userName);
				IPrincipal currentPrincipal = Thread.CurrentPrincipal;
				Thread.CurrentPrincipal = principal;
				originalPrincipal = currentPrincipal;
			}
		}
		
		internal void RevertPrincipal() { if (null != originalPrincipal) Thread.CurrentPrincipal = originalPrincipal; }

		public bool IsSecurityGroupPermission(string roleName, string permissionName) {
			if (string.IsNullOrEmpty(roleName) && string.IsNullOrEmpty(permissionName)) throw new ArgumentNullException("roleName and permissionName", "Both arguments cannot be null.");
			bool isSecurityGroupPermission = false;
			if (Thread.CurrentPrincipal is Security) {
				Security rollPrincipal = Thread.CurrentPrincipal as Security;
				if (!string.IsNullOrEmpty(roleName)) isSecurityGroupPermission = rollPrincipal.IsInRole(roleName);
				if (Thread.CurrentPrincipal is ApplicationSecurity) {
					ApplicationSecurity permissionPrincipal = Thread.CurrentPrincipal as ApplicationSecurity;
					if (!string.IsNullOrEmpty(permissionName)) isSecurityGroupPermission = permissionPrincipal.HasPermission(permissionName);
				}
			}
			return isSecurityGroupPermission;
		}

		public bool IsWindowsUserGroup(string userName, string groupName) {
			if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(groupName)) throw new ArgumentNullException("userName and groupName", "Both arguments cannot be null.");
			bool isWindowsUserGroup = false;
			if (originalPrincipal is WindowsPrincipal) {
				WindowsPrincipal principal = originalPrincipal as WindowsPrincipal;
				if (!string.IsNullOrEmpty(userName)) isWindowsUserGroup = 0 == string.Compare(userName, principal.Identity.Name, true);
				if (!string.IsNullOrEmpty(groupName)) isWindowsUserGroup = principal.IsInRole(groupName);
			}
			return isWindowsUserGroup;
		}
	}
	
	class Identity : IIdentity {
		private string name = string.Empty;
		public Identity(string name) { this.name = name; }
		public bool IsAuthenticated { get { return true; } }
		public string AuthenticationType { get { return "Database"; } }
		public string Name { get { return name; } }
	}

	public class Security : DatabaseStore, IPrincipal {
		private Identity identity = null;
		public Security(string name)  : base(new DatabaseRelation(1)) { identity = new Identity(name); }
		public IIdentity Identity { get { return identity; } }
		public virtual bool IsInRole(string role) { return false; }
	}

	public class ApplicationSecurity : Security {
		public ApplicationSecurity(string name) : base(name) { }
		public virtual bool HasPermission(string permission) { return false; }
	}

}
