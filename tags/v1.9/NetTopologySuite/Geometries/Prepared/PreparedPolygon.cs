using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Predicate;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A prepared version for <see cref="IPolygonal"/> geometries.
    ///</summary>
    /// <author>mbdavis</author>
    public class PreparedPolygon : BasicPreparedGeometry
    {
        private readonly bool _isRectangle;
        // create these lazily, since they are expensive
        private FastSegmentSetIntersectionFinder _segIntFinder;
        private IPointOnGeometryLocator _pia;

        public PreparedPolygon(IPolygonal poly)
            : base((IGeometry)poly)
        {
            _isRectangle = Geometry.IsRectangle;
        }

        public FastSegmentSetIntersectionFinder IntersectionFinder
        {
            get
            {
                /*
                 * MD - Another option would be to use a simple scan for 
                 * segment testing for small geometries.  
                 * However, testing indicates that there is no particular advantage 
                 * to this approach.
                 */
                if (_segIntFinder == null)
                    _segIntFinder =
                        new FastSegmentSetIntersectionFinder(SegmentStringUtil.ExtractSegmentStrings(Geometry));
                return _segIntFinder;
            }
        }

        public IPointOnGeometryLocator PointLocator
        {
            get
            {
                if (_pia == null)
                    _pia = new IndexedPointInAreaLocator(Geometry);

                return _pia;
            }
        }

        public override bool Intersects(IGeometry g)
        {
            // envelope test
            if (!EnvelopesIntersect(g)) return false;

            // optimization for rectangles
            if (_isRectangle)
            {
                return RectangleIntersects.Intersects((IPolygon)Geometry, g);
            }

            return PreparedPolygonIntersects.Intersects(this, g);
        }

        public override bool Contains(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;

            // optimization for rectangles
            if (_isRectangle)
            {
                return RectangleContains.Contains((IPolygon)Geometry, g);
            }

            return PreparedPolygonContains.Contains(this, g);
        }

        public override bool ContainsProperly(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;
            return PreparedPolygonContainsProperly.ContainsProperly(this, g);
        }

        public override bool Covers(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeCovers(g))
                return false;
            // optimization for rectangle arguments
            if (_isRectangle)
            {
                return true;
            }
            return PreparedPolygonCovers.Covers(this, g);
        }
    }
}
