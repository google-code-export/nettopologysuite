using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// A ConnectedElementPointFilter extracts a single point
    /// from each connected element in a Geometry
    /// (e.g. a polygon, linestring or point)
    /// and returns them in a list. The elements of the list are 
    /// <c>com.vividsolutions.jts.operation.distance.GeometryLocation</c>s.
    /// </summary>
    public class ConnectedElementLocationFilter<TCoordinate> : IGeometryFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Returns a list containing a point from each Polygon, LineString, and Point
        /// found inside the specified point. Thus, if the specified point is
        /// not a GeometryCollection, an empty list will be returned.
        /// </summary>
        public static IList GetLocations(IGeometry geom)
        {
            IList locations = new ArrayList();
            geom.Apply(new ConnectedElementLocationFilter(locations));
            return locations;
        }

        private IList locations = null;

        private ConnectedElementLocationFilter(IList locations)
        {
            this.locations = locations;
        }

        public void Filter(IGeometry<TCoordinate> geom)
        {
            if (geom is IPoint || geom is ILineString || geom is IPolygon)
            {
                locations.Add(new GeometryLocation(geom, 0, geom.Coordinate));
            }
        }
    }
}