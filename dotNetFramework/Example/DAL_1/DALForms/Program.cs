using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DALForms {
   using BO; // Business Objects

   class Program : Lukava.Utility.Program.Program {
      //   /// <summary>
      //   /// The main entry point for the application.
      //   /// </summary>
      [STAThread]
      static void Main(string[] args) { new Program().Run(args); }
      protected override void main(string[] args) {
         BOL bol = new BOL(Program.DefaultConnectionString);
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         Application.Run(new Form1());
      }
   }
}
