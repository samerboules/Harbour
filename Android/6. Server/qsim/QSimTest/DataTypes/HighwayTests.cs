using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSim.ConsoleApp.DataTypes;
using System;

namespace QSimTest.DataTypes
{
    [TestClass]
    public class HighwayTests
    {
        [TestMethod]
        public void TestIsInOrientationE()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            Position from = new Position(100, 10, 0, Position.EAST);
            Position to = new Position(300, 10, 0, Position.EAST);
            Highway highway = new Highway(1, from, to, Position.EAST);

            Assert.IsFalse(highway.IsInOrientation(Position.EAST - moreThanLimit), "east minus more than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.EAST - lessThanLimit), "east minus less than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.EAST), "east");
            Assert.IsTrue(highway.IsInOrientation(Position.EAST + lessThanLimit), "east plus less than limit");
            Assert.IsFalse(highway.IsInOrientation(Position.EAST + moreThanLimit), "east plus more than limit");

            Assert.IsFalse(highway.IsInOrientation(Position.WEST), "west");
            Assert.IsFalse(highway.IsInOrientation(Position.SOUTH), "south");
            Assert.IsFalse(highway.IsInOrientation(Position.NORTH), "north");
        }

        [TestMethod]
        public void TestIsInOrientationW()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            Position from = new Position(1000, 100, 0, Position.WEST);
            Position to = new Position(0, 100, 0, Position.WEST);
            Highway highway = new Highway(1, from, to, Position.WEST);

            Assert.IsFalse(highway.IsInOrientation(Position.WEST - moreThanLimit), "west minus more than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.WEST - lessThanLimit), "west minus less than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.WEST), "west");
            Assert.IsTrue(highway.IsInOrientation(Position.WEST + lessThanLimit), "west plus less than limit");
            Assert.IsFalse(highway.IsInOrientation(Position.WEST + moreThanLimit), "west plus more than limit");

            Assert.IsFalse(highway.IsInOrientation(Position.EAST), "east");
            Assert.IsFalse(highway.IsInOrientation(Position.SOUTH), "south");
            Assert.IsFalse(highway.IsInOrientation(Position.NORTH), "north");
        }

        [TestMethod]
        public void TestIsInOrientationN()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            Position from = new Position(0, 0, 0, Position.NORTH);
            Position to = new Position(0, -100, 0, Position.NORTH);
            Highway highway = new Highway(1, from, to, Position.NORTH);

            Assert.IsFalse(highway.IsInOrientation(Position.NORTH - moreThanLimit), "north minus more than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.NORTH - lessThanLimit), "north minus less than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.NORTH), "north");
            Assert.IsTrue(highway.IsInOrientation(Position.NORTH + lessThanLimit), "north plus less than limit");
            Assert.IsFalse(highway.IsInOrientation(Position.NORTH + moreThanLimit), "north plus more than limit");

            Assert.IsFalse(highway.IsInOrientation(Position.EAST), "east");
            Assert.IsFalse(highway.IsInOrientation(Position.WEST), "west");
            Assert.IsFalse(highway.IsInOrientation(Position.SOUTH), "south");
        }

        [TestMethod]
        public void TestIsInOrientationS()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            Position from = new Position(0, 0, 0, Position.SOUTH);
            Position to = new Position(0, -100, 0, Position.SOUTH);
            Highway highway = new Highway(1, from, to, Position.SOUTH);

            Assert.IsFalse(highway.IsInOrientation(Position.SOUTH - moreThanLimit), "south minus more than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.SOUTH - lessThanLimit), "south minus less than limit");
            Assert.IsTrue(highway.IsInOrientation(Position.SOUTH), "south");
            Assert.IsTrue(highway.IsInOrientation(Position.SOUTH + lessThanLimit), "south plus less than limit");
            Assert.IsFalse(highway.IsInOrientation(Position.SOUTH + moreThanLimit), "south plus more than limit");

            Assert.IsFalse(highway.IsInOrientation(Position.EAST), "east");
            Assert.IsFalse(highway.IsInOrientation(Position.WEST), "west");
            Assert.IsFalse(highway.IsInOrientation(Position.NORTH), "north");
        }

        [TestMethod]
        public void TestIsInOrientationD()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            Position from = new Position(1000, 1000, 0, 0);
            Position to = new Position(2000, 2000, 0, 0);
            double orientation = from.AngleTo(to);
            Highway highway = new Highway(1, from, to, orientation);

            Assert.IsFalse(highway.IsInOrientation(orientation - moreThanLimit), "minus more than limit");
            Assert.IsTrue(highway.IsInOrientation(orientation - lessThanLimit), "minus less than limit");
            Assert.IsTrue(highway.IsInOrientation(orientation), "exact");
            Assert.IsTrue(highway.IsInOrientation(orientation + lessThanLimit), "plus less than limit");
            Assert.IsFalse(highway.IsInOrientation(orientation + moreThanLimit), "plus more than limit");

            Assert.IsFalse(highway.IsInOrientation(orientation + Math.PI), "opposite");
        }

        [TestMethod]
        public void TestIsOnHighwayE()
        {
            const double angleLimit = 5 * Math.PI / 1000;
            const double lessThanAngleLimit = 0.9 * angleLimit;
            const double moreThanAngleLimit = 1.1 * angleLimit;

            Position from = new Position(1000, 100, 0, Position.EAST);
            Position to = new Position(3000, 100, 0, Position.EAST);
            Highway highway = new Highway(1, from, to, Position.EAST);

            Assert.IsTrue(highway.IsOnHighway(new Position(1000, 100, 0, Position.EAST)), "from");
            Assert.IsTrue(highway.IsOnHighway(new Position(1000, 100, 0, Position.WEST)), "from in opposite orientation");
            Assert.IsTrue(highway.IsOnHighway(new Position(1000, 100, 0, Position.EAST - lessThanAngleLimit)), "from minus less than angle limit");
            Assert.IsTrue(highway.IsOnHighway(new Position(1000, 100, 0, Position.EAST + lessThanAngleLimit)), "from plus less than angle limit");
            Assert.IsFalse(highway.IsOnHighway(new Position(1000, 100, 0, Position.EAST - moreThanAngleLimit)), "from minus more than angle limit");
            Assert.IsFalse(highway.IsOnHighway(new Position(1000, 100, 0, Position.EAST + moreThanAngleLimit)), "from plus more than angle limit");
            Assert.IsFalse(highway.IsOnHighway(new Position(1000, 100, 0, Position.NORTH)), "from in wrong orientation");

            Assert.IsTrue(highway.IsOnHighway(new Position(3000, 100, 0, Position.EAST)), "to");
            Assert.IsTrue(highway.IsOnHighway(new Position(3000, 100, 0, Position.WEST)), "to in opposite orientation");
            Assert.IsFalse(highway.IsOnHighway(new Position(3000, 100, 0, Position.NORTH)), "to in wrong orientation");

            Assert.IsTrue(highway.IsOnHighway(new Position(2000, 100, 0, Position.EAST)), "in between");
            Assert.IsTrue(highway.IsOnHighway(new Position(2000, 100, 0, Position.WEST)), "in between with opposite orientation");
            Assert.IsFalse(highway.IsOnHighway(new Position(2000, 100, 0, Position.NORTH)), "in between in wrong orientation");

            Assert.IsFalse(highway.IsOnHighway(new Position(100, 100, 0, Position.EAST)), "before from");
            Assert.IsFalse(highway.IsOnHighway(new Position(5000, 100, 0, Position.EAST)), "after to");

            Assert.IsFalse(highway.IsOnHighway(new Position(2000, 200, 0, Position.EAST)), "besides highway");
            Assert.IsFalse(highway.IsOnHighway(new Position(100, 200, 0, Position.EAST)), "besides before highway");
            Assert.IsFalse(highway.IsOnHighway(new Position(5000, 200, 0, Position.EAST)), "besides after highway");
        }

        [TestMethod]
        public void TestIsOnHighwayD()
        {
            const double angleLimit = 5 * Math.PI / 1000;
            const double lessThanAngleLimit = 0.9 * angleLimit;
            const double moreThanAngleLimit = 1.1 * angleLimit;

            Position from = new Position(2000, 4000, 0, 0);
            Position to = new Position(3000, 6000, 0, 0);
            double orientation = from.AngleTo(to);
            Highway highway = new Highway(1, from, to, orientation);

            Assert.IsTrue(highway.IsOnHighway(new Position(2000, 4000, 0, orientation)), "from");
            Assert.IsTrue(highway.IsOnHighway(new Position(2000, 4000, 0, orientation + Math.PI)), "from in opposite orientation");
            Assert.IsTrue(highway.IsOnHighway(new Position(2000, 4000, 0, orientation - lessThanAngleLimit)), "from minus less than angle limit");
            Assert.IsTrue(highway.IsOnHighway(new Position(2000, 4000, 0, orientation + lessThanAngleLimit)), "from plus less than angle limit");
            Assert.IsFalse(highway.IsOnHighway(new Position(2000, 4000, 0, orientation - moreThanAngleLimit)), "from minus more than angle limit");
            Assert.IsFalse(highway.IsOnHighway(new Position(2000, 4000, 0, orientation + moreThanAngleLimit)), "from plus more than angle limit");
            Assert.IsFalse(highway.IsOnHighway(new Position(2000, 4000, 0, orientation + 0.5 * Math.PI)), "from in wrong orientation");

            Assert.IsTrue(highway.IsOnHighway(new Position(3000, 6000, 0, orientation)), "to");
            Assert.IsTrue(highway.IsOnHighway(new Position(3000, 6000, 0, orientation + Math.PI)), "to in opposite orientation");
            Assert.IsFalse(highway.IsOnHighway(new Position(3000, 6000, 0, orientation + 0.5 * Math.PI)), "to in wrong orientation");

            Assert.IsTrue(highway.IsOnHighway(new Position(2500, 5000, 0, orientation)), "in between");
            Assert.IsTrue(highway.IsOnHighway(new Position(2500, 5000, 0, orientation + Math.PI)), "in between with opposite orientation");
            Assert.IsFalse(highway.IsOnHighway(new Position(2500, 5000, 0, orientation + 0.5 * Math.PI)), "in between in wrong orientation");

            Assert.IsFalse(highway.IsOnHighway(new Position(1980, 3992, 0, orientation)), "before from");
            Assert.IsFalse(highway.IsOnHighway(new Position(3020, 6008, 0, orientation)), "after to");

            Assert.IsFalse(highway.IsOnHighway(new Position(2520, 4992, 0, Position.EAST)), "besides");
        }
    }
}
