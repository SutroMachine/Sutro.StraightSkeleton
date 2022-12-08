using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Sutro.StraightSkeleton.Tests
{
    [TestClass]
    public class ExternalSkeletonTests
    {
        [TestMethod]
        public void RectangleExternal()
        {
            var polygon = Polygon2d.MakeRectangle(Vector2d.Zero, 20, 40);
            var boundary = Polygon2d.MakeRectangle(new Vector2d(1, 4), 30, 54);

            var sk = new ExternalSkeletonBuilder()
                .AddBoundary(boundary)
                .Build(new GeneralPolygon2d(polygon));
        }

        [TestMethod]
        public void BowTieExternal()
        {
            var bowtie = MakeBowTie();

            var sk = new ExternalSkeletonBuilder().Build(bowtie);
        }

        [TestMethod]
        public void BowTieInternal()
        {
            var outer = new GeneralPolygon2d(Polygon2d.MakeRectangle(Vector2d.Zero, 30, 30));
            var bowtie = MakeBowTie();
            bowtie.Reverse();
            outer.AddHole(bowtie);

            var sk = new SkeletonBuilder().Build(outer, "BowTieInternal");
        }

        private static Polygon2d MakeBowTie()
        {
            return new Polygon2d(new Vector2d[] {
                new Vector2d(10, 10),
                new Vector2d(9, 7),
                new Vector2d(1, 5),
                new Vector2d(-9, 7),
                new Vector2d(-10, 10),
                new Vector2d(-10, -10),
                new Vector2d(0, -5),
                new Vector2d(10, -10),
            });
        }

        [TestMethod]
        public void RectangleInternal()
        {
            var polygon = Polygon2d.MakeRectangle(Vector2d.Zero, 20, 40);

            var sk = new SkeletonBuilder().Build(polygon);
        }
    }
}