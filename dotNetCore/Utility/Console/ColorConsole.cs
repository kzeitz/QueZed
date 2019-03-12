using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueZed.Utility.Console {
   using System;
   using System.Text;
   using System.Text.RegularExpressions;
   public static partial class Extensions {
      private static object writeGate = new object();
      private static object readGate = new object();
      // This regular expression assumes the pattern - { index }
      private const string matchFormatItem = @"(\{\d\})";
      private const string matchFormatIndex = @"\{(?<index>\d)\}";
      // string.Format's format item actually has the pattern - { index[,alignment][:formatString] }
      // We can tackle the alignment and formatString options another day.
      // Perhaps we can incorporate the color option into the format item (but that would only specify the color of the replaced text, not the literal text in the format string)

      public static void WriteLineBlank(this ColorConsole colorConsole, int lines = 1, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
         lock (writeGate) {
            foregroundBackground(foregroundColor, backgroundColor);
            Console.WriteLine(new string(char.MinValue, lines - 1).Replace(char.MinValue.ToString(), Environment.NewLine));
            Console.ResetColor();
         }
      }
      public static void Write(this ColorConsole colorConsole, string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
         lock (writeGate) {
            foregroundBackground(foregroundColor, backgroundColor);
            Console.Write(text);
            Console.ResetColor();
         }
      }
      public static void WriteLine(this ColorConsole colorConsole, string line, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
         lock (writeGate) {
            foregroundBackground(foregroundColor, backgroundColor);
            Console.WriteLine(line);
            Console.ResetColor();
         }
      }

      //i.e
      //console.WriteLineBicolor("Order : {0} Status : {1} User : {2} Document Count : {3}", "Name", "Status", "UserID", "DocumentCount");
      //console.WriteLineBicolor(ConsoleColor.DarkGreen, ConsoleColor.White, "Table storage update, table: {0}", tableName);
      public static void WriteLineBicolor(this ColorConsole colorConsole, string format, params object[] args) { lock (writeGate) { writeLineBicolor(colorConsole, format, args); } }
      public static void WriteLineBicolor(this ColorConsole colorConsole, ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, string format, params object[] args) { lock (writeGate) { writeLineBicolor(colorConsole, format, args, headingForegroundColor, valueForegroundColor); } }
      public static void WriteLineBicolor(this ColorConsole colorConsole, ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, ConsoleColor headingBackgroundColor, ConsoleColor valueBackgroundColor, string format, params object[] args) { lock (writeGate) { writeLineBicolor(colorConsole, format, args, headingForegroundColor, valueForegroundColor, headingBackgroundColor, valueBackgroundColor); } }
      public static string ReadLine(this ColorConsole colorConsole, string prompt, bool intercept = false, char echo = '*') { lock (readGate) { return ReadLine(colorConsole, prompt, ConsoleColor.Gray, ConsoleColor.Black, intercept, echo); } }
      public static string ReadLine(this ColorConsole colorConsole, string prompt, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black, bool intercept = false, char echo = '*') {
         lock (readGate) {
            colorConsole.Write(prompt, foregroundColor, backgroundColor);
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
      private static void writeLineBicolor(this ColorConsole colorConsole, string format, object[] args, ConsoleColor headingForegroundColor = ConsoleColor.DarkGray, ConsoleColor valueForegroundColor = ConsoleColor.White, ConsoleColor headingBackgroundColor = ConsoleColor.Black, ConsoleColor valueBackgroundColor = ConsoleColor.Black) {
            foreach (string formatPart in Regex.Split(format, matchFormatItem, RegexOptions.Compiled | RegexOptions.IgnoreCase)) {
            Match match = Regex.Match(formatPart, matchFormatIndex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (match.Success) Write(colorConsole, (args[Convert.ToInt32(match.Groups["index"].Value)] ?? string.Empty).ToString(), valueForegroundColor, valueBackgroundColor);
            else Write(colorConsole, formatPart, headingForegroundColor, headingBackgroundColor);
         }
         Console.WriteLine();
      }

      private static void foregroundBackground(ConsoleColor foregroundColor, ConsoleColor backgroundColor) {
         Console.ForegroundColor = foregroundColor;
         Console.BackgroundColor = backgroundColor;
      }

   }
   public class ColorConsole { }
    }
