using System;

namespace GestureNet.Structures
{
    public static class Geometry
    {
        /// <summary>
        ///     Computes the Squared Euclidean Distance between two points in 2D
        /// </summary>
        public static float SqrEuclideanDistance(IPoint a, IPoint b)
        {
            return (a.X - b.X)*(a.X - b.X) + (a.Y - b.Y)*(a.Y - b.Y);
        }

        /// <summary>
        ///     Computes the Euclidean Distance between two points in 2D
        /// </summary>
        public static float EuclideanDistance(IPoint a, IPoint b)
        {
            return (float) Math.Sqrt(SqrEuclideanDistance(a, b));
        }
    }
}