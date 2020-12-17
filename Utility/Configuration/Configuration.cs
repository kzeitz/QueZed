using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueZed.Utility.Configuration {
	using System.Configuration;
	using Microsoft.WindowsAzure;
	using Microsoft.WindowsAzure.ServiceRuntime;

	// Classic indirection pattern here...
	// The issue is that your configuration settings come from different places depending on how the project is launched
	// If you run the service hosted in IIS Express you will get settings from web.config
	// If you run the service hosted in the Windows Azure Compute emulator you will get settings ServiceConfiguration.Local.cscfg by default overriding like settings in web.config
	// If you run the service hosted in the Windows Azure you will get settings ServiceConfiguration.cloud.cscfg by default overriding like settings in web.config

	// So we create an interface and class here that decides which method to retrieve settings with depending on the runtime environment.

	public interface IConfigurationProvider { string GetSetting(string name); }
	class CloudConfigurationProvider : IConfigurationProvider {
		public string GetSetting(string name) { return CloudConfigurationManager.GetSetting(name); }
	}
	class WebConfigurationProvider : IConfigurationProvider {
		public string GetSetting(string name) { return System.Configuration.ConfigurationManager.AppSettings[name]; }	
	}
	public class ConfigurationManager {
		private static IConfigurationProvider provider = new WebConfigurationProvider();
		static ConfigurationManager() {
			if (RoleEnvironment.IsAvailable) provider = new CloudConfigurationProvider();
		}
		public static string GetSetting(string name) { return provider.GetSetting(name); }
	}
}
