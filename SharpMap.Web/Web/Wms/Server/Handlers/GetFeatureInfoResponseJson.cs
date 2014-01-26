namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoResponseJson : AbstractGetFeatureInfoResponse
    {
        public GetFeatureInfoResponseJson(string response) :
            base(response, true) { }

        public override string ContentType
        {
            get { return "text/json"; }
        }
    }
}