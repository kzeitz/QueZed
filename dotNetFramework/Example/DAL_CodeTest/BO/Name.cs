using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace BO {
   using DAL;

   public partial class BOL : DAI {
      private Names names = null;
      public Names Names { get { return names; } set { names = value; } }
   }

   public interface INames : IEnumerable<Name> {
      void Add(string firstName, string LastName);
   }

   public class Names : FindableSortableBindingList<Name>, INames {
      public void Add(string firstName, string lastName) {
         Name newName = new Name() { FirstName = firstName, LastName = lastName };
         base.Add(newName);
         newName.Write<Name>();
      }
      public void Update(object o) { ((Name)o).Write<Name>(); }
   }

   public interface IName {
      string FirstName { get; set; }
      string LastName { get; set; }
      string FullName { get; }
   }

   [DatabindPrimaryKey("ID")]
   [DatabindRead(System.Data.CommandType.Text, @"SELECT ID, FirstName, LastName FROM [dbo.select].[Name]")]
   [DatabindWrite(System.Data.CommandType.StoredProcedure, @"[dom.udi].[Name] @id, @firstName, @lastName")]
   public class Name : DatabaseStore, IName {
      [DatabindField("FirstName", System.Data.SqlDbType.VarChar, null, ParameterName = "@firstName")]
      private string firstName = string.Empty;
      [DatabindField("LastName", System.Data.SqlDbType.VarChar, null, ParameterName = "@lastName")]
      private string lastName = string.Empty;

      public string FirstName{ get { return firstName; } set { firstName = value; } }
      public string LastName { get { return lastName; } set { lastName = value; } }
      public string FullName { get { return string.Format("{0} {1}", firstName, LastName); } }
   }
}
