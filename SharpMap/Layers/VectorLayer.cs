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
using System.Collections.ObjectModel;
using System.Text;
using SharpMap.Geometries;

namespace SharpMap.Layers
{
	/// <summary>
	/// Class for vector layer properties
	/// </summary>
	/// <example>
	/// Adding a VectorLayer to a map:
	/// <code lang="C#">
	/// //Initialize a new map
	/// SharpMap.Map myMap = new SharpMap.Map(new System.Drawing.Size(300,600));
	/// //Create a layer
	/// SharpMap.Layers.VectorLayer myLayer = new SharpMap.Layers.VectorLayer("My layer");
	/// //Add datasource
	/// myLayer.DataSource = new SharpMap.Data.Providers.ShapeFile(@"C:\data\MyShapeData.shp");
	/// //Set up styles
	/// myLayer.Style.Outline = new Pen(Color.Magenta, 3f);
	/// myLayer.Style.EnableOutline = true;
	/// myMap.Layers.Add(myLayer);
	/// //Zoom to fit the data in the view
	/// myMap.ZoomToExtents();
	/// //Render the map:
	/// System.Drawing.Image mapImage = myMap.GetMap();
	/// </code>
	/// </example>
	public class VectorLayer : Layer, IDisposable
	{

		/// <summary>
		/// Initializes a new layer
		/// </summary>
		/// <param name="layername">Name of layer</param>
		public VectorLayer(string layername)
		{
			this.Style = new SharpMap.Styles.VectorStyle();
			this.LayerName = layername;
			this.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		}
		/// <summary>
		/// Initializes a new layer with a specified datasource
		/// </summary>
		/// <param name="layername">Name of layer</param>
		/// <param name="dataSource">Data source</param>
		public VectorLayer(string layername, SharpMap.Data.Providers.IProvider dataSource) : this(layername)
		{
			_DataSource = dataSource;
		}

		private SharpMap.Rendering.Thematics.ITheme _theme;

		/// <summary>
		/// Gets or sets thematic settings for the layer. Set to null to ignore thematics
		/// </summary>
		public SharpMap.Rendering.Thematics.ITheme Theme
		{
			get { return _theme; }
			set { _theme = value; }
		}

		private bool _ClippingEnabled = false;

		/// <summary>
		/// Specifies whether polygons should be clipped prior to rendering
		/// </summary>
		/// <remarks>
		/// <para>Clipping will clip <see cref="SharpMap.Geometries.Polygon"/> and
		/// <see cref="SharpMap.Geometries.MultiPolygon"/> to the current view prior
		/// to rendering the object.</para>
		/// <para>Enabling clipping might improve rendering speed if you are rendering 
		/// only small portions of very large objects.</para>
		/// </remarks>
		public bool ClippingEnabled
		{
			get { return _ClippingEnabled; }
			set { _ClippingEnabled = value; }
		}
	

		private System.Drawing.Drawing2D.SmoothingMode _SmoothingMode;

		/// <summary>
		/// Render whether smoothing (antialiasing) is applied to lines and curves and the edges of filled areas
		/// </summary>
		public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
		{
			get { return _SmoothingMode; }
			set { _SmoothingMode = value; }
		}

		private SharpMap.Data.Providers.IProvider _DataSource;

		/// <summary>
		/// Gets or sets the datasource
		/// </summary>
		public SharpMap.Data.Providers.IProvider DataSource
		{
			get { return _DataSource; }
			set { _DataSource = value; }
		}

		private Styles.VectorStyle _Style;

		/// <summary>
		/// Gets or sets the rendering style of the vector layer.
		/// </summary>
		public Styles.VectorStyle Style
		{
			get { return _Style; }
			set { _Style = value; }
		}

		#region ILayer Members

		/// <summary>
		/// Renders the layer to a graphics object
		/// </summary>
		/// <param name="g">Graphics object reference</param>
		/// <param name="map">Map which is rendered</param>
		public override void Render(System.Drawing.Graphics g, Map map)
		{
			if (map.Center == null)
				throw (new ApplicationException("Cannot render map. View center not specified"));

			g.SmoothingMode = this.SmoothingMode;
			SharpMap.Geometries.BoundingBox envelope = map.Envelope; //View to render
			if (this.CoordinateTransformation != null)
				envelope = SharpMap.CoordinateSystems.Transformations.GeometryTransform.TransformBox(envelope, this.CoordinateTransformation.MathTransform.Inverse());
			
			//List<SharpMap.Geometries.Geometry> features = this.DataSource.GetGeometriesInView(map.Envelope);

			if (this.DataSource == null)
				throw (new ApplicationException("DataSource property not set on layer '" + this.LayerName + "'"));

			//If thematics is enabled, we use a slighty different rendering approach
			if (this.Theme != null) 
			{
				SharpMap.Data.FeatureDataSet ds = new SharpMap.Data.FeatureDataSet();
				this.DataSource.Open();
				this.DataSource.ExecuteIntersectionQuery(envelope, ds);
				this.DataSource.Close();
								
				SharpMap.Data.FeatureDataTable features = (SharpMap.Data.FeatureDataTable)ds.Tables[0];

				if (this.CoordinateTransformation != null)
					for (int i = 0; i < features.Count; i++)
						features[i].Geometry = SharpMap.CoordinateSystems.Transformations.GeometryTransform.TransformGeometry(features[i].Geometry, this.CoordinateTransformation.MathTransform);

				//Linestring outlines is drawn by drawing the layer once with a thicker line
				//before drawing the "inline" on top.
				if (Style.EnableOutline)
				{
					//foreach (SharpMap.Geometries.Geometry feature in features)
					for (int i = 0; i < features.Count; i++)
					{
						SharpMap.Data.FeatureDataRow feature = features[i];
						//Draw background of all line-outlines first
						if(feature.Geometry is SharpMap.Geometries.LineString)
						{
								SharpMap.Styles.VectorStyle outlinestyle1 = this.Theme.GetStyle(feature) as SharpMap.Styles.VectorStyle;
								if (outlinestyle1.Enabled && outlinestyle1.EnableOutline)
									SharpMap.Rendering.VectorRenderer.DrawLineString(g, feature.Geometry as LineString, outlinestyle1.Outline, map);
						}
						else if(feature.Geometry is SharpMap.Geometries.MultiLineString)
						{
								SharpMap.Styles.VectorStyle outlinestyle2 = this.Theme.GetStyle(feature) as SharpMap.Styles.VectorStyle;
								if (outlinestyle2.Enabled && outlinestyle2.EnableOutline)
									SharpMap.Rendering.VectorRenderer.DrawMultiLineString(g, feature.Geometry as MultiLineString, outlinestyle2.Outline, map);
						}
					}
				}

				for(int i=0;i<features.Count;i++)
				{
					SharpMap.Data.FeatureDataRow feature = features[i];
					SharpMap.Styles.VectorStyle style = this.Theme.GetStyle(feature) as SharpMap.Styles.VectorStyle;
					RenderGeometry(g, map, feature.Geometry, style);
				}
			}
			else
			{
				this.DataSource.Open();

				Collection<SharpMap.Geometries.Geometry> geoms = this.DataSource.GetGeometriesInView(envelope);
				this.DataSource.Close();

				if (this.CoordinateTransformation != null)
					for (int i = 0; i < geoms.Count; i++)
						geoms[i] = SharpMap.CoordinateSystems.Transformations.GeometryTransform.TransformGeometry(geoms[i], this.CoordinateTransformation.MathTransform);

				//Linestring outlines is drawn by drawing the layer once with a thicker line
				//before drawing the "inline" on top.
				if (this.Style.EnableOutline)
				{
					foreach (SharpMap.Geometries.Geometry geom in geoms)
					{
						if (geom != null)
						{
							//Draw background of all line-outlines first
							switch (geom.GetType().FullName)
							{
								case "SharpMap.Geometries.LineString":
									SharpMap.Rendering.VectorRenderer.DrawLineString(g, geom as LineString, this.Style.Outline, map);
									break;
								case "SharpMap.Geometries.MultiLineString":
									SharpMap.Rendering.VectorRenderer.DrawMultiLineString(g, geom as MultiLineString, this.Style.Outline, map);
									break;
								default:
									break;
							}
						}
					}
				}

				for (int i = 0; i < geoms.Count; i++)
				{
					if(geoms[i]!=null)
						RenderGeometry(g, map, geoms[i], this.Style);
				}
			}


			base.Render(g, map);
		}

		private void RenderGeometry(System.Drawing.Graphics g, Map map, Geometry feature, SharpMap.Styles.VectorStyle style)
		{
			switch (feature.GetType().FullName)
			{
				case "SharpMap.Geometries.Polygon":
					if (style.EnableOutline)
						SharpMap.Rendering.VectorRenderer.DrawPolygon(g, (Polygon)feature, style.Fill, style.Outline, _ClippingEnabled, map);
					else
						SharpMap.Rendering.VectorRenderer.DrawPolygon(g, (Polygon)feature, style.Fill, null, _ClippingEnabled, map);
					break;
				case "SharpMap.Geometries.MultiPolygon":
					if (style.EnableOutline)
						SharpMap.Rendering.VectorRenderer.DrawMultiPolygon(g, (MultiPolygon)feature, style.Fill, style.Outline, _ClippingEnabled, map);
					else
						SharpMap.Rendering.VectorRenderer.DrawMultiPolygon(g, (MultiPolygon)feature, style.Fill, null, _ClippingEnabled, map);
					break;
				case "SharpMap.Geometries.LineString":
					SharpMap.Rendering.VectorRenderer.DrawLineString(g, (LineString)feature, style.Line, map);
					break;
				case "SharpMap.Geometries.MultiLineString":
					SharpMap.Rendering.VectorRenderer.DrawMultiLineString(g, (MultiLineString)feature, style.Line, map);
					break;
				case "SharpMap.Geometries.Point":
					SharpMap.Rendering.VectorRenderer.DrawPoint(g, (Point)feature, style.Symbol, style.SymbolScale, style.SymbolOffset, style.SymbolRotation, map);
					break;
				case "SharpMap.Geometries.MultiPoint":
					SharpMap.Rendering.VectorRenderer.DrawMultiPoint(g, (MultiPoint)feature, style.Symbol, style.SymbolScale, style.SymbolOffset, style.SymbolRotation, map);
					break;
				case "SharpMap.Geometries.GeometryCollection":
					foreach(Geometries.Geometry geom in (GeometryCollection)feature)
						RenderGeometry(g, map, geom, style);
					break;
				default:
					break;
			}
		}


		/// <summary>
		/// Returns the extent of the layer
		/// </summary>
		/// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
		public override BoundingBox Envelope
		{
			get
			{
				if (this.DataSource == null)
					throw (new ApplicationException("DataSource property not set on layer '" + this.LayerName + "'"));
				
				bool wasOpen = this.DataSource.IsOpen;
				if (!wasOpen)
					this.DataSource.Open();
				SharpMap.Geometries.BoundingBox box = this.DataSource.GetExtents();
				if (!wasOpen) //Restore state
					this.DataSource.Close();
				if (this.CoordinateTransformation != null)
					return SharpMap.CoordinateSystems.Transformations.GeometryTransform.TransformBox(box, this.CoordinateTransformation.MathTransform);
				return box;
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets the SRID of this VectorLayer's data source
		/// </summary>
		public override int SRID
		{
			get {
				if (this.DataSource == null)
					throw (new ApplicationException("DataSource property not set on layer '" + this.LayerName + "'"));
				
				return this.DataSource.SRID; }
			set { this.DataSource.SRID = value; }
		}

	
		#region ICloneable Members

		/// <summary>
		/// Clones the layer
		/// </summary>
		/// <returns>cloned object</returns>
		public override object Clone()
		{
			throw new NotImplementedException();
		}

		#endregion



		#region IDisposable Members

		/// <summary>
		/// Disposes the object
		/// </summary>
		public void Dispose()
		{
			if(DataSource is IDisposable)
				((IDisposable)DataSource).Dispose();
		}

		#endregion
	}
}
