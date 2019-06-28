using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSim.ConsoleApp.DataTypes;
using System;

namespace QSimTest.DataTypes
{
    [TestClass]
    public class PositionTests
    {
        private const double MAX_ANGLE_DELTA = 5 * Math.PI / 1000;

        [TestMethod]
        public void TestEquals()
        {
            Position p = new Position(5, 6, 7, 8);
            Position q = new Position(5, 6, 7, 8);
            Assert.IsTrue(p.Equals(q));
        }

        [TestMethod]
        public void TestEqualsX()
        {
            Position p = new Position(5, 6, 7, 8);
            Position q = new Position(6, 6, 7, 8);
            Assert.IsFalse(p.Equals(q));
        }

        [TestMethod]
        public void TestEqualsY()
        {
            Position p = new Position(5, 6, 7, 8);
            Position q = new Position(5, 7, 7, 8);
            Assert.IsFalse(p.Equals(q));
        }

        [TestMethod]
        public void TestEqualsZ()
        {
            Position p = new Position(5, 6, 7, 8);
            Position q = new Position(5, 6, 8, 8);
            Assert.IsFalse(p.Equals(q));
        }

        [TestMethod]
        public void TestEqualsPhi1()
        {
            Position p = new Position(5, 6, 7, 8);
            Position q = new Position(5, 6, 7, 8 + 2 * Math.PI / 1000);
            Assert.IsTrue(p.Equals(q));
        }

        public void TestEqualsPhi2()
        {
            Position p = new Position(5, 6, 7, 8);
            Position q = new Position(5, 6, 7, 8 + 10 * Math.PI / 1000);
            Assert.IsFalse(p.Equals(q));
        }

        public void TestIsEqualInPlane1()
        {
            Position p = new Position(10, 20, 5, 7);
            Position q = new Position(10, 20, 6, 8);
            Assert.IsTrue(p.IsEqualInPlane(q));
        }

        public void TestIsEqualInPlane2()
        {
            Position p = new Position(10, 20, 5, 7);
            Position q = new Position(11, 20, 5, 7);
            Assert.IsFalse(p.IsEqualInPlane(q));
        }

        public void TestIsEqualInPlane3()
        {
            Position p = new Position(10, 20, 5, 7);
            Position q = new Position(10, 21, 5, 7);
            Assert.IsFalse(p.IsEqualInPlane(q));
        }

        [TestMethod]
        public void TestDistanceToH()
        {
            Position p = new Position(8, 0, 0, 0);
            Position q = new Position(2, 0, 0, 0);
            int distance = p.DistanceTo(q);
            Assert.AreEqual(6, distance);
        }

        [TestMethod]
        public void TestDistanceToV()
        {
            Position p = new Position(0, 10, 0, 0);
            Position q = new Position(0, 20, 0, 0);
            int distance = p.DistanceTo(q);
            Assert.AreEqual(10, distance);
        }

        [TestMethod]
        public void TestDistanceToD1()
        {
            Position p = new Position(2, 4, 0, 0);
            Position q = new Position(5, 8, 0, 0);
            int distance = p.DistanceTo(q);
            Assert.AreEqual(5, distance);
        }

        [TestMethod]
        public void TestDistanceToD2()
        {
            Position p = new Position(2, 5, 0, 0);
            Position q = new Position(5, 8, 0, 0);
            int distance = p.DistanceTo(q);
            Assert.AreEqual(4, distance);
        }

        [TestMethod]
        public void TestDistanceToD3()
        {
            Position p = new Position(1, 5, 0, 0);
            Position q = new Position(5, 1, 0, 0);
            int distance = p.DistanceTo(q);
            Assert.AreEqual(6, distance);
        }

        [TestMethod]
        public void TestAngleToE()
        {
            Position p = new Position(5, 0, 0, 0);
            Position q = new Position(6, 0, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(Position.EAST, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestAngleToW()
        {
            Position p = new Position(6, 0, 0, 0);
            Position q = new Position(5, 0, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(Position.WEST, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestAngleToS()
        {
            Position p = new Position(0, 2, 0, 0);
            Position q = new Position(0, -3, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(Position.NORTH, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestAngleToN()
        {
            Position p = new Position(0, 4, 0, 0);
            Position q = new Position(0, 8, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(Position.SOUTH, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestAngleToD1()
        {
            Position p = new Position(5, 5, 0, 0);
            Position q = new Position(10, 10, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(0.785, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestAngleToD2()
        {
            Position p = new Position(10, 15, 0, 0);
            Position q = new Position(5, 20, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(2.356, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestAngleToD3()
        {
            Position p = new Position(30, 80, 0, 0);
            Position q = new Position(10, 60, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(3.927, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestAngleToD4()
        {
            Position p = new Position(5, 55, 0, 0);
            Position q = new Position(55, 5, 0, 0);
            double angle = p.AngleTo(q);
            Assert.AreEqual(5.498, angle, MAX_ANGLE_DELTA);
        }

        [TestMethod]
        public void TestGetFurtherE()
        {
            Position p = new Position(5, 6, 0, Position.EAST);
            Position q = p.GetFurther(23);
            Position expect = new Position(28, 6, 0, Position.EAST);
            Assert.AreEqual(expect, q);
        }

        [TestMethod]
        public void TestGetFurtherW()
        {
            Position p = new Position(5, 6, 0, Position.WEST);
            Position q = p.GetFurther(7);
            Position expect = new Position(-2, 6, 0, Position.WEST);
            Assert.AreEqual(expect, q);
        }

        [TestMethod]
        public void TestGetFurtherS()
        {
            Position p = new Position(5, 6, 0, Position.SOUTH);
            Position q = p.GetFurther(8);
            Position expect = new Position(5, 14, 0, Position.SOUTH);
            Assert.AreEqual(expect, q);
        }

        [TestMethod]
        public void TestGetFurtherN()
        {
            Position p = new Position(5, 6, 0, Position.NORTH);
            Position q = p.GetFurther(7);
            Position expect = new Position(5, -1, 0, Position.NORTH);
            Assert.AreEqual(expect, q);
        }

        [TestMethod]
        public void TestGetFurtherD()
        {
            Position p = new Position(10, 20, 0, 0.628);
            Position q = p.GetFurther(12);
            Position expect = new Position(20, 27, 0, 0.628);
            Assert.AreEqual(expect, q);
        }

        [TestMethod]
        public void TestGetFurtherNegative()
        {
            Position p = new Position(10, 20, 0, 0.628);
            Position q = p.GetFurther(-12);
            Position expect = new Position(0, 13, 0, 0.628);
            Assert.AreEqual(expect, q);
        }

        [TestMethod]
        public void TestGetPositionTowardsE()
        {
            Position p = new Position(10, 15, 20, 0);
            Position q = new Position(20, 15, 20, 0);
            Position r = p.GetPositionTowards(q, 30);
            Position expect = new Position(40, 15, 20, 0);
            Assert.AreEqual(expect, r);
        }

        [TestMethod]
        public void TestGetPositionTowardsW()
        {
            Position p = new Position(10, 15, 20, 0);
            Position q = new Position(-10, 15, 20, 0);
            Position r = p.GetPositionTowards(q, 3);
            Position expect = new Position(7, 15, 20, 0);
            Assert.AreEqual(expect, r);
        }

        [TestMethod]
        public void TestGetPositionTowardsS()
        {
            Position p = new Position(10, 15, 20, 0);
            Position q = new Position(10, 110, 20, 0);
            Position r = p.GetPositionTowards(q, 8);
            Position expect = new Position(10, 23, 20, 0);
            Assert.AreEqual(expect, r);
        }

        [TestMethod]
        public void TestGetPositionTowardsN()
        {
            Position p = new Position(10, 15, 20, 0);
            Position q = new Position(10, 14, 20, 0);
            Position r = p.GetPositionTowards(q, 40);
            Position expect = new Position(10, -25, 20, 0);
            Assert.AreEqual(expect, r);
        }

        [TestMethod]
        public void TestGetPositionTowardsD()
        {
            Position p = new Position(10, 15, 0, 0);
            Position q = new Position(-20, 75, 0, 0);
            Position r = p.GetPositionTowards(q, 15);
            Position expect = new Position(3, 28, 0, 0);
            Assert.AreEqual(expect, r);
        }

        [TestMethod]
        public void TestGetPositionTowardsNegative()
        {
            Position p = new Position(5, 5, 0, 0);
            Position q = new Position(100, 55, 0, 0);
            Position r = p.GetPositionTowards(q, -17);
            Position expect = new Position(-10, -3, 0, 0);
            Assert.AreEqual(expect, r);
        }

        [TestMethod]
        public void TestIsOppositeAngles()
        {
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.EAST, Position.WEST), "east vs west");
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.WEST, Position.EAST), "west vs east");
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.NORTH, Position.SOUTH), "north vs south");
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.SOUTH, Position.NORTH), "south vs north");

            Assert.IsFalse(Position.InOpposingSemiCircles(0, 2 * Math.PI), "0 vs 2pi");

            Assert.IsFalse(Position.InOpposingSemiCircles(Position.NORTH, Position.NORTH), "north vs north");
            Assert.IsFalse(Position.InOpposingSemiCircles(Position.NORTH, 3.927), "north vs north-west");
            Assert.IsFalse(Position.InOpposingSemiCircles(Position.NORTH, 5.498), "north vs north-east");
            Assert.IsFalse(Position.InOpposingSemiCircles(Position.NORTH, 2 * Math.PI - 0.001), "north vs just north of east");
            Assert.IsFalse(Position.InOpposingSemiCircles(Position.NORTH, Position.WEST + 0.001), "north vs just north of west");

            Assert.IsTrue(Position.InOpposingSemiCircles(Position.NORTH, 0.001), "north vs just south of east");
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.NORTH, Position.WEST - 0.001), "north vs just south of west");
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.NORTH, 2.356), "north vs south-west");
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.NORTH, 0.785), "north vs south-east");
            Assert.IsTrue(Position.InOpposingSemiCircles(Position.NORTH, Position.SOUTH), "north vs south");
        }

        [TestMethod]
        public void TestIsNorthSouth()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            Assert.IsFalse(new Position(0, 0, 0, Position.NORTH - moreThanLimit).IsNorthSouth, "north minus more than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.NORTH - lessThanLimit).IsNorthSouth, "north minus less than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.NORTH).IsNorthSouth, "north");
            Assert.IsTrue(new Position(0, 0, 0, Position.NORTH + lessThanLimit).IsNorthSouth, "north plus less than limit");
            Assert.IsFalse(new Position(0, 0, 0, Position.NORTH + moreThanLimit).IsNorthSouth, "north plus more than limit");

            Assert.IsFalse(new Position(0, 0, 0, Position.SOUTH - moreThanLimit).IsNorthSouth, "south minus more than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.SOUTH - lessThanLimit).IsNorthSouth, "south minus less than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.SOUTH).IsNorthSouth, "south");
            Assert.IsTrue(new Position(0, 0, 0, Position.SOUTH + lessThanLimit).IsNorthSouth, "south plus less than limit");
            Assert.IsFalse(new Position(0, 0, 0, Position.SOUTH + moreThanLimit).IsNorthSouth, "south plus more than limit");

            Assert.IsFalse(new Position(0, 0, 0, Position.EAST).IsNorthSouth, "east");
            Assert.IsFalse(new Position(0, 0, 0, Position.WEST).IsNorthSouth, "west");
        }

        [TestMethod]
        public void TestIsEastWest()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            Assert.IsFalse(new Position(0, 0, 0, Position.EAST - moreThanLimit).IsEastWest, "east minus more than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.EAST - lessThanLimit).IsEastWest, "east minus less than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.EAST).IsEastWest, "east");
            Assert.IsTrue(new Position(0, 0, 0, Position.EAST + lessThanLimit).IsEastWest, "east plus less than limit");
            Assert.IsFalse(new Position(0, 0, 0, Position.EAST + moreThanLimit).IsEastWest, "east plus more than limit");

            Assert.IsFalse(new Position(0, 0, 0, Position.WEST - moreThanLimit).IsEastWest, "west minus more than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.WEST - lessThanLimit).IsEastWest, "west minus less than limit");
            Assert.IsTrue(new Position(0, 0, 0, Position.WEST).IsEastWest, "west");
            Assert.IsTrue(new Position(0, 0, 0, Position.WEST + lessThanLimit).IsEastWest, "west plus less than limit");
            Assert.IsFalse(new Position(0, 0, 0, Position.WEST + moreThanLimit).IsEastWest, "west plus more than limit");

            Assert.IsFalse(new Position(0, 0, 0, Position.NORTH).IsEastWest, "north");
            Assert.IsFalse(new Position(0, 0, 0, Position.SOUTH).IsEastWest, "south");
        }

        [TestMethod]
        public void TestIsSameOrOppositeAngle()
        {
            const double limit = 5 * Math.PI / 1000;
            const double lessThanLimit = 0.9 * limit;
            const double moreThanLimit = 1.1 * limit;

            const double baseAngle = 0.25 * Math.PI;
            const double oppositeAngle = baseAngle + Math.PI;

            Position p = new Position(0, 0, 0, baseAngle);

            Assert.IsFalse(p.IsInOrientationOrOpposite(baseAngle - moreThanLimit), "base angle minus more than limit");
            Assert.IsTrue(p.IsInOrientationOrOpposite(baseAngle - lessThanLimit), "base angle minus less than limit");
            Assert.IsTrue(p.IsInOrientationOrOpposite(baseAngle), "base angle");
            Assert.IsTrue(p.IsInOrientationOrOpposite(baseAngle + lessThanLimit), "base angle plus less than limit");
            Assert.IsFalse(p.IsInOrientationOrOpposite(baseAngle + moreThanLimit), "base angle plus more than limit");

            Assert.IsFalse(p.IsInOrientationOrOpposite(baseAngle - 0.5 * Math.PI), "base angle minus 0.5pi");
            Assert.IsFalse(p.IsInOrientationOrOpposite(baseAngle + 0.5 * Math.PI), "base angle plus 0.5pi");

            Assert.IsFalse(p.IsInOrientationOrOpposite(oppositeAngle - moreThanLimit), "opposite angle minus more than limit");
            Assert.IsTrue(p.IsInOrientationOrOpposite(oppositeAngle - lessThanLimit), "opposite angle minus less than limit");
            Assert.IsTrue(p.IsInOrientationOrOpposite(oppositeAngle), "opposite angle");
            Assert.IsTrue(p.IsInOrientationOrOpposite(oppositeAngle + lessThanLimit), "opposite angle plus less than limit");
            Assert.IsFalse(p.IsInOrientationOrOpposite(oppositeAngle + moreThanLimit), "opposite angle plus more than limit");
        }

        [TestMethod]
        public void TestProjectOnLineH()
        {
            Position p = new Position(30, 10, 0, 0);
            Position from = new Position(10, 20, 0, 0);
            Position to = new Position(50, 20, 0, 0);
            Position projected = p.ProjectOnLine(from, to);
            Assert.AreEqual(new Position(30, 20, 0, 0), projected);
        }

        [TestMethod]
        public void TestProjectOnLineV()
        {
            Position p = new Position(60, 30, 0, 0);
            Position from = new Position(40, 10, 0, 0);
            Position to = new Position(40, 20, 0, 0);
            Position projected = p.ProjectOnLine(from, to);
            Assert.AreEqual(new Position(40, 30, 0, 0), projected);
        }

        [TestMethod]
        public void TestProjectOnLineD()
        {
            Position p = new Position(30, 40, 0, 0);
            Position from = new Position(100, 50, 0, 0);
            Position to = new Position(20, 10, 0, 0);
            Position projected = p.ProjectOnLine(from, to);
            Assert.AreEqual(new Position(40, 20, 0, 0), projected);
        }

        [TestMethod]
        public void TestProjectOnE()
        {
            Position p = new Position(30, -10, 0, Position.EAST);
            Position q = new Position(17, 100, 0, 0);
            Position projected = q.ProjectOn(p);
            Assert.AreEqual(new Position(17, -10, 0, 0), projected);
        }

        [TestMethod]
        public void TestProjectOnW()
        {
            Position p = new Position(8, 50, 0, Position.WEST);
            Position q = new Position(100, 100, 0, 0);
            Position projected = q.ProjectOn(p);
            Assert.AreEqual(new Position(100, 50, 0, 0), projected);
        }

        [TestMethod]
        public void TestProjectOnN()
        {
            Position p = new Position(0, 0, 0, Position.NORTH);
            Position q = new Position(15, 15, 0, 0);
            Position projected = q.ProjectOn(p);
            Assert.AreEqual(new Position(0, 15, 0, 0), projected);
        }

        [TestMethod]
        public void TestProjectOnS()
        {
            Position p = new Position(120, 130, 0, Position.SOUTH);
            Position q = new Position(11, 11, 0, 0);
            Position projected = q.ProjectOn(p);
            Assert.AreEqual(new Position(120, 11, 0, 0), projected);
        }

        [TestMethod]
        public void TestProjectOnD()
        {
            Position p = new Position(-100, 100, 0, 4.000);
            Position q = new Position(-100, 50, 0, 0);
            Position projected = q.ProjectOn(p);
            Assert.AreEqual(new Position(-124, 71, 0, 0), projected);
        }
    }
}
