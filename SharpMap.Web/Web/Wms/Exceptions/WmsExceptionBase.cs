using SharpMap.Web.Wms.Server;
using System;

namespace SharpMap.Web.Wms.Exceptions
{
    public abstract class WmsExceptionBase : Exception
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

        public virtual void WriteToContextAndFlush(IContext context)
        {
            context.Clear();
            context.ContentType = "text/xml";
            context.Write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n");
            context.Write(
                "<ServiceExceptionReport version=\"1.3.0\" xmlns=\"http://www.opengis.net/ogc\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opengis.net/ogc http://schemas.opengis.net/wms/1.3.0/exceptions_1_3_0.xsd\">\n");
            context.Write("<ServiceException");
            if (ExceptionCode != WmsExceptionCode.NotApplicable)
                context.Write(" code=\"" + ExceptionCode + "\"");
            context.Write(">" + Message + "</ServiceException>\n");
            context.Write("</ServiceExceptionReport>");
            context.End();
        }
    }
}
