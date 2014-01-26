namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoResponseHtml : AbstractGetFeatureInfoResponse
    {
        public GetFeatureInfoResponseHtml(string response) :
            base(response) { }

        public override string ContentType
        {
            get { return "text/html"; }
        }
    }
}