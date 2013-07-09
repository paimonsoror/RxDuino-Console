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

namespace RxDuinoConsole
{
    /// <summary>
    /// Interaction logic for RxDuinoConfigView.xaml
    /// </summary>
    public partial class RxDuinoConfigView : Window
    {
        /// <summary>
        /// Constructor to a configuration gui object
        /// </summary>
        public RxDuinoConfigView()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeOptions();
        }

        /// <summary>
        /// Initialize the options by reading what options are currently set in the 
        /// configuration file
        /// </summary>
        private void InitializeOptions()
        {
            caseSensitive.IsChecked     = Convert.ToBoolean(ConfigurationManager.AppSettings["caseSensitive"]);
            rememberCOM.IsChecked       = Convert.ToBoolean(ConfigurationManager.AppSettings["rememberCOM"]);
            disableInternet.IsChecked   = Convert.ToBoolean(ConfigurationManager.AppSettings["disableInternet"]);
            clearAfterSubmit.IsChecked  = Convert.ToBoolean(ConfigurationManager.AppSettings["clearAfterSubmit"]);
            breakBg.ItemsSource         = ConfigurationManager.AppSettings["availableBreakColors"].Split(',');
            breakBg.SelectedItem        = ConfigurationManager.AppSettings["breakColor"];
            fontSize.Value              = Convert.ToInt32(ConfigurationManager.AppSettings["fontSize"]);
        }

        /// <summary>
        /// Fired when the user clicks the OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ok_click(object sender, RoutedEventArgs e)
        {
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Save all of the options
            config.AppSettings.Settings["caseSensitive"].Value   = Convert.ToString(caseSensitive.IsChecked);
            config.AppSettings.Settings["rememberCOM"].Value     = Convert.ToString(rememberCOM.IsChecked);
            config.AppSettings.Settings["disableInternet"].Value = Convert.ToString(disableInternet.IsChecked);
            config.AppSettings.Settings["clearAfterSubmit"].Value = Convert.ToString(clearAfterSubmit.IsChecked);
            config.AppSettings.Settings["breakColor"].Value = Convert.ToString(breakBg.SelectedItem);

            // Double check to make sure the user didn't enter an invalid number
            int tmp;
            if(Int32.TryParse(Convert.ToString(fontSize.Value), out tmp))
                config.AppSettings.Settings["fontSize"].Value = Convert.ToString(fontSize.Value);

            //save to apply changes
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Close();
        }

        /// <summary>
        /// Fired when the user clicks the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancel_click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
