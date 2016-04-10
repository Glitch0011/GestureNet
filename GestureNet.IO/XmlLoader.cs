using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GestureNet.Structures;

namespace GestureNet.IO
{
    internal class XmlLoader : ILoader
    {
        public IEnumerable<Gesture> Load(FileInfo file)
        {
            var points = new List<Vector2>();
            XmlTextReader xmlReader = null;
            var currentStrokeIndex = -1;
            var gestureName = string.Empty;

            try
            {
                xmlReader = new XmlTextReader(File.OpenText(file.FullName));
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType != XmlNodeType.Element) continue;
                    switch (xmlReader.Name)
                    {
                        case "Gesture":
                            gestureName = xmlReader["Name"];
                            if (gestureName.Contains("~"))
                                // '~' character is specific to the naming convention of the MMG set
                                gestureName = gestureName.Substring(0, gestureName.LastIndexOf('~'));
                            if (gestureName.Contains("_"))
                                // '_' character is specific to the naming convention of the MMG set
                                gestureName = gestureName.Replace('_', ' ');
                            break;
                        case "Stroke":
                            currentStrokeIndex++;
                            break;
                        case "Point":
                            points.Add(new Vector2(
                                float.Parse(xmlReader["X"]),
                                float.Parse(xmlReader["Y"])
                                ));
                            break;
                        default:
                            throw new ArgumentException();
                    }
                }
            }
            catch
            {
                yield break;
            }
            finally
            {
                xmlReader?.Close();
            }
            yield return new Gesture(points.ToArray(), gestureName);
        }

        public void Save(IEnumerable<Gesture> gestures, FileInfo file)
        {
            using (var sw = new StreamWriter(file.FullName))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");

                foreach (var gesture in gestures)
                {
                    sw.WriteLine("<Gesture Name = \"{0}\">", gesture.Name);
                    var currentStroke = -1;
                    for (var i = 0; i < gesture.Points.Count; i++)
                    {
                        if (i > 0)
                            sw.WriteLine("\t</Stroke>");
                        sw.WriteLine("\t<Stroke>");

                        sw.WriteLine("\t\t<Point X = \"{0}\" Y = \"{1}\" T = \"0\" Pressure = \"0\" />",
                            gesture.Points[i].X, gesture.Points[i].Y);
                    }
                    sw.WriteLine("\t</Stroke>");
                    sw.WriteLine("</Gesture>");
                }
            }
        }
    }
}