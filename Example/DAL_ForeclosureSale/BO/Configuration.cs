using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO {
   using log4net;
   using DAL;

   public partial class BOL : DAI {
      private Configuration configuration = null;
      public IConfiguration Configuration { get { return configuration; } }
   }

   public interface IConfiguration {
      //IActivatorSettings ActivatorSettings { get; }
      //Configuration.IAppSettings AppSettings { get; }
      //Configuration.ICalcSettings CalcSettings { get; }
      //Configuration.IDataSetting DataSettings { get; }
   }


   class Configuration : DatabaseStore, IConfiguration {
      private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//      private ActivatorSettings activatorSettings;
      public Configuration() {
//         ActivationConfigurationSection section = null;
//         // if we can't read the edits, or the file is still in use, just bail.
//         try {
//            section = (ActivationConfigurationSection)ConfigurationManager.GetSection(ActivationConfigurationSection.Name);
//         } catch (ConfigurationErrorsException ex) { log.Error(ex.GetType().Name, ex); }
//         ConfigurationManager.RefreshSection(section.SectionInformation.Name);
//         activatorSettings = new ActivatorSettings(section.Notification.Activators);
      }

      //private Configuration.CalcSettings calcSettings = new Configuration.CalcSettings();
      //private Configuration.DataSetting dataSettings = new Configuration.DataSetting();

//      public IActivatorSettings ActivatorSettings { get { return activatorSettings; } }
      //public DAL.Configuration.ICalcSettings CalcSettings { get { return calcSettings; } }
      //public DAL.Configuration.IDataSetting DataSettings { get { return dataSettings; } }
   }
}
