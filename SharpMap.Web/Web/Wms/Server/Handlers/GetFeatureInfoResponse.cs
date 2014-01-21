using System;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public abstract class GetFeatureInfoResponse : IHandlerResponse
    {
        private readonly string _response;
        private string _charset;
        private bool _bufferOutput;

        protected GetFeatureInfoResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
                throw new ArgumentNullException("response");
            _response = response;
        }

        public abstract string ContentType { get; }

        public string Charset
        {
            get { return _charset; }
            set { _charset = value; }
        }

        public bool BufferOutput
        {
            get { return _bufferOutput; }
            protected set { _bufferOutput = value; }
        }

        public void WriteToContextAndFlush(IContextResponse response)
        {
            response.Clear();
            if (Charset != null)
            {
                //"windows-1252";
                response.Charset = Charset;
            }
            response.BufferOutput = BufferOutput;
            response.Write(_response);
            response.End();
        }
    }
}