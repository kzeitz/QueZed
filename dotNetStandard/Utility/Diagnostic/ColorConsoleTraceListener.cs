using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//Port to .NET Core
//https://msdn.microsoft.com/en-us/magazine/mt694089.aspx

namespace QueZed.Utility.Diagnostic {
   using System.Diagnostics;

	// To use through configuration file
	/*
	<system.diagnostics>
    <sources>
      <source name="log" switchValue="All">
        <listeners>
          <add name="Console" type="QueZed.Utility.Diagnostic.ColorConsoleTraceListener, ConsoleApplication1" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
	*/

	public class ColorConsoleTraceListener : ConsoleTraceListener {
        private bool writeTraceEventInformation = true;
        SortedList<TraceEventType, ConsoleColor> eventColor = new SortedList<TraceEventType, ConsoleColor>();
        public ColorConsoleTraceListener(bool writeTraceEventInformation = true) {
            this.writeTraceEventInformation = writeTraceEventInformation;
            eventColor.Add(TraceEventType.Critical, ConsoleColor.Red);
            eventColor.Add(TraceEventType.Error, ConsoleColor.DarkRed);
            eventColor.Add(TraceEventType.Information, ConsoleColor.Gray);
            eventColor.Add(TraceEventType.Start, ConsoleColor.DarkCyan);
            eventColor.Add(TraceEventType.Stop, ConsoleColor.DarkCyan);
            eventColor.Add(TraceEventType.Verbose, ConsoleColor.DarkGray);
            eventColor.Add(TraceEventType.Warning, ConsoleColor.Yellow);
        }
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message) { TraceEvent(eventCache, source, eventType, id, "{0}", message); }
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args) {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = getEventColor(eventType, originalColor);
            if (writeTraceEventInformation) base.TraceEvent(eventCache, source, eventType, id, format, args); else base.WriteLine(string.Format(format, args));
            Console.ForegroundColor = originalColor;
        }
        private ConsoleColor getEventColor(TraceEventType eventType, ConsoleColor defaultColor) { if (!eventColor.ContainsKey(eventType)) return defaultColor; return eventColor[eventType]; }
    }
}