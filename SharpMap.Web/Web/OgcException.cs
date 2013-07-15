using System;
using System.Collections.Generic;

namespace SharpMap.Web
{
    /// <summary>
    /// The ServiceExceptionReport element contains one
    /// or more ServiceException elements that describe
    /// a service exception.
    /// </summary>
    [Serializable]
    public class ServiceExceptionReport
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ServiceExceptionReport(string version)
        {
            ServiceException = new List<ServiceExceptionType>();
            Version = version;
        }
        /// <summary>
        /// The Service exception element is used to describe 
        /// a service exception.
        /// </summary>
        public List<ServiceExceptionType> ServiceException { get; private set; }

        /// <summary>
        /// Gets the version number
        /// </summary>
        public string Version { get ; private set; }
    }
    
    /// <summary>
    /// The ServiceExceptionType type defines the ServiceException
    /// element.  The content of the element is an exception message
    /// that the service wished to convey to the client application.
    /// </summary>
    [Serializable]
    public struct ServiceExceptionType
    {
        /// <summary>
        /// A service may associate a code with an exception
        /// by using the code attribute.
        /// </summary>
        public string Code { get; set; }

        /// <summary>                     
        /// The locator attribute may be used by a service to
        /// indicate to a client where in the client's request
        /// an exception was encountered.  If the request included
        /// a 'handle' attribute, this may be used to identify the
        /// offending component of the request.  Otherwise the 
        /// service may try to use other means to locate the 
        /// exception such as line numbers or byte offset from the
        /// begining of the request, etc ...
        /// </summary>
        public string Locator { get; set; }
    }
}