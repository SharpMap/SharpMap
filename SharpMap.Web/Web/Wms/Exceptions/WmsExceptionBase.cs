using SharpMap.Web.Wms.Server;
using System;
using SharpMap.Web.Wms.Server.Handlers;

namespace SharpMap.Web.Wms.Exceptions
{
    public abstract class WmsExceptionBase : Exception, IHandlerResponse
    {
        public WmsExceptionCode ExceptionCode { get; private set; }
        protected WmsExceptionBase(string Message)
            : this(Message, WmsExceptionCode.NotApplicable)
        { }

        protected WmsExceptionBase(string Message, WmsExceptionCode ExceptionCode)
            : base(Message)
        {
            this.ExceptionCode = ExceptionCode;
        }

        public virtual void WriteToContextAndFlush(IContextResponse response)
        {
            response.Clear();
            response.ContentType = "text/xml";
            response.Write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n");
            response.Write(
                "<ServiceExceptionReport version=\"1.3.0\" xmlns=\"http://www.opengis.net/ogc\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opengis.net/ogc http://schemas.opengis.net/wms/1.3.0/exceptions_1_3_0.xsd\">\n");
            response.Write("<ServiceException");
            if (ExceptionCode != WmsExceptionCode.NotApplicable)
                response.Write(" code=\"" + ExceptionCode + "\"");
            response.Write(">" + Message + "</ServiceException>\n");
            response.Write("</ServiceExceptionReport>");
            response.End();
        }
    }
}
