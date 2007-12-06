using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary> 
    /// An <see cref="EdgeEndBuilder{TCoordinate}"/> creates <see cref="EdgeEnd{TCoordinate}"/>s 
    /// for all the "split edges" created by the intersections determined for an 
    /// <see cref="Edge{TCoordinate}"/>.
    /// </summary>
    /// <remarks>
    /// Computes the <see cref="EdgeEnd{TCoordinate}"/>s which arise from a 
    /// noded <see cref="Edge{TCoordinate}"/>.
    /// </remarks>
    public class EdgeEndBuilder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public IEnumerable<EdgeEnd<TCoordinate>> ComputeEdgeEnds(IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                foreach (EdgeEnd<TCoordinate> edgeEnd in ComputeEdgeEnds(edge))
                {
                    yield return edgeEnd;
                }
            }
        }

        /// <summary>
        /// Creates stub edges for all the intersections in this
        /// Edge (if any) and returns them for insertion into a graph.
        /// </summary>
        public IEnumerable<EdgeEnd<TCoordinate>> ComputeEdgeEnds(Edge<TCoordinate> edge)
        {
            EdgeIntersectionList<TCoordinate> eiList = edge.EdgeIntersectionList;
            
            // ensure that the list has entries for the first and last point of the edge
            eiList.AddEndpoints();

            IEnumerator<EdgeIntersection<TCoordinate>> it = eiList.GetEnumerator();
            EdgeIntersection<TCoordinate> eiCurr = null;

            // no intersections, so there is nothing to do
            if (! it.MoveNext())
            {
                yield break;
            }

            EdgeIntersection<TCoordinate> eiNext = it.Current;

            do
            {
                EdgeIntersection<TCoordinate> eiPrev = eiCurr;
                eiCurr = eiNext;
                eiNext = null;

                if (it.MoveNext())
                {
                    eiNext = it.Current;
                }

                if (eiCurr != null)
                {
                    foreach (EdgeEnd<TCoordinate> edgeEnd in CreateEdgeEndForPrev(edge, eiCurr, eiPrev))
                    {
                        yield return edgeEnd;
                    }

                    foreach (EdgeEnd<TCoordinate> edgeEnd in CreateEdgeEndForNext(edge, eiCurr, eiNext))
                    {
                        yield return edgeEnd;
                    }
                }
            } while (eiCurr != null);
        }

        /// <summary>
        /// Create a EdgeStub for the edge before the intersection eiCurr.
        /// </summary>
        /// <remarks>
        /// The previous intersection is provided
        /// in case it is the endpoint for the stub edge.
        /// Otherwise, the previous point from the parent edge will be the endpoint.
        /// eiCurr will always be an EdgeIntersection, but eiPrev may be null.
        /// </remarks>
        public IEnumerable<EdgeEnd<TCoordinate>> CreateEdgeEndForPrev(
            Edge<TCoordinate> edge, EdgeIntersection<TCoordinate> eiCurr, 
            EdgeIntersection<TCoordinate> eiPrev)
        {
            Int32 iPrev = eiCurr.SegmentIndex;

            if (eiCurr.Distance == 0.0)
            {
                // if at the start of the edge there is no previous edge
                if (iPrev == 0)
                {
                    yield break;
                }

                iPrev--;
            }

            TCoordinate pPrev = edge.GetCoordinate(iPrev);

            // if prev intersection is past the previous vertex, use it instead
            if (eiPrev != null && eiPrev.SegmentIndex >= iPrev)
            {
                pPrev = eiPrev.Coordinate;
            }

            Label label = new Label(edge.Label);

            // since edgeStub is oriented opposite to it's parent edge, 
            // have to flip sides for edge label
            label.Flip();
            EdgeEnd<TCoordinate> e = new EdgeEnd<TCoordinate>(
                edge, eiCurr.Coordinate, pPrev, label);
            yield return e;
        }

        /// <summary>
        /// Create a StubEdge for the edge after the intersection eiCurr.
        /// </summary>
        /// <remarks>
        /// The next intersection is provided
        /// in case it is the endpoint for the stub edge.
        /// Otherwise, the next point from the parent edge will be the endpoint.
        /// eiCurr will always be an EdgeIntersection, but eiNext may be null.
        /// </remarks>
        public IEnumerable<EdgeEnd<TCoordinate>> CreateEdgeEndForNext(
            Edge<TCoordinate> edge, EdgeIntersection<TCoordinate> eiCurr, 
            EdgeIntersection<TCoordinate> eiNext)
        {
            Int32 iNext = eiCurr.SegmentIndex + 1;

            // if there is no next edge there is nothing to do            
            if (iNext >= edge.PointCount && eiNext == null)
            {
                yield break;
            }

            TCoordinate pNext = edge.GetCoordinate(iNext);

            // if the next intersection is in the same segment as the current, use it as the endpoint
            if (eiNext != null && eiNext.SegmentIndex == eiCurr.SegmentIndex)
            {
                pNext = eiNext.Coordinate;
            }

            EdgeEnd<TCoordinate> e = new EdgeEnd<TCoordinate>(
                edge, eiCurr.Coordinate, pNext, new Label(edge.Label));
            yield return e;
        }
    }
}