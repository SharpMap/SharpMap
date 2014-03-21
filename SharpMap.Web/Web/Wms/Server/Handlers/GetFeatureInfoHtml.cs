using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using GeoAPI.Features;
using SharpMap.Data;
using SharpMap.Layers;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public class GetFeatureInfoHtml : AbstractGetFeatureInfoText
    {
        private const char NewLine = '\n';

        private const string HtmlTemplate = "<html>\n<head>\n<title>GetFeatureInfo output</title>\n</head>\n<style type=\'text/css\'>\n  table.featureInfo, table.featureInfo td, table.featureInfo th {\n  border:1px solid #ddd;\n  border-collapse:collapse;\n  margin:0;\n  padding:0;\n  font-size: 90%;\n  padding:.2em .1em;\n}\ntable.featureInfo th {\n  padding:.2em .2em;\n  font-weight:bold;\n  background:#eee;\n}\ntable.featureInfo td {\n  background:#fff;\n}\ntable.featureInfo tr.odd td {\n  background:#eee;\n}\ntable.featureInfo caption {\n  text-align:left;\n  font-size:100%;\n  font-weight:bold;\n  padding:.2em .2em;\n}\n</style>\n<body>\n{{HTML_BODY}}\n</body>\n</html>";

        private const string TableTemplate = "<table class=\'featureInfo\'>\n<caption class=\'featureInfo\'>{{LAYER_NAME}}</caption>\n{{LAYER_TABLE}}</table>";
        private const string CssOdd = " class='odd'";
        private const string CssEven = " class='even'";

        public GetFeatureInfoHtml(Capabilities.WmsServiceDescription description) :
            base(description) { }

        public GetFeatureInfoHtml(Capabilities.WmsServiceDescription description,
            GetFeatureInfoParams @params)
            : base(description, @params) { }

        protected override AbstractGetFeatureInfoResponse CreateFeatureInfo(Map map,
            IEnumerable<string> requestedLayers,
            float x, float y,
            int featureCount,
            string cqlFilter,
            int pixelSensitivity,
            WmsServer.InterSectDelegate intersectDelegate)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string requestLayer in requestedLayers)
            {
                ICanQueryLayer layer = GetQueryLayer(map, requestLayer);
                IFeatureCollectionSet fds;
                if (!TryGetData(map, x, y, pixelSensitivity, intersectDelegate, layer, cqlFilter, out fds))
                    continue;

                var table = fds[0];
                var attributeDefinition = table[0].Factory.AttributesDefinition;
                StringBuilder sbTable = new StringBuilder();
                sbTable.AppendFormat("<tr>{0}", NewLine);
                foreach (IFeatureAttributeDefinition col in attributeDefinition)
                    sbTable.AppendFormat("<th>{0}</th>{1}", col.AttributeName, NewLine);
                sbTable.AppendFormat("</tr>{0}", NewLine);
                string rowsText = GetRowsText(table, featureCount);
                sbTable.Append(rowsText);

                string tpl = TableTemplate.
                    Replace("{{LAYER_NAME}}", requestLayer).
                    Replace("{{LAYER_TABLE}}", sbTable.ToString());
                sb.AppendFormat("{0}\n{1}\n", tpl, "<br />");
            }


            string html = HtmlTemplate.Replace("{{HTML_BODY}}", sb.ToString());
            return new GetFeatureInfoResponseHtml(html);
        }

        protected override string FormatRows(IFeatureCollection rows, int[] keys, int maxFeatures)
        {
            StringBuilder sb = new StringBuilder();
            var attributeDefinition = rows[0].Factory.AttributesDefinition;
            for (int k = 0; k < maxFeatures; k++)
            {
                string css = k % 2 == 0 ? CssEven : CssOdd;
                int key = keys[k];
                sb.AppendFormat("<tr{0}>{1}", css, NewLine);
                for (var i = 0; i < attributeDefinition.Count;i++)
                    sb.AppendFormat("<td>{0}</td>{1}", rows[key].Attributes[i], NewLine);
                sb.AppendFormat("</tr>{0}", NewLine);
            }
            return sb.ToString();
        }
    }
}