using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GeoAPI.Geometries;

namespace SharpMap.Data.Providers.ODMatrix
{
    public class ODMatrix : RelationProvider<LocationProvider>, IODMatrix
    {
        private readonly Dictionary<ushort, int> _idToIndex = new Dictionary<ushort, int>();
        private double[][] _matrix;
        private double[] _originSum;
        private double[] _destinationSum;

        private static IList<KeyValuePair<ushort, IPoint>> ToList(IDataReader pointsReader)
        {
            var res = new List<KeyValuePair<ushort, IPoint>>();
            if (pointsReader != null)
            {
                while (pointsReader.Read())
                {
                    var id = Convert.ToUInt16(pointsReader.GetValue(0));
                    var x = Convert.ToDouble(pointsReader.GetValue(1));
                    var y = Convert.ToDouble(pointsReader.GetValue(2));
                    res.Add(new KeyValuePair<ushort, IPoint>(id, Factory.CreatePoint(new Coordinate(x, y))));
                }
            }
            return res;
        }

        /// <summary>
        /// Creates an instance of this matrix
        /// </summary>
        /// <param name="pointsReader"></param>
        /// <param name="matrixReader"> </param>
        public ODMatrix(IDataReader pointsReader, IDataReader matrixReader)
            : this(ToList(pointsReader))
        {
            while (matrixReader.Read())
            {
                this[Convert.ToUInt16(matrixReader.GetValue(0)),
                     Convert.ToUInt16(matrixReader.GetValue(1))] = Convert.ToDouble(matrixReader.GetValue(2));
            }
        }

        public ODMatrix(IList<KeyValuePair<ushort, IPoint>> points)
            : base(new LocationProvider(points))
        {
            _idToIndex = new Dictionary<ushort, int>();

            foreach (var point in points)
            {
                _idToIndex.Add(point.Key, _idToIndex.Count);
            }
            _matrix = NewMatrix(Count);
            _originSum = Enumerable.Repeat(Double.NaN, Count).ToArray();
            _destinationSum = Enumerable.Repeat(Double.NaN, Count).ToArray();
        }

        private static double[][] NewMatrix(int count)
        {
            var res = new double[count][];
            for (var i = 0; i < count; i++)
                res[i] = new double[count];

            return res;
        }

        private static double[][] AddZoneToMatrix(double[][] matrix)
        {
            var newSize = matrix.Length + 1;
            var res = new double[newSize][];
            for (var i = 0; i < matrix.Length; i++)
            {
                res[i] = new double[newSize];
                for (var j = 0; j < matrix.Length; j++)
                    res[i][j] = matrix[i][j];
            }
            res[matrix.Length] = new double[newSize];
            return res;
        }

        private static double[] AddZoneToSum(double[] sumVector)
        {
            var newSize = sumVector.Length + 1;
            var res = new double[newSize];
            for (var i = 0; i < sumVector.Length; i++)
                res[i] = sumVector[i];
            return res;
        }

        public string Name { get; set; }

        public int Size { get { return Count; } }

        public override void Add(ushort id, IPoint point)
        {
            base.Add(id, (IPoint)point.Clone());
            _idToIndex.Add(id, _idToIndex.Count);
            _matrix = AddZoneToMatrix(_matrix);
            _originSum = AddZoneToSum(_originSum);
            _destinationSum = AddZoneToSum(_destinationSum);
        }

        public double this[ushort rowId, ushort colId]
        {
            get
            {
                var rowIndex = _idToIndex[rowId];
                var colIndex = _idToIndex[colId];
                return _matrix[rowIndex][colIndex];
            }
            set
            {
                var rowIndex = _idToIndex[rowId];
                var colIndex = _idToIndex[colId];
                _matrix[rowIndex][colIndex] = value;
            }
        }

        public double this[ushort id, ODMatrixVector vector]
        {
            get
            {
                var res = 0d;
                int row, col, rowIncrement = 0, colIncrement = 0;
                switch (vector)
                {
                    case ODMatrixVector.Origin:
                        row = _idToIndex[id];
                        if (!double.IsNaN(_originSum[row]))
                            return _originSum[row];
                        col = 0;
                        colIncrement = 1;
                        break;
                    case ODMatrixVector.Destination:
                        col = _idToIndex[id];
                        if (!double.IsNaN(_destinationSum[col]))
                            return _destinationSum[col];
                        row = 0;
                        rowIncrement = 1;
                        break;
                    case ODMatrixVector.Both:
                        return this[id, ODMatrixVector.Origin] + this[id, ODMatrixVector.Destination] - this[id, id];
                    default:
                        throw new ArgumentOutOfRangeException("vector");
                }

                for (; row < Size && col < Size; col += colIncrement, row += rowIncrement)
                    res += _matrix[row][col];

                switch (vector)
                {
                    case ODMatrixVector.Origin:
                        _originSum[row] = res;
                        break;
                    default:
                        _destinationSum[col] = res;
                        break;
                }

                return res;
            }
        }
    }
}