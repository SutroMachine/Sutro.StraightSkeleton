using g3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sutro.StraightSkeleton.Tests
{
    internal class SkeletonTestUtil
    {
        public static void AssertExpectedPoints(List<Vector2d> expectedList, List<Vector2d> givenList)
        {
            StringBuilder sb = new StringBuilder();

            if (expectedList.Count != givenList.Count)
            {
                sb.AppendFormat("Number of points doesn't match; expected has {0} points, given list has {1} points\n", expectedList.Count, givenList.Count);
            }

            foreach (Vector2d expected in expectedList)
            {
                if (!ContainsEpsilon(givenList, expected))
                    sb.AppendFormat("Can't find expected point ({0}, {1}) in given list\n", expected.x, expected.y);
            }

            foreach (Vector2d given in givenList)
            {
                if (!ContainsEpsilon(expectedList, given))
                    sb.AppendFormat("Can't find given point ({0}, {1}) in expected list\n", given.x, given.y);
            }

            if (sb.Length > 0)
                throw new InvalidOperationException(sb.ToString());
        }

        public static bool ContainsEpsilon(List<Vector2d> list, Vector2d p)
        {
            return list.Any(l => EqualEpsilon(l.x, p.x) && EqualEpsilon(l.y, p.y));
        }

        public static bool EqualEpsilon(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 5E-6;
        }

        public static List<Vector2d> GetFacePoints(Skeleton sk)
        {
            var ret = new List<Vector2d>();

            foreach (EdgeResult edgeOutput in sk.Edges)
            {
                var points = edgeOutput.Polygon;
                foreach (Vector2d vector2d in points.VerticesItr(false))
                {
                    if (!ContainsEpsilon(ret, vector2d))
                        ret.Add(vector2d);
                }
            }
            return ret;
        }
    }
}