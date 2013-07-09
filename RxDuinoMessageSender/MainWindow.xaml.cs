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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

namespace RxDuinoMessageSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serial = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();

            // Clear the current ports
            comboBox1.Items.Clear();

            // Get all available COM ports
            List<Win32DeviceMgt.DeviceInfo> comPorts = Win32DeviceMgt.GetAllCOMPorts();

            // Add each COM to the list
            foreach (Win32DeviceMgt.DeviceInfo port in comPorts)
            {
                comboBox1.Items.Add(port.name + ", " + port.decsription);
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!addressBox.Text.Equals(""))
            {
                String message = String.Format("MSG {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    addressBox.Text, dataBox0.Text, dataBox1.Text, dataBox2.Text, dataBox3.Text,
                    dataBox4.Text, dataBox5.Text, dataBox6.Text, dataBox7.Text);
                /*
                if (!addressBox_1.Text.Equals(""))
                {
                    message = String.Format("{0}|{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        message, addressBox_1.Text, dataBox0_1.Text, dataBox1_1.Text, dataBox2_1.Text,
                        dataBox3_1.Text, dataBox4_1.Text, dataBox5_1.Text, dataBox6_1.Text, dataBox7_1.Text);

                    if (!addressBox_2.Text.Equals(""))
                    {
                        message = String.Format("{0}|{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                            message, addressBox_2.Text, dataBox0_2.Text, dataBox1_2.Text, dataBox2_2.Text,
                            dataBox3_2.Text, dataBox4_2.Text, dataBox5_2.Text, dataBox6_2.Text, dataBox7_2.Text);
                    }
                }*/
                consoleBox.AppendText("Sending: " + message + "\n");

                // Submit the message if the serial port is open and the 
                // text isn't blank

                if (serial.IsOpen)
                {
                    serial.Write(message);
                    serial.Write("\n");
                }
            }
        }

        /// <summary>
        /// Handler for when the connect button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
             // As long as the serial port isn't already open
            if (!serial.IsOpen)
            {
                // Read the baud rate from the configuration file
                int baudRate = 115200;

                // Store COM
                String comport = comboBox1.SelectedItem.ToString().Split(',')[0];

                // Create a connection to the serial port
                serial =
                    new SerialPort(comport,
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

                consoleBox.AppendText("Connected\n");

                serial.Write("RST");
                serial.Write("\n");
            }
            else
            {
                serial.Close();
                consoleBox.AppendText("Disconnected\n");
            }
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
                        // Add text to the console
                        consoleBox.AppendText(indata);

                        // Move the scrollbar to the end of the conole
                        consoleBox.ScrollToEnd();
                  }
            ));
        }
    }
}
