<%@ WebHandler Language="C#" Class="wms" %>

using System.Drawing;
using System.Web;
using SharpMap;
using SharpMap.Web.Wms;

public class wms : IHttpHandler
{
    #region IHttpHandler Members

    public void ProcessRequest(HttpContext context)
    {
        //Get the path of this page
        string url = (context.Request.Url.Query.Length > 0
                          ? context.Request.Url.AbsoluteUri.Replace(context.Request.Url.Query, "")
                          : context.Request.Url.AbsoluteUri);
        Capabilities.WmsServiceDescription description =
            new Capabilities.WmsServiceDescription("Acme Corp. Map Server", url);

        // The following service descriptions below are not strictly required by the WMS specification.

        // Narrative description and keywords providing additional information 
        description.Abstract =
            "Map Server maintained by Acme Corporation. Contact: webmaster@wmt.acme.com. High-quality maps showing roadrunner nests and possible ambush locations.";
        description.Keywords = new string[3];
        description.Keywords[0] = "bird";
        description.Keywords[1] = "roadrunner";
        description.Keywords[2] = "ambush";

        //Contact information 
        description.ContactInformation.PersonPrimary.Person = "John Doe";
        description.ContactInformation.PersonPrimary.Organisation = "Acme Inc";
        description.ContactInformation.Address.AddressType = "postal";
        description.ContactInformation.Address.Country = "Neverland";
        description.ContactInformation.VoiceTelephone = "1-800-WE DO MAPS";
        //Impose WMS constraints
        description.MaxWidth = 1000; //Set image request size width
        description.MaxHeight = 500; //Set image request size height


        //Call method that sets up the map
        //We just add a dummy-size, since the wms requests will set the image-size
        Map myMap = MapHelper.InitializeMap(new Size(1, 1));

        //Parse the request and create a response
        WmsServer.ParseQueryString(myMap, description,1,PostFilterExistingFeatureDataTable);
    }

    public bool IsReusable
    {
        get { return false; }
    }
	/// <summary>
    /// This method takes a pre-populated FeatureDataTable and removes rows that do not truly intersect testGeometry
    /// </summary>
    /// <param name="featureDataTable">The FeatureDataTable instance to filter</param>
    /// <param name="testEnvelope">the envelope to compare against</param>
    public SharpMap.Data.FeatureDataTable PostFilterExistingFeatureDataTable(SharpMap.Data.FeatureDataTable featureDataTable, GeoAPI.Geometries.Envelope testEnvelope)
    {
        if (featureDataTable == null || featureDataTable.Rows.Count == 0)
            return featureDataTable;
        
        //first we get a GeometryFactory.
        var geometryFactory = ((SharpMap.Data.FeatureDataRow)featureDataTable.Rows[0]).Geometry.Factory;

        //create a test polygon from testenvelope
	    var testPolygon = geometryFactory.ToGeometry(testEnvelope);
        
        //make a prepared geometry from the boundingbox
	    var pp = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(testPolygon);

        //now we loop backwards through the FeatureDataTable 
        for (var i = featureDataTable.Rows.Count - 1; i > -1; i--)
        {
            //we get each row
            var featureDataRow = featureDataTable.Rows[i] as SharpMap.Data.FeatureDataRow;
            if (featureDataRow != null)
            {
                //and get the rows' geometry
                GeoAPI.Geometries.IGeometry compareGeometry = featureDataRow.Geometry;

                //now test for intesection (note other operations such as Contains, Within, Disjoint etc can all be done the same way)
                var intersects = pp.Intersects(compareGeometry);

                //if it doesn't intersect remove the row.
                if (!intersects)
                    featureDataTable.Rows.RemoveAt(i);
            }
            else
            {
                featureDataTable.Rows.RemoveAt(i);
            }
        }
        return featureDataTable;
    }

    #endregion
}