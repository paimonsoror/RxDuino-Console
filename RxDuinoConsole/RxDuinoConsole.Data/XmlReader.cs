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
using System.Xml;
using log4net;
using System.Net;

namespace RxDuinoConsole
{
    /// <summary>
    /// A class that is used to read the xml file found on the server
    /// </summary>
    class XmlReader
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(RxDuinoApplication));

        /// <summary>
        /// 
        /// </summary>
        public XmlReader()
        {
        }

        /// <summary>
        /// Read the xml file from the server and save the values to a map
        /// </summary>
        /// <param name="section">The section of the XML file to grab</param>
        /// <returns></returns>
        public Dictionary<string, string> readXmlFromServer(String section)
        {
            XmlDocument xmldoc = new XmlDocument();
            Dictionary<string, string> elements = new Dictionary<string, string>();
            try
            {
                // If we are able to get the file...
                if (RemoteFileExists(RxDuinoConsole.Properties.Resources.UpdateUrl))
                {
                    // Load the file into an XML object and parse the children from the 
                    // supplied node
                    xmldoc.Load(RxDuinoConsole.Properties.Resources.UpdateUrl);
                    XmlNodeList nodes = xmldoc.SelectNodes("//RxDuinoConsole/" + section);
                    XmlNode node = nodes[0];
                    XmlNodeList children = node.ChildNodes;

                    // For each child found, save the name of the child, and the value
                    foreach (XmlElement child in children)
                    {
                        elements.Add(child.Name, child.InnerText);
                    }
                }
                else
                {
                    logger.Error("Could Not Connect To Update Server");
                }
            }
            catch (WebException)
            {
                throw new WebException("Could not connect to Update Server");
            }
            catch (XmlException xe)
            {
                throw new XmlException("Error parsing Xml File, " + xe.Message);
            }

            return elements;
        }

        /// <summary>
        /// Check if the remote file exists
        /// </summary>
        /// <param name="url">The URL of the remote file</param>
        /// <returns></returns>
        internal static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";

                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                //Returns TURE if the Status code == 200
                bool val = response.StatusCode == HttpStatusCode.OK;

                response.Close();
                return val;
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
    }
}
