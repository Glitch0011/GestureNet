using GestureNet.Structures;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GestureNet.IO
{
	internal class JsonLoader : ILoader
	{
		public IEnumerable<Gesture> Load(FileInfo file)
		{
			var obj = JArray.Parse(File.ReadAllText(file.FullName));

			return obj.Select(gesture => new Gesture(
				gesture["Points"].Select(x => new Vector2((float)x["X"], (float)x["Y"]))
					.ToList(), (string)gesture["Name"]));
		}

		public void Save(IEnumerable<Gesture> gestures, FileInfo file)
		{
			var jObject = JArray.FromObject(gestures);

			File.WriteAllText(file.FullName, jObject.ToString());
		}
	}
}
