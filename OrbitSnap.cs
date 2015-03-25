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
		private int           m_upcnt  = 0;
		private string        m_cfile  = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Orbits.cfg");
		private Settings      m_settings;
		private ScreenMessage m_message = new ScreenMessage(string.Empty, 3, true, ScreenMessageStyle.UPPER_CENTER);

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

			m_settings = Settings.Load(m_cfile);

			Debug.Log(string.Format("OrbitSnap got {0} orbits", m_settings.Orbits.Count));

			foreach( var o in m_settings.Orbits )
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
					continue;

				// we only work on orbiting vessels
				if( vessel.situation != Vessel.Situations.ORBITING )
					continue;

				// we don't work on the active vessel
				if( vessel == FlightGlobals.ActiveVessel )
					continue;

				// only work on communications satellites
				if( !vessel.name.StartsWith("CommSat") )
					continue;

				// only work on packed vessels
				if( !vessel.packed )
					continue;

				// find an orbit that closely matches this vessel
				var snap = m_settings.Orbits.FirstOrDefault(oi => oi.Matches(vessel.orbit));

				// if there's no matching orbit, then go to the next vessel
				if( snap == null )
					continue;

				//Debug.Log("Orbit before correction:");
				//ShowOrbit(vessel.orbit);

				// log the correction
				Debug.Log(string.Format("\t\tcorrecting orbit for {0}", vessel.name));
				ScreenMessages.PostScreenMessage(string.Format("Orbit corrected for {0}", vessel.name), m_message);

				// fix the vessel's orbit
				snap.Fix(vessel.orbit);

				//Debug.Log("Orbit after snap:");
				//ShowOrbit(vessel.orbit);

				// and set it in place
				vessel.orbit.Init();
				vessel.orbit.UpdateFromUT(Planetarium.GetUniversalTime());

				//Debug.Log("Orbit after correction:");
				//ShowOrbit(vessel.orbit);
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
	}
}
