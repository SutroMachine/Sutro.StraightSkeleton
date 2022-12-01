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

            var sk = new ExternalSkeletonBuilder().Build(polygon);

            var writer = new SVGWriter();
            sk.AddToSvg(writer);
            writer.Write("RectangleExternal.svg");
        }

        [TestMethod]
        public void BowTieExternal()
        {
            var bowtie = MakeBowTie();

            var sk = new ExternalSkeletonBuilder().Build(bowtie);

            var writer = new SVGWriter();
            sk.AddToSvg(writer);
            writer.Write("BowTieExternal.svg");
        }

        [TestMethod]
        public void BowTieInternal()
        {
            var outer = new GeneralPolygon2d(Polygon2d.MakeRectangle(Vector2d.Zero, 30, 30));
            var bowtie = MakeBowTie();
            bowtie.Reverse();
            outer.AddHole(bowtie);

            var sk = new SkeletonBuilder().Build(outer);

            var writer = new SVGWriter();
            sk.AddToSvg(writer);
            writer.Write("BowTieInternal.svg");
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

            var writer = new SVGWriter();
            sk.AddToSvg(writer);
            writer.Write("RectangleInternal.svg");
        }
    }
}