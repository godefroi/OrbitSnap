using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrbitSnap
{
	public class OrbitInfo
	{
		public OrbitInfo() { }

		public OrbitInfo(string body, double? inclination, double? eccentricity, double? semiMajorAxis, double? longitudeAN, double? argumentPe)
		{
			Body          = body;
			Inclination   = inclination;
			Eccentricity  = eccentricity;
			SemiMajorAxis = semiMajorAxis;
			LongitudeAN   = longitudeAN;
			ArgumentPe    = argumentPe;
		}

		public OrbitInfo(ConfigNode node)
		{
			Body = node.GetValue("Body");

			if( node.HasValue("Inclination") )
				Inclination = double.Parse(node.GetValue("Inclination"));

			if( node.HasValue("Eccentricity") )
				Eccentricity = double.Parse(node.GetValue("Eccentricity"));

			if( node.HasValue("SemiMajorAxis") )
				SemiMajorAxis = double.Parse(node.GetValue("SemiMajorAxis"));

			if( node.HasValue("LongitudeAN") )
				LongitudeAN = double.Parse(node.GetValue("LongitudeAN"));

			if( node.HasValue("ArgumentPe") )
				ArgumentPe = double.Parse(node.GetValue("ArgumentPe"));
		}

		public string Body;

		public double? Inclination;

		public double? Eccentricity;

		public double? SemiMajorAxis;

		public double? LongitudeAN;

		public double? ArgumentPe;

		public void Fix(Orbit initial)
		{
			if( initial.referenceBody.name != Body )
				throw new InvalidOperationException("Cannot edit an orbit for the wrong reference body.");

			if( Inclination.HasValue )
				initial.inclination = Inclination.Value;

			if( Eccentricity.HasValue )
				initial.eccentricity = Eccentricity.Value;

			if( SemiMajorAxis.HasValue )
				initial.semiMajorAxis = SemiMajorAxis.Value;

			if( LongitudeAN.HasValue )
				initial.LAN = LongitudeAN.Value;

			if( ArgumentPe.HasValue )
				initial.argumentOfPeriapsis = ArgumentPe.Value;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("Orbit around ");
			sb.Append(Body);
			sb.AppendLine();

			sb.Append("\tInclination: ");
			sb.Append(Inclination.HasValue ? Inclination.ToString() : "not provided");
			sb.AppendLine();

			sb.Append("\tEccentricity: ");
			sb.Append(Eccentricity.HasValue ? Eccentricity.ToString() : "not provided");
			sb.AppendLine();

			sb.Append("\tSemiMajorAxis: ");
			sb.Append(SemiMajorAxis.HasValue ? SemiMajorAxis.ToString() : "not provided");
			sb.AppendLine();

			sb.Append("\tLongitudeAN: ");
			sb.Append(LongitudeAN.HasValue ? LongitudeAN.ToString() : "not provided");
			sb.AppendLine();

			sb.Append("\tArgumentPe: ");
			sb.Append(ArgumentPe.HasValue ? ArgumentPe.ToString() : "not provided");

			return sb.ToString();
		}
	}
}
