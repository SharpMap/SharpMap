// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// XPathQueryManager interface
    /// </summary>
    public interface IXPathQueryManager
    {
        /// <summary>
        /// Method to add/register an xml-namespace
        /// </summary>
        /// <param name="prefix">The prefix used to identify the namespace</param>
        /// <param name="ns">The full namespace qualifier</param>
        void AddNamespace(string prefix, string ns);
        
        /// <summary>
        /// Method to compile a <paramref name="xPath"/> to a <see cref="XPathExpression"/>
        /// </summary>
        /// <param name="xPath">The xpath to compile</param>
        /// <returns>The compiled <see cref="XPathExpression"/></returns>
        XPathExpression Compile(string xPath);
        
        /// <summary>
        /// Function to create a deep-copy clone of this <see cref="IXPathQueryManager"/>
        /// </summary>
        /// <returns>An <see cref="IXPathQueryManager"/> instance resembling this one.</returns>
        IXPathQueryManager Clone();

        /// <summary>
        /// Function to get an iterator over <paramref name="xPath"/>
        /// </summary>
        /// <param name="xPath">The xPath expression to iterate</param>
        /// <returns>An <see cref="XPathNodeIterator"/> iterator over <paramref name="xPath"/></returns>
        XPathNodeIterator GetIterator(XPathExpression xPath);

        /// <summary>
        /// Function to get an iterator over <paramref name="xPath"/>, narrowed by <paramref name="queryParameters"/>
        /// </summary>
        /// <param name="xPath">The xPath expression to iterate</param>
        /// <param name="queryParameters">The parameters to narrow the <paramref name="xPath"/></param>
        /// <returns>An <see cref="XPathNodeIterator"/> iterator over <paramref name="xPath"/></returns>
        XPathNodeIterator GetIterator(XPathExpression xPath, DictionaryEntry[] queryParameters);

        /// <summary>
        /// Function to get a value from  <paramref name="xPath"/>
        /// </summary>
        /// <param name="xPath">The <see cref="XPathExpression"/> to get the value from</param>
        /// <returns>A string value</returns>
        string GetValueFromNode(XPathExpression xPath);

        /// <summary>
        /// Function to get a value from  <paramref name="xPath"/>
        /// </summary>
        /// <param name="xPath">The <see cref="XPathExpression"/> to get the value from</param>
        /// <param name="queryParameters">The parameters for narrowing the <paramref name="xPath"/></param>
        /// <returns>A string value</returns>
        string GetValueFromNode(XPathExpression xPath, DictionaryEntry[] queryParameters);

        /// <summary>
        /// Function to get a list of value from  <paramref name="xPath"/>
        /// </summary>
        /// <param name="xPath">The <see cref="XPathExpression"/> to get the values from</param>
        /// <returns>A  list of string value</returns>
        List<string> GetValuesFromNodes(XPathExpression xPath);

        /// <summary>
        /// Function to get a list of value from  <paramref name="xPath"/>
        /// </summary>
        /// <param name="xPath">The <see cref="XPathExpression"/> to get the values from</param>
        /// <param name="queryParameters">The parameters for narrowing the <paramref name="xPath"/></param>
        /// <returns>A  list of string value</returns>
        List<string> GetValuesFromNodes(XPathExpression xPath, DictionaryEntry[] queryParameters);

        /// <summary>
        /// Function to return an instance of <see cref="XPathQueryManager"/> 
        /// in the context of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <returns>A <see cref="IXPathQueryManager"/></returns>
        IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath);

        /// <summary>
        /// Function to return an instance of <see cref="XPathQueryManager"/> 
        /// in the context of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        /// <returns>A <see cref="IXPathQueryManager"/></returns>
        IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath, DictionaryEntry[] queryParameters);

        /// <summary>
        /// This method moves the current instance of <see cref="XPathQueryManager"/> 
        /// to the context of the next node a previously handed over XPath expression addresses.
        /// </summary>
        bool GetContextOfNextNode();

        /// <summary>
        /// This method moves the current instance of <see cref="XPathQueryManager"/> 
        /// to the context of node[index] of current position.
        /// </summary>
        /// <param name="index">The index of the node to search</param>
        bool GetContextOfNode(uint index);

        /// <summary>
        /// Method to reset/delete the current namespace context
        /// </summary>
        void ResetNamespaces();
        
        /// <summary>
        /// Method to reset the inherent <see cref="XPathNavigator"/>.
        /// </summary>
        void ResetNavigator();

        /// <summary>
        /// Method to set the document to parse to <paramref name="documentStream"/>
        /// </summary>
        /// <param name="documentStream">The document stream</param>
        void SetDocumentToParse(Stream documentStream);

        /// <summary>
        /// Method to set the document to parse to <paramref name="document"/>
        /// </summary>
        /// <param name="document">The document stream</param>
        void SetDocumentToParse(byte[] document);

        /// <summary>
        /// Method to set the document to sth defined by <see cref="HttpClientUtil"/>
        /// </summary>
        /// <param name="httpClientUtil">The name of the file to parse</param>
        void SetDocumentToParse(HttpClientUtil httpClientUtil);

        /// <summary>
        /// Method to set the document to parse to a file named <paramref name="documentFilename"/>
        /// </summary>
        /// <param name="documentFilename">The name of the file to parse</param>
        void SetDocumentToParse(string documentFilename);
    }
}