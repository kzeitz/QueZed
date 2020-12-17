using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.IO {
	using System.IO;
	using System.Net.Http;
	using Microsoft.WindowsAzure.Storage;
	using Microsoft.WindowsAzure.Storage.Blob;

	/// <summary>
	/// A container name must be a valid DNS name, conforming to the following naming rules:
	///	Container names must start with a letter or number, and can contain only letters, numbers, and the dash (-) character.
	///	Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.
	///	All letters in a container name must be lowercase.
	///	Container names must be from 3 through 63 characters long.
	/// </summary>
	public class AzureStorageBlobContainer : AzureStorage {

		public static CloudBlobContainer BlobContainer(string containerName, bool createIfNotExists = true) {
			CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower()); // note: container name must use lower case
			if (createIfNotExists) container.CreateIfNotExists();
			BlobContainerPermissions permissions = container.GetPermissions();

			// very likely this isn't want we want ultimately, ok for now I suppose
			if (permissions.PublicAccess == BlobContainerPublicAccessType.Off) {
				permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
				container.SetPermissions(permissions);
			}
			return container;
		}

		public static List<CloudBlobContainer> BlobContainers(string prefix = null) { return new List<CloudBlobContainer>(blobClient.ListContainers()); }

	}
}
