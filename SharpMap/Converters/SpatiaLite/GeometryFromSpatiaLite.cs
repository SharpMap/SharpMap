// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GeoAPI.Geometries;

namespace SharpMap.Converters.SpatiaLite
{
    /// <summary>
    /// Converter of SpatiaLite geometries to NTS
    /// </summary>
    public class GeometryFromSpatiaLite
    {
        /// <summary>
        /// See http://www.gaia-gis.it/gaia-sins/BLOB-Geometry.html
        /// for the specification of the spatialite BLOB geometry format
        /// Derived from WKB, but unfortunately it is not practical to reuse existing
        /// WKB encoding/decoding code
        /// </summary>
        /// <param name="spatialliteGeom">The geometry blob</param>
        /// <param name="factory">The factory to create the result geometry</param>
        /// <returns>A geometry</returns>
        public static IGeometry Parse(byte[] spatialliteGeom, IGeometryFactory factory)
        {
            var nBytes = spatialliteGeom.Length;
            if (spatialliteGeom.Length < 44
        || spatialliteGeom[0] != 0
        || spatialliteGeom[38] != 0x7C
        || spatialliteGeom[nBytes - 1] != 0xFE)
                throw new ApplicationException("Corrupt SpatialLite geom");

            bool isLittleEndian = spatialliteGeom[1] == 0x01;
            if (spatialliteGeom[1] != 0x00 && spatialliteGeom[1] != 0x01)
                throw new ApplicationException("Corrupt SpatialLite geom");
            


            int idx = 39;
            int nGType = ReadUInt32(spatialliteGeom, ref idx, isLittleEndian);

            if (nGType < 1 || nGType > 7)
                throw new ApplicationException("Unsupported geom type!");

            /* -------------------------------------------------------------------- */
            /*      Point                                                           */
            /* -------------------------------------------------------------------- */
            if (nGType == 1)
            {
                return factory.CreatePoint(ReadPoint(spatialliteGeom, ref idx, isLittleEndian));
            }
            /* -------------------------------------------------------------------- */
            /*      LineString                                                      */
            /* -------------------------------------------------------------------- */
            else if (nGType == 2)
            {
                return ReadLineString(spatialliteGeom, ref idx, isLittleEndian, factory);
            }
            /* -------------------------------------------------------------------- */
            /*      Polygon                                                      */
            /* -------------------------------------------------------------------- */
            else if (nGType == 3)
            {
                return ReadPolygon(spatialliteGeom, ref idx, isLittleEndian, factory);
            }
            /* -------------------------------------------------------------------- */
            /*      MultiPoint                          */
            /* -------------------------------------------------------------------- */
            else if (nGType == 4)
            {
                List<GeoAPI.Geometries.IPoint> pts = new List<GeoAPI.Geometries.IPoint>();
                int numGeoms = ReadUInt32(spatialliteGeom, ref idx, isLittleEndian);
                for (int i = 0; i < numGeoms; i++)
                {
                    if (spatialliteGeom[idx] != 0x69)
                        throw new ApplicationException("FormatError in SpatiaLIteGeom");
                    idx++;
                    int gt = ReadUInt32(spatialliteGeom, ref idx, isLittleEndian);
                    if (gt != 1)
                        throw new ApplicationException("MultiPoint must Contain Point entities");

                    pts.Add(factory.CreatePoint(ReadPoint(spatialliteGeom, ref idx, isLittleEndian)));
                }
                return factory.CreateMultiPoint(pts.ToArray());
            }
            /* -------------------------------------------------------------------- */
            /*      MultiLineString                          */
            /* -------------------------------------------------------------------- */
            else if (nGType == 5)
            {
                List<GeoAPI.Geometries.ILineString> lss = new List<GeoAPI.Geometries.ILineString>();
                int numGeoms = ReadUInt32(spatialliteGeom, ref idx, isLittleEndian);
                for (int i = 0; i < numGeoms; i++)
                {
                    if (spatialliteGeom[idx] != 0x69)
                        throw new ApplicationException("FormatError in SpatiaLIteGeom");
                    idx++;
                    int gt = ReadUInt32(spatialliteGeom, ref idx, isLittleEndian);
                    if (gt != 2)
                        throw new ApplicationException("MultiLineString must contain LineString Entities");
                    lss.Add(ReadLineString(spatialliteGeom, ref idx, isLittleEndian, factory));
                }
                return factory.CreateMultiLineString(lss.ToArray());
            }
            /* -------------------------------------------------------------------- */
            /*      MultiPolygon                                                      */
            /* -------------------------------------------------------------------- */
            else if (nGType == 6)
            {
                List<GeoAPI.Geometries.IPolygon> polys = new List<GeoAPI.Geometries.IPolygon>();
                int numPolys = ReadUInt32(spatialliteGeom, ref idx, isLittleEndian);
                for (int i = 0; i < numPolys; i++)
                {
                    if (spatialliteGeom[idx] != 0x69)
                        throw new ApplicationException("FormatError in SpatiaLIteGeom");
                    idx++;
                    int gt = ReadUInt32(spatialliteGeom, ref idx, isLittleEndian);
                    if (gt != 3)
                        throw new ApplicationException("Multipolygon must contain Polygon Entities");

                    polys.Add(ReadPolygon(spatialliteGeom, ref idx, isLittleEndian, factory));
                }
                return factory.CreateMultiPolygon(polys.ToArray());
            }
            
            return null;
        }

        private static GeoAPI.Geometries.IPolygon ReadPolygon(byte[] geom, ref int idx, bool isLittleEndian, IGeometryFactory factory)
        {
            double[] adfTuple = new double[2];
            int nRings;


            nRings = ReadUInt32(geom,ref idx, isLittleEndian);

            if (nRings < 1 || nRings > Int32.MaxValue / (2 * 8))
                throw new ApplicationException("Currupt SpatialLite geom");

            List<GeoAPI.Geometries.ILineString> lineStrings = new List<GeoAPI.Geometries.ILineString>();
            for (int i = 0; i < nRings; i++)
                lineStrings.Add(ReadLineString(geom,ref idx, isLittleEndian, factory));

            List<GeoAPI.Geometries.ILinearRing> holes = null;
            var shell = factory.CreateLinearRing(lineStrings[0].Coordinates);
            if (lineStrings.Count > 1)
            {
                holes = new List<GeoAPI.Geometries.ILinearRing>();
                for (int i = 1; i < lineStrings.Count; i++)
                {
                    holes.Add(new NetTopologySuite.Geometries.LinearRing(lineStrings[i].Coordinates));
                }
            }
            return factory.CreatePolygon(shell, holes == null ? null : holes.ToArray());
        }

        private static GeoAPI.Geometries.ILineString ReadLineString(byte[] geom, ref int idx, bool isLittleEndian, IGeometryFactory factory)
        {
            double[] adfTuple = new double[2];
            int nPointCount;
            int iPoint;
            nPointCount = ReadUInt32(geom, ref idx, isLittleEndian);

            if (nPointCount < 0 || nPointCount > Int32.MaxValue / (2 * 8))
                throw new ApplicationException("Currupt SpatialLite geom");

            List<GeoAPI.Geometries.Coordinate> pts = new List<GeoAPI.Geometries.Coordinate>();

            for (iPoint = 0; iPoint < nPointCount; iPoint++)
            {
                pts.Add(ReadPoint(geom, ref idx, isLittleEndian));
            }

            return factory.CreateLineString(pts.ToArray());
        }

        private static GeoAPI.Geometries.Coordinate ReadPoint(byte[] geom, ref int idx, bool isLittleEndian)
        {
            double[] adfTuple = new double[2];
            adfTuple[0] = ReadDouble(geom, ref idx, isLittleEndian);
            adfTuple[1] = ReadDouble(geom, ref idx, isLittleEndian);

            return new GeoAPI.Geometries.Coordinate(adfTuple[0], adfTuple[1], 0);
        }

        private static double ReadDouble(byte[] geom, ref int idx, bool isLittleEndian)
        {
            double ret;
            if (isLittleEndian)
            {
                ret = BitConverter.ToDouble(geom, idx);
            }
            else
            {
                byte[] data = new byte[8];
                for (int i = 0; i < 8; i++)
                    data[i] = geom[idx + 7 - i];
                ret = BitConverter.ToDouble(data, 0);
            }
            idx += 8;
            return ret;
        }
        private static int ReadUInt32(byte[] geom, ref int idx, bool isLittleEndian)
        {
            int ret;
            if (isLittleEndian)
            {
                ret = (int)BitConverter.ToUInt32(geom, idx);
            }
            else
            {
                byte[] data = new byte[4];
                for (int i = 0; i < 4; i++)
                    data[i] = geom[idx + 3 - i];
                ret = (int)BitConverter.ToUInt32(data, 0);
            }
            idx += 4;
            return ret;
        }
    }
}
