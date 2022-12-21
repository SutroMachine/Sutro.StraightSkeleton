using g3;
using System.Collections.Generic;

namespace Sutro.StraightSkeleton.Tests
{
    public class SampleGeometry
    {
        public List<GeneralPolygon2d> PreviousLayer { get; } = new List<GeneralPolygon2d>();
        public List<GeneralPolygon2d> CurrentLayer { get; } = new List<GeneralPolygon2d>();
    }

    public static class SampleGeometryLibrary
    {
        public static SampleGeometry ChannelOverhang1()
        {
            var layers = new SampleGeometry();

            var previous2 = new Polygon2d(new Vector2d[] {
                new Vector2d(5.6455, -25.1966),
                new Vector2d(5.5603, -24.0354),
                new Vector2d(5.4090, -22.8841),
                new Vector2d(5.1925, -21.7438),
                new Vector2d(4.9113, -20.6164),
                new Vector2d(4.5671, -19.5075),
                new Vector2d(4.1606, -18.4195),
                new Vector2d(3.6931, -17.3575),
                new Vector2d(3.1638, -16.321),
                new Vector2d(-14.6143, 16.1434),
                new Vector2d(-15.1294, 17.1503),
                new Vector2d(-15.1716, 17.2391),
                new Vector2d(-15.5892, 18.1876),
                new Vector2d(-15.6275, 18.277),
                new Vector2d(-15.6624, 18.3679),
                new Vector2d(-16.0252, 19.3385),
                new Vector2d(-16.0570, 19.4315),
                new Vector2d(-16.3921, 20.5143),
                new Vector2d(-16.6673, 21.614),
                new Vector2d(-16.6884, 21.7101),
                new Vector2d(-16.899, 22.8239),
                new Vector2d(-17.0475, 23.9476),
                new Vector2d(-17.0575, 24.0455),
                new Vector2d(-17.1394, 25.1735),
                new Vector2d(-17.3588, 30.1224),
                new Vector2d(-20.0183, 30.1224),
                new Vector2d(-20.0183, -30.1224),
                new Vector2d(5.8638, -30.1224),
            });

            previous2.Simplify();
            layers.PreviousLayer.Add(new GeneralPolygon2d(previous2));

            var previous1 = new Polygon2d(new Vector2d[] {
                new Vector2d(-1.8506, 17.5471),
                new Vector2d(-1.4783, 16.6206),
                new Vector2d(-1.4453, 16.5407),
                new Vector2d(12.9093, -16.3914),
                new Vector2d(13.3363, -17.4539),
                new Vector2d(13.7026, -18.542),
                new Vector2d(14.0065, -19.6494),
                new Vector2d(14.2469, -20.7724),
                new Vector2d(14.4019, -21.7681),
                new Vector2d(14.4236, -21.9071),
                new Vector2d(14.5355, -23.05),
                new Vector2d(14.5823, -24.1971),
                new Vector2d(14.563, -25.3420),
                new Vector2d(14.3447, -30.1224),
                new Vector2d(20.0183, -30.1224),
                new Vector2d(20.0183, 30.1224),
                new Vector2d(-2.8497, 30.1224),
                new Vector2d(-3.0685, 25.3311),
                new Vector2d(-3.0877, 24.2430),
                new Vector2d(-3.0866, 24.1563),
                new Vector2d(-3.0417, 23.0768),
                new Vector2d(-2.9368, 21.9967),
                new Vector2d(-2.9261, 21.9124),
                new Vector2d(-2.7586, 20.8415),
                new Vector2d(-2.5322, 19.7805),
                new Vector2d(-2.5122, 19.6986),
                new Vector2d(-2.2482, 18.7364),
                new Vector2d(-2.2243, 18.652),
                new Vector2d(-2.1984, 18.5729),
                new Vector2d(-1.8787, 17.6231),
            });

            previous1.Simplify();
            layers.PreviousLayer.Add(new GeneralPolygon2d(previous1));

            layers.CurrentLayer.Add(new GeneralPolygon2d(new Polygon2d(new Vector2d[] {
                new Vector2d(20.0183, 30.1224),
                new Vector2d(-20.0183, 30.1224),
                new Vector2d(-20.0183, -30.1224),
                new Vector2d(20.0183, -30.1224),
            })));

            return layers;
        }
    }
}