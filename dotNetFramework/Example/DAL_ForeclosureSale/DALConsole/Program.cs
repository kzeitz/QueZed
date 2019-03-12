using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALConsole {
   using BO; // Business Objects

   class Program : Lukava.Utility.Program.Program {
      static void Main(string[] args) { Console.SetBufferSize(Console.LargestWindowWidth, Console.BufferHeight); new Program().Run(args); }
      protected override void main(string[] args) {
         Console.WriteLine("Console EXE running. @ {0}", DateTime.Now.ToString());
         BOL bol = new BOL(DefaultConnectionString);

Console.WriteLine();
Console.WriteLine("-- All Foreclosures --");
foreach (IForeclosureSaleHeader fs in BOL.DA.ForeclosureSales) Console.WriteLine(string.Format("ID:{0, -20} FileName:{1}", fs.ID, fs.FileNumber));
Console.WriteLine();
Console.WriteLine("-- Open a Foreclosure [1000] --");
BOL.DA.ForeclosureSales.Open(BOL.DA.ForeclosureSales[1000]);
Console.WriteLine(string.Format("ID:{0, -20} FileName:{1} City:{2}", BOL.DA.CurrentForeclosureSale.ID, BOL.DA.CurrentForeclosureSale.FileNumber, BOL.DA.CurrentForeclosureSale.City));
Console.WriteLine();
Console.WriteLine("-- List sales for Foreclosure --");
foreach (ISale s in BOL.DA.CurrentForeclosureSale.Sales) Console.WriteLine(string.Format("NED Date:{0, -20} Sale Date:{1, -20} Sold Date:{2}", s.NEDRecordDate, s.SaleDate, s.SoldDate));
Console.WriteLine();
Console.WriteLine("-- Change the name of the City --");
BOL.DA.CurrentForeclosureSale.City = "Centennial";

         Console.WriteLine("Press Enter to Exit.");
         do { ConsoleKey key = Console.ReadKey().Key; if (ConsoleKey.Enter == key) break; } while (true);

      }

   }
}