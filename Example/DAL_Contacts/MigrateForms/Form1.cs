using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MigrateForms {
    using ContactsMigration;
    public partial class Form1 : Form {
        private Migrate migrate = null;
        public Form1() {
            InitializeComponent();
            migrate = new Migrate(new ListboxOutput(listBox1));
        }

        private void btMigrate_Click(object sender, EventArgs e) {
            migrate.Run(Program.DefaultConnectionString, null);
        }

        private void btClean_Click(object sender, EventArgs e) {
            migrate.Run(Program.DefaultConnectionString, "clean");
        }
    }
    class ListboxOutput : ContactsMigration.ICConsole {
        ListBox listBox = null;
        public ListboxOutput(ListBox listBox) { this.listBox = listBox; }
        public void WriteLineBlank(int lines = 1, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
            listBox.Items.Add(string.Empty);
        }
        public void Write(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
            writeMessage(text);
        }
        public void WriteLine(string line, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
            writeMessage(line);
        }
        public void WriteLineBicolor(string format, params object[] args) {
            writeMessage(string.Format(format, args));
        }
        public void WriteLineBicolor(ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, string format, params object[] args) {
            writeMessage(string.Format(format, args));
        }
        public void WriteLineBicolor(ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, ConsoleColor headingBackgroundColor, ConsoleColor valueBackgroundColor, string format, params object[] args) {
            writeMessage(string.Format(format, args));
        }
        public string ReadLine(string prompt, bool intercept = false, char echo = '*') {
            throw new NotImplementedException();
        }
        public string ReadLine(string prompt, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black, bool intercept = false, char echo = '*') {
            throw new NotImplementedException();
        }
        private void writeMessage(string message) {
            listBox.Items.Add(message);
        }
    }

}
