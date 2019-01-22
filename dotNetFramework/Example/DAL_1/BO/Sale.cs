using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO {
   using DAL;

   public interface ISale {
      string NEDRecordDate { get; }
      string SaleDate { get; }
      string SoldDate { get; }
   }

   public interface ISales : IEnumerable<ISale> { }

   class Sales : FindableSortableBindingList<ISale>, ISales { }

   [DatabindPrimaryKey("ID")]
   [DatabindForeignKey("PropertyID")]
   //	[DatabindRead(System.Data.CommandType.Text, "SELECT ID, PropertyID, NEDRecordedDate AS [NEDRecordDate], OriginalSaleDate AS [SaleDate], DateSold AS [SoldDate] FROM [DOM].Sale")]
   [DatabindRead(System.Data.CommandType.StoredProcedure, "[dal.select].Sale @propertyId")]
   class Sale : DatabaseStore, ISale {
      //		[DatabindField("NEDRecordDate", System.Data.SqlDbType.SmallDateTime, "1/1/0001")] // make sure your 'on dbNull' value passed here is convertible to your destination data type
      [DatabindField("NEDRecordedDate", System.Data.SqlDbType.SmallDateTime, "1/1/0001")] // make sure your 'on dbNull' value passed here is convertible to your destination data type
      private DateTime nedRecordDate = DateTime.MaxValue;
      //		[DatabindField("SaleDate", System.Data.SqlDbType.SmallDateTime, "1/1/0001")]
      [DatabindField("OriginalSaleDate", System.Data.SqlDbType.SmallDateTime, "1/1/0001")]
      private DateTime saleDate = DateTime.MaxValue;
      //      [DatabindField("SoldDate", System.Data.SqlDbType.SmallDateTime, "No Sale")] // Note the ToString() type conversion here!
      [DatabindField("DateSold", System.Data.SqlDbType.SmallDateTime, "No Sale")] // Note the ToString() type conversion here!
      private string soldDate = string.Empty;

      public string NEDRecordDate { get { return nedRecordDate.ToShortDateString(); } }
      public string SaleDate { get { return saleDate.ToShortDateString(); } }
      public string SoldDate { get { return soldDate; } }
   }
}
