using System.Collections.Generic;
using System.IO;
using GestureNet.Structures;

namespace GestureNet.IO
{
    public static class GestureLoader
    {
        public static IEnumerable<Gesture> ReadGestures(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                foreach (var val in new XmlLoader().Load(fileInfo))
                    yield return val;
            }
        }

        public static void SaveGestures(FileInfo file, IEnumerable<Gesture> gestures)
        {
            new XmlLoader().Save(gestures, file);
        }
    }

    internal interface ILoader
    {
        IEnumerable<Gesture> Load(FileInfo file);
        void Save(IEnumerable<Gesture> gestures, FileInfo file);
    }
}
