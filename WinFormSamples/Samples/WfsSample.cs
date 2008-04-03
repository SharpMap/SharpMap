using System;
using System.Collections.Generic;
using System.Text;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Utilities.Wfs;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.Geometries;

namespace WinFormSamples.Samples
{
  public static class WfsSample
  {
    public static Map InitializeMap()
    {
      try
      {
        
        // WARNING
        // This sample needs the GeoServer WFS running on your local machine. 
        // It uses the GeoServer default sample data. Installing and starting it is all you need to do
        // http://docs.codehaus.org/display/GEOS/Download

        // Sample by Peter Robineau

        const string getCapabilitiesURI = "http://localhost:8080/geoserver/wfs";
        const string serviceURI = "http://localhost:8080/geoserver/wfs";

        Map map = new Map(new System.Drawing.Size(600, 600));
        map.MinimumZoom = 0.005;
        map.BackColor = System.Drawing.Color.White;

        VectorLayer layer1 = new VectorLayer("States");
        VectorLayer layer2 = new VectorLayer("SelectedStatesAndHousholds");
        VectorLayer layer3 = new VectorLayer("New Jersey");
        VectorLayer layer4 = new VectorLayer("Roads");
        VectorLayer layer5 = new VectorLayer("Landmarks");
        VectorLayer layer6 = new VectorLayer("Poi");

        // Demo data from Geoserver 1.5.3 and Geoserver 1.6.0 

        WFS prov1 = new WFS(getCapabilitiesURI, "topp", "states", WFS.WFSVersionEnum.WFS1_0_0);

        // Bypass 'GetCapabilities' and 'DescribeFeatureType', if you know all necessary metadata.
        WfsFeatureTypeInfo featureTypeInfo = new WfsFeatureTypeInfo(serviceURI, "topp", null, "states", "the_geom");
        // 'WFS.WFSVersionEnum.WFS1_1_0' supported by Geoserver 1.6.x
        WFS prov2 = new SharpMap.Data.Providers.WFS(featureTypeInfo, WFS.WFSVersionEnum.WFS1_1_0);
        // Bypass 'GetCapabilities' and 'DescribeFeatureType' again...
        // It's possible to specify the geometry type, if 'DescribeFeatureType' does not...(.e.g 'GeometryAssociationType')
        // This helps to accelerate the initialization process in case of unprecise geometry information.
        WFS prov3 = new WFS(serviceURI, "topp", "http://www.openplans.org/topp", "states", "the_geom", GeometryTypeEnum.MultiSurfacePropertyType, WFS.WFSVersionEnum.WFS1_1_0);

        // Get data-filled FeatureTypeInfo after initialization of dataprovider (useful in Web Applications for caching metadata.
        WfsFeatureTypeInfo info = prov1.FeatureTypeInfo;

        // Use cached 'GetCapabilities' response of prov1 (featuretype hosted by same service).
        // Compiled XPath expressions are re-used automatically!
        // If you use a cached 'GetCapabilities' response make sure the data provider uses the same version of WFS as the one providing the cache!!!
        WFS prov4 = new WFS(prov1.GetCapabilitiesCache, "tiger", "tiger_roads", WFS.WFSVersionEnum.WFS1_0_0);
        WFS prov5 = new WFS(prov1.GetCapabilitiesCache, "tiger", "poly_landmarks", WFS.WFSVersionEnum.WFS1_0_0);
        WFS prov6 = new WFS(prov1.GetCapabilitiesCache, "tiger", "poi", WFS.WFSVersionEnum.WFS1_0_0);
        // Clear cache of prov1 - data providers do not have any cache, if they use the one of another data provider  
        prov1.GetCapabilitiesCache = null;

        //Filters
        IFilter filter1 = new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "California");
        IFilter filter2 = new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "Vermont");
        IFilter filter3 = new PropertyIsBetweenFilter_FE1_1_0("HOUSHOLD", "600000", "4000000");
        IFilter filter4 = new PropertyIsLikeFilter_FE1_1_0("STATE_NAME", "New*");

        // SelectedStatesAndHousholds: Green
        OGCFilterCollection filterCollection1 = new OGCFilterCollection();
        filterCollection1.AddFilter(filter1);
        filterCollection1.AddFilter(filter2);
        OGCFilterCollection filterCollection2 = new OGCFilterCollection();
        filterCollection2.AddFilter(filter3);
        filterCollection1.AddFilterCollection(filterCollection2);
        filterCollection1.Junctor = OGCFilterCollection.JunctorEnum.Or;
        prov2.OGCFilter = filterCollection1;

        // Like-Filter('New*'): Bisque
        prov3.OGCFilter = filter4;

        // Layer Style
        layer1.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.IndianRed);    // States
        layer2.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Green); // SelectedStatesAndHousholds
        layer3.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Bisque); // e.g. New York, New Jersey,...
        layer5.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.LightBlue);

        // Labels
        // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
        // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
        // If a label property is set, the quick geometry option is automatically set to 'false'.
        prov3.Label = "STATE_NAME";
        LabelLayer layLabel = new LabelLayer("labels");
        layLabel.DataSource = prov3;
        layLabel.Enabled = true;
        layLabel.LabelColumn = prov3.Label;
        layLabel.Style = new LabelStyle();
        layLabel.Style.CollisionDetection = false;
        layLabel.Style.CollisionBuffer = new System.Drawing.SizeF(5, 5);
        layLabel.Style.ForeColor = System.Drawing.Color.Black;
        layLabel.Style.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 10);
        layLabel.MaxVisible = 90;
        layLabel.Style.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
        // Options 
        // Defaults: MultiGeometries: true, QuickGeometries: false, GetFeatureGETRequest: false
        // Render with validation...
        prov1.QuickGeometries = false;
        // Important when connecting to an UMN MapServer
        prov1.GetFeatureGETRequest = true;
        // Ignore multi-geometries...
        prov1.MultiGeometries = false;

        // Quick geometries
        // We need this option for prov2 since we have not passed a featuretype namespace
        prov2.QuickGeometries = true;
        prov4.QuickGeometries = true;
        prov5.QuickGeometries = true;
        prov6.QuickGeometries = true;

        layer1.DataSource = prov1;
        layer2.DataSource = prov2;
        layer3.DataSource = prov3;
        layer4.DataSource = prov4;
        layer5.DataSource = prov5;
        layer6.DataSource = prov6;

        map.Layers.Add(layer1);
        map.Layers.Add(layer2);
        map.Layers.Add(layer3);
        map.Layers.Add(layer4);
        map.Layers.Add(layer5);
        map.Layers.Add(layer6);
        map.Layers.Add(layLabel);

        map.Center = new Point(-74.0, 40.7);
        map.Zoom = 10;
        // Alternatively zoom closer
        // demoMap.Zoom = 0.2;

        return map;
      }
      catch (System.Net.WebException ex)
      {
        if ((ex.Message.Contains("(502) Bad Gateway")) || 
          (ex.Message.Contains("Unable to connect to the remote server")))
        {
          throw new Exception("The Wfs sample threw an exception. You probably need to install the GeoServer WFS to your local machine. You can get it from here: http://docs.codehaus.org/display/GEOS/Download. The exception message was: " + ex.Message);
        }
        else throw;
      }

    }
  }
}
