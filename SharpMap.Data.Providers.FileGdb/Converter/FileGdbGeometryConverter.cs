// Copyright 2012 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Data.Providers.FileGdb.
// SharpMap.Data.Providers.FileGdb is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Data.Providers.FileGdb is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;


using EsriExtent = Esri.FileGDB.Envelope;

using EsriShapeBuffer = Esri.FileGDB.ShapeBuffer;
using EsriPointShapeBuffer = Esri.FileGDB.PointShapeBuffer;
using EsriMultiPointShapeBuffer = Esri.FileGDB.MultiPointShapeBuffer;
using EsriMultiPartShapeBuffer = Esri.FileGDB.MultiPartShapeBuffer;

using EsriGeometryType = Esri.FileGDB.GeometryType;
using EsriShapeType = Esri.FileGDB.ShapeType;
using EsriShapeModifiers = Esri.FileGDB.ShapeModifiers;



namespace SharpMap.Data.Providers.Converter
{

    internal class FileGdbGeometryConverter
    {

        private static readonly IGeometryFactory geometryFactory = GeometryServiceProvider.Instance.CreateGeometryFactory();


        internal static EsriExtent ToEsriExtent(Envelope bbox)
        {
            return new EsriExtent(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY);
        }

        internal static EsriGeometryType EsriGeometryType(EsriShapeBuffer buffer)
        {
            return (EsriGeometryType)buffer.geometryType;
        }

        private static EsriShapeType EsriShapeType(EsriShapeBuffer buffer)
        {
            return (EsriShapeType)buffer.shapeType;
        }

        internal static IGeometry ToSharpMapGeometry(EsriShapeBuffer buffer)
        {
            if (buffer == null || buffer.IsEmpty)
                return null;

            var geometryType = EsriGeometryType(buffer);

            switch (geometryType)
            {
                case Esri.FileGDB.GeometryType.Null:
                    return null;

                case Esri.FileGDB.GeometryType.Point:
                    return ToSharpMapPoint(buffer);

                case Esri.FileGDB.GeometryType.Multipoint:
                    return ToSharpMapMultiPoint(buffer);

                case Esri.FileGDB.GeometryType.Polyline:
                    return ToSharpMapMultiLineString(buffer);

                case Esri.FileGDB.GeometryType.Polygon:
                    return ToSharpMapMultiPolygon(buffer);

                default:
                    return null;
            }
        }

        private static IGeometry ToSharpMapMultiLineString(EsriShapeBuffer shapeBuffer)
        {
            if (shapeBuffer == null)
                return null;

            var multiPartShapeBuffer = shapeBuffer as EsriMultiPartShapeBuffer;
            if (multiPartShapeBuffer == null)
            {
                Envelope box;
                return FromShapeFilePolyLine(shapeBuffer, out box);
            }

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            var lines = new List<ILineString>();

            var offset = 0;
            for (var i = 0; i < multiPartShapeBuffer.NumParts; i++)
            {
                var vertices = new List<Coordinate>(multiPartShapeBuffer.Parts[i]);
                for (var j = 0; j < multiPartShapeBuffer.Parts[i]; j++)
                {
                    var index = offset + j;
                    var point = multiPartShapeBuffer.Points[index];
                    vertices.Add(hasZ
                            ? new Coordinate(point.x, point.y, multiPartShapeBuffer.Zs[index])
                            : new Coordinate(point.x, point.y));
                }
                lines.Add(new LineString(vertices.ToArray()));
                offset += multiPartShapeBuffer.Parts[i];
            }

            if (lines.Count == 1)
                return lines[0];

            return new MultiLineString(lines.ToArray());

        }

        private static IGeometry ToSharpMapMultiPolygon(EsriShapeBuffer shapeBuffer)
        {
            if (shapeBuffer == null)
                return null;

            var multiPartShapeBuffer = shapeBuffer as EsriMultiPartShapeBuffer;
            if (multiPartShapeBuffer == null)
            {
                Envelope box;
                return FromShapeFilePolygon(shapeBuffer, out box);
            }

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            IList<IPolygon> polygons = new List<IPolygon>();
            //IPolygon poly = null;
            ILinearRing shell = null;
            IList<ILinearRing> holes = new List<ILinearRing>();
            var offset = 0;
            for (var i = 0; i < multiPartShapeBuffer.NumParts; i++)
            {
                var vertices = new List<Coordinate>(multiPartShapeBuffer.Parts[i]);
                for (var j = 0; j < multiPartShapeBuffer.Parts[i]; j++)
                {
                    var index = offset + j;
                    var point = multiPartShapeBuffer.Points[index];
                    vertices.Add(hasZ
                        ? new Coordinate(point.x, point.y, multiPartShapeBuffer.Zs[index])
                        : new Coordinate(point.x, point.y));
                }

                var ring = geometryFactory.CreateLinearRing(vertices.ToArray());
                if (shell == null || !ring.IsCCW())
                {
                    shell = ring;
                    //poly = new Polygon(ring);
                    //polygons.Add(poly);
                }
                else
                {
                    holes.Add(ring);
                    //poly.InteriorRings.Add(ring);
                }

                offset += multiPartShapeBuffer.Parts[i];

                polygons.Add(geometryFactory.CreatePolygon(shell, holes.ToArray()));

            }

            if (polygons.Count == 1)
                return polygons[0];

            return new MultiPolygon(polygons.ToArray());
        }

        private static IGeometry FromShapeFilePolygon(EsriShapeBuffer shapeBuffer, out Envelope box)
        {
            box = null;
            if (shapeBuffer == null)
                return null;

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            var hasM = EsriShapeBuffer.HasMs(shapeBuffer.shapeType);
            using (var reader = new BinaryReader(new MemoryStream(shapeBuffer.shapeBuffer)))
            {
                var type = reader.ReadInt32();
                if (!(type == 5 || type == 15 || type == 25))
                    throw new InvalidOperationException();

                box = createEnvelope(reader);

                var numParts = reader.ReadInt32();
                var numPoints = reader.ReadInt32();
                var allVertices = new List<Coordinate>(numPoints);

                var parts = new int[numParts + 1];
                for (var i = 0; i < numParts; i++)
                    parts[i] = reader.ReadInt32();
                parts[numParts] = numPoints;

                //IPolygon poly = null;
                ILinearRing shell = null;
                IList<ILinearRing> holes = new List<ILinearRing>();
                for (var i = 0; i < numParts; i++)
                {
                    var count = parts[i + 1] - parts[i];
                    var vertices = new List<Coordinate>(count);
                    for (var j = 0; j < count; j++)
                    {
                        var vertex = hasZ
                                         ? new Coordinate(reader.ReadDouble(), reader.ReadDouble(), double.NaN)
                                         : new Coordinate(reader.ReadDouble(), reader.ReadDouble());
                        vertices.Add(vertex);
                        allVertices.Add(vertex);
                    }

                    var ring = geometryFactory.CreateLinearRing(vertices.ToArray());
                    if (shell == null || !ring.IsCCW())
                    {
                        shell = ring;
                        //poly = new Polygon(ring);
                        //res.Polygons.Add(poly);
                    }
                    else
                    {
                        holes.Add(ring);
                        //poly.InteriorRings.Add(ring);
                    }
                }


                if (hasZ)
                {
                    var minZ = reader.ReadDouble();
                    var maxZ = reader.ReadDouble();
                    for (var i = 0; i < numPoints; i++)
                        allVertices[i].Z = reader.ReadDouble();
                }

                return geometryFactory.CreatePolygon(shell, holes.ToArray());


            }
        }


        private static IGeometry FromShapeFilePolyLine(EsriShapeBuffer shapeBuffer, out Envelope box)
        {
            box = null;
            if (shapeBuffer == null)
                return null;

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            var hasM = EsriShapeBuffer.HasMs(shapeBuffer.shapeType);
            using (var reader = new BinaryReader(new MemoryStream(shapeBuffer.shapeBuffer)))
            {
                var type = reader.ReadInt32();
                if (!(type == 3 || type == 13 || type == 23))
                    throw new InvalidOperationException();

                box = createEnvelope(reader);

                var numParts = reader.ReadInt32();
                var numPoints = reader.ReadInt32();
                var allVertices = new List<Coordinate>(numPoints);

                var parts = new int[numParts + 1];
                for (var i = 0; i < numParts; i++)
                    parts[i] = reader.ReadInt32();
                parts[numParts] = numPoints;

                var lines = new List<ILineString>();

                for (var i = 0; i < numParts; i++)
                {
                    var count = parts[i + 1] - parts[i];
                    var vertices = new List<Coordinate>(count);
                    for (var j = 0; j < count; j++)
                    {
                        var vertex = hasZ
                                         ? new Coordinate(reader.ReadDouble(), reader.ReadDouble(), double.NaN)
                                         : new Coordinate(reader.ReadDouble(), reader.ReadDouble());
                        vertices.Add(vertex);
                        allVertices.Add(vertex);
                    }

                    lines.Add(geometryFactory.CreateLineString(vertices.ToArray()));
                }

                if (hasZ)
                {
                    var minZ = reader.ReadDouble();
                    var maxZ = reader.ReadDouble();
                    for (var i = 0; i < numPoints; i++)
                        allVertices[i].Z = reader.ReadDouble();
                }

                if (lines.Count == 1)
                    return lines[0];

                return geometryFactory.CreateMultiLineString(lines.ToArray());

            }
        }

        private static IGeometry FromShapeFileMultiPoint(EsriShapeBuffer shapeBuffer, out Envelope box)
        {
            box = null;
            if (shapeBuffer == null)
                return null;

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            var hasM = EsriShapeBuffer.HasMs(shapeBuffer.shapeType);
            using (var reader = new BinaryReader(new MemoryStream(shapeBuffer.shapeBuffer)))
            {
                var type = reader.ReadInt32();
                if (!(type == 8 || type == 18 || type == 28))
                    throw new InvalidOperationException();

                box = createEnvelope(reader);

                var numPoints = reader.ReadInt32();

                IList<IPoint> points = new List<IPoint>();

                for (var i = 0; i < numPoints; i++)
                {
                    var vertex = hasZ
                                        ? new Point(reader.ReadDouble(), reader.ReadDouble(), double.NaN)
                                        : new Point(reader.ReadDouble(), reader.ReadDouble());
                    points.Add(vertex);
                }

                if (hasZ)
                {
                    var minZ = reader.ReadDouble();
                    var maxZ = reader.ReadDouble();
                    for (var i = 0; i < numPoints; i++)
                        points[i].Z = reader.ReadDouble();
                }

                if (points.Count == 1)
                    return points[0];

                return geometryFactory.CreateMultiPoint(points.ToArray());

            }
        }

        private static IGeometry FromShapeFilePoint(EsriShapeBuffer shapeBuffer, out Envelope box)
        {
            box = null;
            if (shapeBuffer == null)
                return null;

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            var hasM = EsriShapeBuffer.HasMs(shapeBuffer.shapeType);
            using (var reader = new BinaryReader(new MemoryStream(shapeBuffer.shapeBuffer)))
            {
                var type = reader.ReadInt32();
                if (!(type == 1 || type == 11 || type == 21))
                    throw new InvalidOperationException();

                return hasZ
                                    ? new Point(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble())
                                    : new Point(reader.ReadDouble(), reader.ReadDouble());
            }
        }

        private static IGeometry ToSharpMapMultiPoint(EsriShapeBuffer shapeBuffer)
        {
            var multiPointShapeBuffer = shapeBuffer as EsriMultiPointShapeBuffer;
            if (multiPointShapeBuffer == null)
            {
                Envelope box;
                return FromShapeFileMultiPoint(shapeBuffer, out box);
            }

            var hasZ = EsriShapeBuffer.HasZs(multiPointShapeBuffer.shapeType);
            IList<IPoint> points = new List<IPoint>();
            var offset = 0;
            foreach (var point in multiPointShapeBuffer.Points)
                points.Add(hasZ
                    ? new Point(point.x, point.y, multiPointShapeBuffer.Zs[offset++])
                    : new Point(point.x, point.y));

            return geometryFactory.CreateMultiPoint(points.ToArray());
        }

        private static IGeometry ToSharpMapPoint(EsriShapeBuffer shapeBuffer)
        {
            var pointShapeBuffer = shapeBuffer as EsriPointShapeBuffer;
            if (pointShapeBuffer == null)
            {
                Envelope box;
                return FromShapeFilePoint(shapeBuffer, out box);
            }

            var pt = pointShapeBuffer.point;

            return EsriShapeBuffer.HasZs(pointShapeBuffer.shapeType)
                ? new Point(pt.x, pt.y, pointShapeBuffer.Z)
                : new Point(pt.x, pt.y);
        }

        private static Envelope createEnvelope(BinaryReader reader)
        {
            double minX = reader.ReadDouble();
            double minY = reader.ReadDouble();
            double maxX = reader.ReadDouble();
            double maxY = reader.ReadDouble();

            Envelope box = new Envelope(minX, maxX, minY, maxY);
            return box;
        }


    }
}