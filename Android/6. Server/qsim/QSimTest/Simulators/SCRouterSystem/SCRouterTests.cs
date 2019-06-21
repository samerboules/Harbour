using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Simulators.SCRouterSystem;
using System;
using System.Collections.Generic;

namespace QSimTest.Simulators.SCRouterSystem
{
    [TestClass]
    public class SCRouterTests
    {
        const int TURNING_RADIUS = 7500;

        [TestMethod]
        public void TestGetEmptyRoute()
        {
            Position position = new Position(10, 10, 0, Position.EAST);
            Position originalPosition = position.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetRoute(position, position);

            Assert.AreEqual(originalPosition, position, "original position");
            Assert.AreEqual(1, route.Count, "count");
            Assert.AreEqual(position, route[0].Position, "position");
        }

        [TestMethod]
        public void TestGetDirectRoute()
        {
            Position from = new Position(10, 20, 30, 40);
            Position to = new Position(11, 12, 13, 14);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetDirectRoute(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(2, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(to, route[1].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendLengthDirectE()
        {
            Position from = new Position(1000, 1000, 0, Position.EAST);
            Position to = new Position(2000, 1000, 0, Position.EAST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "from");
            Assert.AreEqual(originalTo, to, "to");
            Assert.AreEqual(0, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDirectW()
        {
            Position from = new Position(500, 1000, 0, Position.WEST);
            Position to = new Position(10000, 1000, 0, Position.WEST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(0, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDirectN()
        {
            Position from = new Position(80000, 1000, 0, Position.NORTH);
            Position to = new Position(80000, 3000, 0, Position.NORTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(0, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDirectS()
        {
            Position from = new Position(7000, 2000, 0, Position.SOUTH);
            Position to = new Position(7000, 1000, 0, Position.SOUTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(0, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDirectD()
        {
            Position from = new Position(10000, 10000, 0, Math.PI / 4);
            Position to = new Position(20000, 20000, 0, Math.PI / 4);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(0, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDoubleStraightE()
        {
            Position from = new Position(1000, 1000, 0, Position.EAST);
            Position to = new Position(2000, 16000, 0, Position.EAST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(15000, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDoubleStraightW()
        {
            Position from = new Position(1000, 1000, 0, Position.WEST);
            Position to = new Position(-1000, -15000, 0, Position.WEST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(15000, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDoubleStraightN()
        {
            Position from = new Position(0, 0, 0, Position.NORTH);
            Position to = new Position(100000, 100000, 0, Position.NORTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(15000, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLengthDoubleStraightS()
        {
            Position from = new Position(20000, 20000, 0, Position.SOUTH);
            Position to = new Position(1000, 1000, 0, Position.SOUTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(15000, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLength1()
        {
            Position from = new Position(10000, 10000, 0, Position.EAST);
            Position to = new Position(30000, 16000, 0, Position.EAST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(12000, length, "length");
        }

        [TestMethod]
        public void TestGetSBendLength2()
        {
            Position from = new Position(80000, 80000, 0, Position.SOUTH);
            Position to = new Position(77000, 0, 0, Position.SOUTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(9000, length);
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateDirectE()
        {
            Position from = new Position(10000, 20, 0, Position.EAST);
            Position to = new Position(20000, 20, 0, Position.EAST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from, to);

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(2, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(to, route[1].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateDirectW()
        {
            Position from = new Position(5000, -2500, 0, Position.WEST);
            Position to = new Position(7000, -2500, 0, Position.WEST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(2, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(to, route[1].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateDirectS()
        {
            Position from = new Position(7000, 8000, 0, Position.SOUTH);
            Position to = new Position(7000, 4000, 0, Position.SOUTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(2, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(to, route[1].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateDirectN()
        {
            Position from = new Position(-7000, 6000, 0, Position.NORTH);
            Position to = new Position(-7000, 7000, 0, Position.NORTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(2, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(to, route[1].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateDirectD()
        {
            double orientation = Math.PI / 4;
            Position from = new Position(8000, 10000, 0, orientation);
            Position to = new Position(12000, 14000, 0, orientation);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(2, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(to, route[1].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateTwoStraightE()
        {
            Position from = new Position(1000, 1000, 0, Position.EAST);
            Position to = new Position(20000, 17000, 0, Position.EAST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(4, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(new Position(from.x + TURNING_RADIUS, from.y, 0, route[1].Position.phi), route[1].Position, "corner 1");
            Assert.AreEqual(new Position(from.x + TURNING_RADIUS, to.y, 0, route[2].Position.phi), route[2].Position, "corner 2");
            Assert.AreEqual(to, route[3].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateTwoStraightW()
        {
            Position from = new Position(0, 0, 0, Position.WEST);
            Position to = new Position(-100000, 15000, 0, Position.WEST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(4, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(new Position(from.x - TURNING_RADIUS, from.y, 0, route[1].Position.phi), route[1].Position, "corner 1");
            Assert.AreEqual(new Position(from.x - TURNING_RADIUS, to.y, 0, route[2].Position.phi), route[2].Position, "corner 2");
            Assert.AreEqual(to, route[3].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateTwoStraightS()
        {
            Position from = new Position(-10000, -20000, 0, Position.SOUTH);
            Position to = new Position(10000, 10000, 0, Position.SOUTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(4, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(new Position(from.x, from.y + TURNING_RADIUS, 0, route[1].Position.phi), route[1].Position, "corner 1");
            Assert.AreEqual(new Position(to.x, from.y + TURNING_RADIUS, 0, route[2].Position.phi), route[2].Position, "corner 2");
            Assert.AreEqual(to, route[3].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateTwoStraightN()
        {
            Position from = new Position(10000, 200000, 0, Position.NORTH);
            Position to = new Position(40000, 15000, 0, Position.NORTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(4, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(new Position(from.x, from.y - TURNING_RADIUS, 0, route[1].Position.phi), route[1].Position, "corner 1");
            Assert.AreEqual(new Position(to.x, from.y - TURNING_RADIUS, 0, route[2].Position.phi), route[2].Position, "corner 2");
            Assert.AreEqual(to, route[3].Position, "to");
        }

        [TestMethod]
        public void TestGetSBendAtFrontRouteDegenerateTwoStraightD()
        {
            double orientation = Math.Atan(-0.75);
            Position from = new Position(6000, -15000, 0, orientation);
            Position to = new Position(55500, -24000, 0, orientation);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(4, route.Count, "count");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(new Position(12000, -19500, 0, route[1].Position.phi), route[1].Position, "corner 1");
            Assert.AreEqual(new Position(25500, -1500, 0, route[2].Position.phi), route[2].Position, "corner 2");
            Assert.AreEqual(to, route[3].Position, "to");
        }

        [TestMethod]
        public void GetSBendAtFrontRouteS()
        {
            Position from = new Position(7500, 0, 0, Position.SOUTH);
            Position to = new Position(1500, 15000, 0, Position.SOUTH);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);
            List<RoutePoint> route = router.GetSBendAtFrontRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(12000, length, "length");
            Assert.AreEqual(6, route.Count, "number of route points");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(new Position(7500, 3750, 0, route[1].Position.phi), route[1].Position, "c1");
            Assert.AreEqual(new Position(4500, 6000, 0, route[2].Position.phi), route[2].Position, "mid");
            Assert.AreEqual(new Position(1500, 8250, 0, route[3].Position.phi), route[3].Position, "c2");
            Assert.AreEqual(new Position(1500, 12000, 0, route[4].Position.phi), route[4].Position, "pre-to");
            Assert.AreEqual(to, route[5].Position, "to");
        }

        [TestMethod]
        public void GetSBendAtTailRouteE()
        {
            Position from = new Position(30000, 80000, 0, Position.EAST);
            Position to = new Position(52000, 74000, 0, Position.EAST);
            Position originalFrom = from.Copy();
            Position originalTo = to.Copy();

            SCRouter router = new SCRouter();
            int length = router.GetSBendLength(from, to);
            List<RoutePoint> route = router.GetSBendAtTailRoute(from.Copy(), to.Copy());

            Assert.AreEqual(originalFrom, from, "original from");
            Assert.AreEqual(originalTo, to, "original to");
            Assert.AreEqual(12000, length, "length");
            Assert.AreEqual(6, route.Count, "number of route points");
            Assert.AreEqual(from, route[0].Position, "from");
            Assert.AreEqual(new Position(40000, 80000, 0, route[1].Position.phi), route[1].Position, "post-from");
            Assert.AreEqual(new Position(43750, 80000, 0, route[2].Position.phi), route[2].Position, "c1");
            Assert.AreEqual(new Position(46000, 77000, 0, route[3].Position.phi), route[3].Position, "mid");
            Assert.AreEqual(new Position(48250, 74000, 0, route[4].Position.phi), route[4].Position, "c2");
            Assert.AreEqual(to, route[5].Position, "to");
        }
    }
}
