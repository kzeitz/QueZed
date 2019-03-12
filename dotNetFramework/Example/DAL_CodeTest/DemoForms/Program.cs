using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo {
   using BO;

   class Program : Karlton.Utility.Program.Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
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
