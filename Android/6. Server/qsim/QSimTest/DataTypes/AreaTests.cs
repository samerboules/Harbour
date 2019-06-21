using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Utilities.Clipper;
using System.Collections.Generic;

namespace QSimTest.DataTypes
{
    [TestClass]
    public class AreaTests
    {
        private static List<IntPoint> poly1 = new List<IntPoint>()
        {
            new IntPoint(0,    0),
            new IntPoint(0,    1000),
            new IntPoint(1000, 1000),
            new IntPoint(1000, 0)
        };

        private static List<IntPoint> poly2 = new List<IntPoint>()
        {
            new IntPoint(1001, 1001),
            new IntPoint(1001, 2000),
            new IntPoint(2000, 2000),
            new IntPoint(2000, 1001)
        };

        private static List<IntPoint> poly3 = new List<IntPoint>()
        {
            new IntPoint(500,  500),
            new IntPoint(500,  1500),
            new IntPoint(1500, 1500),
            new IntPoint(1500, 500)
        };

        private static string id = "IdTest";
        private Area area1 = new Area(poly1, id);
        private Area area2 = new Area(poly2, id);
        private Area area3 = new Area(poly3, id);

        [TestMethod]
        public void OwnerGetPolygon()
        {
            Assert.AreEqual(poly1, area1.GetPolygon());
            Assert.AreEqual(id, area1.Owner);
        }

        [TestMethod]
        public void Intersects()
        {
            Assert.IsFalse(area1.Intersects(area2));
            Assert.IsTrue(area2.Intersects(area3));
            Assert.IsTrue(area1.Intersects(area3));
        }

        [TestMethod]
        public void Union()
        {
            var result = area1.Union(area3);
            Assert.AreEqual(8, result.GetPolygon().Count);
            Assert.AreEqual(id, result.Owner);
        }
    }
}
