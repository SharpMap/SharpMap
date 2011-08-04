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
    /// <param name="testGeometry">the geometry to compare against</param>
    public SharpMap.Data.FeatureDataTable PostFilterExistingFeatureDataTable(SharpMap.Data.FeatureDataTable featureDataTable, SharpMap.Geometries.BoundingBox testGeometry)
    {
        //make a geometry from the boundingbox
        SharpMap.Geometries.Polygon envelope = new SharpMap.Geometries.Polygon();
        envelope.ExteriorRing.Vertices.Add(testGeometry.Min); //minx miny
        envelope.ExteriorRing.Vertices.Add(new SharpMap.Geometries.Point(testGeometry.Max.X, testGeometry.Min.Y)); //maxx miny
        envelope.ExteriorRing.Vertices.Add(testGeometry.Max); //maxx maxy
        envelope.ExteriorRing.Vertices.Add(new SharpMap.Geometries.Point(testGeometry.Min.X, testGeometry.Max.Y)); //minx maxy
        envelope.ExteriorRing.Vertices.Add(envelope.ExteriorRing.StartPoint); //close ring
        
        
        //first we create a new GeometryFactory.
        GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory geometryFactory = new GisSharpBlog.NetTopologySuite.Geometries.GeometryFactory();


        //then we convert the testGeometry into the equivalent NTS geometry
        GeoAPI.Geometries.IGeometry testGeometryAsNtsGeom =
            SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(envelope, geometryFactory);


        //now we loop backwards through the FeatureDataTable 
        for (int i = featureDataTable.Rows.Count - 1; i > -1; i--)
        {
            //we get each row
            SharpMap.Data.FeatureDataRow featureDataRow = featureDataTable.Rows[i] as SharpMap.Data.FeatureDataRow;
            //and get the rows' geometry
            SharpMap.Geometries.Geometry compareGeometry = featureDataRow.Geometry;
            //convert the rows' geometry into the equivalent NTS geometry
            GeoAPI.Geometries.IGeometry compareGeometryAsNts =
                SharpMap.Converters.NTS.GeometryConverter.ToNTSGeometry(compareGeometry, geometryFactory);
            //now test for intesection (note other operations such as Contains, Within, Disjoint etc can all be done the same way)
            bool intersects = compareGeometryAsNts.Intersects(testGeometryAsNtsGeom);

            //if it doesn't intersect remove the row.
            if (!intersects)
                featureDataTable.Rows.RemoveAt(i);
        }
        return featureDataTable;
    }

    #endregion
}