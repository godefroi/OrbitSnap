using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbitSnap
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	public class OrbitSnap : MonoBehaviour
	{
		private int                             m_upcnt  = 0;
		private Dictionary<string, List<Orbit>> m_orbits = new Dictionary<string, List<Orbit>>();
		private string                          m_cfile  = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Orbits.cfg");

		/*
		 * Called after the scene is loaded.
		 */
		private void Awake()
		{
			/*foreach( var cb in FlightGlobals.Bodies )
			{
				Debug.Log(string.Format("Celestial body {0} name={1} theName={2}, bodyName={3}", FlightGlobals.Bodies.IndexOf(cb), cb.name, cb.theName, cb.bodyName));
			}*/
			/*
			0 name=Sun
			1 name=Kerbin 
			2 name=Mun
			3 name=Minmus 
			4 name=Moho
			5 name=Eve
			6 name=Duna
			7 name=Ike
			8 name=Jool
			9 name=Laythe 
			10 name=Vall
			11 name=Bop
			12 name=Tylo
			13 name=Gilly 
			14 name=Pol
			15 name=Dres
			16 name=Eeloo 
			*/

			var s = Settings.Load(m_cfile);

			Debug.Log(string.Format("OrbitSnap got {0} orbits", s.Orbits.Count));
			foreach( var o in s.Orbits )
				Debug.Log(o.ToString());
			
			/*var s = new Settings();

			s.foo = "here's foo";
			s.bar = -1;
			s.Orbits = new List<OrbitInfo>()
			{
				//                inc, ecc, sma, lan, aop, mae, eph, bdy
				new OrbitInfo(1, 2, 3),
				new OrbitInfo(4, 5, null),
			};

			s.Save(m_cfile);*/

			//FlightGlobals.Bodies.FindIndex(cb => cb.name.Equals("foo", StringComparison.OrdinalIgnoreCase))

			m_orbits = new Dictionary<string,List<Orbit>>()
			{
				//                inc, ecc, sma, lan, aop, mae, eph, bdy
				{ "Kerbin", new List<Orbit>()
					{
						new Orbit(0,   0,   1376600, 0, 0, 0, 0, FlightGlobals.Bodies[1]),
					}},
			};
		}

		/*
		 * Called next.
		 */
		private void Start()
		{
			DontDestroyOnLoad(this);
		}

		/*
		 * Called at a fixed time interval determined by the physics time step.
		 */
		private void FixedUpdate()
		{
			// run once out of every 100 updates
			if( ++m_upcnt % 100 != 0 )
				return;

			// if there are no vessels, then bail
			if( FlightGlobals.Vessels == null || FlightGlobals.Vessels.Count == 0 )
				return;

			Debug.Log("OrbitSnap: FixedUpdate running!");

			foreach( var vessel in FlightGlobals.Vessels )
			{
				Debug.Log(string.Format("\tworking on {0}", vessel.name));

				// we only work on probes
				if( vessel.vesselType != VesselType.Probe )
				{
					Debug.Log(string.Format("\t\tvessel was a {0}, skipped", vessel.vesselType));
					continue;
				}

				// we only work on orbiting vessels
				if( vessel.situation != Vessel.Situations.ORBITING )
				{
					Debug.Log(string.Format("\t\tsituation was {0}, skipped", vessel.situation));
					continue;
				}

				// we don't work on the active vessel
				if( vessel == FlightGlobals.ActiveVessel )
				{
					Debug.Log("\t\tactive vessel, skipped");
					continue;
				}

				// only work on communications satellites
				if( !vessel.name.StartsWith("CommSat") )
				{
					Debug.Log("\t\tnot a CommSat, skipped");
					continue;
				}

				// only work on packed vessels
				if( !vessel.packed )
				{
					Debug.Log("\t\tnot packed, skipped");
					continue;
				}

				// find an orbit that closely matches this vessel
				var snap = FindMatchingOrbit(vessel.orbit);

				// if there's no matching orbit, then go to the next vessel
				if( snap == null )
					continue;

				// log the correction
				Debug.Log(string.Format("OrbitSnap: FixedUpdate correcting orbit for {0} to incl={1}, ecc={2}, sma={3}", vessel.name, snap.inclination, snap.eccentricity, snap.semiMajorAxis));
				print(string.Format("Correcting orbit for {0}", vessel.name));

				// correct the orbit
				vessel.orbit.inclination   = snap.inclination;
				vessel.orbit.eccentricity  = snap.eccentricity;
				vessel.orbit.semiMajorAxis = snap.semiMajorAxis;

				// and set it in place
				vessel.orbit.Init();
				vessel.orbit.UpdateFromUT(Planetarium.GetUniversalTime());
			}
		}

		private void ShowOrbit(Orbit orb)
		{
			Debug.Log(string.Format("Semi-major axis: {0}", orb.semiMajorAxis));
			Debug.Log(string.Format("Eccentricity: {0}", orb.eccentricity));
			Debug.Log(string.Format("Inclination: {0}", orb.inclination));
			Debug.Log(string.Format("Argument of periapsis: {0}", orb.argumentOfPeriapsis));
			Debug.Log(string.Format("Longitude of AN: {0}", orb.LAN));
			Debug.Log(string.Format("Mean Anomaly at Epoch: {0}", orb.meanAnomalyAtEpoch));
			Debug.Log(string.Format("Epoch: {0}", orb.epoch));
		}

		private Orbit FindMatchingOrbit(Orbit currentOrbit)
		{
			// if we have no orbits for this body, then there's definitely
			// no matching orbit
			if( !m_orbits.ContainsKey(currentOrbit.referenceBody.name) )
				return null;

			foreach( var orbit in m_orbits[currentOrbit.referenceBody.name] )
			{
				// if this orbit matches perfectly, then there's no need to go on
				if( currentOrbit.inclination == orbit.inclination && currentOrbit.eccentricity == orbit.eccentricity && currentOrbit.semiMajorAxis == orbit.semiMajorAxis )
					return null;

				// check inclination (within 0.2 degrees)
				if( Math.Abs(currentOrbit.inclination - orbit.inclination) > 0.2d )
					continue;

				// check eccentricity (within 0.001 whatever)
				if( Math.Abs(currentOrbit.eccentricity - orbit.eccentricity) > 0.001d )
					continue;

				// check semi-major axis (within 0.5%, I think)
				if( Math.Abs(1d - (currentOrbit.semiMajorAxis / orbit.semiMajorAxis)) > 0.005 )
					continue;

				// we found a very closely-matching orbit, so return it
				return orbit;
			}

			// no orbit was found
			return null;
		}
	}
}
