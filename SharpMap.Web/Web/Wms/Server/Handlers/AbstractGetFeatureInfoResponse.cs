using System;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public abstract class AbstractGetFeatureInfoResponse : IHandlerResponse
    {
        private readonly string _response;
        private string _charset;
        private readonly bool _bufferOutput;

        protected AbstractGetFeatureInfoResponse(string response, bool bufferOutput)
        {
            if (String.IsNullOrEmpty(response))
                throw new ArgumentNullException("response");
            _response = response;
            _bufferOutput = bufferOutput;
        }

        protected AbstractGetFeatureInfoResponse(string response) : 
            this(response, false) { }

        public abstract string ContentType { get; }

        public string Charset
        {
            get { return _charset; }
            set { _charset = value; }
        }

        public string Response
        {
            get { return _response; }
        }

        public void WriteToContextAndFlush(IContextResponse response)
        {
            response.Clear();
            if (Charset != null)
            {
                //"windows-1252";
                response.Charset = Charset;
            }
            response.BufferOutput = _bufferOutput;
            response.Write(Response);
            response.End();
        }
    }
}