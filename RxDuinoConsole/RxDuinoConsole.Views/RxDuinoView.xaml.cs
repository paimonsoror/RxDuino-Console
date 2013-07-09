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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using System.Windows.Forms;
using System.Diagnostics;
using log4net;
using System.IO;
using System.ComponentModel;
using System.Xml;
using System.Net;
using System.Windows.Input;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Media;
using System.Windows.Documents;

using Xceed.Wpf.Toolkit;

namespace RxDuinoConsole
{
    /// <summary>
    /// Interaction logic for RxDuinoView.xaml
    /// </summary>
    public partial class RxDuinoView : Window
    {
        private SerialPort serial = new SerialPort();
        private string line;
        private static readonly ILog logger = LogManager.GetLogger(typeof(RxDuinoApplication));
        private static string firmwareVersion = "";
        private static Boolean internetDisabled = 
            Convert.ToBoolean(ConfigurationManager.AppSettings["disableInternet"]);
        private String comport = "";

        // Available commands
        private String[] commandList     = RxDuinoConsole.Properties.Resources.CommandList.Split(',');
        private String[] commandListText = RxDuinoConsole.Properties.Resources.CommandText.Split(',');

        // Create a FlowDocument to contain content for the RichTextBox.
        private FlowDocument myFlowDoc = new FlowDocument();    

        /// <summary>
        /// Main application constructor.
        /// </summary>
        public RxDuinoView()
        {
            myFlowDoc.Blocks.Add(new Paragraph(new Run("")));

            logger.Info("Initializing View Objects");
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            // Initialize command list
            logger.Info("Creating List of Available Commands");
            msgList.Items.Add("");
            foreach (String cmd in commandList)
                msgList.Items.Add(cmd);
            
            TimeSpan timeout = TimeSpan.FromSeconds(5);
            Thread thread = new Thread(() => { CheckForMotd(); });
            thread.Start();
            DateTime timeStarted = DateTime.UtcNow;

            // We are at a safe point now so check the thread status.
            TimeSpan span = DateTime.UtcNow - timeStarted; // How long has the thread been running.
            TimeSpan wait = timeout - span; // How much more time should we wait.
            if (!thread.Join(wait))
            {
                thread.Abort(); // This is an unsafe operation so use as a last resort.
            }

            logger.Info("Assembling Flow Document");
            console.Document = myFlowDoc;

            logger.Info("Reading Configuration For Font");
            console.FontSize = Convert.ToInt32(ConfigurationManager.AppSettings["fontSize"]);
        }

        /// <summary>
        /// Fired when the window is loaded.  Does nothing more than refresh the COM
        /// port list and enforce all variants in the GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Window Loaded Event Fired");
            refreshButton_Click(sender, e);
            enforceVariants();
        }

        /// <summary>
        /// Fired when the window is closed.  Calls the exit click button which saves
        /// the configuration options before closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Window_Closed(object sender, CancelEventArgs e)
        {
            logger.Info("Exiting Application, " + DateTime.Now);
            exit_Click(sender, null);
        }

        /// <summary>
        /// Fired when the connect button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> connectButton_Click");

            // Check to see if the user selected a valid port
            if (comPortCombo.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show(RxDuinoConsole.Properties.Resources.InvalidComPortString);
            }
            else
            {
                // As long as the serial port isn't already open
                if (!serial.IsOpen)
                {
                    // Read the baud rate from the configuration file
                    int baudRate = 
                        Convert.ToInt32(ConfigurationManager.AppSettings["baudRate"]);

                    // Store COM
                    this.comport = comPortCombo.SelectedItem.ToString().Split(',')[0];

                    // Create a connection to the serial port
                    serial = 
                        new SerialPort(this.comport, 
                                       baudRate, 
                                       Parity.None, 
                                       8, 
                                       StopBits.One);

                    // Enable ready bit
                    serial.DtrEnable = true;
                    
                    // Attach callback for when data is received
                    serial.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

                    // Open the serial port
                    serial.Open();

                    // Send a reset
                    if (serial.IsOpen)
                    {
                        serial.Write("RST");
                        console.AppendText("User Connected To Device at " + DateTime.Now.ToString("hh:mm:ss tt") + "\n");
                    }
                }
                else
                {
                    console.AppendText("User Disconnected From Device at " + DateTime.Now.ToString("hh:mm:ss tt") + "\n");

                    // If the serial port is already connected, the user probably selected
                    // "Disconnect", so close the connection
                    serial.Close();
                }
            }

            enforceVariants(); 
            logger.Info("<< connectButton_Click");
        }

        /// <summary>
        /// Enforce GUI objects that are variant based on certain conditions
        /// </summary>
        private void enforceVariants()
        {
            // If connected, set the button to Disconnect, else Connect
            connectButton.Content = serial.IsOpen ? "Disconnect" : "Connect";

            // Once connected, enable the Update button
            updateFirmware.IsEnabled = comPortCombo.SelectedIndex == -1 ? false : true;

            // Enable clear button if the console contains text
            clearButton.IsEnabled = (new TextRange(console.Document.ContentStart, console.Document.ContentEnd)).Text.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Callback handler for the serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // The object that is referencing this callback is the serial port
            SerialPort sp = (SerialPort)sender;

            // Read the data on the line
            string indata = sp.ReadExisting();

            // Assuming there is valid data...
            this.Dispatcher.Invoke(
                 new Action(
                  () =>
                  {
                      // Forget random null characters that might be present on the
                      // line
                      if (indata != "\0\0")
                      {
                          // Add text to the console
                          console.AppendText(indata);

                          // Control to check for the firmware version
                          line += indata;

                          // If our line ends with a break
                          if (line.EndsWith("\n"))
                          {
                              // Check if it is the firmware line
                              int index = line.IndexOf("|RxDUINO||v");

                              // if found...
                              if(index != -1) {

                                  // Read the version number
                                  string version = line.Substring(index + "|RxDuino||v".Length, 4);
                                  firmwareLabel.Content = String.Format("Firmware Version: v{0}", version);
                                  firmwareVersion = version;

                                  // Check for a firmware update now that we know what version the user is 
                                  // currently running
                                  Thread updateThread = new Thread(new ThreadStart(RxDuinoView.checkFirmwareVersion));
                                  updateThread.IsBackground = true;
                                  updateThread.Start();
                              }
                              line = "";
                          }

                          // Move the scrollbar to the end of the conole
                          console.ScrollToEnd();
                      }
                  }
            ));
        }

        /// <summary>
        /// Check to see if a new version of the RxDuino firmware is available
        /// </summary>
        /// <param name="version">The current firmware version</param>
        static void checkFirmwareVersion()
        {
            // Assuming the user hasn't disabled internet checking
            if (!internetDisabled)
            {
                logger.Info("Checking For Firmware Updates");
                string remoteVersion = "", location = "";
                try
                {
                    // Read the XML file from the server
                    Dictionary<string, string> firmwareReader =
                        (new XmlReader()).readXmlFromServer("Firmware");

                    // Get the latest version, and the location of the firmware
                    remoteVersion = firmwareReader["version"];
                    location = firmwareReader["location"];
                }
                catch (NullReferenceException)
                {
                    // We must not be able to connect to the server, so the dictionary
                    // is going to be empty
                    return;
                }
                catch (KeyNotFoundException)
                {
                    // We must not be able to connect to the server, so the dictionary
                    // is going to be empty
                    return;
                }

                // If the firmware doesn't equal the version on the server
                if (!firmwareVersion.Equals(remoteVersion))
                {
                    // Display a message to the user and ask him if he would like to download
                    // the firmware
                    string updateMessage =
                        String.Format("Version {0} of the RxDuino firmware is available for download.  Would you like to download now?",
                            remoteVersion);

                    DialogResult result =
                        System.Windows.Forms.MessageBox.Show(updateMessage,
                                                             RxDuinoConsole.Properties.Resources.UpdateAvailableString,
                                                             MessageBoxButtons.YesNo,
                                                             MessageBoxIcon.Question);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(location);
                    }
                }
            }
        }

        /// <summary>
        /// Handler for when the user exits the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> exit_Click");

            // Shut down the serial port
            if(serial.IsOpen)
                serial.Close();

            // Control for the current COM
            int currentComInt = -1;

            try
            {
                // If the user has selected to remember the COM port, do so by first reading
                // the COM port that is selected
                if(Convert.ToBoolean(ConfigurationManager.AppSettings["rememberCOM"])) {
                    string currentCom = comPortCombo.SelectedItem.ToString().Split(',')[0];
                    currentComInt = Convert.ToInt32(currentCom.Substring(3));
                }  
            }
            catch (NullReferenceException) { }
            finally {
                // Finally, save the COM number to the configuration file
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["lastCom"].Value = currentComInt.ToString();
                config.Save(ConfigurationSaveMode.Modified);
            }

            // Only send a close command if the "EXIT" button was pressed, not if the
            // application was exited per the titlebar
            if(e != null)
                this.Close();

            logger.Info("<< exit_Click");
        }

        /// <summary>
        /// Fired when a COM port is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comPortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            logger.Info(">> comPortCombo_SelectionChanged");
            this.comport = comPortCombo.SelectedItem.ToString().Split(',')[0];
            enforceVariants();
            logger.Info("<< comPortCombo_SelectionChanged");
        }

        /// <summary>
        /// Fired when the user clicks to clear the console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> clearButton_Click");
            (new TextRange(console.Document.ContentStart, console.Document.ContentEnd)).Text = "";
            enforceVariants();
            logger.Info("<< clearButton_Click");
        }

        /// <summary>
        /// Fired when the user clicks the About button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void about_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> about_Click");
            (new RxDuinoAbout()).Show();
            logger.Info("<< about_Click");
        }

        /// <summary>
        /// Fired when the user wants to check for an updated firmware
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateFirmware_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> updateFirmware_Click");
            serial.Close();
            (new RxDuinoUpdateView(this.comport)).Show();
            enforceVariants();
            logger.Info("<< updateFirmware_Click");
        }

        /// <summary>
        /// Opens the weburl associated to the weblink button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void weblink_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> weblink_Click");
            Process.Start(RxDuinoConsole.Properties.Resources.WebURL);
            logger.Info("<< weblink_Click");
        }

        /// <summary>
        /// Refresh the available COM ports
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> refreshButton_Click");

            // Clear the current ports
            comPortCombo.Items.Clear();

            // Get all available COM ports
            List<Win32DeviceMgt.DeviceInfo> comPorts = Win32DeviceMgt.GetAllCOMPorts();

            // Check to see if the config asks us to remember our COM
            string comToRemember = "COM" + ConfigurationManager.AppSettings["lastCom"];

            int index = -1;

            // Add each COM to the list
            foreach (Win32DeviceMgt.DeviceInfo port in comPorts)
            {
                comPortCombo.Items.Add(port.name + ", " + port.decsription);
                if (port.name.Equals(comToRemember)) index = comPortCombo.Items.Count;
            }

            if (index != -1)
                comPortCombo.SelectedIndex = index - 1;
            
            logger.Info("<< refreshButton_Click");
        }

        /// <summary>
        /// Check for application updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkForUpdates_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> checkForUpdates_Click");
            RxDuinoApplication.checkForUpdates();
            logger.Info("<< checkForUpdates_Click");
        }

        /// <summary>
        /// Submit the command to the serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> submitButton_Click");

            // Submit the message if the serial port is open and the 
            // text isn't blank
            if (serial.IsOpen && !commandText.Text.Equals(""))
            {
                string commandToSend = commandText.Text;

                // If we selected a predefined message, send just that, unless
                // the message is a 'txt' type which needs user input
                if (msgList.SelectedIndex > 0)
                {
                    if (msgList.SelectedItem.ToString().Equals("TXT"))
                    {
                        commandToSend = msgList.SelectedItem.ToString() + " " + commandText.Text;
                    }
                    else
                    {
                        commandToSend = msgList.SelectedItem.ToString();
                    }
                }

                commandToSend = Convert.ToBoolean(ConfigurationManager.AppSettings["caseSensitive"]) ?
                    commandToSend : commandToSend.ToUpper();
                serial.Write(commandToSend);
                logger.Debug("User Submitted \"" + commandToSend + "\" to console");

                // If the user wants the box cleared after a send, clear it
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["clearAfterSubmit"]))
                {
                    commandText.Text = "";
                }
            }
            else
            {
                // If the serial port isn't even open
                if (!serial.IsOpen)
                {
                    System.Windows.MessageBox.Show(RxDuinoConsole.Properties.Resources.ConnectString);
                }

                // If the text is blank
                else if (commandText.Text.Equals(""))
                {
                    System.Windows.MessageBox.Show(RxDuinoConsole.Properties.Resources.BlankMessagesNotAllowedString);
                }
            }
            logger.Info("<< submitButton_Click");
        }

        /// <summary>
        /// Link the user to a website to download FTDI drivers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ftdi_Click(object sender, RoutedEventArgs e)
        {
            logger.Info(">> ftdi_Click");
            Process.Start(RxDuinoConsole.Properties.Resources.FTDIDriverUrl);
            logger.Info("<< ftdi_Click");
        }

        /// <summary>
        /// Save the log file so that the user can send it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveLogFile_Click(object sender, RoutedEventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = RxDuinoConsole.Properties.Resources.LogFileFilter;
            saveFileDialog1.Title = "Save Log File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                File.Copy(Environment.GetEnvironmentVariable("TEMP") + "\\example.log", saveFileDialog1.FileName);
            }
        }

        /// <summary>
        /// Handler for when a key is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commandText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Submit only when the user clicks the Enter button
            if (e.Key == Key.Return)
            {
                this.submitButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Fired when the user wants to view the configuration dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configuration_Click(object sender, RoutedEventArgs e)
        {
            (new RxDuinoConfigView()).Show();
        }

        /// <summary>
        /// Check for the message of the day
        /// </summary>
        private void CheckForMotd(Boolean forced = false)
        {
            if (!internetDisabled)
            {
                try
                {
                    logger.Info("Checking For MOTD");

                    Dictionary<string, string> motd = (new XmlReader()).readXmlFromServer("Motd");
                    int id = Convert.ToInt32(motd["id"]);

                    //Create the object
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    if (forced || id != Convert.ToInt16(config.AppSettings.Settings["lastMotd"].Value))
                    {
                        string updateMessage =
                                String.Format("Message of the Day: {0}", motd["text"]);

                        DialogResult result =
                            System.Windows.Forms.MessageBox.Show(updateMessage,
                                                                 RxDuinoConsole.Properties.Resources.MotdString,
                                                                 MessageBoxButtons.OK,
                                                                 MessageBoxIcon.Information);

                        config.AppSettings.Settings["lastMotd"].Value = id.ToString();

                        //save to apply changes
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                    else
                    {
                        logger.Info("We have already seen this message");
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message);
                }
            }
        }

        /// <summary>
        /// View the log file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewLogFile_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", Environment.GetEnvironmentVariable("TEMP") + "\\example.log");
        }

        /// <summary>
        /// Handler for when the save console button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the text box
            // text
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "XAML File|*.xaml";
            saveFileDialog1.Title = "Save the Console Text";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                //File.WriteAllText(saveFileDialog1.FileName, (new TextRange(console.Document.ContentStart, console.Document.ContentEnd)).Text);
                // Save Document to file
                FileStream docStream = new FileStream(saveFileDialog1.FileName, FileMode.OpenOrCreate);
                System.Windows.Markup.XamlWriter.Save(console.Document, docStream);
                docStream.Close(); 
            }            
        }

        /// <summary>
        /// Handle when the Load console button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog loadFileDialog = new OpenFileDialog();
                loadFileDialog.Filter = "XAML File|*.xaml";
                loadFileDialog.Title = "Load Console Text File";
                loadFileDialog.ShowDialog();

                if (loadFileDialog.FileName != "")
                {
                    FileStream file = new FileStream(loadFileDialog.FileName, FileMode.Open);
                    FlowDocument newFlow = System.Windows.Markup.XamlReader.Load(file) as FlowDocument;
                    List<Block> flowDocumentBlocks = new List<Block>(newFlow.Blocks);

                    foreach (Block blk in flowDocumentBlocks) 
                    {
                        myFlowDoc.Blocks.Add(blk);
                    }

                    file.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error Loading File: " + ex.Message);
            }
        }

        /// <summary>
        /// Handler for when a message was selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void messageSelected(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as System.Windows.Controls.ComboBox).SelectedIndex;

            // Set the text box control
            if (index == 0 || commandList[index - 1].Equals("TXT"))
                commandText.IsEnabled = true;
            else
                commandText.IsEnabled = false;

            // Set the command text
            if (index == 0)
                commandText.Text = "";
            else
                commandText.Text = commandListText[index - 1];
        }

        /// <summary>
        /// Insert a break in the console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void breakButton_Click(object sender, RoutedEventArgs e)
        {
            // Add paragraphs to the FlowDocument.
            Paragraph para = new Paragraph(new Run("---------- Break ----------"));
            Brush color = (SolidColorBrush)new BrushConverter().ConvertFromString(ConfigurationManager.AppSettings["breakColor"]); // Brushes.Yellow;
            para.Background = color;
            myFlowDoc.Blocks.Add(para);
            myFlowDoc.Background = Brushes.White;
            para = new Paragraph(new Run(""));
            para.Background = Brushes.White;
            myFlowDoc.Blocks.Add(para);
            console.ScrollToEnd();
            enforceVariants();
        }

        /// <summary>
        /// Show the message of the day regardless of configuration value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showMotd_Click(object sender, RoutedEventArgs e)
        {
            CheckForMotd(true);
        }
    }
}
