// Copyright 2014: Peter Löfås
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
using System.Drawing;
using System.IO;
using Common.Logging;
using GeoAPI.Geometries;
using OSGeo.GDAL;
using SharpMap.Data;
using SharpMap.Data.Providers;

namespace SharpMap.Layers
{
    /// <summary>
    /// Implementation of TILEINDEX of GDAL raster-layers
    /// 
    /// A tileindex is a shapefile that ties together several datasets into a single layer. Therefore, you don’t need to create separate layers for each piece of imagery or each county’s road data; make a tileindex and let MapServer piece the mosaic together on the fly.
    /// Making a tileindex is easy using gdaltindex for GDAL data sources (rasters) and ogrtindex for OGR data sources (vectors). You just run them, specifying the index file to create and the list of data sources to add to the index.
    ///
    /// For example, to make a mosaic of several TIFFs:
    ///
    /// gdaltindex imagery.shp imagery/*.tif
    
    /// See: http://mapserver.org/optimization/tileindex.html
    /// </summary>
    public class GdalTileIndexRasterLayer : GdalRasterLayer
    {

        class CacheHolder
        {
            public Dataset Dataset;
            public string Projection;
            public Size ImageSize;
            public Envelope Envelope;
            public Rectangle HistoBounds;
            public int Bands;


        }

        private static readonly ILog _logger = LogManager.GetLogger(typeof (GdalTileIndexRasterLayer));

        private readonly ShapeFile _shapeFile;
        private readonly string _fieldName;
        private readonly string _fileName;
        private readonly Envelope _extents;
        private Dictionary<string, CacheHolder> _openDatasets;

        /// <summary>
        /// Open a TileIndex shapefile
        /// 
        /// A tileindex is a shapefile that ties together several datasets into a single layer. Therefore, you don’t need to create separate layers for each piece of imagery or each county’s road data; make a tileindex and let SharpMap piece the mosaic together on the fly.
        /// Making a tileindex is easy using gdaltindex for GDAL data sources (rasters). You just run the tool, specifying the index file to create and the list of data sources to add to the index.
        ///
        /// For example, to make a mosaic of several TIFFs:
        ///
        /// gdaltindex imagery.shp imagery/*.tif

        /// See: http://mapserver.org/optimization/tileindex.html
        /// </summary>
        /// <param name="layerName">Name of the layer</param>
        /// <param name="fileName">Path to the ShapeFile containing tile-indexes</param>
        /// <param name="fieldName">FieldName in the shapefile storing full or relative path-names to the datasets</param>
        public GdalTileIndexRasterLayer(string layerName, string fileName, string fieldName) : base(layerName)
        {
            _fileName = fileName;
            _shapeFile = new ShapeFile(fileName, true);
            _shapeFile.Open();
            _extents = _shapeFile.GetExtents();
            _shapeFile.Close();
            _fieldName = fieldName;
            _openDatasets = new Dictionary<string, CacheHolder>();
        }

        

        public override Envelope Envelope
        {
            get
            {
                return _extents;
            }
        }

        public override void Render(Graphics g, MapViewport map)
        {
            try
            {
                _shapeFile.Open();
                var ds = new FeatureDataSet();
                _shapeFile.ExecuteIntersectionQuery(map.Envelope, ds);

                var dt = ds.Tables[0];
                foreach (FeatureDataRow fdr in dt.Rows)
                {
                    if (fdr.Geometry.EnvelopeInternal.Intersects(map.Envelope))
                    {
                        var file = fdr[_fieldName] as string;
                        if (!Path.IsPathRooted(file))
                            file = Path.Combine(Path.GetDirectoryName(_fileName), file);

                        if (file == null)
                            continue;

                        if (_logger.IsDebugEnabled)
                            _logger.Debug("Drawing " + file);

                        if (!_openDatasets.ContainsKey(file))
                        {
                            OpenDataset(file);
                            _openDatasets.Add(file, new CacheHolder(){
                                Bands = Bands,
                                Dataset = _gdalDataset,
                                Envelope = _envelope,
                                HistoBounds = HistoBounds,
                                ImageSize = _imageSize,
                                Projection = Projection
                            });
                        }
                        else
                        {
                            CacheHolder hld = _openDatasets[file];
                            Bands = hld.Bands;
                            _gdalDataset = hld.Dataset;
                            _envelope = hld.Envelope;
                            HistoBounds = hld.HistoBounds;
                            _imageSize = hld.ImageSize;
                            Projection = hld.Projection;
                        }

                        //base.Render(g, map);
                        _envelope = null;
                        _gdalDataset = null;
                    }
                }

            }
            catch (Exception)
            {
                
                _shapeFile.Close();
            }
        }

        protected override void ReleaseManagedResources()
        {
            _shapeFile.Dispose();

            foreach (var kvp in _openDatasets)
            {
                if (kvp.Value  != null)
                {
                    kvp.Value.Dataset.Dispose();
                }
            }
            
            base.ReleaseManagedResources();
        }
    }
}
