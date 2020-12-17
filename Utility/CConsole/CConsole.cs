using System;
using System.Text;

namespace Karlton.Utility.Console {
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	public class CConsole : ICConsole {
		private static object writeGate = new object();
		private static object readGate = new object();
		// This regular expression assumes the pattern - { index }
		private const string matchFormatItem = @"(\{\d\})";
		private const string matchFormatIndex = @"\{(?<index>\d)\}";
		// string.Format's format item actually has the pattern - { index[,alignment][:formatString] }
		// We can tackle the alignment and formatString options another day.
		// Perhaps we can incorporate the color option into the format item (but that would only specify the color of the replaced text, not the literal text in the format string)

		public void WriteLineBlank(int lines = 1, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
			lock (writeGate) {
				foregroundBackground(foregroundColor, backgroundColor);
				System.Console.WriteLine(new string(char.MinValue, lines - 1).Replace(char.MinValue.ToString(), Environment.NewLine));
				System.Console.ResetColor();
			}
		}
		public void Write(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
			lock (writeGate) {
				foregroundBackground(foregroundColor, backgroundColor);
				System.Console.Write(text);
				System.Console.ResetColor();
			}
		}
		public void WriteLine(string line, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black) {
			lock (writeGate) {
				foregroundBackground(foregroundColor, backgroundColor);
				System.Console.WriteLine(line);
				System.Console.ResetColor();
			}
		}
		public void WriteLineBicolor(string format, params object[] args) { lock (writeGate) { writeLineBicolor(format, args); } }
		public void WriteLineBicolor(ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, string format, params object[] args) { lock (writeGate) { writeLineBicolor(format, args, headingForegroundColor, valueForegroundColor); } }
		public void WriteLineBicolor(ConsoleColor headingForegroundColor, ConsoleColor valueForegroundColor, ConsoleColor headingBackgroundColor, ConsoleColor valueBackgroundColor, string format, params object[] args) { lock (writeGate) { writeLineBicolor(format, args, headingForegroundColor, valueForegroundColor, headingBackgroundColor, valueBackgroundColor); } }
		public string ReadLine(string prompt, bool intercept = false, char echo = '*') { lock (readGate) { return ReadLine(prompt, ConsoleColor.Gray, ConsoleColor.Black, intercept, echo); } }
		public string ReadLine(string prompt, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black, bool intercept = false, char echo = '*') {
			lock (readGate) {
				Write(prompt, foregroundColor, backgroundColor);
				if (!intercept) return System.Console.ReadLine();
				else {
					int left = System.Console.CursorLeft;
					StringBuilder sb = new StringBuilder();
					ConsoleKeyInfo keyInfo;
					while ((keyInfo = System.Console.ReadKey(intercept)).Key != ConsoleKey.Enter) {
						if (ConsoleKey.LeftArrow == keyInfo.Key || ConsoleKey.RightArrow == keyInfo.Key) continue;
						if (ConsoleKey.Backspace == keyInfo.Key) { if (System.Console.CursorLeft > left) { Write("\b \b"); if (sb.Length > 0) sb.Remove(sb.Length - 1, 1); } continue; }
						System.Console.Write(echo); sb.Append(keyInfo.KeyChar);
					};
					WriteLineBlank();
					return sb.ToString();
				}
			}
		}
		private void writeLineBicolor(string format, object[] args, ConsoleColor headingForegroundColor = ConsoleColor.DarkGray, ConsoleColor valueForegroundColor = ConsoleColor.White, ConsoleColor headingBackgroundColor = ConsoleColor.Black, ConsoleColor valueBackgroundColor = ConsoleColor.Black) {
			foreach (string formatPart in Regex.Split(format, matchFormatItem, RegexOptions.Compiled | RegexOptions.IgnoreCase)) {
				Match match = Regex.Match(formatPart, matchFormatIndex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (match.Success) Write((args[Convert.ToInt32(match.Groups["index"].Value)] ?? string.Empty).ToString(), valueForegroundColor, valueBackgroundColor); 
				else Write(formatPart, headingForegroundColor, headingBackgroundColor);
			}
			System.Console.WriteLine();
		}

		private static void foregroundBackground(ConsoleColor foregroundColor, ConsoleColor backgroundColor) {
			System.Console.ForegroundColor = foregroundColor;
			System.Console.BackgroundColor = backgroundColor;
		}

	}
}
