using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO {
   using DAL;
   public partial class BOL : DAI {
      public BOL(string connectionString) : base(connectionString) { }
      public static new BOL DA { get { return (BOL)DAI.DA; } }
      public override void Initialize() {
         // create a top-level DAI objects, there shouldn't be too many.
         // note '1' is the ID of the record we're interested in in 'app.Application' There should only be one record in this case
         // hard-coding the id ensures only one record is returned.
         // Better to constrain the table to only record, and remove the hard coded ID
         // Ultimately we make the whole issue go away and convert this configuration data to name-value pairs
         //settings = new DatabaseStore(new DatabaseRelation(1)).Read<BO.Settings>();
         //settings = new BO.Settings();
         foreclosureSales = new DatabaseStore().Read<ForeclosureSale, ForeclosureSales, IForeclosureSaleHeader>();
         configuration = new BO.Configuration();
      }
   }

}
