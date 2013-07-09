﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RxDuinoConsole.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RxDuinoConsole.Properties.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RxDuinoConsole.Data/rxduinologo.jpg.
        /// </summary>
        public static string ApplicationSplash {
            get {
                return ResourceManager.GetString("ApplicationSplash", resourceCulture);
            }
        }
        
        public static byte[] avrconf {
            get {
                object obj = ResourceManager.GetObject("avrconf", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        public static byte[] avrdude {
            get {
                object obj = ResourceManager.GetObject("avrdude", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please Note That You Are Currently Running a Beta Version of This Application.  Support is Limited.
        /// </summary>
        public static string BetaVersionString {
            get {
                return ResourceManager.GetString("BetaVersionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, Blank Messages Are Not Allowed.
        /// </summary>
        public static string BlankMessagesNotAllowedString {
            get {
                return ResourceManager.GetString("BlankMessagesNotAllowedString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RST,TXT,NEXT,PREV,UP,DOWN,ALT,OFF,PING,DUMP.
        /// </summary>
        public static string CommandList {
            get {
                return ResourceManager.GetString("CommandList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reset Device,Set The Stored Text,Next Mode,Previous Mode,Previous Monitor,Next Monitor,Alternate Mode,Shut Off Device,Ping Device,Dump Debug Information.
        /// </summary>
        public static string CommandText {
            get {
                return ResourceManager.GetString("CommandText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please Connect To Device First!.
        /// </summary>
        public static string ConnectString {
            get {
                return ResourceManager.GetString("ConnectString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, This Version Has Been Disabled.  Please Update To The Latest Version.
        /// </summary>
        public static string DisabledVersionString {
            get {
                return ResourceManager.GetString("DisabledVersionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.ftdichip.com/Drivers/VCP.htm.
        /// </summary>
        public static string FTDIDriverUrl {
            get {
                return ResourceManager.GetString("FTDIDriverUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please Select Valid COM Port.
        /// </summary>
        public static string InvalidComPortString {
            get {
                return ResourceManager.GetString("InvalidComPortString", resourceCulture);
            }
        }
        
        public static byte[] libusb0 {
            get {
                object obj = ResourceManager.GetObject("libusb0", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;log4net&gt;
        ///  &lt;appender name=&quot;Console&quot; type=&quot;log4net.Appender.ConsoleAppender&quot;&gt;
        ///    &lt;layout type=&quot;log4net.Layout.PatternLayout&quot;&gt;
        ///      &lt;!-- Pattern to output the caller&apos;s file name and line number --&gt;
        ///      &lt;conversionPattern value=&quot;%5level [%thread] (%file:%line) - %message%newline&quot; /&gt;
        ///    &lt;/layout&gt;
        ///  &lt;/appender&gt;
        ///
        ///  &lt;appender name=&quot;RollingFile&quot; type=&quot;log4net.Appender.RollingFileAppender&quot;&gt;
        ///    &lt;file value=&quot;${TEMP}\example.log&quot; /&gt;
        ///    &lt;appendToFile value=&quot;true&quot; /&gt;
        ///    &lt;maximumFileSize value=&quot;100KB&quot;  [rest of string was truncated]&quot;;.
        /// </summary>
        public static string log4net {
            get {
                return ResourceManager.GetString("log4net", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Log File|*.log.
        /// </summary>
        public static string LogFileFilter {
            get {
                return ResourceManager.GetString("LogFileFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message of the Day.
        /// </summary>
        public static string MotdString {
            get {
                return ResourceManager.GetString("MotdString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RxDuino Files (*.rxd;*.hex)|*.rxd;*.hex.
        /// </summary>
        public static string RxduinoFileFilter {
            get {
                return ResourceManager.GetString("RxduinoFileFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://therxduino.com/media/setup.exe.
        /// </summary>
        public static string SetupFileUrl {
            get {
                return ResourceManager.GetString("SetupFileUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1000.
        /// </summary>
        public static string SplashTimeInMS {
            get {
                return ResourceManager.GetString("SplashTimeInMS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Update Available!.
        /// </summary>
        public static string UpdateAvailableString {
            get {
                return ResourceManager.GetString("UpdateAvailableString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.normalexception.net/applications/update.xml.
        /// </summary>
        public static string UpdateUrl {
            get {
                return ResourceManager.GetString("UpdateUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.normalexception.net/redmine/Projects/RxDuino.
        /// </summary>
        public static string WebURL {
            get {
                return ResourceManager.GetString("WebURL", resourceCulture);
            }
        }
    }
}