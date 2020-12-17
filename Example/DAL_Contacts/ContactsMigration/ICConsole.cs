using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsMigration{
    public interface ICConsole {
        void WriteLineBlank(int lines = 1, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black);
        void Write(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black);
        void WriteLine(string line, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black);
        void WriteLineBicolor(string format, params object[] args);
        void WriteLineBicolor(ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, string format, params object[] args);
        void WriteLineBicolor(ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, ConsoleColor headingBackgroundColor, ConsoleColor valueBackgroundColor, string format, params object[] args);
        string ReadLine(string prompt, bool intercept = false, char echo = '*');
        string ReadLine(string prompt, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black, bool intercept = false, char echo = '*');
    }
}
