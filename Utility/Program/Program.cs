﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karlton.Utility.Program {
   using System.Diagnostics;
   using System.Reflection;
   using System.IO;
   using System.Configuration;
   using System.Security.Principal;
   using log4net;
   using log4net.Repository.Hierarchy;
   using log4net.Appender;

   public abstract class Program {
      private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      private static string applicationPath = Assembly.GetExecutingAssembly().Location;

      private static KeyValueOriginCollection settings = new KeyValueOriginCollection();
      private static ConnectionStringSettingsCollection connectionStrings = null;
      private static FileSystemWatcher configurationFileWatcher = null;

      public static string ApplicationPath { get { return applicationPath; } }
      public static string ApplicationDir { get { return Path.GetDirectoryName(applicationPath); } }
      public static KeyValueOriginCollection Settings { get { return settings; } }
      public static string DefaultConnectionString {
         get {
            const string key = "UseConnection";
            const string defaultKey = "Default";
            string connectionString = string.Empty;
            if (settings.Exists(defaultKey)) connectionString = settings[defaultKey];
            if (settings.Exists(key)) {
               // UseConnection overrides "Default"
               string useConnection = settings[key];
               // containing an '=' sign likely means its the connection string itself
               if (useConnection.Contains("=")) connectionString = useConnection;
               // otherwise see if 'useConnection' is a key and use that connecetionString
               else if (settings.Exists(useConnection)) connectionString = settings[useConnection];
            }
            if (string.IsNullOrEmpty(connectionString) && (connectionStrings != null || connectionStrings.Count > 0)) connectionString = connectionStrings[0].ConnectionString;
            return connectionString;
         }
      }

      protected abstract void main(string[] args);
		protected virtual void configurationFile_Changed(object sender, FileSystemEventArgs e) { log.Info("Configuration file changed"); }

		public void Run(string[] args) {
         AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
			ConfigureLog();
			ConfigureApplication(args);
			configurationFileWatcher = configureFileWatcher(new FileSystemEventHandler(configurationFile_Changed));
			configurationFileWatcher.EnableRaisingEvents = true;
			log.Info("Main start");
			try { main(args); } catch (Exception e) { log.Fatal(e); }
			log.Info("Main stop");
		}

      protected virtual void ConfigureLog() {
         string sourceConfiguration = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
         if (System.Configuration.ConfigurationManager.GetSection("log4net") != null) log4net.Config.XmlConfigurator.Configure();
         else {
            sourceConfiguration = "Properties.Resources.DefaultLog4netXML";
            using (Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(Properties.Resources.DefaultLog4netXML))) {
               if (stream != null) log4net.Config.XmlConfigurator.Configure(stream);
               else {
                  sourceConfiguration = "None, default logging to Console.";
                  log4net.Config.BasicConfigurator.Configure();
               }
            }
         }
         log.InfoFormat("Logging initialized. Source configuration : {0}", sourceConfiguration);
         FileAppender rootAppender = ((Hierarchy)LogManager.GetRepository()).Root.Appenders.OfType<FileAppender>().FirstOrDefault();
         if (null != rootAppender) sourceConfiguration = string.Format("Log file path: {0}", rootAppender.File);
      }

      protected virtual void ConfigureApplication(string[] args) {
         // command line overrides configuration file
         // -dev DeveloperMode - Login database combo box to allow selection or not
         // -use "key or connection string" UseConnection - Works one of three ways
         // 1.  If omitted, ConfigurationManager.ConnectionStrings[Default] or ConfigurationManager.ConnectionStrings[0] is used (in that order)
         // 2.  Specifies the connection string to use
         // 3.  Specifies the key as in ConfigurationManager.ConnectionStrings[key] to use
         log.Info("Process application configuration file.");
         settings.Add(ConfigurationManager.AppSettings, KeyValueOrigin.Origins.AppConfig);
         connectionStrings = ConfigurationManager.ConnectionStrings;
         foreach (ConnectionStringSettings css in connectionStrings) {
            KeyValueOrigin.Origins origin = KeyValueOrigin.Origins.AppConfig;
            // Find a better way to determine if a setting was inherrited from MACHINE.config
            if (0 == string.Compare(css.Name, "LocalSqlServer", StringComparison.Ordinal)) origin = KeyValueOrigin.Origins.MachineConfig;
            settings.Add(css.Name, css.ConnectionString, origin);
         }
         log.Info("Process command line.");
         for (int i = 0; i < args.Length; ++i) {
            string s = args[i];
            if (s.StartsWith("-dev", StringComparison.OrdinalIgnoreCase)) settings.Add("DeveloperMode", "true", KeyValueOrigin.Origins.CommandLine);
            if (s.StartsWith("-use", StringComparison.OrdinalIgnoreCase) && args.Length >= (i + 2)) settings.Add("UseConnection", args[i + 1], KeyValueOrigin.Origins.CommandLine, true);
         }
         log.Info("Global application configuration settings:");
         foreach (KeyValueOrigin nvo in settings.Settings) log.InfoFormat("  {0, -15}{1, -25}{2}", nvo.Origin.ToString(), nvo.Key, nvo.Value);
      }

      private static FileSystemWatcher configureFileWatcher(FileSystemEventHandler onFileChanged) {
         System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         if (!config.HasFile) Trace.WriteLine("No configuration file exists.");
         string configFilePath = Path.GetDirectoryName(config.FilePath);
         string configFileName = Path.GetFileName(config.FilePath);
         FileSystemWatcher configFileWatcher = lastWriteFileSystemWatcher(configFilePath, configFileName, onFileChanged);
         configFileWatcher.EnableRaisingEvents = false;
         return configFileWatcher;
      }

      private static FileSystemWatcher lastWriteFileSystemWatcher(string path, string filter, FileSystemEventHandler onChanged) {
         FileSystemWatcher watcher = new FileSystemWatcher(path, filter);
         watcher.NotifyFilter = NotifyFilters.LastWrite;
         watcher.Changed += onChanged;
         return watcher;
      }

   }
}
