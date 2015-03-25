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

		public bool Matches(Orbit orbit)
		{
			if( orbit.referenceBody.name != Body )
				return false;

			// check inclination (within 0.2 degrees)
			if( Inclination.HasValue && Math.Abs(Inclination.Value - orbit.inclination) > 0.2d )
				return false;

			// check eccentricity (within 0.001 whatever)
			if( Eccentricity.HasValue && Math.Abs(Eccentricity.Value - orbit.eccentricity) > 0.001d )
				return false;

			// check semi-major axis (within 0.5%, I think)
			if( SemiMajorAxis.HasValue && Math.Abs(1d - (SemiMajorAxis.Value / orbit.semiMajorAxis)) > 0.005d )
				return false;

			// check longitude of the ascending node (within 5 degrees?)
			if( LongitudeAN.HasValue && Math.Abs(LongitudeAN.Value - orbit.LAN) > 5d )
				return false;

			// check argument of periapsis (within 2.5 degrees?)
			if( ArgumentPe.HasValue && Math.Abs(ArgumentPe.Value - orbit.argumentOfPeriapsis) > 2.5d )
				return false;

			// if it matches exactly, then no fix is needed
			if( (!Inclination.HasValue   || (Inclination.HasValue && Inclination.Value     == orbit.inclination))   &&
				(!Eccentricity.HasValue  || (Eccentricity.HasValue && Eccentricity.Value   == orbit.eccentricity))  &&
				(!SemiMajorAxis.HasValue || (SemiMajorAxis.HasValue && SemiMajorAxis.Value == orbit.semiMajorAxis)) &&
				(!LongitudeAN.HasValue   || (LongitudeAN.HasValue && LongitudeAN.Value     == orbit.LAN))           &&
				(!ArgumentPe.HasValue    || (ArgumentPe.HasValue && ArgumentPe.Value       == orbit.argumentOfPeriapsis)) )
				return false;

			// we found a very closely-matching orbit, so return it
			return true;
		}

		public void Fix(Orbit orbit)
		{
			if( orbit.referenceBody.name != Body )
				throw new InvalidOperationException("Cannot edit an orbit for the wrong reference body.");

			if( Inclination.HasValue )
				orbit.inclination = Inclination.Value;

			if( Eccentricity.HasValue )
				orbit.eccentricity = Eccentricity.Value;

			if( SemiMajorAxis.HasValue )
				orbit.semiMajorAxis = SemiMajorAxis.Value;

			if( LongitudeAN.HasValue )
				orbit.LAN = LongitudeAN.Value;

			if( ArgumentPe.HasValue )
				orbit.argumentOfPeriapsis = ArgumentPe.Value;
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
