using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;

namespace WinFormSamples.Samples
{
    public partial class FormSqlServerOpts : Form
    {
        public SharpMap.Forms.MapBox MapBox { get; set; }
        public string ConnectionString { get; set; }

        public FormSqlServerOpts()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            // NO LONGER REQUIRED (performed internally by SharpMap.SqlServerSpatialObjects)
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            SqlServerSample.InitialiseTables(ConnectionString);

            WireHandlers();

            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        private void WireHandlers()
        {
            this.optDataProviderWKB.Click += this.optDataProvider_Click;
            this.optDataProviderNative.Click += this.optDataProvider_Click;

            this.optSpatialGeog.Click += this.optSpatial_Click;
            this.optSpatialGeom.Click += this.optSpatial_Click;
            this.chkSpatialValidate.CheckedChanged += this.chkSpatialValidate_Click;

            this.optExtentsIndividual.Click += this.optExtents_Click;
            this.optExtentsAggregate.Click += this.optExtents_Click;
            this.optExtentsSpatialIndex.Click += this.optExtents_Click;
            this.cmdGetExtents.Click += this.cmdGetExtents_Click;

            this.chkHintIndex.CheckedChanged += this.chkHints_Click;
            this.chkHintNoLock.CheckedChanged += this.chkHints_Click;
            this.chkHintsSeek.CheckedChanged += this.chkHints_Click;

            this.optDefQueryNone.Click += this.optDefQuery_Click;
            this.optDefQueryId.Click += this.optDefQuery_Click;
            this.optDefQueryName.Click += this.optDefQuery_Click;
        }

        private void optDataProvider_Click(object sender, EventArgs e)
        {
            if (optDataProviderNative.Checked && !chkSpatialValidate.Checked)
                // MUST be true for Native Types
                chkSpatialValidate.Checked = true;

            optSpatial_Click(null, null);
        }

        private void optSpatial_Click(object sender, EventArgs e)
        {
            // reset map layers
            MapBox.Map.Layers.Clear();

            SharpMap.Layers.VectorLayer spatialLyr;

            string spatialTable;
            string geomColumn;
            SqlServerSpatialObjectType geomType;
            Brush symBrush;

            if (optSpatialGeog.Checked)
            {
                spatialTable = SqlServerSample.GeogTable;
                geomColumn = "Geog4326";
                geomType = SqlServerSpatialObjectType.Geography;
                symBrush = new SolidBrush(optDataProviderWKB.Checked ? Color.Orange : Color.DeepSkyBlue);
                labTable.Text = "Table: " + SqlServerSample.GeogTable;
            }
            else
            {
                // ensure checked
                optSpatialGeom.Checked = true;
                spatialTable = SqlServerSample.GeomTable;
                geomColumn = "Geom4326";
                geomType = SqlServerSpatialObjectType.Geometry;
                symBrush = new SolidBrush(optDataProviderWKB.Checked ? Color.Red : Color.DodgerBlue);
                labTable.Text = "Table: " + SqlServerSample.GeomTable;

                chkSpatialValidate.Enabled = true;
            }

            spatialLyr = new SharpMap.Layers.VectorLayer("Spatial");
            if (optDataProviderWKB.Checked)
                spatialLyr.DataSource = new SqlServer2008(ConnectionString, spatialTable, geomColumn, "Id", geomType, 4326, SqlServer2008ExtentsMode.QueryIndividualFeatures);
            else
                spatialLyr.DataSource = new SqlServer2008Ex(ConnectionString, spatialTable, geomColumn, "Id", geomType, 4326, SqlServer2008ExtentsMode.QueryIndividualFeatures);

            spatialLyr.SRID = spatialLyr.DataSource.SRID;
            spatialLyr.TargetSRID = 3857;

            spatialLyr.Style.PointColor = symBrush;

            //Set up a label layer
            var labelLyr = new SharpMap.Layers.LabelLayer("Labels")
            {
                DataSource = spatialLyr.DataSource,
                //SRID=4326,
                TargetSRID = 3857,
                Enabled = true,
                LabelColumn = "Name",
                //MultipartGeometryBehaviour = SharpMap.Layers.LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                LabelFilter = SharpMap.Rendering.LabelCollisionDetection.QuickAccurateCollisionDetectionMethod,
                Style = new SharpMap.Styles.LabelStyle()
                {
                    ForeColor = System.Drawing.Color.DarkSlateGray,
                    Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSerif, 8),
                    //BackColor = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0)),
                    HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Left,
                    CollisionDetection = true,
                    Offset = new PointF(10, 0)
                    //MaxVisible = 90,
                    //MinVisible = 30
                }
            };


            MapBox.Map.Layers.Add(spatialLyr);
            MapBox.Map.Layers.Add(labelLyr);

            // configure data provider options
            chkSpatialValidate_Click(null, null);
            optExtents_Click(null, null);
            chkHints_Click(null, null);
            optDefQuery_Click(null, null);

            MapBox.Refresh();
        }

        private SqlServer2008 GetSqlServerDataProvider()
        {
            if (MapBox.Map.Layers.Count == 0)
                return null;

            VectorLayer lyr = (VectorLayer)MapBox.Map.Layers[0];
            return (SqlServer2008)lyr.DataSource;
        }

        private void chkSpatialValidate_Click(object sender, EventArgs e)
        {
            var sqlDP = GetSqlServerDataProvider();
            if (sqlDP == null) return;

            if (sqlDP is SqlServer2008Ex)
                if (sender != null && !chkSpatialValidate.Checked)
                {
                    chkSpatialValidate.Checked = true;
                    MessageBox.Show("Validate is cannot be disabled for Native Types",
                        "Validate Geometries", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            sqlDP.ValidateGeometries = chkSpatialValidate.Checked;

            if (sender != null)
                MapBox.Refresh();
        }

        private void optExtents_Click(object sender, EventArgs e)
        {
            var sqlDP = GetSqlServerDataProvider();
            if (sqlDP == null) return;

            try
            {
                if (optExtentsIndividual.Checked)
                    sqlDP.ExtentsMode = SqlServer2008ExtentsMode.QueryIndividualFeatures;
                else if (optExtentsAggregate.Checked)
                    sqlDP.ExtentsMode = SqlServer2008ExtentsMode.EnvelopeAggregate;
                else if (optExtentsSpatialIndex.Checked)
                {
                    sqlDP.ExtentsMode = SqlServer2008ExtentsMode.SpatialIndex;
                    MessageBox.Show("Are you sure? \nThis mode uses the extent of the Spatial Index GRID, " +
                        "not the extents of the DATA, and does not take into account any Definition Query. " +
                        "\nTo see this in action, click Get Extents for each Extent mode and see the resulting envelope output to Console.",
                        "Extents Mode", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }

                if (sender != null)
                    MapBox.Refresh();
            }
            catch (Exception ex)
            {
                this.optExtentsSpatialIndex.Checked = false;
                MessageBox.Show(ex.Message, "Extents Mode", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void chkHints_Click(object sender, EventArgs e)
        {
            var sqlDP = GetSqlServerDataProvider();
            if (sqlDP == null) return;

            sqlDP.NoLockHint = chkHintNoLock.Checked;

            sqlDP.ForceSeekHint = chkHintsSeek.Checked;

            if (chkHintIndex.Checked)
            {
                if (optSpatialGeom.Checked)
                    sqlDP.ForceIndex = SqlServerSample.GeomSpatialIndex;
                else
                    sqlDP.ForceIndex = SqlServerSample.GeogSpatialIndex;
            }
            else
                sqlDP.ForceIndex = string.Empty;

            if (sender != null)
                MapBox.Refresh();
        }

        private void optDefQuery_Click(object sender, EventArgs e)
        {
            var sqlDP = GetSqlServerDataProvider();
            if (sqlDP == null) return;

            if (optDefQueryName.Checked)
                sqlDP.DefinitionQuery = optDefQueryName.Text;
            else if (optDefQueryId.Checked)
                sqlDP.DefinitionQuery = optDefQueryId.Text;
            else
                sqlDP.DefinitionQuery = string.Empty;

            if (sender != null)
                MapBox.Refresh();
        }

        private void cmdGetExtents_Click(object sender, EventArgs e)
        {
            labExtentsTime.Text = "";

            var sqlDP = GetSqlServerDataProvider();
            if (sqlDP == null) return;

            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            var env = sqlDP.GetExtents();
            stopWatch.Stop();
            labExtentsTime.Text = stopWatch.ElapsedMilliseconds.ToString("0.000") + "ms";

            Console.WriteLine("Spatial Extents " + env.ToString());

        }

    }
}
