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
using SharpMap.Geometries;

using EsriExtent = Esri.FileGDB.Envelope;

using EsriShapeBuffer = Esri.FileGDB.ShapeBuffer;
using EsriPointShapeBuffer = Esri.FileGDB.PointShapeBuffer;
using EsriMultiPointShapeBuffer = Esri.FileGDB.MultiPointShapeBuffer;
using EsriMultiPartShapeBuffer = Esri.FileGDB.MultiPartShapeBuffer;

using EsriGeometryType = Esri.FileGDB.GeometryType;
using EsriShapeType = Esri.FileGDB.ShapeType;
using EsriShapeModifiers = Esri.FileGDB.ShapeModifiers;
using Point = SharpMap.Geometries.Point;


namespace SharpMap.Data.Providers.Converter
{
    internal class FileGdbGeometryConverter
    {
        internal static EsriExtent ToEsriExtent(BoundingBox bbox)
        {
            return new EsriExtent(bbox.Min.X, bbox.Min.Y, bbox.Max.X, bbox.Max.Y);
        }

        internal static EsriGeometryType EsriGeometryType(EsriShapeBuffer buffer)
        {
            return (EsriGeometryType)buffer.geometryType;
        }
        private static EsriShapeType EsriShapeType(EsriShapeBuffer buffer)
        {
            return (EsriShapeType)buffer.shapeType;
        }

        internal static Geometry ToSharpMapGeometry(EsriShapeBuffer buffer)
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

        private static Geometry ToSharpMapMultiLineString(EsriShapeBuffer shapeBuffer)
        {
            if (shapeBuffer == null)
                return null;

            var multiPartShapeBuffer = shapeBuffer as EsriMultiPartShapeBuffer;
            if (multiPartShapeBuffer == null)
            {
                BoundingBox box;
                return FromShapeFilePolyLine(shapeBuffer, out box);
            }

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            var res = new MultiLineString();
            var lines = res.LineStrings;

            var offset = 0;
            for (var i = 0; i < multiPartShapeBuffer.NumParts; i++)
            {
                var vertices = new List<Point>(multiPartShapeBuffer.Parts[i]);
                for (var j = 0; j < multiPartShapeBuffer.Parts[i]; j++ )
                {
                    var index = offset + j;
                    var point = multiPartShapeBuffer.Points[index];
                    vertices.Add(hasZ
                            ? new Point3D(point.x, point.y, multiPartShapeBuffer.Zs[index])
                            : new Point(point.x, point.y));
                }
                lines.Add(new LineString(vertices));
                offset += multiPartShapeBuffer.Parts[i];
            }
            
            if (lines.Count == 1)
                return lines[0];
            return res;
        }

        private static Geometry ToSharpMapMultiPolygon(EsriShapeBuffer shapeBuffer)
        {
            if (shapeBuffer == null)
                return null;

            var multiPartShapeBuffer = shapeBuffer as EsriMultiPartShapeBuffer;
            if (multiPartShapeBuffer == null)
            {
                BoundingBox box;
                return FromShapeFilePolygon(shapeBuffer, out box);
            }

            var hasZ = EsriShapeBuffer.HasZs(shapeBuffer.shapeType);
            var res = new MultiPolygon();
            Polygon poly = null;
            var offset = 0;
            for (var i = 0; i < multiPartShapeBuffer.NumParts; i++)
            {
                var vertices = new List<Point>(multiPartShapeBuffer.Parts[i]);
                for (var j = 0; j < multiPartShapeBuffer.Parts[i]; j++)
                {
                    var index = offset + j;
                    var point = multiPartShapeBuffer.Points[index];
                    vertices.Add(hasZ 
                        ? new Point3D(point.x, point.y, multiPartShapeBuffer.Zs[index])
                        : new Point(point.x, point.y));
                }

                var ring = new LinearRing(vertices);
                if (poly == null || !ring.IsCCW())
                {
                    poly = new Polygon(ring);
                    res.Polygons.Add(poly);
                } 
                else
                {
                    poly.InteriorRings.Add(ring);
                }

                offset += multiPartShapeBuffer.Parts[i];
            }

            if (res.NumGeometries == 1)
                return res.Polygons[0];
            
            return res;
        }

        private static Geometry FromShapeFilePolygon(EsriShapeBuffer shapeBuffer, out BoundingBox box)
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

                box = new BoundingBox(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

                var numParts = reader.ReadInt32();
                var numPoints = reader.ReadInt32();
                var allVertices = new List<Point>(numPoints);

                var parts = new int[numParts+1];
                for (var i = 0; i < numParts; i++)
                    parts[i] = reader.ReadInt32();
                parts[numParts] = numPoints;

                var res = new MultiPolygon();
                Polygon poly = null;
                for (var i = 0; i < numParts; i++)
                {
                    var count = parts[i + 1] - parts[i];
                    var vertices = new List<Point>(count);
                    for (var j = 0; j < count; j++)
                    {
                        var vertex = hasZ
                                         ? new Point3D(reader.ReadDouble(), reader.ReadDouble(), double.NaN)
                                         : new Point(reader.ReadDouble(), reader.ReadDouble());
                        vertices.Add(vertex);
                        allVertices.Add(vertex);
                    }

                    var ring = new LinearRing(vertices);
                    if (poly == null || !ring.IsCCW())
                    {
                        poly = new Polygon(ring);
                        res.Polygons.Add(poly);
                    }
                    else
                    {
                        poly.InteriorRings.Add(ring);
                    }
                }

                if (hasZ)
                {
                    var minZ = reader.ReadDouble();
                    var maxZ = reader.ReadDouble();
                    for (var i = 0; i < numPoints; i++)
                        ((Point3D) allVertices[i]).Z = reader.ReadDouble();
                }

                if (res.NumGeometries == 1)
                    return res.Polygons[0];

                return res;

            }
        }

        private static Geometry FromShapeFilePolyLine(EsriShapeBuffer shapeBuffer, out BoundingBox box)
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

                box = new BoundingBox(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

                var numParts = reader.ReadInt32();
                var numPoints = reader.ReadInt32();
                var allVertices = new List<Point>(numPoints);

                var parts = new int[numParts+1];
                for (var i = 0; i < numParts; i++)
                    parts[i] = reader.ReadInt32();
                parts[numParts] = numPoints;

                var res = new MultiLineString();
                var lines = res.LineStrings;

                for (var i = 0; i < numParts; i++)
                {
                    var count = parts[i + 1] - parts[i];
                    var vertices = new List<Point>(count);
                    for (var j = 0; j < count; j++)
                    {
                        var vertex = hasZ
                                         ? new Point3D(reader.ReadDouble(), reader.ReadDouble(), double.NaN)
                                         : new Point(reader.ReadDouble(), reader.ReadDouble());
                        vertices.Add(vertex);
                        allVertices.Add(vertex);
                    }

                    lines.Add(new LineString(vertices));
                }

                if (hasZ)
                {
                    var minZ = reader.ReadDouble();
                    var maxZ = reader.ReadDouble();
                    for (var i = 0; i < numPoints; i++)
                        ((Point3D) allVertices[i]).Z = reader.ReadDouble();
                }

                if (res.NumGeometries == 1)
                    return lines[0];

                return res;

            }
        }

        private static Geometry FromShapeFileMultiPoint(EsriShapeBuffer shapeBuffer, out BoundingBox box)
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

                box = new BoundingBox(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

                var numPoints = reader.ReadInt32();

                var res = new MultiPoint();
                var points = res.Points;

                for (var i = 0; i < numPoints; i++)
                {
                    var vertex = hasZ
                                        ? new Point3D(reader.ReadDouble(), reader.ReadDouble(), double.NaN)
                                        : new Point(reader.ReadDouble(), reader.ReadDouble());
                    points.Add(vertex);
                }

                if (hasZ)
                {
                    var minZ = reader.ReadDouble();
                    var maxZ = reader.ReadDouble();
                    for (var i = 0; i < numPoints; i++)
                        ((Point3D)points[i]).Z = reader.ReadDouble();
                }

                if (res.NumGeometries == 1)
                    return points[0];

                return res;

            }
        }

        private static Geometry FromShapeFilePoint(EsriShapeBuffer shapeBuffer, out BoundingBox box)
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
                                    ? new Point3D(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble())
                                    : new Point(reader.ReadDouble(), reader.ReadDouble());
            }
        }

        private static Geometry ToSharpMapMultiPoint(EsriShapeBuffer shapeBuffer)
        {
            var multiPointShapeBuffer = shapeBuffer as EsriMultiPointShapeBuffer;
            if (multiPointShapeBuffer == null)
            {
                BoundingBox box;
                return FromShapeFileMultiPoint(shapeBuffer, out box);
            }

            var res = new MultiPoint();
            var hasZ = EsriShapeBuffer.HasZs(multiPointShapeBuffer.shapeType);
            var points = res.Points;
            var offset = 0;
            foreach (var point in multiPointShapeBuffer.Points)
                points.Add(hasZ 
                    ? new Point3D(point.x, point.y, multiPointShapeBuffer.Zs[offset++]) 
                    : new Point(point.x, point.y));

            return res;
        }

        private static Geometry ToSharpMapPoint(EsriShapeBuffer shapeBuffer)
        {
            var pointShapeBuffer = shapeBuffer as EsriPointShapeBuffer;
            if (pointShapeBuffer == null)
            {
                BoundingBox box;
                return FromShapeFilePoint(shapeBuffer, out box);
            }
            
            var pt = pointShapeBuffer.point;

            return EsriShapeBuffer.HasZs(pointShapeBuffer.shapeType) 
                ? new Point3D(pt.x, pt.y, pointShapeBuffer.Z) 
                : new Point(pt.x, pt.y);
        }


    }
}