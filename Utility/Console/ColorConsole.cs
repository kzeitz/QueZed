using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueZed.Utility.Console {
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    // TODO wrap foreground and background and make them easier to use
    public static partial class Extensions {
        private static object writeGate = new object();
        private static object readGate = new object();

        public static void WriteLineBlank(this ColorConsole colorConsole, int lines = 1, ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black) {
            lock (writeGate) {
                foregroundBackground(fg, bg);
                Console.WriteLine(new string(char.MinValue, lines - 1).Replace(char.MinValue.ToString(), Environment.NewLine));
                Console.ResetColor();
            }
        }
        public static void Write(this ColorConsole colorConsole, string text, ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black) {
            lock (writeGate) {
                foregroundBackground(fg, bg);
                Console.Write(text);
                Console.ResetColor();
            }
        }
        public static void WriteLine(this ColorConsole colorConsole, string line, ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black) {
            lock (writeGate) {
                foregroundBackground(fg, bg);
                Console.WriteLine(line);
                Console.ResetColor();
            }
        }

        //i.e
        //console.WriteLineBicolor("Order : {0} Status : {1} User : {2} Document Count : {3}", "Name", "Status", "UserID", "DocumentCount");
        //console.WriteLineBicolor(ConsoleColor.DarkGreen, ConsoleColor.White, "Table storage update, table: {0}", tableName);

        /// <summary>
        /// Write console data in two colors
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Array of strings to substitute into the format string</param>
        public static void WriteLineBicolor(this ColorConsole colorConsole, string format, params string[] args) { lock (writeGate) { writeLineBicolor(colorConsole, format, args); } }
        public static void WriteLineBicolor(this ColorConsole colorConsole, ConsoleColor headingFg, ConsoleColor valueFg, string format, params string[] args) { lock (writeGate) { writeLineBicolor(colorConsole, format, args, headingFg, valueFg); } }
        public static void WriteLineBicolor(this ColorConsole colorConsole, ConsoleColor headingFg, ConsoleColor valueFg, ConsoleColor headingBg, ConsoleColor valueBg, string format, params string[] args) { lock (writeGate) { writeLineBicolor(colorConsole, format, args, headingFg, valueFg, headingBg, valueBg); } }
        public static string ReadLine(this ColorConsole colorConsole, string prompt, bool intercept = false, char echo = '*') { lock (readGate) { return ReadLine(colorConsole, prompt, ConsoleColor.Gray, ConsoleColor.Black, intercept, echo); } }
        public static string ReadLine(this ColorConsole colorConsole, string prompt, ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black, bool intercept = false, char echo = '*') {
            lock (readGate) {
                colorConsole.Write(prompt, fg, bg);
                if (!intercept) return Console.ReadLine();
                else {
                    int left = Console.CursorLeft;
                    StringBuilder sb = new StringBuilder();
                    ConsoleKeyInfo keyInfo;
                    while ((keyInfo = Console.ReadKey(intercept)).Key != ConsoleKey.Enter) {
                        if (ConsoleKey.LeftArrow == keyInfo.Key || ConsoleKey.RightArrow == keyInfo.Key) continue;
                        if (ConsoleKey.Backspace == keyInfo.Key) { if (Console.CursorLeft > left) { colorConsole.Write("\b \b"); if (sb.Length > 0) sb.Remove(sb.Length - 1, 1); } continue; }
                        Console.Write(echo); sb.Append(keyInfo.KeyChar);
                    };
                    colorConsole.WriteLineBlank();
                    return sb.ToString();
                }
            }
        }

        // These regular expressions assumes the pattern - { index }
        private const string matchItem = @"(\{\d\})";
        private const string matchFormat = @"{\s*(?<index>\d+)\s*(,\s*(?<spacing>[-+]?\d+))?(\s*:\s*(?<format>.*))?}";
        // string.Format's format item actually has the pattern - { index[,alignment][:formatString] }
        // We can tackle the alignment and formatString options another day.
        // Perhaps we can incorporate the color option into the format item (but that would only specify the color of the replaced text, not the literal text in the format string)
        private static Regex regItem = new Regex(matchItem, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex regFormat = new Regex(matchFormat, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static void writeLineBicolor(this ColorConsole colorConsole, string format, string[] args, ConsoleColor headingFg = ConsoleColor.DarkGray, ConsoleColor valueFg = ConsoleColor.White, ConsoleColor headingBg = ConsoleColor.Black, ConsoleColor valueBg = ConsoleColor.Black) {
            foreach (string item in regItem.Split(format)) {
                Match match = regFormat.Match(item);
                if (match.Success) Write(colorConsole, null != args ? (args[Convert.ToInt32(match.Groups["index"].Value)] ?? string.Empty).ToString() : item, valueFg, valueBg);
                else Write(colorConsole, item, headingFg, headingBg);
            }
            Console.WriteLine();
        }

        private static void foregroundBackground(ConsoleColor fg, ConsoleColor bg) {
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
        }

    }
    public class ColorConsole { }
} 