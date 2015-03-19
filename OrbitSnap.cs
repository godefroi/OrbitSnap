using System;
using System.Collections.Generic;
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

		/*
		 * Called after the scene is loaded.
		 */
		private void Awake()
		{
			//GameEvents.onVesselLoaded.Add(VesselLoaded);
			//GameEvents.onVesselDestroy.Add(VesselUnloaded);

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

			m_orbits = new Dictionary<string,List<Orbit>>()
			{
				//                inc, ecc, sma, lan, aop, mae, eph, bdy
				{ "Kerbin", new List<Orbit>()
					{
						new Orbit(0,   0,   1376600, 0, 0, 0, 0, FlightGlobals.Bodies[1]),
					}},
			};
		}

		private void VesselLoaded(Vessel v)
		{
			Debug.Log(string.Format("OrbitSnap: VesselLoaded vessel: {0} ({1})", v.name, v.id));

			ShowOrbit(v.orbit);
		}

		private void VesselUnloaded(Vessel v)
		{
			Debug.Log(string.Format("OrbitSnap: VesselUnloaded vessel: {0} ({1}), loaded={2}", v.name, v.id, v.loaded));

			if( v.loaded )
			{
				//UpdateOrbit(v, "destroyed");
			}
			//else if( m_pendmod.Contains(v.id) )
			//{
				//UpdateOrbit(v.id);
			//}
		}

		/*
		 * Called next.
		 */
		private void Start()
		{
			DontDestroyOnLoad(this);
			//Debug.Log("OrbitSnap [" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.0000") + "]: Start");

			/*if( FlightGlobals.ActiveVessel != m_vessel )
			{
				if( m_vessel != null )
					Debug.Log(string.Format("old current vessel was {0}", m_vessel.name));
				else
					Debug.Log("old current vessel was null");

				if( FlightGlobals.ActiveVessel != null )
					Debug.Log(string.Format("new current vessel is {0}", FlightGlobals.ActiveVessel.name));
				else
					Debug.Log("new current vessel is null");

				m_vessel = FlightGlobals.ActiveVessel;
			}*/
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

		/*
		 * Called when the game is leaving the scene (or exiting). Perform any clean up work here.
		 */
		/*private void OnDestroy()
		{
		}*/

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

		private void UpdateOrbit(Guid id)
		{
			var vessel = FlightGlobals.fetch.vessels.SingleOrDefault(v => v.id == id);

			try
			{
				OrbitPhysicsManager.HoldVesselUnpack(60);
			}
			catch( NullReferenceException )
			{
				Debug.Log("OrbitSnap: OrbitPhysicsManager.HoldVesselUnpack threw NullReferenceException");
			}

			var allVessels = FlightGlobals.fetch == null ? (IEnumerable<Vessel>)new[] { vessel } : FlightGlobals.Vessels;
			foreach( var vs in allVessels.Where(v => v.packed == false) )
				vs.GoOnRails();

			UpdateOrbit(vessel, "pending and not loaded");
		}

		private void UpdateOrbit(Vessel v, string when)
		{
			Debug.Log(string.Format("OrbitSnap: updating orbit for vessel {0} at {1}, packed={2}, loaded={3}", v.name, when, v.packed, v.loaded));

			if( v == null )
				return;

			ShowOrbit(v.orbit);

			try
			{
				OrbitPhysicsManager.HoldVesselUnpack(60);
			}
			catch( NullReferenceException )
			{
				Debug.Log("OrbitSnap: OrbitPhysicsManager.HoldVesselUnpack threw NullReferenceException");
			}

			var allVessels = FlightGlobals.fetch == null ? (IEnumerable<Vessel>)new[] { v } : FlightGlobals.Vessels;
			foreach( var vs in allVessels.Where(vsl => vsl.packed == false) )
				vs.GoOnRails();

			var orbit = v.orbitDriver.orbit;

			/*var no = new Orbit(0, 0, 1376600, orbit.LAN, orbit.argumentOfPeriapsis, orbit.meanAnomalyAtEpoch, orbit.epoch, orbit.referenceBody);

			no.Init();
			no.UpdateFromUT(Planetarium.GetUniversalTime());

			Debug.Log("new orbit:");
			ShowOrbit(no);*/

			// inclination
			orbit.inclination = 0;

			// eccentricity
			orbit.eccentricity = 0;

			// semi-major axis
			orbit.semiMajorAxis = 1376600;

			// longitude of ascending node
			//orb.LAN

			// mean anomaly at epoch
			//orb.meanAnomalyAtEpoch
			
			// epoch
			//orb.epoch

			orbit.Init();
			orbit.UpdateFromUT(Planetarium.GetUniversalTime());

			v.orbitDriver.pos = v.orbit.pos.xzy;
			v.orbitDriver.vel = v.orbit.vel;

			//v.orbit.UpdateFromOrbitAtUT(no, Planetarium.GetUniversalTime(), no.referenceBody);

			//v.SetPosition(no.pos);
			//v.SetWorldVelocity(no.vel);
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
