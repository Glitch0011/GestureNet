using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GestureNet.Structures;
using System.Linq;

namespace GestureNet.IO
{
	internal class XmlLoader : ILoader
	{
		public IEnumerable<Gesture> Load(FileInfo file)
		{
			return Load(File.ReadAllText(file.FullName));
		}

		public IEnumerable<Gesture> Load(string text)
		{
			XmlDocument doc = new XmlDocument();

			doc.LoadXml(text);

			var gestures = doc["Gestures"];

			foreach (XmlNode gesture in gestures)
			{
				var gestureName = gesture.Attributes["Name"];
				
				foreach (XmlNode pointList in gesture.SelectNodes("Points"))
				{
					var pList = new List<Vector2>();

					foreach (XmlNode point in pointList)
					{
						pList.Add(new Vector2(float.Parse(point.Attributes["X"].InnerText), float.Parse(point.Attributes["Y"].InnerText)));
					}

					yield return new Gesture(pList, gestureName.InnerText);
				}
			}
		}

		public void Save(IEnumerable<Gesture> gestures, FileInfo file)
		{
			using (var writer = XmlWriter.Create(file.FullName, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Auto }))
			{
				writer.WriteStartDocument();

				writer.WriteStartElement("Gestures");

				foreach (var gesture in gestures.GroupBy(x => x.Name))
				{
					writer.WriteStartElement("Gesture");

					writer.WriteAttributeString("Name", gesture.Key);

					foreach (var pointSet in gesture)
					{
						writer.WriteStartElement("Points");

						foreach (var point in pointSet.Points)
						{
							writer.WriteStartElement("Point");
							writer.WriteAttributeString("X", point.X.ToString());
							writer.WriteAttributeString("Y", point.Y.ToString());
							writer.WriteEndElement();
						}
						
						writer.WriteEndElement();
					}

					writer.WriteEndElement();
				}

				writer.WriteEndElement();

				writer.WriteEndDocument();
			}
		}
	}
}