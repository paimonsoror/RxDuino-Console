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
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace RxDuinoConsole
{
    /// <summary>
    /// Interaction logic for RxDuinoView.xaml
    /// </summary>
    public partial class RxDuinoView : Window
    {
        private SerialPort serial = new SerialPort();
        private string line;

        /// <summary>
        /// 
        /// </summary>
        public RxDuinoView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshButton_Click(sender, e);
            enforceVariants();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (comPortCombo.SelectedIndex == -1)
            {
                System.Windows.MessageBox.Show("Please Select Valid COM Port");
            }
            else
            {
                if (!serial.IsOpen)
                {
                    serial = 
                        new SerialPort(comPortCombo.SelectedItem.ToString(), 
                                       9600, 
                                       Parity.None, 
                                       8, 
                                       StopBits.One);
                    serial.DtrEnable = true;
                    serial.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
                    serial.Open();
                }
                else
                {
                    serial.Close();
                }
            }

            enforceVariants();
        }

        /// <summary>
        /// 
        /// </summary>
        private void enforceVariants()
        {
            connectButton.Content = serial.IsOpen ? "Disconnect" : "Connect";
            updateFirmware.IsEnabled = comPortCombo.SelectedIndex == -1 ? false : true;
            clearButton.IsEnabled = console.Text.Count() > 0 ? true : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            this.Dispatcher.Invoke(
                 new Action(
                  () =>
                  {
                      if (indata != "\0\0")
                      {
                          console.AppendText(indata);
                          line += indata;
                          if (line.EndsWith("\n"))
                          {
                              int index = line.IndexOf("RxDUINO v");
                              if(index != -1) {
                                  string version = line.Substring(index + "RxDuino v".Length);
                                  firmwareLabel.Content = String.Format("Firmware Version: v{0}", version); 
                              }
                              line = "";
                          }

                          console.ScrollToEnd();
                      }
                  }
            ));
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_Click(object sender, RoutedEventArgs e)
        {
            serial.Close();
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comPortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            enforceVariants();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            console.Clear();
            enforceVariants();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void about_Click(object sender, RoutedEventArgs e)
        {
            (new RxDuinoAbout()).Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateFirmware_Click(object sender, RoutedEventArgs e)
        {
            (new RxDuinoUpdateView()).Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void weblink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.normalexception.net/redmine/Projects/RxDuino");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            comPortCombo.Items.Clear();
            string[] theSerialPortNames = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string port in theSerialPortNames)
                comPortCombo.Items.Add(port);
        }
    }
}
