using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace BO {
   using DAL;

   public partial class BOL : DAI {
      private ForeclosureSales foreclosureSales = null;
      public IForeclosureSales ForeclosureSales { get { return foreclosureSales; } }
      public IForeclosureSale CurrentForeclosureSale { get { return BO.ForeclosureSales.Current; } }
   }

   public interface IForeclosureSaleHeader {
      string ID { get; }
      string FileNumber { get; }
      string City { get; set; }
      bool Active { get; }
      DateTime LastEvaluation { get; }
   }

   public interface IForeclosureSale : IForeclosureSaleHeader {
      ISales Sales { get; }
   }

   public interface IForeclosureSales : IEnumerable<IForeclosureSaleHeader> {
      void Open(IForeclosureSaleHeader foreclosureSale);
      IForeclosureSaleHeader this[int index] { get; }
   }

   class ForeclosureSales : FindableSortableBindingList<IForeclosureSaleHeader>, IForeclosureSales {
      private static ForeclosureSale openForeclosure = null;
      public void Open(IForeclosureSaleHeader foreclosureSale) { openForeclosure = foreclosureSale as ForeclosureSale; }
      public static ForeclosureSale Current { get { return openForeclosure; } }
   }

   // For testing
   //IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dom.udi].[ForeclosureSalesTest]') AND type in (N'P', N'PC'))
   //DROP PROCEDURE [dom.udi].[ForeclosureSalesTest]
   //GO
   //CREATE PROCEDURE [dom.udi].[ForeclosureSalesTest]
   //   (@ID int = NULL, @PTFileName varchar(255) = NULL, @PropCity varchar(255) = NULL) AS 
   //SET NOCOUNT ON
   //IF @ID IS NULL BEGIN
   //   INSERT dom.Property ([PTFileName], [PropCity]) VALUES (@PTFileName, @PropCity)
   //   SELECT SCOPE_IDENTITY() 
   //END
   //ELSE BEGIN
   //   IF COALESCE(NULL, CONVERT(VARCHAR, @PTFileName), CONVERT(VARCHAR, @PropCity)) IS NOT NULL
   //      UPDATE dom.Property SET [PTFileName]=@PTFileName, [PropCity]=@PropCity WHERE ID = @ID
   //   ELSE DELETE dom.Property WHERE ID = @ID
   //   SELECT @@ROWCOUNT
   //END
   //GO

   //IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dom.select].[ForeclosureSalesTest]'))
   //DROP VIEW [dom.select].[ForeclosureSalesTest]
   //GO
   //CREATE VIEW [dom.select].[ForeclosureSalesTest]
   //AS
   //SELECT p.ID, p.PTFileName AS [FileNumber], propCity AS [City], ac.Active, ac.LastEvaluation 
   //FROM [dom].Property p 
   //INNER JOIN [dom.select.logic].ActiveCases() ac ON ac.PropertyID = p.ID
   //WHERE p.deleted = 0
   //GO

   //IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dal.select].[ForeclosureSalesTest]') AND type in (N'P', N'PC'))
   //DROP PROCEDURE [dal.select].[ForeclosureSalesTest]
   //GO
   //CREATE PROCEDURE [dal.select].[ForeclosureSalesTest] (
   //   @testParam	INT = NULL
   //) AS SET NOCOUNT ON BEGIN
   //   SELECT [ID], [FileNumber], [City], [Active], [LastEvaluation] FROM [dom.select].[ForeclosureSalesTest]
   //END
   //GO

   //IF OBJECT_ID('[dal.update].ForeclosureSalesTest', 'P') IS NOT NULL DROP PROCEDURE [dal.update].ForeclosureSalesTest
   //GO
   //CREATE PROCEDURE [dal.update].ForeclosureSalesTest (
   //   @id												INT = NULL,
   //   @ptFileName										VARCHAR(255) = NULL,
   //   @propCity										VARCHAR(255) = NULL
   //) AS SET NOCOUNT ON BEGIN
   //DECLARE @result TABLE (ReturnValue INT)
   //   INSERT @result EXEC [dom.udi].ForeclosureSalesTest @id, @ptFileName, @propCity
   //   IF @id IS NULL SELECT ReturnValue AS [ID] FROM @result
   //END
   //GO

   [DatabindRead(System.Data.CommandType.Text, @"SELECT p.ID, p.PTFileName AS [FileNumber], propCity AS [City], ac.Active, ac.LastEvaluation FROM [dom].Property p INNER JOIN [dom.select.logic].ActiveCases() ac ON ac.PropertyID = p.ID")]
   //	[DatabindRead(System.Data.CommandType.StoredProcedure, @"[dal.select].ForeclosureSalesTest  @id")]

   //	[DatabindWrite(System.Data.CommandType.Text, @"UPDATE [dom].Property SET PTFileName = @ptFileName, propCity = @propCity")]
   [DatabindWrite(System.Data.CommandType.StoredProcedure, @"[dal.update].ForeclosureSalesTest @id, @ptFileName, @propCity")]

   //	[DatabaseSelect(System.Data.CommandType.StoredProcedure, @"[dal.select].Property @propertyId")]
   //	[DatabaseUpdate(System.Data.CommandType.StoredProcedure, @"[dal.update].Property @id, @publicTrusteeId, @baseDateAffidavitPostingReceivedFromAttorneyId, @allPortion, @file_Number, @isCOP, @isCOR, @isFileInBK, @isIRSInvolved, @propAgricultural, @propCity, @propCounty, @propLegalDescription, @propState, @propStatus, @propStreet, @propZip, @scheduleNum, @pTFileName, @iDLawFirm, @propSub, @mostCurrentSaleDate, @propStreet2, @actualPendingSaleDate, @lastDateToRedeem, @chronoSort, @firstSaleDate, @forAttention, @forNotify, @aGCReceptionNumber, @aGCRecordedDate, @aGCReRecordedRecNum, @aGCReRecordedDate, @aGCReceivedDate, @prevailingStatute, @numberOfMines, @isNewCase, @defaultFees, @isTimeshare, @scrivenerRecordDate, @scrivenerReceptionNumber, @scrivenerVerbiage, @administrativeClosedDate, @initialEligibleForDeferment, @expeditedSale)]

   [DatabindPrimaryKey("ID")]
   class ForeclosureSale : DatabaseStore, IForeclosureSale {
      //[DatabindField("ID", System.Data.SqlDbType.Int, null, ParameterName = "@ID")]
      private string id = string.Empty;
      [DatabindField("FileNumber", System.Data.SqlDbType.VarChar, null, ParameterName = "@ptFileName")]
      private string fileNumber = string.Empty;
      [DatabindField("City", System.Data.SqlDbType.VarChar, null, ParameterName = "@propCity")]
      private string city = string.Empty;
      [DatabindField("Active", System.Data.SqlDbType.Bit, false)]
      private bool active = false;
      [DatabindField("LastEvaluation", System.Data.SqlDbType.SmallDateTime, "1/1/0001")]
      private DateTime lastEvaluation = DateTime.MinValue;

      private Sales sales = null;
      private Sales readSales() { return Read<Sale, Sales, ISale>(); }

      public string ID { get { return id; } }
      public string FileNumber { get { return fileNumber; } }
      public string City { get { return city; } set { city = value; Write<ForeclosureSale>(); } }
      public bool Active { get { return active; } }
      public DateTime LastEvaluation { get { return lastEvaluation; } }
      public ISales Sales { get { return ReadStoredObject(sales, new ReadDelegate<Sales>(readSales)); } }
   }
}
