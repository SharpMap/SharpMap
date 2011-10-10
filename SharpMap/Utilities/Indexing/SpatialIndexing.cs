// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using SharpMap.Geometries;

namespace SharpMap.Utilities.SpatialIndexing
{
    /// <summary>
    /// Heuristics used for tree generation
    /// </summary>
    public struct Heuristic
    {
        /// <summary>
        /// Maximum tree depth
        /// </summary>
        public int maxdepth;

        /// <summary>
        /// Minimum Error metric – the volume of a box + a unit cube.
        /// The unit cube in the metric prevents big boxes that happen to be flat having a zero result and muddling things up.
        /// </summary>
        public int minerror;

        /// <summary>
        /// Minimum object count at node
        /// </summary>
        public int mintricnt;

        /// <summary>
        /// Target object count at node
        /// </summary>
        public int tartricnt;
    }


    /// <summary>
    /// Constructs a Quad-tree node from a object list and creates its children recursively
    /// </summary>
    public class QuadTree : IDisposable
    {
        private BoundingBox _box;
        
        private QuadTree _child0;
        private QuadTree _child1;

        /// <summary>
        /// Gets the number of Nodes
        /// </summary>
        public int NodeCount 
        { 
            get
            {
                if (_child0 != null)
                    return _child0.NodeCount + _child1.NodeCount;

                return _objList != null ? _objList.Count : 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this branch is prunable
        /// </summary>
        public bool IsPrunable { get { return NodeCount == 0; } }

        /// <summary>
        /// Nodes depth in a tree
        /// </summary>
        private readonly uint _depth;

        /// <summary>
        /// Node ID
        /// </summary>
        public uint? _ID;

        private List<BoxObjects> _objList;

        /// <summary>
        /// Creates a node and either splits the objects recursively into sub-nodes, or stores them at the node depending on the heuristics.
        /// Tree is built top->down
        /// </summary>
        /// <param name="objList">Geometries to index</param>
        /// <param name="depth">Current depth of tree</param>
        /// <param name="heurdata">Heuristics data</param>
        public QuadTree(List<BoxObjects> objList, uint depth, Heuristic heurdata)
        {
            _depth = depth;

            _box = objList[0].Box;
            for (int i = 0; i < objList.Count; i++)
                _box = _box.Join(objList[i].Box);

            // test our build heuristic - if passes, make children
            if (depth < heurdata.maxdepth && objList.Count > heurdata.mintricnt &&
                (objList.Count > heurdata.tartricnt || ErrorMetric(_box) > heurdata.minerror))
            {
                var objBuckets = new List<BoxObjects>[2]; // buckets of geometries
                objBuckets[0] = new List<BoxObjects>();
                objBuckets[1] = new List<BoxObjects>();

                uint longaxis = _box.LongestAxis; // longest axis
                double geoavg = 0; // geometric average - midpoint of ALL the objects

                // go through all bbox and calculate the average of the midpoints
                double frac = 1.0f/objList.Count;
                for (int i = 0; i < objList.Count; i++)
                    geoavg += objList[i].Box.GetCentroid()[longaxis]*frac;

                // bucket bbox based on their midpoint's side of the geo average in the longest axis
                for (int i = 0; i < objList.Count; i++)
                    objBuckets[geoavg > objList[i].Box.GetCentroid()[longaxis] ? 1 : 0].Add(objList[i]);

                //If objects couldn't be splitted, just store them at the leaf
                //TODO: Try splitting on another axis
                if (objBuckets[0].Count == 0 || objBuckets[1].Count == 0)
                {
                    _child0 = null;
                    _child1 = null;
                    // copy object list
                    _objList = objList;
                }
                else
                {
                    //We don't need the list anymore;
                    objList.Clear();

                    // create new children using the buckets
                    _child0 = new QuadTree(objBuckets[0], depth + 1, heurdata);
                    _child1 = new QuadTree(objBuckets[1], depth + 1, heurdata);
                }
            }
            else
            {
                // otherwise the build heuristic failed, this is 
                // set the first child to null (identifies a leaf)
                _child0 = null;
                _child1 = null;
                // copy object list
                _objList = objList;
            }
        }

        /// <summary>
        /// This means areas overlap by 5%
        /// </summary>
        private const double SplitRatio = 0.55d;

        private static void SplitBoundingBox(BoundingBox input, out BoundingBox out1, out BoundingBox out2)
        {
            double range;

            /* -------------------------------------------------------------------- */
            /*      Split in X direction.                                           */
            /* -------------------------------------------------------------------- */
            if ((input.Width) > (input.Height))
            {
                range = input.Width*SplitRatio;

                out1 = new BoundingBox(input.BottomLeft.Clone(), new Point(input.Left + range, input.Top));
                out2 = new BoundingBox(new Point(input.Right - range, input.Bottom), input.TopRight.Clone());
            }

                /* -------------------------------------------------------------------- */
                /*      Otherwise split in Y direction.                                 */
                /* -------------------------------------------------------------------- */
            else
            {
                range = input.Height*SplitRatio;

                out1 = new BoundingBox(input.BottomLeft.Clone(), new Point(input.Right, input.Bottom + range));
                out2 = new BoundingBox(new Point(input.Left, input.Top - range), input.TopRight.Clone());
            }
            //Debug.Assert(out1.Intersects(out2));
        }

        /// <summary>
        /// Adds a new <see cref="BoxObjects"/> to this node.
        /// </summary>
        /// <param name="o">The boxed object</param>
        /// <param name="h">The child node creation heuristic</param>
        public void AddNode(BoxObjects o, Heuristic h)
        {
          /* -------------------------------------------------------------------- */
          /*      If there are subnodes, then consider whether this object        */
          /*      will fit in them.                                               */
          /* -------------------------------------------------------------------- */
            if (_child0 != null && _depth < h.maxdepth)
            {
                if (_child0.Box.Contains(o.Box.GetCentroid()))
                    _child0.AddNode(o, h);
                else if (_child1.Box.Contains(o.Box.GetCentroid()))
                    _child1.AddNode(o, h);
                return;
            }
        
            /* -------------------------------------------------------------------- */
            /*      Otherwise, consider creating two subnodes if could fit into     */
            /*      them, and adding to the appropriate subnode.                    */
            /* -------------------------------------------------------------------- */
            if( h.maxdepth > _depth && !IsLeaf )
            {
                BoundingBox half1, half2;
                SplitBoundingBox(Box, out half1, out half2);
            

                if( half1.Contains(o.Box.GetCentroid())) 
                {
                    _child0 = new QuadTree(half1, _depth + 1);
                    _child1 = new QuadTree(half2, _depth + 1);
                    _child0.AddNode(o, h);
                    return;
                }    
	            if(half2.Contains(o.Box.GetCentroid()))
	            {
                    _child0 = new QuadTree(half1, _depth + 1);
                    _child1 = new QuadTree(half2, _depth + 1);
	                _child1.AddNode(o, h);
	                return;
	            }
            }
        
            /* -------------------------------------------------------------------- */
            /*      If none of that worked, just add it to this nodes list.         */
            /* -------------------------------------------------------------------- */
            //Debug.Assert(_child0 == null);
            
            if (_objList == null)
                _objList = new List<BoxObjects>();

            if (!Box.Contains(o.Box))
                Box = Box.Join(o.Box);
            
            _objList.Add(o);

        }


        /// <summary>
        /// This instantiator is used internally for loading a tree from a file
        /// </summary>
        private QuadTree()
        {
        }

        /// <summary>
        /// This instantiator is used internally for linear creation of quadtrees
        /// </summary>
        /// <param name="box">The initial bounding box</param>
        /// <param name="depth">The depth</param>
        private QuadTree(BoundingBox box, uint depth)
        {
            _box = box;
            _depth = depth;
        }

        #region Read/Write index to/from a file

        private const double INDEXFILEVERSION = 1.0;

        /// <summary>
        /// Loads a quadtree from a file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static QuadTree FromFile(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                if (br.ReadDouble() != INDEXFILEVERSION) //Check fileindex version
                {
                    fs.Close();
                    fs.Dispose();
                    throw new ObsoleteFileFormatException(
                        "Invalid index file version. Please rebuild the spatial index by either deleting the index");
                }
                var node = ReadNode(0, br);
                return node;
            }
        }

        /// <summary>
        /// Reads a node from a stream recursively
        /// </summary>
        /// <param name="depth">Current depth</param>
        /// <param name="br">Binary reader reference</param>
        /// <returns></returns>
        private static QuadTree ReadNode(uint depth, BinaryReader br)
        {
            var bbox = new BoundingBox(br);
            var node = new QuadTree(bbox, depth);
            
            var isLeaf = br.ReadBoolean();
            if (isLeaf)
            {
                var featureCount = br.ReadInt32();
                node._objList = new List<BoxObjects>();
                for (int i = 0; i < featureCount; i++)
                {
                    var box = new BoxObjects();
                    box.Box = new BoundingBox(br);
                    box.ID = (uint) br.ReadInt32();
                    node._objList.Add(box);
                }
            }
            else
            {
                node.Child0 = ReadNode(depth + 1, br);
                node.Child1 = ReadNode(depth + 1, br);
            }
            return node;
        }

        /// <summary>
        /// Saves the Quadtree to a file
        /// </summary>
        /// <param name="filename"></param>
        public void SaveIndex(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(INDEXFILEVERSION); //Save index version
                SaveNode(this, bw);
            }
        }

        /// <summary>
        /// Saves a node to a stream recursively
        /// </summary>
        /// <param name="node">Node to save</param>
        /// <param name="sw">Reference to BinaryWriter</param>
        private static void SaveNode(QuadTree node, BinaryWriter sw)
        {
            //Write node boundingbox
            sw.Write(node.Box.Min.X);
            sw.Write(node.Box.Min.Y);
            sw.Write(node.Box.Max.X);
            sw.Write(node.Box.Max.Y);
            sw.Write(node.IsLeaf);
            if (node.IsLeaf || node.Child0 == null)
            {
                if (node._objList == null)
                {
                    sw.Write(0);
                    return;
                }

                sw.Write(node._objList.Count); //Write number of features at node
                for (int i = 0; i < node._objList.Count; i++) //Write each featurebox
                {
                    sw.Write(node._objList[i].Box.Min.X);
                    sw.Write(node._objList[i].Box.Min.Y);
                    sw.Write(node._objList[i].Box.Max.X);
                    sw.Write(node._objList[i].Box.Max.Y);
                    sw.Write(node._objList[i].ID);
                }
            }
            else if (!node.IsLeaf) //Save next node
            {
                SaveNode(node.Child0, sw);
                SaveNode(node.Child1, sw);
            }
        }

        /// <summary>
        /// Creates a quadtree root node.
        /// </summary>
        /// <param name="b">The root bounding box</param>
        /// <returns>The root node for the quadtree</returns>
        public static QuadTree CreateRootNode(BoundingBox b)
        {
            return new QuadTree(b, 0);
        }

        internal class ObsoleteFileFormatException : Exception
        {
            /// <summary>
            /// Exception thrown when layer rendering has failed
            /// </summary>
            /// <param name="message"></param>
            public ObsoleteFileFormatException(string message)
                : base(message)
            {
            }
        }

        #endregion

        /// <summary>
        /// Determines whether the node is a leaf (if data is stored at the node, we assume the node is a leaf)
        /// </summary>
        public bool IsLeaf
        {
            get { return (_objList != null); }
        }

        ///// <summary>
        ///// Gets/sets the list of objects in the node
        ///// </summary>
        //public List<SharpMap.Geometries.IGeometry> ObjList
        //{
        //    get { return _objList; }
        //    set { _objList = value; }
        //}

        /// <summary>
        /// Gets/sets the Axis Aligned Bounding Box
        /// </summary>
        public BoundingBox Box
        {
            get { return _box; }
            set { _box = value; }
        }

        /// <summary>
        /// Gets/sets the left child node
        /// </summary>
        public QuadTree Child0
        {
            get { return _child0; }
            set { _child0 = value; }
        }

        /// <summary>
        /// Gets/sets the right child node
        /// </summary>
        public QuadTree Child1
        {
            get { return _child1; }
            set { _child1 = value; }
        }

        /// <summary>
        /// Gets the depth of the current node in the tree
        /// </summary>
        public uint Depth
        {
            get { return _depth; }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the node
        /// </summary>
        public void Dispose()
        {
            //this._box = null;
            _child0 = null;
            _child1 = null;
            _objList = null;
        }

        #endregion

        /// <summary>
        /// Calculate the floating point error metric 
        /// </summary>
        /// <returns></returns>
        public static double ErrorMetric(BoundingBox box)
        {
            Point temp = new Point(1, 1) + (box.Max - box.Min);
            return temp.X*temp.Y;
        }

        /// <summary>
        /// Searches the tree and looks for intersections with the boundingbox 'bbox'
        /// </summary>
        /// <param name="box">Boundingbox to intersect with</param>
        public Collection<uint> Search(BoundingBox box)
        {
            Collection<uint> objectlist = new Collection<uint>();
            IntersectTreeRecursive(box, this, ref objectlist);
            return objectlist;
        }

        /// <summary>
        /// Recursive function that traverses the tree and looks for intersections with a boundingbox
        /// </summary>
        /// <param name="box">Boundingbox to intersect with</param>
        /// <param name="node">Node to search from</param>
        /// <param name="list">List of found intersections</param>
        private static void IntersectTreeRecursive(BoundingBox box, QuadTree node, ref Collection<uint> list)
        {
            if (node.IsLeaf) //Leaf has been reached
            {
                foreach (var boxObject in node._objList)
                {
                    if(box.Intersects(boxObject.Box))
                        list.Add(boxObject.ID);

                }
                /*
                for (int i = 0; i < node._objList.Count; i++)
                {
                    list.Add(node._objList[i].ID);
                }
                */
            }
            else
            {
                if (node.Box.Intersects(box))
                {
                    if (node.Child0 != null)
                        IntersectTreeRecursive(box, node.Child0, ref list);
                    if (node.Child1 != null)
                        IntersectTreeRecursive(box, node.Child1, ref list);
                }
            }
        }

        #region Nested type: BoxObjects

        /// <summary>
        /// BoundingBox and Feature ID structure used for storing in the quadtree 
        /// </summary>
        public struct BoxObjects
        {
            /// <summary>
            /// Boundingbox
            /// </summary>
            public BoundingBox Box;

            /// <summary>
            /// Feature ID
            /// </summary>
            public uint ID;
        }

        #endregion
    }
}