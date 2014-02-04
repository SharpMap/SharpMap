namespace WinFormSamples.Samples
{
    public class GdiImageLayerSample
    {
        public static SharpMap.Map InitializeMap(float angle)
        {
            using (var ofn = new System.Windows.Forms.OpenFileDialog())
            {
                ofn.Filter = "All files|*.*";
                ofn.FilterIndex = 0;

                if (ofn.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var m = new SharpMap.Map();
                    var l = new SharpMap.Layers.GdiImageLayer(ofn.FileName);
                    m.Layers.Add(l);

                    m.ZoomToExtents();

                    var mat = new System.Drawing.Drawing2D.Matrix();
                    mat.RotateAt(angle, m.WorldToImage(m.Center));
                    m.MapTransform = mat;

                    return m;
                }
            }
            return null;

        }
    }
}