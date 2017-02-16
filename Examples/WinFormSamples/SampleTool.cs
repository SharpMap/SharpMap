using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common.Logging;
using GeoAPI.Geometries;
using SharpMap;
using SharpMap.Data;
using SharpMap.Forms;
using SharpMap.Forms.Tools;
using SharpMap.Layers;

namespace WinFormSamples
{
    public class SampleTool : MapTool, IDisposable
    {
        private ILog Logger = LogManager.GetLogger(typeof(SampleTool));

        public SampleTool() : base("Sample", "A sample tool that does nothing really useful")
        {
            Logger.Debug(fmh => fmh("Created \"{0}\"-Tool, {1}", Name,Description));
            Enabled = true;
        }

        void IDisposable.Dispose()
        {
            HandleCancel();
        }

        private ToolTip _toolTip;

        public SampleTool(MapBox mapBox1) : this()
        {
            MapControl = mapBox1;
            MapControl.MapChanged += (sender, args) => { HandleCancel(); };
            _cts.Token.Register(HandleCancel);
        }

        public override bool DoKeyDown(Coordinate mapPosition, KeyEventArgs keyEventArgs)
        {
            Logger.Debug(fmh => fmh("KeyDown {1} at {0}", mapPosition, keyEventArgs.KeyCode));
            return base.DoKeyDown(mapPosition, keyEventArgs);
        }

        public override bool DoKeyUp(Coordinate mapPosition, KeyEventArgs keyEventArgs)
        {
            Logger.Debug(fmh => fmh("KeyUp {1} at {0}", mapPosition, keyEventArgs.KeyCode));
            return base.DoKeyUp(mapPosition, keyEventArgs);
        }

        public override bool DoMouseDoubleClick(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            Logger.Debug(fmh => fmh("MouseDoubleClick {1} at {0}", mapPosition, mouseEventArgs.Button));
            return base.DoMouseDoubleClick(mapPosition, mouseEventArgs);
        }

        public override bool DoMouseDown(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            Logger.Debug(fmh => fmh("MouseDown {1} at {0}", mapPosition, mouseEventArgs.Button));
            return base.DoMouseDown(mapPosition, mouseEventArgs);
        }

        public override bool DoMouseEnter()
        {
            Logger.Debug(fmh => fmh("MouseEnter"));
            return base.DoMouseEnter();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled)
            {
                if (_toolTip != null) _toolTip.RemoveAll();
            }
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public override bool DoMouseHover(Coordinate mapPosition)
        {
            Logger.Debug(fmh => fmh("MouseHover at {0}", mapPosition));

            _cts.Cancel();
            _cts.Token.WaitHandle.WaitOne();
            _cts.Dispose();

            _cts = new CancellationTokenSource();
            _cts.Token.Register(HandleCancel);
            
            var t = new Task<FeatureDataRow>(FindGeoNearPoint, mapPosition, _cts.Token);
            t.Start();
            t.ContinueWith(res => ShowToolTip(res.Result));

            return base.DoMouseHover(mapPosition);
        }

        BackgroundWorker _bw = new BackgroundWorker();

        void HandleCancel()
        {
            if (_toolTip != null)
            {
                MapControl.Invoke(new MethodInvoker(() =>
                {
                    _toolTip.Hide(MapControl);
                    _toolTip.Dispose();
                }));
                _toolTip = null;
            }
        }

        private void ShowToolTip(FeatureDataRow fdr)
        {
            if (fdr != null)
            {
                
                MapControl.BeginInvoke(new Action(() =>
                {
                    var _t = new ToolTip();
                    _toolTip = _t;
                    _t/*oolTip*/.ToolTipTitle = fdr.Table.TableName;
                    
                    _t/*oolTip*/.Show(ToText(fdr), MapControl);
                }));
            }
            else
            {
                //MapControl.BeginInvoke(new MethodInvoker( () => _toolTip.Hide(MapControl)));
            }
        }

        private string ToText(FeatureDataRow fdr)
        {
            var sb = new StringBuilder();
            if (fdr.Geometry != null)
            {
                sb.AppendFormat("Geometry:\n  Type: {0}\n  SRID: {1}\n", 
                    fdr.Geometry.GeometryType, fdr.Geometry.SRID);
                switch (fdr.Geometry.Dimension)
                {
                    case Dimension.Surface:
                        sb.AppendFormat("  Area: {0}\n", fdr.Geometry.Area);
                        break;
                    case Dimension.Curve:
                        sb.AppendFormat("  Length: {0}\n", fdr.Geometry.Length);
                        break;
                    case Dimension.Point:
                        sb.AppendFormat("  Position: {0}\n", fdr.Geometry.AsText());
                        break;
                }
            }

            sb.Append("Data:\n");
            foreach (DataColumn col in fdr.Table.Columns)
                sb.AppendFormat("  {0}: {1}\n", col.Caption, fdr[col] == DBNull.Value ? "NULL" : fdr[col]);

            Logger.Debug(fmh => fmh("\n{0}\n{1}", fdr.Table.TableName,  sb.ToString(0, sb.Length-1)));

            return sb.ToString(0, sb.Length - 1);
        }

        private FeatureDataRow FindGeoNearPoint(object/*Coordinate*/ coord)
        {
            var mapPosition = (Coordinate) coord;
            var env = new Envelope(mapPosition);
            env.ExpandBy(5 * Map.PixelWidth);
            var g = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(Map.Factory.ToGeometry(env));

            var fdrs = new List<Tuple<double, FeatureDataRow>>();
            var fds = new FeatureDataSet();
            var tableCount = 0;

            var layersToQuery = GetLayersToQuery(Map);

            for (var i = layersToQuery.Count - 1; i >= 0; i--)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    Logger.Debug("Cancellation requested");
                    return null;
                }

                var l = layersToQuery[i];
                if (l.Enabled && l.MinVisible < Map.Zoom &&
                    l.MaxVisible >= Map.Zoom)
                {
                    if (!l.IsQueryEnabled) continue;
                    l.ExecuteIntersectionQuery(env, fds);
                    for (var j = tableCount; j < fds.Tables.Count; j++)
                    {
                        var fdt = fds.Tables[j];
                        for (var k = 0; k < fdt.Rows.Count; k++)
                        {
                            var fdr = (FeatureDataRow) fdt.Rows[k];
                            if (g.Intersects(fdr.Geometry))
                            {
                                var distance = g.Geometry.InteriorPoint.Distance(fdr.Geometry);
                                if (fdr.Geometry.Dimension == Dimension.Surface)
                                    distance += 5* Map.PixelWidth;
                                fdrs.Add(Tuple.Create(distance, fdr));
                            }
                        }
                    }
                    tableCount = fds.Tables.Count;
                }

            }
            if (fdrs.Count > 0)
            {
                fdrs.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));
                return fdrs[0].Item2;
            }
            return null;
        }

        private static List<ICanQueryLayer> GetLayersToQuery(Map map)
        {
            var res = new List<ICanQueryLayer>();
            for (var i = 0; i < map.BackgroundLayer.Count; i++)
            {
                if (map.BackgroundLayer[i] is ICanQueryLayer)
                {
                    if (((ICanQueryLayer)map.BackgroundLayer[i]).IsQueryEnabled)
                        res.Add((ICanQueryLayer)map.BackgroundLayer[i]);
                }
            }
            for (var i = 0; i < map.Layers.Count; i++)
            {
                if (map.Layers[i] is ICanQueryLayer)
                {
                    if (((ICanQueryLayer)map.Layers[i]).IsQueryEnabled)
                        res.Add((ICanQueryLayer)map.Layers[i]);
                }
            }
            return res;
        }

        public MapBox MapControl { get; private set; }

        public override bool DoMouseLeave()
        {
            Logger.Debug(fmh => fmh("MouseLeave"));
            //_toolTip.RemoveAll();
            return base.DoMouseLeave();
        }

        public override bool DoMouseMove(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            Logger.Debug(fmh => fmh("MouseMove {1} at {0}", mapPosition, mouseEventArgs.Button));
            //_toolTip.RemoveAll();
            return base.DoMouseMove(mapPosition, mouseEventArgs);
        }

        public override bool DoMouseUp(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            Logger.Debug(fmh => fmh("MouseUp {1} at {0}", mapPosition, mouseEventArgs.Button));
            return base.DoMouseUp(mapPosition, mouseEventArgs);
        }

        public override bool DoMouseWheel(Coordinate mapPosition, MouseEventArgs mouseEventArgs)
        {
            Logger.Debug(fmh => fmh("MouseWheel {1} at {0}", mapPosition, mouseEventArgs.Button));
            return base.DoMouseWheel(mapPosition, mouseEventArgs);
        }

        public override void Cancel()
        {
            Logger.Debug(fmh => fmh("Cancel"));
            base.Cancel();
        }

        public override void DoPaint(PaintEventArgs e)
        {
            Logger.Debug(fmh => fmh("Paint"));
            base.DoPaint(e);
        }
    }
}