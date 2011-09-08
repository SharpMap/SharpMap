//This example requires references to 
//SharpMap.dll (or project)
//SharpMap.Extensions.dll (or project) 
//NetTopologySuite.dll (from the ExternalReferences directory)
//GeoAPI.dll (from the ExternalReferences directory)

#region Using Statements

using NetTopologySuite.Geometries;
using SharpMap.Converters.NTS;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SMGeometry=SharpMap.Geometries.Geometry;

#endregion

namespace ExampleCodeSnippets
{
    using System;

    /// <summary>
    /// Contains two methods for doing true intersection testing on geometries using NetTopologySuite
    /// </summary>
    public class TrueIntersectionTestingWithNTS
    {
        /// <summary>
        /// This method returns a FeatureDataTable containing all the rows from the shapefile that intersect the testGeometry.
        /// The ShapeFile.ExecuteIntersectionQuery method only tests bounding boxes so we use the FilterDelegate property to add a true 
        /// intersection test using NetTopologySuite
        /// </summary>
        /// <param name="pathToShapefile">The path to the shapefile</param>
        /// <param name="testGeometry">The geometry that we want to test against</param>
        /// <returns></returns>
        public FeatureDataTable GetIntersectingFeaturesUsingFilterDelegate(string pathToShapefile, SMGeometry testGeometry)
        {
            //create a new shapefile provider 
            using (ShapeFile shapefile = new ShapeFile(pathToShapefile))
            {
                //create an nts GeometryFactory
                GeometryFactory geometryFactory = new GeometryFactory();

                //convert the testGeometry into the equivalent NTS geometry
                GeoAPI.Geometries.IGeometry testGeometryAsNtsGeom = GeometryConverter.ToNTSGeometry(testGeometry, geometryFactory);

                SMGeometry check = GeometryConverter.ToSharpMapGeometry(testGeometryAsNtsGeom);
                if (!check.Equals(testGeometry))
                    throw new ApplicationException("conversion error");

                //set the shapefile providers' FilterDelegate property to a new anonymous method
                //this delegate method will be called for each potential row
                shapefile.FilterDelegate = delegate(FeatureDataRow featureDataRow)
                                               {
                                                   //get the geometry from the featureDataRow
                                                   SMGeometry rowGeometry = featureDataRow.Geometry;
                                                   //convert it to the equivalent NTS geometry
                                                   GeoAPI.Geometries.IGeometry compareGeometryAsNtsGeometry =
                                                           GeometryConverter.ToNTSGeometry(rowGeometry, geometryFactory);
                                                   //do the test. Note that the testGeometryAsNtsGeometry is available here because it is 
                                                   //declared in the same scope as the anonymous method.
                                                   bool intersects =
                                                       testGeometryAsNtsGeom.Intersects(compareGeometryAsNtsGeometry);
                                                   //return the result
                                                   return intersects;
                                               };


                //create a new FeatureDataSet
                FeatureDataSet featureDataSet = new FeatureDataSet();
                //open the shapefile
                shapefile.Open();
                //call ExecuteIntersectionQuery. The FilterDelegate will be used to limit the result set
                shapefile.ExecuteIntersectionQuery(testGeometry, featureDataSet);
                //close the shapefile
                shapefile.Close();
                //return the populated FeatureDataTable
                return featureDataSet.Tables[0];
            }
        }

        /// <summary>
        /// This method takes a pre-populated FeatureDataTable and removes rows that do not truly intersect testGeometry
        /// </summary>
        /// <param name="featureDataTable">The FeatureDataTable instance to filter</param>
        /// <param name="testGeometry">the geometry to compare against</param>
        public void PostFilterExistingFeatureDataTable(FeatureDataTable featureDataTable, SMGeometry testGeometry)
        {
            //first we create a new GeometryFactory.
            var geometryFactory = new GeometryFactory();


            //then we convert the testGeometry into the equivalent NTS geometry
            GeoAPI.Geometries.IGeometry testGeometryAsNtsGeom = GeometryConverter.ToNTSGeometry(testGeometry, geometryFactory);


            //now we loop backwards through the FeatureDataTable 
            for (int i = featureDataTable.Rows.Count - 1; i > -1; i--)
            {
                //we get each row
                FeatureDataRow featureDataRow = featureDataTable.Rows[i] as FeatureDataRow;
                //and get the rows' geometry
                SMGeometry compareGeometry = featureDataRow.Geometry;
                //convert the rows' geometry into the equivalent NTS geometry
                GeoAPI.Geometries.IGeometry compareGeometryAsNts = GeometryConverter.ToNTSGeometry(compareGeometry, geometryFactory);
                //now test for intesection (note other operations such as Contains, Within, Disjoint etc can all be done the same way)
                bool intersects = testGeometryAsNtsGeom.Intersects(compareGeometryAsNts);

                //if it doesn't intersect remove the row.
                if (!intersects)
                    featureDataTable.Rows.RemoveAt(i);
            }
        }
    }
}