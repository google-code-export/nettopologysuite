﻿namespace NetTopologySuite.Tests.Various
{
    using System;
    using System.Collections.Generic;

    using GeoAPI.Geometries;

    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.Operation.Union;

    using global::NUnit.Framework;

    [TestFixture]
    public class Issue123Test
    {
        [Test]
        public void CascadedUnionError()
        {
            String[] wkt =
            {
                "MULTIPOLYGON (((-2.775 -37.382, -2.7694818956884695 -37.302294048833446, -4.381 -37.19, -4.379 -37.16, -2.7674053419183364 -37.272299383264858, -2.766 -37.252, -2.703 -37.257, -2.712 -37.386, -2.775 -37.382)), ((-0.558 -16.355, -0.556624473051351 -16.33528411373603, -2.168 -16.223, -2.165 -16.193, -0.55452706181921063 -16.305221219408683, -0.549 -16.226, -0.485 -16.23, -0.494 -16.36, -0.558 -16.355)))",
                "MULTIPOLYGON (((-4.218 -16.08, -4.216 -16.05, -2.924 -16.14, -2.926 -16.17, -4.218 -16.08)), ((-5.291 -18.097, -5.243 -17.415, -5.239 -17.352, -5.15929328747628 -17.357518157020873, -5.071 -16.091, -5.041 -16.093, -5.1292306097055169 -17.359599419328081, -5.109 -17.361, -5.114 -17.424, -5.161 -18.106, -5.291 -18.097)))"
            };

            IList<IGeometry> items = new List<IGeometry>();
            IGeometryFactory factory = GeometryFactory.Default;
            WKTReader reader = new WKTReader(factory);
            IGeometry geoms = reader.Read(wkt[0]);
            for (int i = 0; i < geoms.NumGeometries; i++)
            {
                IGeometry geom = geoms.GetGeometryN(i);
                items.Add(geom);
            }
            geoms = reader.Read(wkt[1]);
            for (int i = 0; i < geoms.NumGeometries; i++)
            {
                IGeometry geom = geoms.GetGeometryN(i);
                items.Add(geom);
            }

            UnaryUnionOp op = new UnaryUnionOp(items, new GeometryFactory(new PrecisionModel(100)));
            IGeometry result = op.Union();
            Assert.IsNotNull(result);
        }
    }
}
