using System;
using System.Collections.Generic;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using QuickGraph;
using GeoAPI.Geometries;
using System.Collections.ObjectModel;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Algorithms;

namespace RoutingExample.RoutingSystem
{
    public class GraphFactory
    {
        static GraphFactory()
        {
            if (GeoAPI.GeometryServiceProvider.Instance == null)
            {
                GeoAPI.GeometryServiceProvider.Instance = new NtsGeometryServices();
            }
        }

        private readonly IGeometryFactory _geomFactory;

        /// <summary>
        /// We Store Each Line In A Vector Layer Seperately. 
        /// </summary>
        private readonly Dictionary<uint, ILineString> _theLineStrings = new Dictionary<uint, ILineString>();

        /// <summary>
        /// The Graph that Dijsktra is performed upon
        /// </summary>
        AdjacencyGraph<Coordinate, Edge<Coordinate>> _graph =
            new AdjacencyGraph<Coordinate, Edge<Coordinate>>(false);

        /// <summary>
        /// The Edge Costs - The distance between two points.
        /// </summary>
        Dictionary<Edge<Coordinate>, double> _edgeCost;

        ///// <summary>
        ///// We need to store some points.
        ///// </summary>
        //Dictionary<uint, Coordinate> _lookup;

        /// <summary>
        /// Store the pair of points representing the end points.
        /// </summary>
        readonly List<LineEndPoints> _listOfEndPoints;

        DijkstraShortestPathAlgorithm<Coordinate, Edge<Coordinate>> _dijkstra;

        // Used to process the terminate early condition of Dijsktra shortest path algortihm.
        IPoint _theDestination;

        /// <summary>
        /// Creates an instance of this class, internally creating a geometry factory with the <paramref name="srid"/> value.
        /// </summary>
        /// <param name="srid">The spatial reference id</param>
        public GraphFactory()
            : this(GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory())
        { }

        /// <summary>
        /// Creates an instance of this class, internally creating a geometry factory with the <paramref name="srid"/> value.
        /// </summary>
        /// <param name="srid">The spatial reference id</param>
        public GraphFactory(int srid)
            : this(GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(srid))
        {}

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="geomFactory"/>.
        /// </summary>
        /// <param name="geomFactory">The geometry factory</param>
        public GraphFactory(IGeometryFactory geomFactory)
        {
            _geomFactory = geomFactory ?? new GeometryFactory();
        }

        /// <summary>
        /// This Constructor Doesnt do anything special. It just treats the layer as a graph.
        /// </summary>
        /// <param name="theLayer">The Layer</param>
        public GraphFactory(VectorLayer theLayer)
        {
            _geomFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(theLayer.SRID);
            
            var featureCount = theLayer.DataSource.GetFeatureCount();
            System.Diagnostics.Debug.WriteLine("Feature Count: = " + featureCount);

            theLayer.DataSource.Open();
            var featureIds = theLayer.DataSource.GetObjectIDsInView(theLayer.Envelope);
            _edgeCost = new Dictionary<Edge<Coordinate>, double>();
            
            _listOfEndPoints = new List<LineEndPoints>();

            foreach (uint featureID  in featureIds)
            {
                var feature = theLayer.DataSource.GetGeometryByID(featureID);
                
                // We need this  incase the lines are multistring.
                if (feature.OgcGeometryType == OgcGeometryType.LineString)
                {
                    var ls = (ILineString)feature;
                    //var numberOfPoints = ls.NumPoints;


                    // We need a copy of the feature so we can display it. Or perhaps we dont who knows.
                    _theLineStrings.Add(featureIds[(int)featureID], (ILineString)feature);

                    var pp = new LineEndPoints(ls.StartPoint.Coordinate, ls.EndPoint.Coordinate, ls.Length);
                    _listOfEndPoints.Add(pp);

                        if (!_graph.ContainsVertex(ls.StartPoint.Coordinate))
                            _graph.AddVertex(ls.StartPoint.Coordinate);

                        if (!_graph.ContainsVertex(ls.EndPoint.Coordinate))
                            _graph.AddVertex(ls.EndPoint.Coordinate);

                        var theEdge = new Edge<Coordinate>(ls.StartPoint.Coordinate, ls.EndPoint.Coordinate);
                        _graph.AddEdge(theEdge);
                        _edgeCost.Add(theEdge,ls.Length);

                        var theReverseEdge = new Edge<Coordinate>(ls.EndPoint.Coordinate, ls.StartPoint.Coordinate);
                        _graph.AddEdge(theReverseEdge);
                        _edgeCost.Add(theReverseEdge, ls.Length);
                }
                else if (feature.OgcGeometryType == OgcGeometryType.MultiLineString)
                {
                    var mls = (IMultiLineString)feature;

                    for(var i = 0; i < mls.NumGeometries; i++)
                    {
                        var ls = (ILineString) mls.GetGeometryN(i);
                        // We need a copy of the feature so we can display it. Or perhaps we dont who knows.
                        _theLineStrings.Add(featureIds[(int)featureID], (ILineString)feature);

                        var pp = new LineEndPoints(ls.StartPoint.Coordinate, ls.EndPoint.Coordinate, ls.Length);
                        _listOfEndPoints.Add(pp);
                        
                            if (!_graph.ContainsVertex(ls.StartPoint.Coordinate))
                                _graph.AddVertex(ls.StartPoint.Coordinate);

                            if (!_graph.ContainsVertex(ls.EndPoint.Coordinate))
                                _graph.AddVertex(ls.EndPoint.Coordinate);

                            var theEdge = new Edge<Coordinate>(ls.StartPoint.Coordinate, ls.EndPoint.Coordinate);
                            _graph.AddEdge(theEdge);
                            _edgeCost.Add(theEdge, ls.Length);

                            var theReverseEdge = new Edge<Coordinate>(ls.EndPoint.Coordinate, ls.StartPoint.Coordinate);
                            _graph.AddEdge(theReverseEdge);
                            _edgeCost.Add(theReverseEdge, ls.Length);
                        
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Graph Stats:" + _graph.EdgeCount + " , " + _graph.VertexCount);

        }

        /// <summary>
        /// When the user clicks on a source point we will need to trabnslate that location to the nearest point on the network 
        /// </summary>
        /// <param name="userPoint"></param>
        /// <param name="tolerence"></param>
        /// <param name="growValue"></param>
        /// <param name="theLayer"></param>
        /// <returns></returns>
        public IntersectionPackage TranslateToPointOnLine(Coordinate userPoint, double tolerence, double growValue, VectorLayer theLayer)
        {
            try
            {
                // How much should we increase the search area by until we reach tolerence.
                double GROWVALUE = growValue;
                
                // Record how many features are in the bounding box
                int featureCount = 0;

                // The features that are withinthe boundbox of GROWVALUE of UserPoints.
                Collection<IGeometry> geometrysInTolerence = null;


                IGeometry clickPointAsNts = null;
                while ((featureCount == 0) && (GROWVALUE < tolerence))
                {
                    // Take the point where the user clicked. Grow it by a set a given amount. We use the boundry 
                    // box to find all lines within a given tolerence. 

                    clickPointAsNts = _geomFactory.CreatePoint(userPoint);
                    var clickPointBoundry = new Envelope(userPoint);
                    clickPointBoundry = clickPointBoundry.Grow(GROWVALUE);

                    var originalLayer = theLayer;
                    originalLayer.DataSource.Open();

                    geometrysInTolerence =
                        originalLayer.DataSource.GetGeometriesInView(clickPointBoundry);

                    GROWVALUE *= 2;

                    if (geometrysInTolerence != null)
                        featureCount = geometrysInTolerence.Count;
                }

                // If there are any geometries in the boundry box then we loop around them them. We are looking for the cloest point so
                // we can **try** to perform a snap operation. NOTE: This entire procedure is a bit flawed.
                if (geometrysInTolerence == null)
                    return null;

                if (geometrysInTolerence.Count > 0)
                {
                    double closestDistance = double.MaxValue;
                    Coordinate intersectionPointAsNts = null;
                    ILineString closestToThisLine = null;



                    foreach (IGeometry geometryInTolerence in geometrysInTolerence)
                    {

                        var nearestPoints = NetTopologySuite.Operation.Distance.DistanceOp.NearestPoints(clickPointAsNts, geometryInTolerence);

                        if (nearestPoints == null)
                            return null;
                        
                        // We get two points back. The point where we clicked and the point of intersection (?????) on the line). If
                        // we calculate the distance between the two we can 
                        var p1 = nearestPoints[0];
                        var p2 = nearestPoints[1];

                        var lDistance = p1.Distance(p2);
                        if (lDistance < closestDistance)
                        {
                            closestDistance = lDistance;
                            intersectionPointAsNts = p2;
                            closestToThisLine = (ILineString)geometryInTolerence;
                        }
                    }

                    // Getting Here would mean that we now know the line we
                    if (closestDistance < double.MaxValue)
                    {
                        var theDeliveryPackage = new IntersectionPackage((IPoint)clickPointAsNts,
                            _geomFactory.CreatePoint(intersectionPointAsNts), closestToThisLine);
                        return theDeliveryPackage;
                    }
                }
                else
                    return null;


                return null;
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.ToString());
                return null;
            }
        }

        public ILineString[] SplitTheLine(ILineString toBeSplit, IPoint splitPoint)
        {
            var llStart = new LinearLocation(0, 0);
            var llSplit = LocationIndexOfPoint.IndexOf(toBeSplit, splitPoint.Coordinate);
            var llEnd = new LinearLocation(0, 1);

            var res = new[]
                {
                    (ILineString)ExtractLineByLocation.Extract(toBeSplit, llStart, llSplit),
                    (ILineString)ExtractLineByLocation.Extract(toBeSplit, llSplit, llEnd)
                };

            return res;
            
            var cDistance = double.MaxValue;
            Coordinate cPoint = null;

            // From this point on it becomes a bit of a 'fudge'. Because the NTS functionality is a map matching dream (ie it returns 
            // a point that may not be on the line. But We need to split the line into two. Therefore we have to get the distance 
            // from the intersection to the cloest point on the line. More on this when we get there.
            // ToDo: Perhaps theres a much better way of doing this already in either SharpMap/ or more likely NTS?????????????????
            for (var i = 0; i < toBeSplit.Coordinates.Length; i++)
            {
                var pointOnTheLine = toBeSplit.Coordinates[i];

                double lDistance = pointOnTheLine.Distance(splitPoint.Coordinate);

                if (lDistance < cDistance)
                {
                    cDistance = lDistance;
                    cPoint = pointOnTheLine;
                }

            }

            // Now We Need To Try And Calculate The Length Along The Line Of That Node.
            var indexAlongLine = 0;
            var firstPart = new List<Coordinate>();
            var secondPart = new List<Coordinate>();


            for (var i = 0; i < toBeSplit.Coordinates.Length; i++)
            {
                var pointOnTheLine =
                    toBeSplit.Coordinates[i];

                firstPart.Add(pointOnTheLine);

                if (ReferenceEquals(pointOnTheLine, cPoint))
                    break;

                indexAlongLine++;
            }

            for (int i = indexAlongLine; i < toBeSplit.Coordinates.Length; i++)
            {
                var pointOnTheLine = toBeSplit.Coordinates[i];

                secondPart.Add(pointOnTheLine);
            }

            var firstPartAsString = _geomFactory.CreateLineString(firstPart.ToArray());
            var secondPartAsString = _geomFactory.CreateLineString(secondPart.ToArray());

            var splitLineParts = new ILineString[2];
            splitLineParts[0] = firstPartAsString;
            splitLineParts[1] = secondPartAsString;

            return splitLineParts;
        }

        /// <summary>
        /// Hopefully some of the stuff above will become slightly more clear in this part of the code. But now we need to 
        /// replace some of the edges in the graph with the corresponding edges based on the users selections for the source 
        /// and destination.
        /// </summary>
        /// <param name="sourceLines">The Two Lines Making Up What Was The Line Closest To The Source</param>
        /// <param name="destinationLine">The Two Lines Making Up What Was The Line Cloest To The Destination </param>
        /// <param name="sourcePoint">Where The User Wants The Source.</param>
        /// <param name="destinationPoint">Where The User Wants The Destination</param>
        /// <param name="useCondensedFlag"></param>
        /// <returns>True When It Works, False When It Doesnt.</returns>
        public bool ReconstructGraph(ILineString[] sourceLines, ILineString[] destinationLine, Coordinate sourcePoint, Coordinate destinationPoint,bool useCondensedFlag)
        {
            _graph = new AdjacencyGraph<Coordinate, Edge<Coordinate>>(false);
            _edgeCost = new Dictionary<Edge<Coordinate>, double>();
            //_lookup = new Dictionary<uint, Coordinate>();

            var index = (uint)_theLineStrings.Count;

            _theLineStrings.Add(index, sourceLines[0]);
            index++;
            _theLineStrings.Add(index, sourceLines[1]);
            index++;
            _theLineStrings.Add(index, destinationLine[0]);
            index++;
            _theLineStrings.Add(index, destinationLine[1]);


            if (useCondensedFlag)
            {
                foreach (var pp in _listOfEndPoints)
                {
                    if ((pp.First.Equals(sourceLines[0].StartPoint.Coordinate)) && (pp.Last.Equals(sourceLines[1].EndPoint.Coordinate)))
                    {
                        // We Are Replacting This Line With The Source Line
                        if (!_graph.ContainsVertex(pp.First))
                            _graph.AddVertex(pp.First);

                        if (!_graph.ContainsVertex(pp.Last))
                            _graph.AddVertex(pp.Last);

                        if (!_graph.ContainsVertex(sourceLines[0].EndPoint.Coordinate))
                            _graph.AddVertex(sourceLines[0].EndPoint.Coordinate);

                        if (!_graph.ContainsVertex(sourceLines[1].StartPoint.Coordinate))
                            _graph.AddVertex(sourceLines[1].StartPoint.Coordinate);

                        var e1 = new Edge<Coordinate>(pp.First, sourceLines[0].EndPoint.Coordinate);
                        var e2 = new Edge<Coordinate>(sourceLines[1].StartPoint.Coordinate, pp.Last);
                        var e1R = new Edge<Coordinate>(sourceLines[0].EndPoint.Coordinate, pp.First);
                        var e2R = new Edge<Coordinate>(pp.Last, sourceLines[1].StartPoint.Coordinate);

                        _graph.AddEdge(e1);
                        _graph.AddEdge(e2);
                        _graph.AddEdge(e1R);
                        _graph.AddEdge(e2R);
                        _edgeCost.Add(e1, sourceLines[0].Length);
                        _edgeCost.Add(e2, sourceLines[1].Length);
                        _edgeCost.Add(e1R, sourceLines[0].Length);
                        _edgeCost.Add(e2R, sourceLines[1].Length);
                    }
                    else if ((pp.First.Equals(destinationLine[0].StartPoint.Coordinate)) && 
                             (pp.Last.Equals(destinationLine[1].EndPoint.Coordinate)))
                    {
                        // We Are Replacing This Line With The Destination Line
                        if (!_graph.ContainsVertex(pp.First))
                            _graph.AddVertex(pp.First);

                        if (!_graph.ContainsVertex(pp.Last))
                            _graph.AddVertex(pp.Last);

                        if (!_graph.ContainsVertex(destinationLine[0].EndPoint.Coordinate))
                            _graph.AddVertex(destinationLine[0].EndPoint.Coordinate);

                        if (!_graph.ContainsVertex(destinationLine[1].StartPoint.Coordinate))
                            _graph.AddVertex(destinationLine[1].StartPoint.Coordinate);

                        var e1 = new Edge<Coordinate>(pp.First, destinationLine[0].EndPoint.Coordinate);
                        var e2 = new Edge<Coordinate>(destinationLine[1].StartPoint.Coordinate, pp.Last);
                        var e1R = new Edge<Coordinate>(destinationLine[0].EndPoint.Coordinate, pp.First);
                        var e2R = new Edge<Coordinate>(pp.Last, destinationLine[1].StartPoint.Coordinate);

                        _graph.AddEdge(e1);
                        _graph.AddEdge(e2);
                        _graph.AddEdge(e1R);
                        _graph.AddEdge(e2R);
                        _edgeCost.Add(e1, destinationLine[0].Length);
                        _edgeCost.Add(e2, destinationLine[1].Length);
                        _edgeCost.Add(e1R, destinationLine[0].Length);
                        _edgeCost.Add(e2R, destinationLine[1].Length);
                    }
                    else
                    {
                        // We Are Carrying On As Normal.
                        if (!_graph.ContainsVertex(pp.First))
                            _graph.AddVertex(pp.First);

                        if (!_graph.ContainsVertex(pp.Last))
                            _graph.AddVertex(pp.Last);

                        var e = new Edge<Coordinate>(pp.First, pp.Last);
                        var er = new Edge<Coordinate>(pp.Last, pp.First);
                        _graph.AddEdge(e);
                        _edgeCost.Add(e, pp.Length);
                        _graph.AddEdge(er);
                        _edgeCost.Add(er, pp.Length);
                    }
                }
            } // END if using condensed flag
            else
            {
                foreach (ILineString aLineString in _theLineStrings.Values)
                {
                    
                    int numberOfPointsInTheString = aLineString.NumPoints;

                    for (int i = 0; i < (numberOfPointsInTheString - 1); i++)
                    {
                        if (!_graph.ContainsVertex(aLineString.Coordinates[i]))
                            _graph.AddVertex(aLineString.Coordinates[i]);

                        if (!_graph.ContainsVertex(aLineString.Coordinates[i + 1]))
                            _graph.AddVertex(aLineString.Coordinates[i + 1]);

                        var e = new Edge<Coordinate>(aLineString.Coordinates[i], aLineString.Coordinates[i + 1]);
                        var er = new Edge<Coordinate>(aLineString.Coordinates[i + 1], aLineString.Coordinates[i]);
                        double distance = aLineString.Coordinates[i].Distance(aLineString.Coordinates[i + 1]);
                        
                        _graph.AddEdge(e);
                        _edgeCost.Add(e, distance);
                        _graph.AddEdge(er);
                        _edgeCost.Add(er, distance);
                    }

                } // end for
            } // end else (using condensed flag)

            return true;
        }


        internal ILineString PerformShortestPathAnalysis(IPoint source, IPoint destination,bool usesCondensedGraph)
        {
            // We keep  a copy so we can terminate the search early.
            _theDestination = destination;

            // This is an instrance of the shortest path algortihm.
            _dijkstra = new DijkstraShortestPathAlgorithm<Coordinate, Edge<Coordinate>>(_graph, AlgorithmExtensions.GetIndexer(_edgeCost));
            
            // Quick Graph uses 'observers'  to record the distance traveled and the path travelled througth, 
            var distObserver = new VertexDistanceRecorderObserver<Coordinate, Edge<Coordinate>>(AlgorithmExtensions.GetIndexer(_edgeCost));
            var predecessorObserver = new VertexPredecessorRecorderObserver<Coordinate, Edge<Coordinate>>();
            distObserver.Attach(_dijkstra);
            predecessorObserver.Attach(_dijkstra);

            // Having this present means that when we finally reach the target node 
            // the dijkstra algortihm can quit without scanning other nodes in the graph, leading to
            // performance increases.
            _dijkstra.FinishVertex += dijkstra_FinishVertex;


            // This is where the shortest path is calculated. 
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            _dijkstra.Compute(source.Coordinate);
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Dijsktra Took: " + sw.ElapsedMilliseconds);

            // get the cost of the path. If one is found then d (=distance) should be greater than zero so we get the edges making up
            // the path.
            double d = AlgorithmExtensions.ComputePredecessorCost(predecessorObserver.VertexPredecessors, _edgeCost, destination.Coordinate);
            System.Diagnostics.Debug.WriteLine(d);

            if (d > 0)
            {
                IEnumerable<Edge<Coordinate>> edgesInShortestPath;
                if (predecessorObserver.TryGetPath(destination.Coordinate, out edgesInShortestPath))
                {
                    var theCompleteShortestPath = new List<Coordinate>();

                    // We need to use a different approach when using the condensed graph.
                    if (usesCondensedGraph)
                    {
                        foreach (var edgeInShortPath in edgesInShortestPath)
                        {
                            var ls = GetLineStringInformation(edgeInShortPath);

                            if (ls != null)
                            {
                                // We need to get each of the nodes that makes up the lines.
                                // we need to add each of them on one list. 
                                var count = ls.Coordinates.Length;

                                for (var i = 0; i < count; i++)
                                    theCompleteShortestPath.Add(ls.Coordinates[i]);
                            } // End Of If
                        } // Loop Around Each Edge In The Shortest Path
                    } // End of If
                    else
                    {
                        foreach (var edgeInShortPath in edgesInShortestPath)
                        {
                            theCompleteShortestPath.Add(edgeInShortPath.Source);
                            theCompleteShortestPath.Add(edgeInShortPath.Target);
                        }
                    } // End Of Else

                    var theShortestPath = _geomFactory.CreateLineString(theCompleteShortestPath.ToArray());
                    return theShortestPath;

                } // There Was A Shortest Path

                // Return null. 
                // ToDo: Need to improve this bit so it at least indicates if the SP didnt get a path/
                return null; 
            }

            return null;

        }

        /// <summary>
        /// this is used when performing a search using a condensed graph.
        /// </summary>
        /// <param name="edgeInShortPath"></param>
        /// <returns></returns>
        private ILineString GetLineStringInformation(Edge<Coordinate> edgeInShortPath)
        {
            try
            {
                foreach (KeyValuePair<uint, ILineString> p in _theLineStrings)
                {
                    var ls = p.Value;

                    if ((((ls.StartPoint.X == edgeInShortPath.Source.X) && (ls.StartPoint.Y == edgeInShortPath.Source.Y)) &&
                     ((ls.EndPoint.X == edgeInShortPath.Target.X) && (ls.EndPoint.Y == edgeInShortPath.Target.Y))) ||
                        (((ls.EndPoint.X == edgeInShortPath.Source.X) && (ls.EndPoint.Y == edgeInShortPath.Source.Y)) &&
                     ((ls.StartPoint.X == edgeInShortPath.Target.X) && (ls.StartPoint.Y == edgeInShortPath.Target.Y))))
                    {
                        return ls;
                    }

                }

                return _geomFactory.CreateLineString((ICoordinateSequence)null);
            }
            catch (Exception)
            {
                return _geomFactory.CreateLineString((ICoordinateSequence)null);
            }
        }

        void dijkstra_FinishVertex(Coordinate vertex)
        {
            if (ReferenceEquals(vertex, _theDestination.Coordinate))
                _dijkstra.Abort();

        }
    }
}
