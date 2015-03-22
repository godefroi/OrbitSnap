using System;
using System.Collections.Generic;
using System.IO;

namespace OrbitSnap
{
	public class Settings
	{
		public Settings()
		{
			Orbits = new List<OrbitInfo>();
		}

		public List<OrbitInfo> Orbits;

		public static Settings Load(string fileName)
		{
			// make sure the file exists
			if( !File.Exists(fileName) )
				return null;

			var s  = new Settings();
			var cn = ConfigNode.Load(fileName).GetNode(typeof(Settings).Name);

			// populate the class
			ConfigNode.LoadObjectFromConfig(s, cn);

			// load the orbits node
			var on = cn.GetNode("Orbits");

			if( on != null )
			{
				// load each orbit
				foreach( var n in on.GetNodes("Orbit") )
					s.Orbits.Add(new OrbitInfo(n));
			}

			return s;
		}

		public void Save(string fileName)
		{
			// serialize us into a config node
			var cn = ConfigNode.CreateConfigFromObject(this, new ConfigNode(typeof(Settings).Name));

			// make the node that we're actually going to save
			var node = new ConfigNode(typeof(Settings).Name);

			// add our data to the node
			node.AddNode(cn);

			// save it
			node.Save(fileName);
		}
	}
}
