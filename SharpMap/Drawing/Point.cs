using System.Drawing.Drawing2D;

namespace SharpMap.Drawing
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public struct PointF
    {
        public PointF Empty { get { return new PointF();} }
        public float X { get; set; }
        public float Y { get; set; }
    }

    public struct PointD
    {
        public double X { get; set; }
        public double Y { get; set; }

        
    }

    public enum MatrixOrder
    {
        Append,
        Prepend
    }
}