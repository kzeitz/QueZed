using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.IO {
	using System.IO;
	using System.Net.Http;
	using System.Web;
	using Microsoft.WindowsAzure.Storage.Blob;

	public class AzureStorageBlob : AzureStorageBlobContainer {

		CloudBlobContainer container = null;
		public AzureStorageBlob(string containerName) { this.container = BlobContainer(containerName); }

		private static CloudBlockBlob blockBlob(AzureStorageUri asUri) {
			CloudBlobContainer container = blobClient.GetContainerReference(asUri.Container);
			CloudBlockBlob blob = container.GetBlockBlobReference(asUri.Path);
			blob.Properties.ContentType = MimeMapping.GetMimeMapping(asUri.Path);
			return blob; 
		}

		public static ICloudBlob Read(Uri uri) {
			AzureStorageUri asUri = new AzureStorageUri(uri);
			CloudBlockBlob blob = blockBlob(asUri);
			if (!blob.Exists()) throw new FileNotFoundException(asUri.Path);
			return blob;
		}

		public static void Delete(Uri uri) { blockBlob(new AzureStorageUri(uri)).Delete(); }
		public static Stream ReadStream(Uri uri) { return Read(uri).OpenRead(); }
		public static void WriteStream(Uri uri, Stream stream) { using (stream) { blockBlob(new AzureStorageUri(uri)).UploadFromStream(stream); }  }

		public List<IListBlobItem> Read(string prefix, bool useflatBlobListing) { return new List<IListBlobItem>(container.ListBlobs(prefix, useflatBlobListing)); }
		public AzureStorageMultipartFileStreamProvider StreamProvider(string blobNamePrefix) { return new AzureStorageMultipartFileStreamProvider(container, blobNamePrefix); }

		#region save

		public class StreamContentWithCompletion : StreamContent {
			public StreamContentWithCompletion(Stream stream) : base(stream) { }
			public StreamContentWithCompletion(Stream stream, Action<Exception> onComplete) : base(stream) { OnComplete = onComplete; }
			public Action<Exception> OnComplete { get; set; }
			protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context) {
				return base.SerializeToStreamAsync(stream, context).ContinueWith(
				task => {
					if (OnComplete != null) if (task.IsFaulted) OnComplete(task.Exception.GetBaseException()); else OnComplete(null);
				}, TaskContinuationOptions.ExecuteSynchronously);
			}
		}

		#endregion
	}

	public class AzureStorageMultipartFileStreamProvider : MultipartFileStreamProvider {
		private CloudBlobContainer container;
		private string blobNamePrefix = string.Empty;
		private List<ICloudBlob> blobs = new List<ICloudBlob>();
		public AzureStorageMultipartFileStreamProvider(CloudBlobContainer container, string blobNamePrefix) : base(Path.GetTempPath()) { this.container = container; this.blobNamePrefix = blobNamePrefix; }
		public List<ICloudBlob> Blobs { get { return blobs; } }
		public override Task ExecutePostProcessingAsync() {
			// Upload the files to azure blob storage and remove them from local disk
			foreach (MultipartFileData multipartFileData in FileData) {
				string blobName = Path.GetFileName(multipartFileData.Headers.ContentDisposition.FileName.Trim('"'));
				if (!string.IsNullOrWhiteSpace(blobNamePrefix)) blobName = string.Format("{0}/{1}", blobNamePrefix, blobName);
				// Retrieve reference to a blob
				ICloudBlob blob = container.GetBlockBlobReference(blobName);
				blob.Properties.ContentType = multipartFileData.Headers.ContentType.MediaType;
				using (FileStream fs = File.OpenRead(multipartFileData.LocalFileName)) { blob.UploadFromStream(fs); }
				File.Delete(multipartFileData.LocalFileName);
				Blobs.Add(blob);
			}
			return base.ExecutePostProcessingAsync();
		}
	}

}
