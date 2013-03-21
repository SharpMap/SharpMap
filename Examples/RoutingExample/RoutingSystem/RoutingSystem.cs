using System;
using GeoAPI.Geometries;

namespace RoutingExample.RoutingSystem
{
    /// <summary>
    /// This class just groups together all the information needed to perform a routeing operation
    /// </summary>
    public class RoutingSystem
    {
        /// <summary>
        /// Indicates if a condensed flag is to be used. STILL IN BETA - ONLY RETURNS A PATH CLOSE TO THE SHORTER PATH, 
        /// NESSECERILLY THE ACTUAL SHORTEST PATH.
        /// </summary>
        public bool Condensed { get; set; }

        /// <summary>
        /// Indicates the maximum distance from a point that we search for geomtries.
        /// </summary>
        public int MaxTolerence = 100;

        /// <summary>
        /// How much does the search distance increase per iteration until it finds a geometry or maxtolerence is exceeded.
        /// </summary>
        public double growValue_ = 5.0; 

        /// <summary>
        /// The Layer we want to perform the analysis on.
        /// </summary>
        private SharpMap.Layers.VectorLayer _analysisLayer;

        /// <summary>
        /// The point on the map the user clicks to indicate a source
        /// </summary>
        private Coordinate _userClickPointForSource;

        /// <summary>
        /// The point on the map the user clicks to indicate a destination
        /// </summary>
        private Coordinate _userClickPointsForDestination;

        /// <summary>
        /// An array of two lines splitting the line cloest to the user click point for the source in 2.
        /// </summary>
        private ILineString[] _splitSourceLine;

        /// <summary>
        /// An array of two lines splitting the line cloest to the user click point for the destination in 2.
        /// </summary>
        private ILineString[] _splitDestinationLine;


        /// <summary>
        /// If set to true will try and condesne  the graph, if not will apply Disjktra as normal. STILL IN BETA WITH BUGS.
        /// </summary>
        public bool UseCondensedGraph
        {
            get
            {
                return Condensed;
            }

            set
            {
                Condensed = value;
            }
        }

        /// <summary>
        /// The User Selected Source
        /// </summary>
        public Coordinate UserSource
        {
            get
            {
                return _userClickPointForSource;
            }

            set
            {
                if (_analysisLayer == null)
                    throw new ArgumentException("No layer to perform analysis on...");
                
                var gf = new GraphFactory();
                _userClickPointForSource = value;
                var p = gf.TranslateToPointOnLine(_userClickPointForSource, MaxTolerence,growValue_, _analysisLayer);
                _splitSourceLine = gf.SplitTheLine(p.ClosestLine, p.IntersectionPoint);
            }
        }

        /// <summary>
        /// The User Selected Destination
        /// </summary>
        public Coordinate UserDestination
        {
            get
            {
                return _userClickPointsForDestination;
            }

            set
            {
                if (_analysisLayer == null)
                    throw new ArgumentException("No layer to perform analysis on...");
                
                var gf = new GraphFactory();
                _userClickPointsForDestination = value;
                var p = gf.TranslateToPointOnLine(_userClickPointsForDestination, MaxTolerence,growValue_, _analysisLayer);
                _splitDestinationLine = gf.SplitTheLine(p.ClosestLine, p.IntersectionPoint);
            }
        }

        /// <summary>
        /// The User Selected Source
        /// </summary>
        public ILineString[] SourceLine
        {
            get
            {
                return _splitSourceLine;
            }

            set
            {
                _splitSourceLine = value;
            }
        }

        /// <summary>
        /// The User Selected Destination
        /// </summary>
        public ILineString[] DestinationLine
        {
            get
            {
                return _splitDestinationLine;
            }

            set
            {
                _splitDestinationLine = value;
            }
        }

        public SharpMap.Layers.VectorLayer AnalysisLayer
        {
            get
            {
                return _analysisLayer;
            }

            set
            {
                _analysisLayer = value;
            }
        }

        /// <summary>
        /// The point the shortest path analysis will be carried out from
        /// </summary>
        public IPoint SourcePointForCalculation
        {
            get
            {
                if (_splitSourceLine == null)
                    throw new ArgumentException("An Error Occured - There are proabably no geomtryies within tolerence");
                
                // Get the last point on the source line.
                return _splitSourceLine[1].StartPoint;
            }
        }

        /// <summary>
        /// The point the shortest path analysis will be carried out to
        /// </summary>
        public IPoint DestinationPointForCalculation
        {
            get
            {
                if (_splitDestinationLine == null)
                    throw new ArgumentException("An Error Occured - There are proabably no geomtryies within tolerence");
                return _splitDestinationLine[0].EndPoint;
            }
        }


        /// <summary>
        /// The maximum distance around a user point that we search for some geomtrys.
        /// </summary>
        public int MaximumTolerence
        {
            get
            {
                return MaxTolerence;
            }

            set
            {
                MaxTolerence = value;
            }
        }

        /// <summary>
        /// How much does the search distance increase per iteration until it finds a geometry or maxtolerence is exceeded.
        /// </summary>
        public double GrowValue
        {
            get
            {
                return growValue_;
            }

            set
            {
                growValue_ = value;
            }
        }

        public ILineString PerformShortestPathAnalysis(bool usesCondensedGraph)
        {
            try
            {
                // Some validation code first
                if (_analysisLayer == null)
                    throw new ArgumentException("The Analysis has not been created...");

                if (_userClickPointForSource == null)
                    throw new ArgumentException("The User Click Point - Source has not been created...");

                if (_userClickPointsForDestination == null)
                    throw new ArgumentException("The User Click Point - Destination has not been created...");


                // Pass it to the graph factory.
                var gf = new GraphFactory(_analysisLayer);

                // Reconstruct the graph, taking into account where the user wants the source and destination to be. If the condensed flag is 
                // set to  true then it uses the 'Superedge' pricniple. If not is just applies a normal Dijsktra approach - slower, but more accurate 
                // currently. TODO: Improve the condensed version. Theres currently a logic flaw in there. Oooops.
                if (!gf.ReconstructGraph(_splitSourceLine, _splitDestinationLine, 
                    _userClickPointForSource, _userClickPointsForDestination, Condensed))
                    return null;
                
                // Perform the analysis, returning the shortest path as a Linestring.
                return gf.PerformShortestPathAnalysis(SourcePointForCalculation, 
                                                      DestinationPointForCalculation, 
                                                      Condensed);
            }
            catch (Exception)
            {
                return null;
            }
        }
        

    }
}
