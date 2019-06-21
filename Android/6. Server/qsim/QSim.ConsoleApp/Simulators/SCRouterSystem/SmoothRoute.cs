using QSim.ConsoleApp.DataTypes;
using System;
using System.Collections.Generic;

namespace QSim.ConsoleApp.Simulators.SCRouterSystem
{
    public static class SmoothRoute
    {
        public static List<RoutePoint> GetSmoothRoute(List<RoutePoint> routePoints, int radius)
        {
            if (routePoints.Count < 3)
            {
                return routePoints;
            }

            var result = new List<RoutePoint>();
            result.Add(routePoints[0]);

            for (int i = 0; i < routePoints.Count - 2; i++)
            {
                Position currentPoint = routePoints[i].Position;
                Position nextPoint = routePoints[i + 1].Position;
                Position nextNextPoint = routePoints[i + 2].Position;

                if ((nextNextPoint.x == currentPoint.x &&
                    nextNextPoint.x == nextPoint.x) ||
                    (nextNextPoint.y == currentPoint.y &&
                    nextNextPoint.y == nextPoint.y))
                    continue;

                var roundPoints = GetRoundSegment(nextPoint, currentPoint, nextNextPoint, (float)radius);
                foreach (var point in roundPoints)
                {
                    result.Add(new RoutePoint(point));
                }
            }

            result.Add(routePoints[routePoints.Count - 1]);
            return result;
        }

        private static List<Position> GetRoundSegment(Position angularPoint, Position p1, Position p2, float radius)
        {
            var result = new List<Position>();
            //Vector 1
            double dx1 = angularPoint.x - p1.x;
            double dy1 = angularPoint.y - p1.y;

            //Vector 2
            double dx2 = angularPoint.x - p2.x;
            double dy2 = angularPoint.y - p2.y;

            //Angle between vector 1 and vector 2 divided by 2
            double angle = (Math.Atan2(dy1, dx1) - Math.Atan2(dy2, dx2)) / 2;

            // The length of segment between angular point and the
            // points of intersection with the circle of a given radius
            double tan = Math.Abs(Math.Tan(angle));
            double segment = radius / tan;

            //Check the segment
            double length1 = GetLength(dx1, dy1);
            double length2 = GetLength(dx2, dy2);

            double length = Math.Min(length1, length2);

            if (segment > length)
            {
                segment = length;
                radius = (float)(length * tan);
            }

            // Points of intersection are calculated by the proportion between 
            // the coordinates of the vector, length of vector and the length of the segment.
            var p1Cross = GetProportionPoint(angularPoint, segment, length1, dx1, dy1, p1.phi);
            var p2Cross = GetProportionPoint(angularPoint, segment, length2, dx2, dy2, p2.phi);

            // Calculation of the coordinates of the circle 
            // center by the addition of angular vectors.
            double dx = angularPoint.x * 2 - p1Cross.x - p2Cross.x;
            double dy = angularPoint.y * 2 - p1Cross.y - p2Cross.y;

            double L = GetLength(dx, dy);
            double d = GetLength(segment, radius);

            var circlePoint = GetProportionPoint(angularPoint, d, L, dx, dy, 0);

            //StartAngle and EndAngle of arc
            var startAngle = Math.Atan2(p1Cross.y - circlePoint.y, p1Cross.x - circlePoint.x);
            var endAngle = Math.Atan2(p2Cross.y - circlePoint.y, p2Cross.x - circlePoint.x);
            if (startAngle < 0)
            {
                startAngle += 2 * Math.PI;
            }
            if (endAngle < 0)
            {
                endAngle += 2 * Math.PI;
            }
            var sweepAngle = endAngle - startAngle;
            bool reverse = false;
            if (sweepAngle < 0)
            {
                var swap = endAngle;
                endAngle = startAngle;
                startAngle = swap;
                sweepAngle = endAngle - startAngle;
                reverse = true;
            }
            if (sweepAngle > Math.PI)
            {
                var swap = endAngle;
                endAngle = startAngle;
                startAngle = swap;
                sweepAngle = 2 * Math.PI - sweepAngle;
                reverse = !reverse;
            }

            var arcPoints = GetArcPoints(circlePoint, radius, sweepAngle, startAngle, p1.phi);
            if (reverse)
            {
                arcPoints.Reverse();
            }

            result.Add(p1Cross);
            result.AddRange(arcPoints);
            result.Add(p2Cross);

            return result;
        }

        private static double GetLength(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static Position GetProportionPoint(Position point, double segment,
                                          double length, double dx, double dy, double phi)
        {
            double factor = segment / length;

            return new Position((int)(point.x - dx * factor),
                                (int)(point.y - dy * factor),
                                0, phi);
        }

        private static List<Position> GetArcPoints(Position circlePoint, double radius, double sweepAngle, double startAngle, double startPhi)
        {
            const double RADIANS_PER_POINT = 4 * Math.PI / 180;

            int pointsCount = (int)Math.Abs(sweepAngle / RADIANS_PER_POINT);
            int sign = Math.Sign(sweepAngle);

            var points = new List<Position>();
            for (int i = 1; i < pointsCount; ++i)
            {
                var angle = startAngle + (double)i * sign * RADIANS_PER_POINT;
                var pointX = (int)(circlePoint.x + Math.Cos(angle) * radius);
                var pointY = (int)(circlePoint.y + Math.Sin(angle) * radius);
                double orientation = startPhi + (double)i * sign * RADIANS_PER_POINT;

                points.Add(new Position(pointX, pointY, 0, orientation));
            }

            return points;
        }
    }
}
