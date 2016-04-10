using GestureNet.Structures;
using System.Collections.Generic;
using System.IO;

namespace GestureNet.IO
{
	internal interface ILoader
	{
		IEnumerable<Gesture> Load(FileInfo file);
		IEnumerable<Gesture> Load(string text);
		void Save(IEnumerable<Gesture> gestures, FileInfo file);
	}
}
