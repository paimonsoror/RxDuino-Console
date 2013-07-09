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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using log4net;

namespace RxDuinoConsole
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class RxDuinoUpdateView : Window
    {
        private String comport = "";
        private static readonly ILog logger = LogManager.GetLogger(typeof(RxDuinoApplication));

        /// <summary>
        /// Constructor to an update GUI
        /// </summary>
        public RxDuinoUpdateView(String com)
        {
            this.comport = com;
            InitializeComponent();
        }

        /// <summary>
        /// Fired when the window is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            enforceVariants();
        }

        /// <summary>
        /// Enforce all of the gui options that are variants based on current
        /// conditions
        /// </summary>
        private void enforceVariants()
        {
            updateButton.IsEnabled = fileBox.Text.Count() > 1 ? true : false;
        }

        /// <summary>
        /// Fired when the browse button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an open dialog
            OpenFileDialog open = new OpenFileDialog();

            // Set the filters in the dialog
            open.Filter = RxDuinoConsole.Properties.Resources.RxduinoFileFilter;
            open.FilterIndex = 1;
            open.RestoreDirectory = true;
            open.ShowDialog();

            // Save the file that was selected to the text box
            fileBox.Text = open.FileName;
            enforceVariants();
        }

        /// <summary>
        /// Fired when the user clicks the update button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a file name for each of our objects in temp
            string tempFile = Environment.GetEnvironmentVariable("TEMP") + "\\firmupdate.exe";
            string confFile = Environment.GetEnvironmentVariable("TEMP") + "\\avrdude.conf";
            string dllFile  = Environment.GetEnvironmentVariable("TEMP") + "\\libusb0.dll";

            // Copy the files from our resources to the newly created
            File.WriteAllBytes(tempFile, Properties.Resources.avrdude);
            File.WriteAllBytes(confFile, Properties.Resources.avrconf);
            File.WriteAllBytes(dllFile, Properties.Resources.libusb0);

            // Get the filename of the firmware selected
            string fileName = fileBox.Text;
            string hexFile  = fileName;

            // Is it an RXD file? if so, decrypt it
            if(fileName.Contains(".rxd"))
                hexFile = RxDuinoCryptLib.Crypto.DecryptStringAES(fileName, "rxduinoCrypto");

            // Execute the updater
            string executionParams = "-C" + confFile + 
                " -v -v -p" + ConfigurationManager.AppSettings["flash.chipset"] +
                " -c " + ConfigurationManager.AppSettings["flash.boot"] + 
                " -P" + comport +
                " -b" + ConfigurationManager.AppSettings["flash.baud"] +
                " -D -Uflash:w:\"" + hexFile + "\":i";

            System.Diagnostics.Process.Start(tempFile, executionParams);
        }
    }
}
