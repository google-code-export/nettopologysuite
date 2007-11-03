using System;
using System.Collections;
using System.Text;
using GeoAPI.CoordinateSystems;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Strtree
{
    /// <summary>  
    /// A query-only R-tree created using the Sort-Tile-Recursive (STR) algorithm.
    /// For two-dimensional spatial data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The STR packed R-tree is simple to implement and maximizes space
    /// utilization; that is, as many leaves as possible are filled to capacity.
    /// Overlap between nodes is far less than in a basic R-tree. However, once the
    /// tree has been built (explicitly or on the first call to <see cref="Query"/>), 
    /// items may not be added or removed. 
    /// </para>
    /// <para>
    /// Described in: P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
    /// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
    /// </para>
    /// </remarks>
    public class StrTree<TCoordinate, TItem> : AbstractStrTree, ISpatialIndex<TCoordinate, TItem>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<TCoordinate>, IConvertible
    {
        // TODO: Convert this to a Compare<T> delegate
        private class AnonymousXComparerImpl<TCoordinate, TItem> : IComparer
        {
            private StrTree<TCoordinate, TItem> container = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public AnonymousXComparerImpl(StrTree container)
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="o1"></param>
            /// <param name="o2"></param>
            /// <returns></returns>
            public int Compare(object o1, object o2) 
            {
                return container.CompareDoubles(container.CentreX((IExtents) ((IBoundable) o1).Bounds),
                                                container.CentreX((IExtents) ((IBoundable) o2).Bounds));
            }
        }

        // TODO: Convert this to a Compare<T> delegate
        private class AnonymousYComparerImpl : IComparer
        {
            private STRtree container = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public AnonymousYComparerImpl(STRtree container)
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="o1"></param>
            /// <param name="o2"></param>
            /// <returns></returns>
            public int Compare(object o1, object o2) 
            {
                return container.CompareDoubles(container.CentreY((IExtents) ((IBoundable) o1).Bounds),
                                                container.CentreY((IExtents) ((IBoundable) o2).Bounds));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class AnonymousAbstractNodeImpl : AbstractNode
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="nodeCapacity"></param>
            public AnonymousAbstractNodeImpl(int nodeCapacity) :
                base(nodeCapacity) { }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override object ComputeBounds() 
            {
                IExtents bounds = null;
                for (IEnumerator i = ChildBoundables.GetEnumerator(); i.MoveNext(); ) 
                {
                    IBoundable childBoundable = (IBoundable) i.Current;
                    if (bounds == null) 
                         bounds =  new Envelope((IExtents) childBoundable.Bounds);                
                    else bounds.ExpandToInclude((IExtents) childBoundable.Bounds);
                }
                return bounds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class AnonymousIntersectsOpImpl : IIntersectsOp
        {
            private StrTree container = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public AnonymousIntersectsOpImpl(STRtree container)
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="aBounds"></param>
            /// <param name="bBounds"></param>
            /// <returns></returns>
            public bool Intersects(object aBounds, object bBounds) 
            {
                return ((IExtents) aBounds).Intersects((IExtents) bBounds);
            }
        }

        /// <summary> 
        /// Constructs an STRtree with the default (10) node capacity.
        /// </summary>
        public StrTree() : this(10) { }

        /// <summary> 
        /// Constructs an STRtree with the given maximum number of child nodes that
        /// a node may have.
        /// </summary>
        public StrTree(int nodeCapacity) :
            base(nodeCapacity) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double Avg(double a, double b)
        {
            return (a + b) / 2d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private double CentreX(IExtents e)
        {
            return Avg(e.MinX, e.MaxX);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private double CentreY(IExtents e)
        {
            return Avg(e.MinY, e.MaxY);
        }

        /// <summary>
        /// Creates the parent level for the given child level. First, orders the items
        /// by the x-values of the midpoints, and groups them into vertical slices.
        /// For each slice, orders the items by the y-values of the midpoints, and
        /// group them into runs of size M (the node capacity). For each run, creates
        /// a new (parent) node.
        /// </summary>
        /// <param name="childBoundables"></param>
        /// <param name="newLevel"></param>
        protected override IList CreateParentBoundables(IList childBoundables, int newLevel)
        {
            Assert.IsTrue(childBoundables.Count != 0);
            int minLeafCount = (int)Math.Ceiling((childBoundables.Count / (double)NodeCapacity));
            ArrayList sortedChildBoundables = new ArrayList(childBoundables);
            sortedChildBoundables.Sort(new AnonymousXComparerImpl(this));
            IList[] verticalSlices = VerticalSlices(sortedChildBoundables,
                (int) Math.Ceiling(Math.Sqrt(minLeafCount)));
            IList tempList = CreateParentBoundablesFromVerticalSlices(verticalSlices, newLevel);
            return tempList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verticalSlices"></param>
        /// <param name="newLevel"></param>
        /// <returns></returns>
        private IList CreateParentBoundablesFromVerticalSlices(IList[] verticalSlices, int newLevel)
        {
            Assert.IsTrue(verticalSlices.Length > 0);
            IList parentBoundables = new ArrayList();
            for (int i = 0; i < verticalSlices.Length; i++)
            {
                IList tempList = CreateParentBoundablesFromVerticalSlice(verticalSlices[i], newLevel);
                foreach (object o in tempList)
                    parentBoundables.Add(o);
            }
            return parentBoundables;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childBoundables"></param>
        /// <param name="newLevel"></param>
        /// <returns></returns>
        protected IList CreateParentBoundablesFromVerticalSlice(IList childBoundables, int newLevel)
        {
            return base.CreateParentBoundables(childBoundables, newLevel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childBoundables">Must be sorted by the x-value of the envelope midpoints.</param>
        /// <param name="sliceCount"></param>
        protected IList[] VerticalSlices(IList childBoundables, int sliceCount)
        {
            int sliceCapacity = (int)Math.Ceiling(childBoundables.Count / (double)sliceCount);
            IList[] slices = new IList[sliceCount];
            IEnumerator i = childBoundables.GetEnumerator();
            for (int j = 0; j < sliceCount; j++)
            {
                slices[j] = new ArrayList();
                int boundablesAddedToSlice = 0;
                /* 
                 *          Diego Guidi says:
                 *          the line below introduce an error: 
                 *          the first element at the iteration (not the first) is lost! 
                 *          This is simply a different implementation of Iteration in .NET against Java
                 */
                // while (i.MoveNext() && boundablesAddedToSlice < sliceCapacity)
                while (boundablesAddedToSlice < sliceCapacity && i.MoveNext())
                {
                    IBoundable childBoundable = (IBoundable) i.Current;
                    slices[j].Add(childBoundable);
                    boundablesAddedToSlice++;
                }
            }
            return slices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected override AbstractNode CreateNode(int level) 
        {
            return new AnonymousAbstractNodeImpl(level);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override IIntersectsOp IntersectsOp
        {
            get
            {
                return new AnonymousIntersectsOpImpl(this);
            }
        }

        /// <summary>
        /// Inserts an item having the given bounds into the tree.
        /// </summary>
        /// <param name="itemEnv"></param>
        /// <param name="item"></param>
        public void Insert(IExtents itemEnv, TItem item) 
        {
            if (itemEnv.IsNull)  
                return;
            base.Insert(itemEnv, item);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given envelope.
        /// </summary>
        /// <param name="searchEnv"></param>
        public IList Query(IExtents searchEnv) 
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an object, not an Envelope. [Jon Aquino 10/24/2003]
            return base.Query(searchEnv);
        }

        /// <summary>
        /// Returns items whose bounds intersect the given envelope.
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <param name="visitor"></param>
        public void Query(IExtents searchEnv, IItemVisitor visitor)
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an Object, not an Envelope. [Jon Aquino 10/24/2003]
            base.Query(searchEnv, visitor);
        }

        /// <summary> 
        /// Removes a single item from the tree.
        /// </summary>
        /// <param name="itemEnv">The Envelope of the item to remove.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item was found.</returns>
        public bool Remove(IExtents itemEnv, object item) 
        {
            return base.Remove(itemEnv, item);
        }        
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IComparer GetComparer() 
        {
            return new AnonymousYComparerImpl(this);
        }

        #region ISpatialIndex<TCoordinate,TItem> Members

        public void Insert(IExtents<TCoordinate> itemEnv, object item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public System.Collections.Generic.IEnumerable<TItem> Query(IExtents<TCoordinate> searchEnv)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Query(IExtents<TCoordinate> searchBounds, Action<TItem> visitor)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Remove(IExtents<TCoordinate> itemBounds, TItem item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}