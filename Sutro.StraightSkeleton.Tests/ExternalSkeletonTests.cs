﻿using g3;
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
                .Build(new GeneralPolygon2d(polygon), "RectangleExternal");
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

            var sk = new ExternalSkeletonBuilder()
                .AddBoundary(boundary)
                .Build(new GeneralPolygon2d(bowtie), "BowTieExternal");
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
        public void ChannelOverhang()
        {
            var sample = SampleGeometryLibrary.ChannelOverhang1();

            var svgWriter = new SVGWriter();
            foreach (var gpoly in sample.PreviousLayer)
            {
                svgWriter.AddPolygon(gpoly.Outer, SVGWriter.Style.Filled("cyan", "cyan", strokeWidth: 0.05f, opacity: 0.1f));
            }

            foreach (var gpoly in sample.CurrentLayer)
            {
                svgWriter.AddPolygon(gpoly.Outer, SVGWriter.Style.Filled("black", "black", strokeWidth: 0.05f, opacity: 0.1f));
            }

            svgWriter.Write("ChannelOverhang.svg");

            var sk = new ExternalSkeletonBuilder()
                .AddBoundary(sample.CurrentLayer[0].Outer)
                .Build(sample.PreviousLayer, "ChannelOverhang");
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