// Copyright 2006 - Morten Nielsen (www.iter.dk)
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

using System.Collections.Generic;
using System.Linq;
using System;

namespace SharpMap.Rendering
{      
    /// <summary>
    /// Class defining delegate for label collision detection and static predefined methods
    /// </summary>
    public class LabelCollisionDetection
    {
        #region Delegates

        /// <summary>
        /// Delegate method for filtering labels. Useful for performing custom collision detection on labels
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public delegate void LabelFilterMethod(List<BaseLabel> labels);

        #endregion

        #region Label filter methods

        /// <summary>
        /// Simple and fast label collision detection.
        /// </summary>
        /// <param name="labels"></param>
        [Obsolete("This is like no collision detection at all")]
        public static void SimpleCollisionDetection(List<BaseLabel> labels)
        {
            labels.Sort(); // sort labels by intersectiontests of labelbox
            //remove labels that intersect other labels
            for (int i = labels.Count - 1; i > 0; i--)
                if (labels[i].CompareTo(labels[i - 1]) == 0)
                {
                    if (labels[i].Priority == labels[i - 1].Priority) continue;

                    if (labels[i].Priority > labels[i - 1].Priority)
                        //labels.RemoveAt(i - 1);
                        labels[i - 1].Show = false;
                    else
                        //labels.RemoveAt(i)
                        labels[i].Show = false;
                }
        }

        /// <summary>
        /// Thorough label collision detection.
        /// </summary>
        /// <param name="labels"></param>
        public static void ThoroughCollisionDetection(List<BaseLabel> labels)
        {
            labels.Sort(); // sort labels by intersectiontests of labelbox
            //remove labels that intersect other labels
            for (int i = labels.Count - 1; i > 0; i--)
            {
                if (!labels[i].Show) continue;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (!labels[j].Show) continue;
                    if (labels[i].CompareTo(labels[j]) == 0)
                        if (labels[i].Priority >= labels[j].Priority)
                        {
                            labels[j].Show = false;
                            //labels.RemoveAt(j);
                            //i--;
                        }
                        else
                        {
                            labels[i].Show = false;
                            //labels.RemoveAt(i);
                            //i--;
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Quick (O(n log n)) and accurate collision detection
        /// </summary>
        /// <param name="labels"></param>
        public static void QuickAccurateCollisionDetectionMethod(List<BaseLabel> labels)
	    {
		    // find minimum / maximum coordinates
		    double minX = double.MaxValue;
		    double maxX = double.MinValue;
		    double minY = double.MaxValue;
		    double maxY = double.MinValue;
		    foreach (BaseLabel l in labels)
            {
                var box = new ProperBox(l is PathLabel pl ? new LabelBox(pl.Location.GetBounds()): l.Box);
			    if (box.Left < minX) minX = box.Left;
			    if (box.Right > maxX) maxX = box.Right;
                if (box.Bottom > maxY) maxY = box.Bottom;
			    if (box.Top < minY) minY = box.Top;
		    }   

		    // sort by area (highest priority first, followed by low area to maximize the amount of labels displayed)
		    var sortedLabels = labels.OrderByDescending(label => label.Priority).ThenBy(label =>
            {
                var box = label is PathLabel pl ? new LabelBox(pl.Location.GetBounds()) : label.Box;
                return box.Width * box.Height;
            });

		    // make visible if it does not collide with other labels. Uses a quadtree and is therefore fast O(n log n) 
		    QuadtreeNode<ProperBox> quadTree = new QuadtreeNode<ProperBox>(minX, maxX, minY, maxY, new LabelBoxContainmentChecker(), 0, 10);
		    foreach (BaseLabel l in sortedLabels) {
			    if (!l.Show) continue;
                var box = new ProperBox(l is PathLabel pl ? new LabelBox(pl.Location.GetBounds()) : l.Box);
			    if (quadTree.CollidesWithAny(box))
                {
				    l.Show = false;
			    }
                else
                {
				    quadTree.Insert(box);
			    }
		    }
	    }

        #endregion

        #region "Tools"

        public class QuadtreeNode<T>
        {
            List<QuadtreeNode<T>> children = null;
            List<T> objects;
            int maxObjects;
            int level;
            int maxLevel;

            public double Left { get; set; }
            public double Right { get; set; }
            public double Top { get; set; }
            public double Bottom { get; set; }

            protected QuadtreeContainmentChecker<T> containmentChecker;

            public const int DEFAULT_MAX_OBJECTS_PER_NODE = 5;

            public QuadtreeNode(double left, double right, double top, double bottom, QuadtreeContainmentChecker<T> containmentChecker, int maxObjects, int level, int maxLevel)
            {
                this.Left = left;
                this.Right = right;
                this.Top = top;
                this.Bottom = bottom;
                this.containmentChecker = containmentChecker;
                this.maxObjects = maxObjects;
                this.objects = new List<T>(maxObjects);
                this.level = level;
                this.maxLevel = maxLevel;
            }

            public QuadtreeNode(double left, double right, double top, double bottom, QuadtreeContainmentChecker<T> containmentChecker, int level, int maxLevel)
                : this(left, right, top, bottom, containmentChecker, QuadtreeNode<T>.DEFAULT_MAX_OBJECTS_PER_NODE, level, maxLevel) { }

            public bool Insert(T obj)
            {
                if (this.IsFullyContained(obj))
                {
                    if (this.IsLeaf())
                    {
                        if (objects.Count >= maxObjects && level < maxLevel)
                        {
                            this.Divide();
                            if (!this.InsertIntoChildren(obj)) this.objects.Add(obj);
                        }
                        else
                        {
                            this.objects.Add(obj);

                        }
                    }
                    else
                    {
                        if (!this.InsertIntoChildren(obj)) this.objects.Add(obj);
                    }
                    return true;
                }
                return false;
            }

            public bool IsFullyContained(T obj)
            {
                return this.containmentChecker.IsFullyContained(this, obj);
            }

            public bool CollidesWithOrContains(T obj)
            {
                return this.containmentChecker.IsContainedOrIntersects(this, obj);
            }

            public bool IsLeaf()
            {
                return this.children == null;
            }

            public List<QuadtreeNode<T>> GetChildren()
            {
                return this.children;
            }

            public bool CollidesWithAny(T obj)
            {
                if (this.CollidesWithOrContains(obj))
                {
                    //collides with any of current level?
                    foreach (T o in this.objects)
                    {
                        if (this.containmentChecker.IsContainedOrIntersects(o, obj)) return true;
                    }

                    //collides with a child?
                    if (!this.IsLeaf())
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (children[i].CollidesWithAny(obj)) return true;
                        }
                    }
                }
                return false;
            }

            public List<T> GetObjects(double x, double y)
            {
                if (this.IsLeaf())
                {
                    return this.objects;
                }
                else
                {
                    foreach (QuadtreeNode<T> node in this.children)
                    {
                        if (x >= node.Left && x <= node.Right && y >= node.Bottom && y <= node.Top)
                        {
                            return node.GetObjects(x, y);
                        }
                    }
                }
                return new List<T>();
            }

            protected bool InsertIntoChildren(T obj)
            {
                bool inserted = false;
                for (int i = 0; i < 4; i++)
                {
                    if (children[i].Insert(obj)) inserted = true;
                }
                return inserted;
            }

            protected void Divide()
            {
                this.children = new List<QuadtreeNode<T>>(4);
                double middleX = Left + ((Right - Left) / 2);
                double middleY = Top + (Math.Abs(Top - Bottom) / 2);
                this.children.Add(new QuadtreeNode<T>(Left, middleX, Top, middleY, containmentChecker, maxObjects, level + 1, maxLevel));
                this.children.Add(new QuadtreeNode<T>(middleX, Right, Top, middleY, containmentChecker, maxObjects, level + 1, maxLevel));
                this.children.Add(new QuadtreeNode<T>(Left, middleX, middleY, Bottom, containmentChecker, maxObjects, level + 1, maxLevel));
                this.children.Add(new QuadtreeNode<T>(middleX, Right, middleY, Bottom, containmentChecker, maxObjects, level + 1, maxLevel));

                //move objects to child nodes
                List<T> remainingObjects = new List<T>();
                foreach (T obj in this.objects)
                {
                    if (!this.InsertIntoChildren(obj)) remainingObjects.Add(obj);
                }
                this.objects = remainingObjects;
            }

            public interface QuadtreeContainmentChecker<P>
            {
                bool IsContainedOrIntersects(QuadtreeNode<P> node, P obj);

                bool IsContainedOrIntersects(P obj1, P obj2);

                bool IsFullyContained(QuadtreeNode<P> node, P obj);


            }
        }

        public class ProperBox
        {
            private LabelBox box;
            public ProperBox(LabelBox box)
            {
                this.box = box;
            }

            public float Left
            {
                get { return box.Left; }
            }

            public float Right
            {
                get { return box.Right; }
            }

            public float Height
            {
                get { return box.Height; }
            }

            public float Width
            {
                get { return box.Width; }
            }

            public float Bottom
            {
                get { return box.Top + Height; }
            }

            public float Top
            {
                get { return box.Top; }
            }
        }

        public class LabelBoxContainmentChecker : QuadtreeNode<ProperBox>.QuadtreeContainmentChecker<ProperBox>
        {

            public bool IsContainedOrIntersects(QuadtreeNode<ProperBox> node, ProperBox obj)
            {
                return !(obj.Left > node.Right | obj.Right < node.Left | obj.Top > node.Bottom | obj.Bottom < node.Top);
            }

            public bool IsFullyContained(QuadtreeNode<ProperBox> node, ProperBox obj)
            {
                return obj.Left >= node.Left & obj.Right <= node.Right & obj.Bottom <= node.Bottom & obj.Top >= node.Top;
            }

            public bool IsContainedOrIntersects(ProperBox obj1, ProperBox obj2)
            {
                return !(obj1.Left > obj2.Right | obj1.Right < obj2.Left | obj1.Top > obj2.Bottom | obj1.Bottom < obj2.Top);
            }
        }

        #endregion
    }
}
