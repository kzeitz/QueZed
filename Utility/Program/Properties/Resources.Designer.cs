﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QueZed.Utility.Program.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("QueZed.Utility.Program.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;log4net&gt;
        ///  &lt;appender name=&quot;Trace&quot; type=&quot;log4net.Appender.TraceAppender&quot;&gt;
        ///    &lt;layout type=&quot;log4net.Layout.PatternLayout&quot;&gt;
        ///      &lt;!-- Pattern to output the caller&apos;s file name and line number --&gt;
        ///      &lt;conversionPattern value=&quot;%-6timestamp [%-3thread] %-5level %-25logger{1} - %message%newline&quot; /&gt;
        ///    &lt;/layout&gt;
        ///  &lt;/appender&gt;
        ///  &lt;appender name=&quot;Console&quot; type=&quot;log4net.Appender.ConsoleAppender&quot;&gt;
        ///    &lt;layout type=&quot;log4net.Layout.PatternLayout&quot;&gt;
        ///      &lt;!-- Pattern to output the caller&apos;s file name and line [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DefaultLog4netXML {
            get {
                return ResourceManager.GetString("DefaultLog4netXML", resourceCulture);
            }
        }
    }
}
