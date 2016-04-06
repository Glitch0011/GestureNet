using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GestureNet.Structures
{
    /// <summary>
    ///     Implements a gesture as a cloud of points (i.e., an unordered set of points).
    ///     Gestures are normalized with respect to scale, translated to origin, and resampled into a fixed number of 32
    ///     points.
    /// </summary>
    public class Gesture
    {
        private const int SamplingResolution = 32;

        public readonly string Name; // gesture class
        public readonly List<Vector2> Points; // gesture points (normalized)
            
        /// <summary>
        ///     Constructs a gesture from an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="gestureName"></param>
        public Gesture(IReadOnlyList<Vector2> points, string gestureName = "")
        {
            Name = gestureName;

            if (points.Count == 0)
                throw new ArgumentException("Can not have zero points");

            // normalizes the array of points with respect to scale, origin, and number of points
            Points = Scale(points);
            Points = TranslateTo(Points, Centroid(Points));
            Points = Resample(Points, SamplingResolution);

			if (Points.Count == 1)
			{
				if (float.IsNaN(Points[0].X))
					throw new ArgumentException();
				if (float.IsNaN(Points[0].Y))
					throw new ArgumentException();
			}
        }
        
        /// <summary>
        ///     Performs scale normalization with shape preservation into [0..1]x[0..1]
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static List<Vector2> Scale(IReadOnlyList<Vector2> points)
        { 
            float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
            foreach (var t in points)
            {
                if (minx > t.X) minx = t.X;
                if (miny > t.Y) miny = t.Y;
                if (maxx < t.X) maxx = t.X;
                if (maxy < t.Y) maxy = t.Y;
            }

            var newPoints = new List<Vector2>(points.Count);
            var scale = Math.Max(maxx - minx, maxy - miny);

            newPoints.AddRange(points.Select(t => new Vector2((t.X - minx)/scale, (t.Y - miny)/scale)));

            return newPoints;
        }

        /// <summary>
        ///     Translates the array of points by p
        /// </summary>
        /// <param name="points"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static List<Vector2> TranslateTo(IReadOnlyList<Vector2> points, Vector2 p)
        {
            return points.Select(t => new Vector2(t.X - p.X, t.Y - p.Y)).Cast<Vector2>().ToList();
        }

        /// <summary>
        ///     Computes the centroid for an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static Vector2 Centroid(IReadOnlyCollection<Vector2> points)
        {
            float cx = 0, cy = 0;

            foreach (var t in points)
            {
                cx += t.X;
                cy += t.Y;
            }

            return new Vector2(cx/points.Count, cy/points.Count);
        }

        /// <summary>
        ///     Resamples the array of points into n equally-distanced points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static List<Vector2> Resample(IReadOnlyList<Vector2> points, int n)
        {
            var newPoints = new List<Vector2>(n)
            {
                new Vector2(points[0].X, points[0].Y)
            };

            var numPoints = 1;

            var I = PathLength(points)/(n - 1); // computes interval length
            float D = 0;
            for (var i = 1; i < points.Count; i++)
            {

                var d = Vector2.Distance(points[i - 1], points[i]);
                if (D + d >= I)
                {
                    var firstPoint = points[i - 1];
                    while (D + d >= I)
                    {
                        // add interpolated point
                        var t = Math.Min(Math.Max((I - D)/d, 0.0f), 1.0f);

                        if (float.IsNaN(t))
                            t = 0.5f;

                        numPoints++;

                        newPoints.Add(new Vector2(
                            (1.0f - t)*firstPoint.X + t*points[i].X,
                            (1.0f - t)*firstPoint.Y + t*points[i].Y));

                        // update partial length
                        d = D + d - I;
                        D = 0;
                        firstPoint = newPoints[numPoints - 1];
                    }
                    D = d;
                }
                else
                    D += d;
            }

            if (numPoints == n - 1)
                // sometimes we fall a rounding-error short of adding the last point, so add it if so
                newPoints.Add(new Vector2(points[points.Count - 1].X, points[points.Count - 1].Y));

            return newPoints;
        }

        /// <summary>
        ///     Computes the path length for an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static float PathLength(IReadOnlyList<Vector2> points)
        {
            float length = 0;

            for (var i = 1; i < points.Count; i++)
                length += Vector2.Distance(points[i - 1], points[i]);

            return length;
        }
    }
}