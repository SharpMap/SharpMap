using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Layers;
using SharpMap.Web.Wms.Exceptions;

namespace SharpMap.Web.Wms.Server.Handlers
{
    public abstract class AbstractHandler : IHandler
    {
        protected const StringComparison Case = StringComparison.InvariantCultureIgnoreCase;
        private const StringComparison Ordinal = StringComparison.Ordinal;
        private static readonly NumberFormatInfo NfInfo = NumberFormatInfo.InvariantInfo;

        private readonly Capabilities.WmsServiceDescription _description;


        protected AbstractHandler(Capabilities.WmsServiceDescription description)
        {
            _description = description;
        }

        protected Capabilities.WmsServiceDescription Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Parses a boundingbox string to a boundingbox geometry from the format minx,miny,maxx,maxy. Returns null if the format is invalid
        /// </summary>
        /// <param name="boundingBox">string representation of a boundingbox</param>
        /// <param name="flipXY">Value indicating that x- and y-ordinates should be changed.</param>
        /// <returns>Boundingbox or null if invalid parameter</returns>
        public static Envelope ParseBBOX(string boundingBox, bool flipXY)
        {
            const NumberStyles ns = NumberStyles.Float;
            NumberFormatInfo nf = Map.NumberFormatEnUs;

            string[] arr = boundingBox.Split(new[] { ',' });
            if (arr.Length != 4)
                return null;

            double minx, miny, maxx, maxy;
            if (!Double.TryParse(arr[0], ns, nf, out minx))
                return null;
            if (!Double.TryParse(arr[2], ns, nf, out maxx))
                return null;
            if (maxx < minx)
                return null;
            if (!Double.TryParse(arr[1], ns, nf, out miny))
                return null;
            if (!Double.TryParse(arr[3], ns, nf, out maxy))
                return null;
            if (maxy < miny)
                return null;

            if (flipXY)
                return new Envelope(miny, maxy, minx, maxx);
            return new Envelope(minx, maxx, miny, maxy);
        }

        public abstract IHandlerResponse Handle(Map map, IContextRequest request);

        protected abstract WmsParams ValidateParams(IContextRequest request, int targetSrid);

        /// <summary>
        /// Validate common arguments for GetFeatureInfo and GetMap requests
        /// </summary>
        protected WmsParams ValidateCommons(IContextRequest request, int targetSrid)
        {
            string version = request.GetParam("VERSION");
            if (version == null)
                throw new WmsParameterNotSpecifiedException("VERSION");
            if (!String.Equals(version, "1.3.0", Case))
                throw new WmsInvalidParameterException("Only version 1.3.0 supported");
            string layers = request.GetParam("LAYERS");
            if (layers == null)
                throw new WmsParameterNotSpecifiedException("LAYERS");
            string styles = request.GetParam("STYLES");
            if (styles == null)
                throw new WmsParameterNotSpecifiedException("STYLES");
            string crs = request.GetParam("CRS");
            if (crs == null)
                throw new WmsParameterNotSpecifiedException("CRS");
            if (crs != "EPSG:" + targetSrid)
                throw new WmsInvalidCRSException("CRS not supported");
            string bbox = request.GetParam("BBOX");
            if (bbox == null)
                throw new WmsParameterNotSpecifiedException("BBOX");
            string width = request.GetParam("WIDTH");
            if (width == null)
                throw new WmsParameterNotSpecifiedException("WIDTH");
            string height = request.GetParam("HEIGHT");
            if (height == null)
                throw new WmsParameterNotSpecifiedException("HEIGHT");
            string format = request.GetParam("FORMAT");
            if (format == null)
                throw new WmsParameterNotSpecifiedException("FORMAT");
            string cqlFilter = request.GetParam("CQL_FILTER");
            short w, h;
            if (!Int16.TryParse(width, out w) || !Int16.TryParse(height, out h))
                throw new WmsInvalidParameterException("Invalid parameters for HEIGHT or WITDH");
            Envelope env = ParseBBOX(bbox, targetSrid == 4326);
            if (env == null)
                throw new WmsInvalidBboxException(bbox);

            return new WmsParams
            {
                Layers = layers,
                Styles = styles,
                CRS = crs,
                BBOX = env,
                Width = w,
                Height = h,
                Format = format,
                CqlFilter = cqlFilter
            };
        }

        // TODO: refactor. too many lines!
        /// <summary>
        /// Filters the features to be processed by a CQL filter
        /// </summary>
        /// <param name="row">A <see cref="T:SharpMap.Data.FeatureDataRow"/> to test.</param>
        /// <param name="cqlString">A CQL string defining the filter </param>
        /// <returns>GeoJSON string with featureinfo results</returns>
        public bool CqlFilter(FeatureDataRow row, string cqlString)
        {
            bool toreturn = true;
            // check on filter type (AND, OR, NOT)
            string[] splitstring = { " " };
            string[] cqlStringItems = cqlString.Split(splitstring, StringSplitOptions.RemoveEmptyEntries);
            string[] comparers = { "==", "!=", "<", ">", "<=", ">=", "BETWEEN", "LIKE", "IN" };
            for (int i = 0; i < cqlStringItems.Length; i++)
            {
                bool tmpResult = true;
                // check first on AND OR NOT, only the case if multiple checks have to be done
                bool AND = true;
                bool OR = false;
                bool NOT = false;
                if (cqlStringItems[i] == "AND") { i++; }
                if (cqlStringItems[i] == "OR") { AND = false; OR = true; i++; }
                if (cqlStringItems[i] == "NOT") { AND = false; NOT = true; i++; }
                if ((NOT && !toreturn) || (AND && !toreturn))
                    break;
                // valid cql starts always with the column name
                string column = cqlStringItems[i];
                int columnIndex = row.Table.Columns.IndexOf(column);
                Type t = row.Table.Columns[columnIndex].DataType;
                if (columnIndex < 0)
                    break;
                i++;
                string comparer = cqlStringItems[i];
                i++;
                // if the comparer isn't in the comparerslist stop
                if (!comparers.Contains(comparer))
                    break;
                if (comparer == comparers[8])//IN 
                {
                    // read all the items until the list is closed by ')' and merge them
                    // all items are assumed to be separated by space merge them first
                    // items are merged because a item might contain a space itself, 
                    // and in this case it's splitted at the wrong place
                    string IN = "";
                    while (!cqlStringItems[i].Contains(")"))
                    {
                        IN = IN + " " + cqlStringItems[i];
                        i++;
                    }
                    IN = IN + " " + cqlStringItems[i];
                    string[] splitters = { "('", "', '", "','", "')" };
                    List<string> items = IN.Split(splitters, StringSplitOptions.RemoveEmptyEntries).ToList();

                    tmpResult = items.Contains(Convert.ToString(row[columnIndex], NfInfo));
                }
                else if (comparer == comparers[7])//LIKE
                {
                    // to implement
                    //tmpResult = true;
                }
                else if (comparer == comparers[6])//BETWEEN
                {
                    // get type number of string
                    if (t == typeof(string))
                    {
                        string string1 = cqlStringItems[i];
                        i += 2; // skip the AND in BETWEEN
                        string string2 = cqlStringItems[i];
                        tmpResult = 0 < String.Compare(Convert.ToString(row[columnIndex], NfInfo), string1, Ordinal) &&
                                    0 > String.Compare(Convert.ToString(row[columnIndex], NfInfo), string2, Ordinal);

                    }
                    else if (t == typeof(double))
                    {
                        double value1 = Convert.ToDouble(cqlStringItems[i], NfInfo);
                        i += 2; // skip the AND in BETWEEN
                        double value2 = Convert.ToDouble(cqlStringItems[i], NfInfo);
                        tmpResult = value1 < Convert.ToDouble(row[columnIndex], NfInfo) && value2 > Convert.ToDouble(row[columnIndex], NfInfo);
                    }
                    else if (t == typeof(int))
                    {
                        int value1 = Convert.ToInt32(cqlStringItems[i]);
                        i += 2;
                        int value2 = Convert.ToInt32(cqlStringItems[i]);
                        tmpResult = value1 < Convert.ToInt32(row[columnIndex], NfInfo) && value2 > Convert.ToInt32(row[columnIndex], NfInfo);
                    }
                }
                else
                {
                    if (t == typeof(string))
                    {
                        string cqlValue = Convert.ToString(cqlStringItems[i], NfInfo);
                        string rowValue = Convert.ToString(row[columnIndex], NfInfo);
                        if (comparer == comparers[5]) //>=
                        {
                            tmpResult = 0 <= String.Compare(rowValue, cqlValue, Ordinal);
                        }
                        else if (comparer == comparers[4]) //<=
                        {
                            tmpResult = 0 >= String.Compare(rowValue, cqlValue, Ordinal);
                        }
                        else if (comparer == comparers[3]) //>
                        {
                            tmpResult = 0 < String.Compare(rowValue, cqlValue, Ordinal);
                        }
                        else if (comparer == comparers[2]) //<
                        {
                            tmpResult = 0 > String.Compare(rowValue, cqlValue, Ordinal);
                        }
                        else if (comparer == comparers[1]) //!=
                        {
                            tmpResult = rowValue != cqlValue;
                        }
                        else if (comparer == comparers[0]) //==
                        {
                            tmpResult = rowValue == cqlValue;
                        }
                    }
                    else
                    {
                        double value = Convert.ToDouble(cqlStringItems[i], NfInfo);
                        if (comparer == comparers[5]) //>=
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex], NfInfo) >= value;
                        }
                        else if (comparer == comparers[4]) //<=
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex], NfInfo) <= value;
                        }
                        else if (comparer == comparers[3]) //>
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex], NfInfo) > value;
                        }
                        else if (comparer == comparers[2]) //<
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex], NfInfo) < value;
                        }
                        else if (comparer == comparers[1]) //!=
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex], NfInfo) != value;
                        }
                        else if (comparer == comparers[0]) //==
                        {
                            tmpResult = Convert.ToDouble(row[columnIndex], NfInfo) == value;
                        }
                    }
                }
                if (AND)
                    toreturn = tmpResult;
                if (OR && tmpResult)
                    toreturn = true;
                if (toreturn && NOT && tmpResult)
                    toreturn = false;

            }
            //OpenLayers.Filter.Comparison.EQUAL_TO = “==”;
            //OpenLayers.Filter.Comparison.NOT_EQUAL_TO = “!=”;
            //OpenLayers.Filter.Comparison.LESS_THAN = “<”;
            //OpenLayers.Filter.Comparison.GREATER_THAN = “>”;
            //OpenLayers.Filter.Comparison.LESS_THAN_OR_EQUAL_TO = “<=”;
            //OpenLayers.Filter.Comparison.GREATER_THAN_OR_EQUAL_TO = “>=”;
            //OpenLayers.Filter.Comparison.BETWEEN = “..”;
            //OpenLayers.Filter.Comparison.LIKE = “~”;
            //IN (,,)

            return toreturn;
        }

        protected int TargetSrid(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            ILayer layer = map.Layers.FirstOrDefault();
            if (layer == null)
                throw new ArgumentException("map has no layers");
            return layer.TargetSRID;
        }
    }
}
