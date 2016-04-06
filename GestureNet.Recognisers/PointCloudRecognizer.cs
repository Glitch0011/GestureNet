using System;
using System.Collections.Generic;
using System.Linq;
using GestureNet.Structures;

namespace GestureNet.Recognisers
{
    /// <summary>
    ///     Implements the $P recognizer
    /// </summary>
    public static class PointCloudRecognizer
    {
        /// <summary>
        // controls the number of greedy search trials (eps is in [0..1])
        /// </summary>
        public static float Eps { get; } = 0.5f;

        /// <summary>
        ///     Main function of the $P recognizer.
        ///     Classifies a candidate gesture against a set of training samples.
        ///     Returns the class of the closest neighbor in the training set.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="trainingSet"></param>
        /// <returns></returns>
        public static IEnumerable<Result> Classify(Gesture candidate, IEnumerable<Gesture> trainingSet)
        {
            return
                trainingSet.Select(
                    x => new Result {Name = x.Name, Score = GreedyCloudMatch(candidate.Points, x.Points)})
                    .OrderBy(x => x.Score);
        }

        /// <summary>
        ///     Implements greedy search for a minimum-distance matching between two point clouds
        /// </summary>
        /// <param name="points1"></param>
        /// <param name="points2"></param>
        /// <returns></returns>
        private static float GreedyCloudMatch(IReadOnlyList<IPoint> points1, IReadOnlyList<IPoint> points2)
        {
            var n = points1.Count; // the two clouds should have the same number of points by now
            var step = (int) Math.Floor(Math.Pow(n, 1.0f - Eps));
            var minDistance = float.MaxValue;

            for (var i = 0; i < n; i += step)
            {
                var dist1 = CloudDistance(points1, points2, i); // match points1 --> points2 starting with index point i
                var dist2 = CloudDistance(points2, points1, i); // match points2 --> points1 starting with index point i
                minDistance = Math.Min(minDistance, Math.Min(dist1, dist2));
            }

            return minDistance;
        }

        /// <summary>
        ///     Computes the distance between two point clouds by performing a minimum-distance greedy matching
        ///     starting with point startIndex
        /// </summary>
        /// <param name="points1"></param>
        /// <param name="points2"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static float CloudDistance(IReadOnlyList<IPoint> points1, IReadOnlyList<IPoint> points2, int startIndex)
        {
            var n = points1.Count; // the two clouds should have the same number of points by now
            var matched = new bool[n]; // matched[i] signals whether point i from the 2nd cloud has been already matched
            Array.Clear(matched, 0, n); // no points are matched at the beginning

            float sum = 0;
                // computes the sum of distances between matched points (i.e., the distance between the two clouds)
            var i = startIndex;
            do
            {
                var index = -1;
                var minDistance = float.MaxValue;

                for (var j = 0; j < Math.Min(points1.Count, points2.Count); j++)
                    if (!matched[j])
                    {
                        // use squared Euclidean distance to save some processing time
                        var dist = Geometry.SqrEuclideanDistance(points1[i], points2[j]);

                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            index = j;
                        }
                    }

				if (index != -1)
				{
					matched[index] = true; // point index from the 2nd cloud is matched to point i from the 1st cloud
					var weight = 1.0f - (i - startIndex + n) % n / (1.0f * n);

					// weight each distance with a confidence coefficient that decreases from 1 to 0
					sum += weight * minDistance;

					i = (i + 1) % n;
				}

            } while (i != startIndex);
            return sum;
        }
    }
}