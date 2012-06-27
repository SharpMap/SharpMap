using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpMap.Forms
{
    public partial class WktGeometryCreator : Form
    {
        private readonly Dictionary<string, string> _wktTokens = new Dictionary<string, string>();

        public WktGeometryCreator()
        {
            InitializeComponent();

            foreach(var kvp in _wktTokens)
                cboWktKeywords.Items.Add(kvp);
            
            cboWktKeywords.DisplayMember = "Key";
            cboWktKeywords.ValueMember = "Value";

            _wktTokens.Add("POINT", "POINT(10 10)");
            _wktTokens.Add("LINESTRING", "LINESTRING(5 5, 7 16, 3 8)");
            _wktTokens.Add("POLYGON", "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10), (12 12, 18 12, 18 18, 12 18, 12 12))");
            _wktTokens.Add("MULTIPOINT", "MULTIPOINT((10 10), (15 15), (13, 9))");
            _wktTokens.Add("MULTILINESTRING", "MULTILINESTRING((5 5, 7 16, 3 8), (15 15, 13, 9))");
            _wktTokens.Add("MULTIPOLYGON", "MULTIPOLYGON(((10 10, 10 20, 20 20, 20 10, 10 10), (12 12, 18 12, 18 18, 12 18, 12 12)), ((21 21, 21 31, 31 31, 31 21, 21 21)))");
            _wktTokens.Add("GEOMETRYCOLLECTION", "GEOMETRYCOLLECTION(MULTIPOINT((10 10), (15 15), (13, 9)), LINESTRING(5 5, 7 16, 3 8))");

            ShowInTaskbar = false;
            
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Hide();
        }

        private GeoAPI.Geometries.IGeometry _geometry;
        public GeoAPI.Geometries.IGeometry Geometry
        {
            get
            {
                return _geometry;
            }

            set 
            {
                if (value != _geometry)
                {
                    _geometry = value;
                    OnGeometrySet(EventArgs.Empty);
                }
            }
        }

        private NetTopologySuite.IO.WKTWriter _wktWriter = new NetTopologySuite.IO.WKTWriter(2) 
                                                               { Formatted = true, MaxCoordinatesPerLine = 3, Tab = 2 };
        
        private void OnGeometrySet(EventArgs eventArgs)
        {
            if (_geometry == null)
                txtWkt.Text = string.Empty;

            txtWkt.Text = _wktWriter.Write(_geometry);
        }

        private NetTopologySuite.IO.WKTReader _wktReader = new NetTopologySuite.IO.WKTReader(
            GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory());
        
        
        private void txtWkt_TextChanged(object sender, EventArgs e)
        {
            var txt = txtWkt.Text;
            if (string.IsNullOrEmpty(txt))
                return;

            GeoAPI.Geometries.IGeometry geometry = null;
            try
            {
                geometry = _wktReader.Read(txt);
                if (geometry != _geometry)
                    Geometry = geometry;
                txtWkt.ForeColor = SystemColors.WindowText;
            }
            catch (Exception)
            {
                txtWkt.ForeColor = Color.Red;
                throw;
            }
        }
    }
}
