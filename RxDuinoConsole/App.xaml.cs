/************************************************************************
 * RXDUINO Software, All Rights Reserved 2012
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Xml;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using System.Net;
using System.IO;

namespace RxDuinoConsole
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    static class RxDuinoApplication
    {
        static SplashScreen splash = new SplashScreen(RxDuinoConsole.Properties.Resources.ApplicationSplash);
        static RxDuinoConsole.App runApp = new RxDuinoConsole.App();
        static Boolean internetDisabled = Convert.ToBoolean(ConfigurationManager.AppSettings["disableInternet"]);
        private static readonly ILog logger = LogManager.GetLogger(typeof(RxDuinoApplication));

        /// <summary>
        /// Launcher for the application.  Starts by loading the logger's resources
        /// and then  displaying the splash screen.  Once the splash screen has been
        /// displayed, the application view will start
        /// </summary>
        /// <param name="args">Command line arguments for application</param>
        [STAThread]
        static void Main(String[] args)
        {
            try
            {
                // Configure the log4net logger
                System.Xml.XmlDocument objDocument = new System.Xml.XmlDocument();
                System.Xml.XmlElement objElement;

                // Load the log4net properties file
                objDocument.LoadXml(RxDuinoConsole.Properties.Resources.log4net);
                objElement = objDocument.DocumentElement;

                // Apply logger configuration
                log4net.Config.XmlConfigurator.Configure(objElement);

                // Display first log message
                logger.Info("Starting Application, " + DateTime.Now);

                // Show the application splash screen
                splash.Show(true);
                runApp.InitializeComponent();

                // Create splash screen thread
                Thread splashThread = new Thread(new ThreadStart(RxDuinoApplication.showSplash));
                splashThread.IsBackground = true;
                splashThread.Start();

                // Create application updater thread
                if (!internetDisabled)
                {
                    Thread updateThread = new Thread(new ThreadStart(RxDuinoApplication.checkForUpdates));
                    updateThread.Start();
                    updateThread.Join();
                }

                // Start the application
                logger.Info("Starting View");
                runApp.Run();
            }
            catch (ConfigurationException ce)
            {
                logger.Error("There was an error with the configuration system: " + ce.Message);
            }
            catch (Exception e)
            {
                logger.Error("An Unexpected Error Ocurred: " + e.Message);
            }
        }

        /// <summary>
        /// Display the splash screen for the application
        /// </summary>
        static void showSplash()
        {
            logger.Info("Splash Screen Thread Started");
            splash.Close(
                TimeSpan.FromMilliseconds(
                    double.Parse(RxDuinoConsole.Properties.Resources.SplashTimeInMS)));
        }

        /// <summary>
        /// Check for application updates
        /// </summary>
        public static void checkForUpdates()
        {
            logger.Info("Checking For Console Updates");

            string downloadUrl = "";
            string stage = "";

            Boolean kill = false;
            Version newVersion = null;
            try
            {
                Dictionary<string, string> elements = (new XmlReader()).readXmlFromServer("Application");
                newVersion = new Version(elements["version"]);
                downloadUrl = elements["url"];
                stage = elements["stage"];
                kill = elements["kill"].CompareTo("yes") == 0 ? true : false;
            }
            catch (WebException)
            {
                // Catch an exception if we couldn't connect to the server
                Thread.CurrentThread.Abort();
            }
            catch (NullReferenceException)
            {
                // Catch an exception if we had an empty dictionary
                logger.Warn("Empty Dictionary Reference For The Update, Probably Couldn't Connect");
                Thread.CurrentThread.Abort();
            }
            catch (KeyNotFoundException) 
            {
                // Catch an exception if we had an empty dictionary
                logger.Warn("Empty Dictionary Reference For The Update, Probably Couldn't Connect");
                Thread.CurrentThread.Abort();
            }

            Version applicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (applicationVersion.CompareTo(newVersion) < 0)
            {
                string updateMessage = 
                    String.Format("Version {0}.{1}.{2}.{3} of this application is available for download.  Would you like to download now?",
                        newVersion.Major, newVersion.Minor, newVersion.MinorRevision, newVersion.Build);

                DialogResult result =
                    System.Windows.Forms.MessageBox.Show(updateMessage, 
                                                         RxDuinoConsole.Properties.Resources.UpdateAvailableString, 
                                                         MessageBoxButtons.YesNo, 
                                                         MessageBoxIcon.Question);
                switch(result) {
                    case DialogResult.Yes:
                        System.Diagnostics.Process.Start(downloadUrl);
                        break;
                }
            } 
            else 
            {
                if (stage.CompareTo("beta") == 0)
                {
                    System.Windows.Forms.MessageBox.Show(RxDuinoConsole.Properties.Resources.BetaVersionString,
                        "Beta Version Detected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                if (kill)
                {
                    System.Windows.Forms.MessageBox.Show(RxDuinoConsole.Properties.Resources.DisabledVersionString,
                        "Disabled Version, Please Update",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(-2);
                }
            }
        }
    }
}
