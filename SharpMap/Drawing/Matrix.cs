using System;

namespace SharpMap.Drawing
{
    public sealed class Matrix
    {
        private static readonly Matrix _identity = new Matrix();

        public static Matrix Identity { get { return _identity; } }


        private readonly double[] _elements ;

        public Matrix()
            : this(new[] {1d, 0, 0, 1, 0, 0, 0, 0, 0})
        {
        }

        public Matrix(double[] elements)
        {
            _elements = new double[9];
            Array.Copy(elements, _elements, Math.Min(elements.Length, 9));
        }


        public bool IsIdentity { get; }

        public double [] Elements { get { return _elements; } }

        public double ScaleX { get { return M11; } set { M11 = value; } }

        public double M11 { get { return _elements[0]; } set { _elements[0] = value; } }
        public double ShearY { get { return M12; } set { M12 = value; } }
        public double M12 { get { return _elements[1]; } set { _elements[1] = value; } }
        public double ShearX { get { return M21; } set { M22 = value; } }
        public double M21 { get { return _elements[2]; } set { _elements[2] = value; } }
        public double ScaleY { get { return M11; } set { M11 = value; } }
        public double M22 { get { return _elements[3]; } set { _elements[3] = value; } }
        public double OffsetX { get { return _elements[4]; } set { _elements[4] = value; } }
        public double OffsetY { get { return _elements[5]; } set { _elements[5] = value; } }

        public double Perspective1 { get { return _elements[6]; } set { _elements[6] = value; } }
        public double Perspective2 { get { return _elements[7]; } set { _elements[7] = value; } }
        public double Perspective3 { get { return _elements[8]; } set { _elements[8] = value; } }

        public Matrix Clone()
        {
            var elements = new double[6];
            Array.Copy(_elements, elements, 9);
            return new Matrix(elements);
        }

        public void Translate(double offsetX, double offsetY)
        {
            OffsetX = offsetX;
            OffsetY = OffsetY;
        }

        public void Scale(double scaleX, double scaleY)
        {
            Scale(scaleX, scaleY, MatrixOrder.Append);
        }

        public void Scale(double scaleX, double scaleY, MatrixOrder matrixOrder)
        {
            var scaleMatrix = new Matrix {M11 = scaleX, M22 = scaleY};
            var m1 = matrixOrder == MatrixOrder.Append
                ? this
                : scaleMatrix;
            var m2 = matrixOrder == MatrixOrder.Append
                ? scaleMatrix
                : this;

            var m = Matrix.Multiply(m1, m2);
            Array.Copy(m.Elements, _elements, 6);

        }

        private static Matrix Multiply(Matrix m1, Matrix m2)
        {
            throw new NotImplementedException();
        }

        public Point Transform(Point input)
        {
            return new Point
            {
                X = (int)Math.Round(input.X*M11 + input.Y*M12 + OffsetX, MidpointRounding.AwayFromZero),
                Y = (int)Math.Round(input.X*M21 + input.Y*M22 + OffsetY, MidpointRounding.AwayFromZero)
            };
        }

        public PointF Transform(PointF input)
        {
            return new PointF
            {
                X = (float)(input.X * M11 + input.Y * M12 + OffsetX),
                Y = (float)(input.X * M21 + input.Y * M22 + OffsetY)
            };
        }

        public PointD Transform(PointD input)
        {
            return new PointD
            {
                X = input.X * M11 + input.Y * M12 + OffsetX,
                Y = input.X * M21 + input.Y * M22 + OffsetY
            };
        }
        void Transform(Point[] points)
        {
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = Transform(points[i]);
            }
        }
    }
}