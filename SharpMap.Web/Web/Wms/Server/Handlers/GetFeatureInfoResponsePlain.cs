namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoResponsePlain : GetFeatureInfoResponse
    {
        public GetFeatureInfoResponsePlain(string response) : base(response) { }

        public override string ContentType
        {
            get { return "text/plain"; }
        }
    }
}