using System;
using SharpMap.Data;
using SharpMap.Geometries;

namespace ExampleCodeSnippets
{
    /// <summary>
    /// Examples of creating spatial data 
    /// </summary>
    public class CreatingData
    {
        /// <summary>
        /// Creates a FeatureDataTable from arrays of x, y and z components
        /// </summary>
        /// <param name="xcomponents">an array of doubles representing the x ordinate values</param>
        /// <param name="ycomponents">an array of doubles representing the y ordinate values</param>
        /// <param name="zcomponents">an array of doubles representing the z ordinate values</param>
        /// <returns></returns>
        public FeatureDataTable CreatePointFeatureDataTableFromArrays(double[] xcomponents, 
                                                                 double[] ycomponents,
                                                                 double[] zcomponents)
        {
            bool threedee = false;
            if (zcomponents != null)
            {
                if (!(zcomponents.Length == ycomponents.Length && zcomponents.Length == xcomponents.Length))
                    throw new ApplicationException("Mismatched Array Lengths");

                threedee = true;
            }
            else
            {
                if (!(ycomponents.Length == xcomponents.Length))

                    throw new ApplicationException("Mismatched Array Lengths");
            }

            FeatureDataTable fdt = new FeatureDataTable();
            fdt.Columns.Add("TimeStamp", typeof (DateTime)); // add example timestamp attribute
            for (int i = 0; i < xcomponents.Length; i++)
            {
                FeatureDataRow fdr = fdt.NewRow();

                fdr.Geometry = threedee
                                   ? new Point3D(xcomponents[i], ycomponents[i], zcomponents[i])
                                   : new Point(xcomponents[i], ycomponents[i]);

                fdt.AddRow(fdr);
                fdr["TimeStamp"] = DateTime.Now; //set the timestamp property
            }

            return fdt;
        }
    }
}