using System;
using System.Collections.Generic;
using System.Linq;

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
        public readonly List<IPoint> Points; // gesture points (normalized)
            
        /// <summary>
        ///     Constructs a gesture from an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="gestureName"></param>
        public Gesture(IReadOnlyList<Point> points, string gestureName = "")
        {
            Name = gestureName;

            // normalizes the array of points with respect to scale, origin, and number of points
            Points = Scale(points);
            Points = TranslateTo(Points, Centroid(Points));
            Points = Resample(Points, SamplingResolution);
        }
        
        /// <summary>
        ///     Performs scale normalization with shape preservation into [0..1]x[0..1]
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static List<IPoint> Scale(IReadOnlyList<IPoint> points)
        { 
            float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
            foreach (var t in points)
            {
                if (minx > t.X) minx = t.X;
                if (miny > t.Y) miny = t.Y;
                if (maxx < t.X) maxx = t.X;
                if (maxy < t.Y) maxy = t.Y;
            }

            var newPoints = new List<IPoint>(points.Count);
            var scale = Math.Max(maxx - minx, maxy - miny);

            newPoints.AddRange(points.Select(t => new Point((t.X - minx)/scale, (t.Y - miny)/scale, t.StrokeId)));

            return newPoints;
        }

        /// <summary>
        ///     Translates the array of points by p
        /// </summary>
        /// <param name="points"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static List<IPoint> TranslateTo(IReadOnlyList<IPoint> points, IPoint p)
        {
            return points.Select(t => new Point(t.X - p.X, t.Y - p.Y, t.StrokeId)).Cast<IPoint>().ToList();
        }

        /// <summary>
        ///     Computes the centroid for an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static IPoint Centroid(IReadOnlyCollection<IPoint> points)
        {
            float cx = 0, cy = 0;

            foreach (var t in points)
            {
                cx += t.X;
                cy += t.Y;
            }

            return new Point(cx/points.Count, cy/points.Count, 0);
        }

        /// <summary>
        ///     Resamples the array of points into n equally-distanced points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static List<IPoint> Resample(IReadOnlyList<IPoint> points, int n)
        {
            var newPoints = new List<IPoint>(n)
            {
                new Point(points[0].X, points[0].Y, points[0].StrokeId)
            };

            var numPoints = 1;

            var I = PathLength(points)/(n - 1); // computes interval length
            float D = 0;
            for (var i = 1; i < points.Count; i++)
            {
                if (points[i].StrokeId == points[i - 1].StrokeId)
                {
                    var d = Geometry.EuclideanDistance(points[i - 1], points[i]);
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

                            newPoints.Add(new Point(
                                (1.0f - t)*firstPoint.X + t*points[i].X,
                                (1.0f - t)*firstPoint.Y + t*points[i].Y,
                                points[i].StrokeId));

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
            }

            if (numPoints == n - 1)
                // sometimes we fall a rounding-error short of adding the last point, so add it if so
                newPoints.Add(new Point(points[points.Count - 1].X, points[points.Count - 1].Y,
                    points[points.Count - 1].StrokeId));

            return newPoints;
        }

        /// <summary>
        ///     Computes the path length for an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static float PathLength(IReadOnlyList<IPoint> points)
        {
            float length = 0;

            for (var i = 1; i < points.Count; i++)
                if (points[i].StrokeId == points[i - 1].StrokeId)
                    length += Geometry.EuclideanDistance(points[i - 1], points[i]);

            return length;
        }
    }
}