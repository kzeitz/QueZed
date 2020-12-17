using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.IO {
	using System.Reflection;
	using System.Linq.Expressions;
	using System.IO;
	using System.Net.Http;
	using Microsoft.WindowsAzure;
	using Microsoft.WindowsAzure.Storage;
	using Microsoft.WindowsAzure.Storage.Blob;
	using Microsoft.WindowsAzure.Storage.Table;
	using Microsoft.WindowsAzure.Storage.Queue;
	using QueZed.Utility.Configuration;

	public static partial class Extensions { }

	public class AzureStorageUri : Uri {
		private enum uriPart { Container, Path }
		private string[] parsed = null;
		public AzureStorageUri(Uri uri) : this(uri.ToString()) { }
		public AzureStorageUri(string uriString) : base(uriString) { parsed = base.LocalPath.Split(new char[] { '/' }, Enum.GetValues(typeof(uriPart)).Length, StringSplitOptions.RemoveEmptyEntries); }
		public string Container { get { return parsed[(int)uriPart.Container]; } }
		public string Path { get { return parsed[(int)uriPart.Path]; } }
	}

	public class AzureStorage {
		private static CloudStorageAccount storageAccount = null;
		public AzureStorage() { }
		protected static CloudBlobClient blobClient { get { return cloudStorageAccount().CreateCloudBlobClient(); } }
		protected static CloudTableClient tableClient { get { return cloudStorageAccount().CreateCloudTableClient(); } }
		protected static CloudQueueClient queueClient { get { return cloudStorageAccount().CreateCloudQueueClient(); } }
		// this method helps us call generic methods when we only have a Type object
		protected static MethodInfo getMethod<T>(Expression<Action<T>> expression) { return ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition(); }
		private static CloudStorageAccount cloudStorageAccount() {
			if (null == storageAccount) storageAccount = CloudStorageAccount.Parse(ConfigurationManager.GetSetting("CloudStorageConnectionString"));
			return storageAccount;
		}
	}

}
