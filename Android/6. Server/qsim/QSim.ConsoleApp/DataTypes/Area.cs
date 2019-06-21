using QSim.ConsoleApp.Utilities;
using QSim.ConsoleApp.Utilities.Clipper;
using System.Collections.Generic;

namespace QSim.ConsoleApp.DataTypes
{
    public class Area
    {
        private readonly List<IntPoint> polygon;

        public Area(List<IntPoint> _polygon, string _owner)
        {
            polygon = _polygon;
            Owner = _owner;
        }

        public string Owner { get; }

        public List<IntPoint> GetPolygon()
        {
            return polygon;
        }

        public bool Intersects(List<IntPoint> poly)
        {
            Clipper clipper = new Clipper();
            clipper.AddPath(polygon, PolyType.ptSubject, true);
            clipper.AddPath(poly, PolyType.ptClip, poly.Count > 2);
            var result = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctIntersection, result, PolyFillType.pftNonZero);
            return result.Count != 0;
        }

        public bool Intersects(Area other)
        {
            return Intersects(other.GetPolygon());
        }

        public Area Union(params Area[] polygons)
        {
            Clipper clipper = new Clipper();
            clipper.AddPath(polygon, PolyType.ptSubject, true);

            foreach (var p in polygons)
            {
                clipper.AddPath(p.GetPolygon(), PolyType.ptSubject, true);
            }

            var result = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, result, PolyFillType.pftNonZero);

            if (result.Count == 0)
                return this;

            return new Area(result[0], Owner);
        }

        public override string ToString()
        {
            return $"{Owner}: {polygon.ToRectangle()}";
        }
    }
}
