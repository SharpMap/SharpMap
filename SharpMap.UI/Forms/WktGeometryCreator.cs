using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NetTopologySuite.IO;

namespace SharpMap.Forms
{
    /// <summary>
    /// A geometry editor for WKT Text
    /// </summary>
    public partial class WktGeometryCreator : Form
    {
        private static readonly Dictionary<string, string> _wktTokens = new Dictionary<string, string>();

        static WktGeometryCreator()
        {
            _wktTokens.Add("POINT", "POINT(10 10)");
            _wktTokens.Add("LINESTRING", "LINESTRING(5 5, 7 16, 3 8)");
            _wktTokens.Add("POLYGON", "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10), (12 12, 18 12, 18 18, 12 18, 12 12))");
            _wktTokens.Add("MULTIPOINT", "MULTIPOINT((10 10), (15 15), (13 9))");
            _wktTokens.Add("MULTILINESTRING", "MULTILINESTRING((5 5, 7 16, 3 8), (15 15, 13 9))");
            _wktTokens.Add("MULTIPOLYGON", "MULTIPOLYGON(((10 10, 10 20, 20 20, 20 10, 10 10), (12 12, 18 12, 18 18, 12 18, 12 12)), ((21 21, 21 31, 31 31, 31 21, 21 21)))");
            _wktTokens.Add("GEOMETRYCOLLECTION", "GEOMETRYCOLLECTION(MULTIPOINT((10 10), (15 15), (13 9)), LINESTRING(5 5, 7 16, 3 8))");
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public WktGeometryCreator()
        {
            InitializeComponent();

            ShowInTaskbar = false;

            foreach (var kvp in _wktTokens)
                cboWktKeywords.Items.Add(kvp);

            cboWktKeywords.DisplayMember = "Key";
            cboWktKeywords.ValueMember = "Value";
            
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Hide();
        }

        private GeoAPI.Geometries.Geometry _geometry;
        
        /// <summary>
        /// Gets or sets a value indicating the current geometry
        /// </summary>
        public GeoAPI.Geometries.Geometry Geometry
        {
            get
            {
                return _geometry;
            }

            set 
            {
                if (ReferenceEquals(_geometry, value))
                    return;

                _geometry = value;
                OnGeometrySet();
            }
        }

        private readonly WKTWriter _wktWriter =
            new WKTWriter(2) {Formatted = true, MaxCoordinatesPerLine = 3, Tab = 2};
        private WKTReader _wktReader = new WKTReader();


        private void OnGeometrySet()
        {
            if (_geometry == null)
            {
                txtWkt.Text = string.Empty;
                return;
            }

            txtWkt.Text = _wktWriter.Write(_geometry);
            _wktReader = new WKTReader(_geometry.Factory);
        }

        /// <summary>
        /// Gets a value indicating the spatial reference id of the geometry created
        /// </summary>
        public int SRID
        {
            get => _wktReader?.Factory.SRID ?? 0;
        }

        private void txtWkt_TextChanged(object sender, EventArgs e)
        {
            string txt = txtWkt.Text;
            if (string.IsNullOrEmpty(txt))
                return;

            try
            {
                var geometry = _wktReader.Read(txt);
                if (geometry.EqualsExact(_geometry))
                    return;

                Geometry = geometry;
                txtWkt.ForeColor = SystemColors.WindowText;
                lblError.Text = @"No Errors";

            }
            catch (Exception ex)
            {
                txtWkt.ForeColor = Color.Red;
                lblError.Text = ex.Message;
            }
        }
    }
}
