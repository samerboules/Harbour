using QSim.ConsoleApp.Utilities.Clipper;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace QSim.ConsoleApp.Utilities
{
    public static class ExtensionMethods
    {
        public static List<IntPoint> ToIntPoints(this Rectangle r)
        {
            return new List<IntPoint>()
            {
                new IntPoint(r.X, r.Y),
                new IntPoint(r.X + r.Width, r.Y),
                new IntPoint(r.X + r.Width, r.Y + r.Height),
                new IntPoint(r.X, r.Y + r.Height)
            };
        }

        public static Rectangle ToRectangle(this List<IntPoint> points)
        {
            long xMin = Int32.MaxValue;
            long yMin = Int32.MaxValue;
            long xMax = Int32.MinValue;
            long yMax = Int32.MinValue;

            foreach (var point in points)
            {
                xMin = Math.Min(xMin, point.X);
                yMin = Math.Min(yMin, point.Y);
                xMax = Math.Max(xMax, point.X);
                yMax = Math.Max(yMax, point.Y);
            }

            var result = Rectangle.FromLTRB((int)xMin, (int)yMin, (int)xMax, (int)yMax);
            return result;
        }
    }
}
