// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class provides an easy to use interface for HTTP-GET and HTTP-POST requests.
    /// </summary>
    public class HttpClientUtil
    {
        #region Fields and Properties

        private readonly NameValueCollection _requestHeaders;
        private byte[] _postData;
        private string _proxyUrl;

        private string _url;
        private HttpWebRequest _webRequest;
        private HttpWebResponse _webResponse;

        /// <summary>
        /// Gets ans sets the Url of the request.
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// Gets and sets the proxy Url of the request. 
        /// </summary>
        public string ProxyUrl
        {
            get { return _proxyUrl; }
            set { _proxyUrl = value; }
        }

        /// <summary>
        /// Sets the data of a HTTP POST request as byte array.
        /// </summary>
        public byte[] PostData
        {
            set { _postData = value; }
        }

        /// <summary>
        /// Gets or sets the network credentials used for authenticating the request with the Internet resource
        /// </summary>
        public ICredentials Credentials { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientUtil"/> class.
        /// </summary>
        public HttpClientUtil()
        {
            _requestHeaders = new NameValueCollection();
        }

        #endregion

        #region Public Member

        /// <summary>
        /// Adds a HTTP header.
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <param name="value">The value of the header</param>
        public void AddHeader(string name, string value)
        {
            _requestHeaders.Add(name, value);
        }

        /// <summary>
        /// Performs a HTTP-GET or HTTP-POST request and returns a datastream for reading.
        /// </summary>
        public Stream GetDataStream()
        {
            if (string.IsNullOrEmpty(_url))
                throw new ArgumentNullException("Request Url is not set!");

            // Free all resources of the previous request, if it hasn't been done yet...
            Close();

            try
            {
                _webRequest = (HttpWebRequest) WebRequest.Create(_url);
            }
            catch (SecurityException ex)
            {
                Trace.TraceError("An exception occured due to security reasons while initializing a request to " + _url +
                                 ": " + ex.Message);
                throw ex;
            }
            catch (NotSupportedException ex)
            {
                Trace.TraceError("An exception occured while initializing a request to " + _url + ": " + ex.Message);
                throw ex;
            }

            _webRequest.Timeout = 90000;

            if (!string.IsNullOrEmpty(_proxyUrl))
                _webRequest.Proxy = new WebProxy(_proxyUrl);

            try
            {
                _webRequest.Headers.Add(_requestHeaders);

                if (Credentials != null)
                {
                    _webRequest.UseDefaultCredentials = false;
                    _webRequest.Credentials = Credentials;
                }

                /* HTTP POST */
                if (_postData != null)
                {
                    _webRequest.ContentLength = _postData.Length;
                    _webRequest.Method = WebRequestMethods.Http.Post;
                    using (Stream requestStream = _webRequest.GetRequestStream())
                    {
                        requestStream.Write(_postData, 0, _postData.Length);
                    }
                }
                    /* HTTP GET */
                else
                    _webRequest.Method = WebRequestMethods.Http.Get;

                _webResponse = (HttpWebResponse) _webRequest.GetResponse();
                return _webResponse.GetResponseStream();
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured during a HTTP request to " + _url + ": " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// This method resets all configurations.
        /// </summary>
        public void Reset()
        {
            _url = null;
            _proxyUrl = null;
            _postData = null;
            _requestHeaders.Clear();
        }

        /// <summary>
        /// This method closes the WebResponse object.
        /// </summary>
        public void Close()
        {
            if (_webResponse != null)
            {
                /*
                 * The Close method closes the response stream and releases the connection 
                 * to the resource for reuse by other requests.
                 * 
                 * You must call either the Stream.Close or the HttpWebResponse.Close 
                 * method to close the stream and release the connection for reuse. 
                 * It is not necessary to call both Stream.Close and HttpWebResponse.Close,
                 * but doing so does not cause an error. Failure to close the stream 
                 * can cause your application to run out of connections. 
                 */
                _webResponse.Close();
                //_webResponse.GetResponseStream().Dispose();
            }
        }

        #endregion
    }
}