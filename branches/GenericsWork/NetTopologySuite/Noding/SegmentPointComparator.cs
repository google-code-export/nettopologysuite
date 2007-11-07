using System;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Implements a robust method of comparing the relative position of two points along the same segment.
    /// The coordinates are assumed to lie "near" the segment.
    /// This means that this algorithm will only return correct results
    /// if the input coordinates have the same precision and correspond to rounded values
    /// of exact coordinates lying on the segment.
    /// </summary>
    public class SegmentPointComparator
    {
        /// <summary>
        ///  Compares two <see cref="Coordinate" />s for their relative position along a segment
        /// lying in the specified <see cref="Octant" />.
        /// </summary>
        /// <returns>
        /// -1 if node0 occurs first, or
        ///  0 if the two nodes are equal, or
        ///  1 if node1 occurs first.
        /// </returns>
        public static Int32 Compare(Octants octant, ICoordinate p0, ICoordinate p1)
        {
            // nodes can only be equal if their coordinates are equal
            if (p0.Equals2D(p1))
            {
                return 0;
            }

            Int32 xSign = RelativeSign(p0.X, p1.X);
            Int32 ySign = RelativeSign(p0.Y, p1.Y);

            switch (octant)
            {
                case Octants.Zero:
                    return CompareValue(xSign, ySign);
                case Octants.One:
                    return CompareValue(ySign, xSign);
                case Octants.Two:
                    return CompareValue(ySign, -xSign);
                case Octants.Three:
                    return CompareValue(-xSign, ySign);
                case Octants.Four:
                    return CompareValue(-xSign, -ySign);
                case Octants.Five:
                    return CompareValue(-ySign, -xSign);
                case Octants.Six:
                    return CompareValue(-ySign, xSign);
                case Octants.Seven:
                    return CompareValue(xSign, -ySign);
            }

            Assert.ShouldNeverReachHere("invalid octant value: " + octant);
            return 0;
        }

        public static Int32 RelativeSign(Double x0, Double x1)
        {
            if (x0 < x1)
            {
                return -1;
            }
            if (x0 > x1)
            {
                return 1;
            }
            return 0;
        }

        private static Int32 CompareValue(Int32 compareSign0, Int32 compareSign1)
        {
            if (compareSign0 < 0)
            {
                return -1;
            }
            if (compareSign0 > 0)
            {
                return 1;
            }
            if (compareSign1 < 0)
            {
                return -1;
            }
            if (compareSign1 > 0)
            {
                return 1;
            }
            return 0;
        }
    }
}