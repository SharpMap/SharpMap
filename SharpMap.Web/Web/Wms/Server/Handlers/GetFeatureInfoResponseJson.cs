namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoResponseJson : GetFeatureInfoResponse
    {
        public GetFeatureInfoResponseJson(string response)
            : base(response)
        {
            BufferOutput = true;
        }

        public override string ContentType
        {
            get { return "text/json"; }
        }
    }
}