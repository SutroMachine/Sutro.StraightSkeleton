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

            var sk = new SkeletonBuilder()
                .AddBoundary(boundary)
                .Build(new GeneralPolygon2d(polygon), "RectangleExternal", external: true);
        }

        [TestMethod]
        public void BowTieExternal()
        {
            var bowtie = MakeBowTie();
            var boundary = new Polygon2d(new[] {
                new Vector2d(-15, -20),
                new Vector2d(-10, -22),
                new Vector2d(10, -18),
                new Vector2d(15, -20),
                new Vector2d(15, 20),
                new Vector2d(-15, 20),
            });

            var sk = new SkeletonBuilder()
                .AddBoundary(boundary)
                .Build(new GeneralPolygon2d(bowtie), "BowTieExternal", external: true);
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

        [TestMethod]
        public void MultipleWavefrontSources()
        {
            var source1 = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(0, 10), 6, 8));
            var source2 = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(0, -10), 6, 8));
            var boundary = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(0, 0), 10, 40));

            var sk = new SkeletonBuilder().AddBoundary(boundary).Build(
                new List<GeneralPolygon2d>() { source1, source2}, 
                "MultipleWavefrontSources", external: true);
        }


        [TestMethod]
        public void MultipleWavefrontInternal()
        {
            var source1 = Polygon2d.MakeRectangle(new Vector2d(0, 10), 6, 8);
            var source2 = Polygon2d.MakeRectangle(new Vector2d(0, -10), 6, 8);
            var gpoly = new GeneralPolygon2d(Polygon2d.MakeRectangle(new Vector2d(0, 0), 100, 100));

            source1.Reverse();
            source2.Reverse();

            gpoly.AddHole(source1);
            gpoly.AddHole(source2);

            var sk = new SkeletonBuilder().Build(gpoly,
                "MultipleWavefrontInternal", external: false);
        }


        [TestMethod]
        public void ChannelOverhang()
        {
            var sample = SampleGeometryLibrary.ChannelOverhang1();

            var sk = new SkeletonBuilder()
                .AddBoundary(sample.CurrentLayer[0].Outer)
                .Build(sample.PreviousLayer, "ChannelOverhang", external: true);
        }

        [TestMethod]
        public void ChannelOverhangInternal()
        {
            var sample = SampleGeometryLibrary.ChannelOverhangInternal();

            var svgWriter = new SVGWriter();
            svgWriter.AddPolygon(sample, SVGWriter.Style.Filled("yellow", "black", 0.05f, 0.25f));
            svgWriter.Write("ChannelOverhangInternal.svg");

            var sk = new SkeletonBuilder()
                .Build(sample, "ChannelOverhangInternal", external: false);
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

            var sk = new SkeletonBuilder().Build(new GeneralPolygon2d(polygon), "RectangleInternal");
        }
    }
}