﻿using g3;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

namespace Sutro.StraightSkeleton.Benchmark
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<StraightSkeletonBenchmark>();
        }
    }

    public class StraightSkeletonBenchmark
    {
        private GeneralPolygon2d grassfireLogo;

        [GlobalSetup]
        public void Setup()
        {
            grassfireLogo = MakeGrassFireLogo();
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            SkeletonBuilder.Build(grassfireLogo);
        }

        private static GeneralPolygon2d MakeGrassFireLogo()
        {
            var outer = new Polygon2d(new Vector2d[] {
            new Vector2d(325.72446  , -221.26372),
            new Vector2d(326.35727  , -221.82446),
            new Vector2d(326.77071  , -222.74591),
            new Vector2d(326.96477  , -223.89938),
            new Vector2d(326.93947  , -225.15622),
            new Vector2d(326.69478  , -226.38774),
            new Vector2d(326.23072  , -227.46528),
            new Vector2d(325.54728  , -228.26017),
            new Vector2d(324.64447  , -228.64372),
            new Vector2d(311.68447  , -228.64372),
            new Vector2d(311.99947  , -233.23372),
            new Vector2d(312.04447  , -237.82372),
            new Vector2d(311.95597  , -240.19189),
            new Vector2d(311.68267  , -242.61938),
            new Vector2d(311.21268  , -245.07694),
            new Vector2d(310.53412  , -247.53528),
            new Vector2d(309.63513  , -249.96515),
            new Vector2d(308.50385  , -252.33727),
            new Vector2d(307.1284   , -254.62239),
            new Vector2d(305.49693  , -256.79122),
            new Vector2d(303.59757  , -258.81451),
            new Vector2d(301.41846  , -260.66298),
            new Vector2d(298.94772  , -262.30737),
            new Vector2d(296.17349  , -263.71841),
            new Vector2d(293.08392  , -264.86683),
            new Vector2d(289.66713  , -265.72337),
            new Vector2d(285.91125  , -266.25875),
            new Vector2d(281.80443  , -266.44372),
            new Vector2d(277.43943  , -266.24122),
            new Vector2d(273.88443  , -265.90372),
            new Vector2d(271.73568  , -267.24808),
            new Vector2d(270.42928  , -268.21343),
            new Vector2d(269.11443  , -269.3686),
            new Vector2d(267.90084  , -270.70934),
            new Vector2d(266.89818  , -272.23142),
            new Vector2d(266.21615  , -273.93063),
            new Vector2d(265.96443  , -275.80272),
            new Vector2d(266.15885  , -277.31653),
            new Vector2d(266.74349  , -278.73111),
            new Vector2d(267.72049  , -280.01484),
            new Vector2d(269.09193  , -281.13609),
            new Vector2d(270.85994  , -282.06324),
            new Vector2d(273.02662  , -282.76464),
            new Vector2d(275.59408  , -283.20868),
            new Vector2d(278.56443  , -283.36372),
            new Vector2d(288.59956  , -283.09372),
            new Vector2d(299.98543  , -282.82372),
            new Vector2d(303.96198  , -282.93622),
            new Vector2d(308.17485  , -283.38622),
            new Vector2d(310.29608  , -283.79052),
            new Vector2d(312.3878   , -284.34247),
            new Vector2d(314.42047  , -285.06317),
            new Vector2d(316.36456  , -285.97372),
            new Vector2d(318.19053  , -287.0952),
            new Vector2d(319.86886  , -288.44872),
            new Vector2d(321.37001  , -290.05536),
            new Vector2d(322.66445  , -291.93622),
            new Vector2d(323.72264  , -294.11239),
            new Vector2d(324.51506  , -296.60497),
            new Vector2d(325.01217  , -299.43505),
            new Vector2d(325.18443  , -302.62372),
            new Vector2d(324.93816  , -306.23907),
            new Vector2d(324.21834  , -309.71865),
            new Vector2d(323.05344  , -313.049),
            new Vector2d(321.47195  , -316.21669),
            new Vector2d(319.50233  , -319.20825),
            new Vector2d(317.17308  , -322.01025),
            new Vector2d(314.51266  , -324.60922),
            new Vector2d(311.54956  , -326.99172),
            new Vector2d(308.31225  , -329.1443),
            new Vector2d(304.82921  , -331.05351),
            new Vector2d(301.12892  , -332.70589),
            new Vector2d(297.23985  , -334.088),
            new Vector2d(293.1905   , -335.18639),
            new Vector2d(289.00932  , -335.98761),
            new Vector2d(284.72481  , -336.4782),
            new Vector2d(280.36543  , -336.64472),
            new Vector2d(276.38113  , -336.5091),
            new Vector2d(272.72459  , -336.11667),
            new Vector2d(269.38607  , -335.48905),
            new Vector2d(266.35579  , -334.64785),
            new Vector2d(263.62401  , -333.61469),
            new Vector2d(261.18096  , -332.4112),
            new Vector2d(259.0169   , -331.05901),
            new Vector2d(257.12206  , -329.57972),
            new Vector2d(255.48668  , -327.99496),
            new Vector2d(254.10101  , -326.32636),
            new Vector2d(252.95529  , -324.59553),
            new Vector2d(252.03976  , -322.82409),
            new Vector2d(251.34467  , -321.03368),
            new Vector2d(250.86025  , -319.24589),
            new Vector2d(250.57676  , -317.48237),
            new Vector2d(250.48446  , -315.76472),
            new Vector2d(250.61679  , -313.71103),
            new Vector2d(251.00234  , -311.89397),
            new Vector2d(251.62395  , -310.34728),
            new Vector2d(252.46446  , -309.10472),
            new Vector2d(258.17946  , -303.4121),
            new Vector2d(264.70446  , -297.04372),
            new Vector2d(265.8407   , -295.83717),
            new Vector2d(266.63934  , -294.68135),
            new Vector2d(266.82766  , -294.1383),
            new Vector2d(266.83066  , -293.62695),
            new Vector2d(266.61385  , -293.15365),
            new Vector2d(266.14383  , -292.72472),
            new Vector2d(263.56371  , -291.85498),
            new Vector2d(261.12914  , -290.62883),
            new Vector2d(258.90129  , -289.09478),
            new Vector2d(256.94133  , -287.30135),
            new Vector2d(255.31043  , -285.29701),
            new Vector2d(254.06977  , -283.1303),
            new Vector2d(253.28051  , -280.8497),
            new Vector2d(253.00383  , -278.50372),
            new Vector2d(253.14727  , -277.84883),
            new Vector2d(253.61133  , -277.1766),
            new Vector2d(255.70383  , -275.44372),
            new Vector2d(258.05494  , -273.77028),
            new Vector2d(260.47346  , -271.77622),
            new Vector2d(262.89216  , -269.54591),
            new Vector2d(265.24383  , -267.16372),
            new Vector2d(266.48096  , -265.38622),
            new Vector2d(266.889    , -264.50591),
            new Vector2d(267.04383  , -263.74372),
            new Vector2d(264.70664  , -262.35013),
            new Vector2d(262.35258  , -260.59372),
            new Vector2d(260.08289  , -258.46606),
            new Vector2d(257.99883  , -255.95872),
            new Vector2d(256.20164  , -253.06325),
            new Vector2d(254.79258  , -249.77122),
            new Vector2d(254.26524  , -247.97386),
            new Vector2d(253.87289  , -246.07419),
            new Vector2d(253.62821  , -244.07116),
            new Vector2d(253.54381  , -241.96372),
            new Vector2d(253.69731  , -239.10762),
            new Vector2d(254.14744  , -236.35166),
            new Vector2d(254.87865  , -233.70747),
            new Vector2d(255.87537  , -231.18664),
            new Vector2d(257.12206  , -228.80078),
            new Vector2d(258.60315  , -226.5615),
            new Vector2d(260.30308  , -224.4804),
            new Vector2d(262.20631  , -222.56909),
            new Vector2d(264.29727  , -220.83918),
            new Vector2d(266.56041  , -219.30226),
            new Vector2d(268.98017  , -217.96996),
            new Vector2d(271.541    , -216.85386),
            new Vector2d(274.22733  , -215.96558),
            new Vector2d(277.02362  , -215.31673),
            new Vector2d(279.9143   , -214.91891),
            new Vector2d(282.88381  , -214.78372),
            new Vector2d(285.42038  , -214.86462),
            new Vector2d(287.88739  , -215.09313),
            new Vector2d(292.53669  , -215.90876),
            new Vector2d(296.67955  , -217.06188),
            new Vector2d(300.16381  , -218.38376),
            new Vector2d(303.75255  , -219.89689),
            new Vector2d(306.32869  , -220.76876),
            new Vector2d(308.56714  , -221.16813),
            new Vector2d(311.14281  , -221.26373)
        });

            var inner1 = new Polygon2d(new Vector2d[] {
            new Vector2d(288.10481, -293.98373),
            new Vector2d(288.10481, -293.98373),
            new Vector2d(283.61045, -294.06513),
            new Vector2d(279.14994, -294.36572),
            new Vector2d(275.29711, -294.97006),
            new Vector2d(273.7779, -295.41257),
            new Vector2d(272.62581, -295.96272),
            new Vector2d(271.01421, -297.10706),
            new Vector2d(269.45316, -298.50241),
            new Vector2d(267.99331, -300.12979),
            new Vector2d(266.68531, -301.97022),
            new Vector2d(265.57982, -304.00471),
            new Vector2d(264.72747, -306.21428),
            new Vector2d(264.17892, -308.57995),
            new Vector2d(263.98481, -311.08272),
            new Vector2d(264.07571, -312.88465),
            new Vector2d(264.34936, -314.63913),
            new Vector2d(264.80705, -316.33665),
            new Vector2d(265.4501, -317.96772),
            new Vector2d(266.27984, -319.52286),
            new Vector2d(267.29757, -320.99256),
            new Vector2d(268.50461, -322.36735),
            new Vector2d(269.90229, -323.63772),
            new Vector2d(271.49193, -324.79418),
            new Vector2d(273.27483, -325.82725),
            new Vector2d(275.25233, -326.72743),
            new Vector2d(277.42573, -327.48522),
            new Vector2d(279.79636, -328.09114),
            new Vector2d(282.36553, -328.53569),
            new Vector2d(285.13457, -328.80938),
            new Vector2d(288.10479, -328.90268),
            new Vector2d(291.04416, -328.7991),
            new Vector2d(293.8493, -328.49592),
            new Vector2d(296.51256, -328.00448),
            new Vector2d(299.02631, -327.33612),
            new Vector2d(301.38288, -326.50217),
            new Vector2d(303.57465, -325.51397),
            new Vector2d(305.59396, -324.38286),
            new Vector2d(307.43317, -323.12018),
            new Vector2d(309.08463, -321.73726),
            new Vector2d(310.5407, -320.24545),
            new Vector2d(311.79373, -318.65608),
            new Vector2d(312.83609, -316.98049),
            new Vector2d(313.66012, -315.23002),
            new Vector2d(314.25817, -313.416),
            new Vector2d(314.62261, -311.54977),
            new Vector2d(314.74579, -309.64268),
            new Vector2d(314.65769, -307.65736),
            new Vector2d(314.39557, -305.81733),
            new Vector2d(313.96257, -304.1202),
            new Vector2d(313.36187, -302.56362),
            new Vector2d(312.59661, -301.14519),
            new Vector2d(311.66999, -299.86256),
            new Vector2d(310.58515, -298.71335),
            new Vector2d(309.34527, -297.69518),
            new Vector2d(307.95352, -296.80568),
            new Vector2d(306.41306, -296.04248),
            new Vector2d(304.72705, -295.40321),
            new Vector2d(302.89868, -294.88549),
            new Vector2d(298.82747, -294.20522),
            new Vector2d(294.22477, -293.98268),
        });

            var inner2 = new Polygon2d(new Vector2d[] {
            new Vector2d(299.26477, -242.50368),
            new Vector2d(299.02676, -238.02266),
            new Vector2d(298.30571, -233.8243),
            new Vector2d(297.76074, -231.86144),
            new Vector2d(297.09106, -230.00563),
            new Vector2d(296.29534, -228.269),
            new Vector2d(295.37227, -226.66368),
            new Vector2d(294.32053, -225.2018),
            new Vector2d(293.1388, -223.89548),
            new Vector2d(291.82575, -222.75685),
            new Vector2d(290.38008, -221.79805),
            new Vector2d(288.80047, -221.03121),
            new Vector2d(287.08559, -220.46845),
            new Vector2d(285.23413, -220.12189),
            new Vector2d(283.24477, -220.00368),
            new Vector2d(281.65097, -220.09558),
            new Vector2d(280.11417, -220.36787),
            new Vector2d(278.64069, -220.81523),
            new Vector2d(277.23685, -221.4324),
            new Vector2d(275.90898, -222.2141),
            new Vector2d(274.66339, -223.15506),
            new Vector2d(273.50643, -224.25),
            new Vector2d(272.4444, -225.49365),
            new Vector2d(271.48363, -226.88074),
            new Vector2d(270.63045, -228.40599),
            new Vector2d(269.89117, -230.06414),
            new Vector2d(269.27213, -231.8499),
            new Vector2d(268.77965, -233.75801),
            new Vector2d(268.42004, -235.78318),
            new Vector2d(268.19964, -237.92015),
            new Vector2d(268.12474, -240.16365),
            new Vector2d(268.21374, -242.50624),
            new Vector2d(268.47416, -244.73783),
            new Vector2d(268.89595, -246.85287),
            new Vector2d(269.46909, -248.84584),
            new Vector2d(270.18355, -250.71118),
            new Vector2d(271.02932, -252.44338),
            new Vector2d(271.99638, -254.03688),
            new Vector2d(273.07471, -255.48615),
            new Vector2d(274.25429, -256.78566),
            new Vector2d(275.5251, -257.92986),
            new Vector2d(276.87712, -258.91323),
            new Vector2d(278.30034, -259.73021),
            new Vector2d(279.78472, -260.37529),
            new Vector2d(281.32026, -260.84291),
            new Vector2d(282.89693, -261.12754),
            new Vector2d(284.50471, -261.22364),
            new Vector2d(286.27228, -261.13404),
            new Vector2d(287.92963, -260.86962),
            new Vector2d(289.47624, -260.43697),
            new Vector2d(290.91159, -259.8427),
            new Vector2d(292.23513, -259.09339),
            new Vector2d(293.44635, -258.19563),
            new Vector2d(294.54472, -257.15602),
            new Vector2d(295.52971, -255.98114),
            new Vector2d(296.4008, -254.67759),
            new Vector2d(297.15745, -253.25196),
            new Vector2d(298.32534, -250.06083),
            new Vector2d(299.02916, -246.46048),
        });

            var poly = new GeneralPolygon2d(outer);
            // poly.AddHole(inner1, false, true);
            poly.AddHole(inner2, false, true);

            return poly;
        }
    }
}