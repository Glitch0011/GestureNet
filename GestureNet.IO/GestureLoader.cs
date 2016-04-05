using System.Collections.Generic;
using System.IO;
using System.Linq;
using GestureNet.Structures;
using Newtonsoft.Json.Linq;

namespace GestureNet.IO
{
    public class GestureLoader
    {
        public static IEnumerable<Gesture> ReadGesture(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                foreach (var val in new XmlLoader().Load(fileInfo))
                    yield return val;

                foreach (var val in new JsonLoader().Load(fileInfo))
                    yield return val;
            }
        }

        public static void SaveGestures(FileInfo file, IEnumerable<Gesture> gestures)
        {
            new JsonLoader().Save(gestures, file);
        }
    }

    internal interface ILoader
    {
        IEnumerable<Gesture> Load(FileInfo file);
        void Save(IEnumerable<Gesture> gestures, FileInfo file);
    }
}
