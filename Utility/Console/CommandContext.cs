using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace QueZed.Utility.Console {

    /// <summary>
    /// This area needs work...
    /// The idea is to be able to define a grammar of available commands
    /// These commands are automatically bound functions and executed
    /// </summary>

    public interface ICommandContext<PType>  {
        string CommandLine { get; }
        string Arguments { get; }
        Verb Verb { get; }
        PType ParentInstance { get; }
        int ChildIndex { get; }
        //CType this[int key] { get; }
    }

    public class CommandContext<PType> : ICommandContext<PType> {
        private readonly string commandLine = null;
        private readonly string arguments = null;
        private readonly Verb verb = null;
        private readonly PType instance = default(PType);
        public CommandContext(string commandLine, string arguments, Verb verb, PType instance) { this.commandLine = commandLine; this.arguments = arguments; this.verb = verb; this.instance = instance; }
        public string CommandLine { get { return commandLine; } }
        public string Arguments { get { return arguments; } }
        public Verb Verb { get { return verb; } }
        public PType ParentInstance { get { return instance; } }
        public int ChildIndex { get { return 0; } }
        //public CType this[int key] { get { return children[key]; } }

    }

    public class Verb {
        private enum captureTag { command, index, arguments };

        private string name = string.Empty;
        private Func<object, Task> action = null;
        private Type implementor = null;
        private string description = string.Empty;
        private List<string> aliases = new List<string>();
        private Tag indexerTag = Tag._none;
        private Tag argumentTag = Tag._none;
        private string argumentHelp = null;
        private string customHelp = null;
        private string matchingExpression = null;

        public const string DefaultImplementorName = "Application";
        public enum Tag : byte { _none = 0, required, optional };

        public Verb(string name, Func<object, Task> actionToPerform, string description = null, string[] aka = null, Tag indexer = Tag._none, Tag arguments = Tag._none, string argumentHelp = null, string customCommandHelp = null) {
            this.name = name;
            action = actionToPerform;
            if (action != null) implementor = actionToPerform.Method.DeclaringType;
            if (!string.IsNullOrEmpty(description)) this.description = description; 
            if (aka != null) aliases.AddRange(aka);
            indexerTag = indexer;
            argumentTag = arguments;
            this.argumentHelp = argumentHelp;
            customHelp = customCommandHelp;
        }
        public string Name { get { return name; } }

        public string MatchingExpression {
            get {
                if (string.IsNullOrEmpty(matchingExpression)) {
                    string aliasExpression = string.Empty;
                    if (aliases.Count > 0) aliasExpression = string.Format("|{0}", string.Join("|", aliases));
                    string commandExpression = string.Format("(?<{0}>{1}{2})", commandTag, name, aliasExpression);
                    string indexerExpression = string.Empty;
                    if (indexerTag > Tag._none) indexerExpression = string.Format(@"\s*(?<{0}>\d){1}", indexTag, Tag.optional == indexerTag ? "?" : string.Empty);
                    string argumentsExpression = string.Empty;
                    if (argumentTag > Tag._none) argumentsExpression = string.Format(@"\s*(?<{0}>\w+){1}", argumentsTag, Tag.optional == argumentTag ? "?" : string.Empty);
                    matchingExpression = string.Format("{0}{1}{2}", commandExpression, indexerExpression, argumentsExpression);
                }
                return matchingExpression;
            }
        }

        public Func<object, Task> Action { get { return action; } }
        public Type Implementor { get { return implementor; } }

        public string ImplementorName { get { return implementor?.Name ?? DefaultImplementorName; } }

        public string ArgumentHelp {
            get {
                if (argumentTag == Tag._none) return string.Empty;
                if (!string.IsNullOrEmpty(argumentHelp)) return argumentHelp;
                return string.Format("{0}", argumentTag == Tag.required ? "arguments" : "[arguments]");
            }
        }
        public string[] Aliases { get { return aliases.ToArray(); } }
        public bool IsCommand(string command) { return 0 == string.Compare(command, name, true); }
        public Tag Indexer { get { return indexerTag; } }
        public string ArgumentsTag { get { return argumentsTag; } }

        private string commandTag { get { return captureTag.command.ToString(); } }
        private string indexTag { get { return string.Format("{0}{1}", indexerTag.ToString(), captureTag.index.ToString()); } }
        private string argumentsTag { get { return string.Format("{0}{1}", argumentTag.ToString(), captureTag.arguments.ToString()); } }
    }

    public class Grammar {
        // This help area is still under construction
        // I'd like to use a more featured string template tool
        // For now I'll do it manually here with string.Format().
        // Look at SmartFormat.NET as a solution. https://github.com/axuno/SmartFormat.NET

        private static ColorConsole console = new ColorConsole();
        class FormatTemplate {
            private struct ColumnWidths { public int Command; public int Alias; }
            private ConsoleColor option = ConsoleColor.Cyan;
            private ConsoleColor syntax = ConsoleColor.Gray;
            private Dictionary<string, ColumnWidths> widths = new Dictionary<string, ColumnWidths>();
            public FormatTemplate() { widths.Add(Verb.DefaultImplementorName, new ColumnWidths() { Command = 0, Alias = 0 }); }

            public void Add(Verb verb) {
                string scope = verb.ImplementorName;
                if (0 != string.Compare(scope, Verb.DefaultImplementorName)) {
                    if (!widths.ContainsKey(scope)) widths.Add(scope, new ColumnWidths() { Command = 0, Alias = 0 });
                    ColumnWidths scopedColumnWidths = widths[scope];
                    scopedColumnWidths.Command = Math.Max(scopedColumnWidths.Command, verb.Name.Length);
                    scopedColumnWidths.Alias = Math.Max(scopedColumnWidths.Alias, formatAliases(verb.Aliases).Length);
                    widths[scope] = scopedColumnWidths;
                }
                ColumnWidths allColumnWidths = widths[Verb.DefaultImplementorName];
                allColumnWidths.Command = Math.Max(allColumnWidths.Command, verb.Name.Length);
                allColumnWidths.Alias = Math.Max(allColumnWidths.Alias, formatAliases(verb.Aliases).Length);
                widths[Verb.DefaultImplementorName] = allColumnWidths;
            }


            private readonly char[] escChars = { '\\' };
            private const string indexString = "instance#";
            private readonly string optionalIndexString = string.Format("[{0}]", indexString);
            private const string prompt = ">";
            private string formatAliases(string[] aliases) { return string.Format("| {0}", string.Join(" | ", aliases).Trim(escChars)); }
            public void Help (Verb verb, string scope = Verb.DefaultImplementorName) {
                string command = verb.Name;
                string aliases = string.Empty;
                if (verb.Aliases.Length > 0) aliases = formatAliases(verb.Aliases);
                string indexer = string.Empty;
                if (verb.Indexer > Verb.Tag._none) indexer = (verb.Indexer == Verb.Tag.required ? indexString : optionalIndexString);
                string arguments = verb.ArgumentHelp;

                ColumnWidths columnWidths = widths[scope];
                string indexerPart = string.Format("{{0,-{0}}}{{{{1}}}}", optionalIndexString.Length);
                string syntaxPartFormat = string.Format("{0} {{0}} {1}", prompt, string.Format(indexerPart, indexer));
                string optionPartFormat = string.Format("{{0,-{0}}} {{1,-{1}}}", columnWidths.Command, columnWidths.Alias);
                string optionPart = string.Format(optionPartFormat, command, aliases );
                console.WriteLineBicolor(syntax, option, syntaxPartFormat, optionPart, arguments);
            }
        }

        private FormatTemplate template = new FormatTemplate();
        private Dictionary<string, Verb> verbs = new Dictionary<string, Verb>();
        public Grammar() { }
        public Grammar(IEnumerable<Verb> verbs) { foreach (Verb v in verbs) { Add(v); template.Add(v); } }
        public Verb this[string key] { get { return verbs[key]; } set { verbs[key] = value; } }
        public void Add(Verb verb) { verbs.Add(verb.Name, verb); }

        public dynamic Interpret(string commandLine, object root) {
            Match match = null;
            foreach (KeyValuePair<string, Verb> kvp in verbs) {
                if ((match = Regex.Match(commandLine, kvp.Value.MatchingExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase)).Success) {
                    string arguments = match.Groups[kvp.Value.ArgumentsTag].Value;
                    Type implementor = typeof(object);
                    if (kvp.Value.Implementor != null) implementor = kvp.Value.Implementor;
                    Type generic = typeof(CommandContext<>).MakeGenericType(implementor);
                    object o = Activator.CreateInstance(generic, commandLine, arguments, kvp.Value, root);
                    return Convert.ChangeType(o, generic);
                }
            }
            return null;
        }

        private const ConsoleColor heading = ConsoleColor.Yellow;
        private const ConsoleColor subheading = ConsoleColor.Gray;
        public void Help(Type scope) {
            console.WriteLine("Commands:", heading);
            string implementor = scope?.Name ?? "All";
            // Force "Application" items (DefaultImplementorName) to sort last
            foreach (KeyValuePair<string, Verb> kvp in verbs.OrderBy(e => e.Value.ImplementorName == Verb.DefaultImplementorName).ThenBy(e => e.Value.ImplementorName)) { 
                Verb verb = kvp.Value;
                string currentScope = verb.ImplementorName;
                if (0 != string.Compare(implementor, currentScope)) { console.WriteLine(string.Format("{0} commands.", currentScope), subheading); implementor = currentScope; }
                template.Help(verb);
            }
        }
    }
}
