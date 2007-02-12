using System;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Samples.Geometries
{	
	public class ExtendedCoordinateExample
	{		
		[STAThread]
		public static void main(string[] args)
		{
			ExtendedCoordinateSequenceFactory seqFact = ExtendedCoordinateSequenceFactory.Instance();
			
			ExtendedCoordinate[] array1 = new ExtendedCoordinate[] { new ExtendedCoordinate(0, 0, 0, 91), 
                new ExtendedCoordinate(10, 0, 0, 92), new ExtendedCoordinate(10, 10, 0, 93), 
                new ExtendedCoordinate(0, 10, 0, 94), new ExtendedCoordinate(0, 0, 0, 91)};
			ICoordinateSequence seq1 = seqFact.Create(array1);
			
			ICoordinateSequence seq2 = seqFact.Create(new ExtendedCoordinate[] { new ExtendedCoordinate(5, 5, 0, 91), 
                new ExtendedCoordinate(15, 5, 0, 92), new ExtendedCoordinate(15, 15, 0, 93), 
                new ExtendedCoordinate(5, 15, 0, 94), new ExtendedCoordinate(5, 5, 0, 91)});
			
			GeometryFactory fact = new GeometryFactory(ExtendedCoordinateSequenceFactory.Instance());
			
			Geometry g1 = fact.CreatePolygon(fact.CreateLinearRing(seq1), null);
			Geometry g2 = fact.CreatePolygon(fact.CreateLinearRing(seq2), null);
			
			Console.WriteLine("WKT for g1: " + g1);
			Console.WriteLine("Internal rep for g1: " + ((LineString) ((Polygon) g1).ExteriorRing).CoordinateSequence);
			
			Console.WriteLine("WKT for g2: " + g2);
			Console.WriteLine("Internal rep for g2: " + ((LineString) ((Polygon) g2).ExteriorRing).CoordinateSequence);
			
			Geometry gInt = (Geometry) g1.Intersection(g2);
			
			Console.WriteLine("WKT for gInt: " + gInt);
			Console.WriteLine("Internal rep for gInt: " + ((LineString) ((Polygon) gInt).ExteriorRing).CoordinateSequence);
		}
		
		public ExtendedCoordinateExample() { }
	}
}