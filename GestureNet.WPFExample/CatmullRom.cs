using GestureNet.Structures;
using System.Collections.Generic;

namespace GestureNet.WPFExample
{
    public static class CatmullRom
    {
        /// <summary>
        /// http://stackoverflow.com/a/7816020
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="smoothDistance"></param>
        /// <param name="smoothness"></param>
        public static IEnumerable<Vector2> Smooth(List<Vector2> pointList, float smoothDistance, float smoothness)
        {
            for (var i = 1; i < pointList.Count; i++)
            { 
                if (Vector2.Distance(pointList[i - 1], pointList[i]) < smoothDistance)
                {
                    pointList.RemoveAt(i);
                    i--;
                }
            }

            if (pointList.Count < 4)
                yield break;

            yield return pointList[0];

            for (var i = 1; i < pointList.Count - 2; i++)
            {
                yield return pointList[i];

                yield return PointOnCurve(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2],
                    smoothness + 0.0f);
                yield return PointOnCurve(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2],
                    smoothness + 0.1f);
                yield return PointOnCurve(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2],
                    smoothness + 0.1f);
                yield return PointOnCurve(pointList[i - 1], pointList[i], pointList[i + 1], pointList[i + 2],
                    smoothness + 0.2f);
            }

            yield return pointList[pointList.Count - 2];
            yield return pointList[pointList.Count - 1];
        }

        /// <summary>
        /// Calculates interpolated point between two points using Catmull-Rom Spline/// </summary>
        /// http://tehc0dez.blogspot.co.uk/2010/04/nice-curves-catmullrom-spline-in-c.html
        /// <remarks>
        /// Points calculated exist on the spline between points two and three./// </remarks>
        /// <param name="p0">First Point</param>
        /// <param name="p1">Second Point</param>
        /// <param name="p2">Third Point</param>
        /// <param name="p3">Fourth Point</param>
        /// <param name="t">
        /// Normalised distance between second and third point /// where the spline point will be calculated/// </param>
        /// <returns>
        /// Calculated Spline Point/// </returns>
        private static Vector2 PointOnCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            var ret = new Vector2();

            var t2 = t*t;
            var t3 = t2*t;

            ret.X = 0.5f*((2.0f*p1.X) +
                          (-p0.X + p2.X)*t +
                          (2.0f*p0.X - 5.0f*p1.X + 4*p2.X - p3.X)*t2 +
                          (-p0.X + 3.0f*p1.X - 3.0f*p2.X + p3.X)*t3);

            ret.Y = 0.5f*((2.0f*p1.Y) +
                          (-p0.Y + p2.Y)*t +
                          (2.0f*p0.Y - 5.0f*p1.Y + 4*p2.Y - p3.Y)*t2 +
                          (-p0.Y + 3.0f*p1.Y - 3.0f*p2.Y + p3.Y)*t3);

            return ret;
        }
    }
}