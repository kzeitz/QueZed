using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsMigration {
    public class CConsole : Karlton.Utility.Console.CConsole, ICConsole { };
}

namespace Console {
    using ContactsMigration;

    class Program : Karlton.Utility.Program.Program {
        static void Main(string[] args) { new Program().Run(args); }
        protected override void main(string[] args)  {
            if (0 == args.Length) {
                System.Console.WriteLine("Press 'C' to clean, any other key runs migration.");
                if (ConsoleKey.C == System.Console.ReadKey().Key) args = new string[]{ "clean" };
            }
            Migrate migrate = new Migrate(new ContactsMigration.CConsole());
            migrate.Run(Program.DefaultConnectionString, args.Length > 0 ? args[0] : null);
            System.Console.WriteLine("Press any key...");
            System.Console.ReadKey();
        }
    }
}