using System;
using System.Collections.Generic;
using System.Text;

namespace QSim.ConsoleApp.DataTypes
{
    public class Position
    {
        private const double MAX_ANGLE_DELTA = 5 * Math.PI / 1000;

        public const double EAST = 0.0;
        public const double SOUTH = 0.5 * Math.PI;
        public const double WEST = Math.PI;
        public const double NORTH = 1.5 * Math.PI;

        public static readonly Position Zero = new Position(0, 0, 0, 0);

        public int x, y, z;
        public double phi;

        public Position(int _x, int _y, int _z, double _phi)
        {
            x = _x;
            y = _y;
            z = _z;
            phi = _phi;
        }

        public override bool Equals(object othObj)
        {
            if (othObj is Position othPos)
            {
                return x == othPos.x &&
                       y == othPos.y &&
                       z == othPos.z &&
                       IsSameOrientation(othPos);
            }
            return false;
        }

        public bool IsEqualInPlane(Position other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, phi);
        }

        public override string ToString()
        {
            return $"(x={x}, y={y}, z={z}, phi={phi})";
        }

        public Position Copy()
        {
            return new Position(x, y, z, phi);
        }

        public int DistanceTo(Position other)
        {
            double dx = other.x - x;
            double dy = other.y - y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return (int)Math.Round(distance);
        }

        public double AngleTo(Position other)
        {
            double angle = Math.Atan2(other.y - y, other.x - x);
            return NormaliseAngle(angle);
        }

        public Position GetFurther(double distance)
        {
            return GetPositionAtCircle(phi, distance);
        }

        public Position GetPositionAtCircle(double angle, double radius)
        {
            Position pos = Copy();
            pos.x += (int)Math.Round(radius * Math.Cos(angle));
            pos.y += (int)Math.Round(radius * Math.Sin(angle));
            pos.z = 0;

            return pos;
        }

        public Position GetPositionTowards(Position other, int distance)
        {
            double fraction = distance / (double)DistanceTo(other);
            Position pos = Copy();
            pos.x += (int)Math.Round(fraction * (other.x - this.x));
            pos.y += (int)Math.Round(fraction * (other.y - this.y));
            pos.z += (int)Math.Round(fraction * (other.z - this.z));
            return pos;
        }

        public static bool InOpposingSemiCircles(double angle1, double angle2)
        {
            double delta = NormaliseAngle(angle2 - angle1);
            return delta > 0.5 * Math.PI && delta < 1.5 * Math.PI;
        }

        public bool IsNorthSouth
        {
            get { return IsInOrientationOrOpposite(NORTH); }
        }

        public bool IsEastWest
        {
            get { return IsInOrientationOrOpposite(EAST); }
        }

        public static double NormaliseAngle(double angle)
        {
            angle %= 2.0 * Math.PI;
            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }
            return angle;
        }

        public bool IsSameOrientation(Position other, double maxDelta = MAX_ANGLE_DELTA)
        {
            return IsInOrientation(other.phi, maxDelta);
        }

        public bool IsInOrientation(double otherPhi, double maxDelta = MAX_ANGLE_DELTA)
        {
            return IsSameAngle(phi, otherPhi, maxDelta);
        }

        public bool IsInOrientationOrOpposite(double otherPhi)
        {
            return IsInOrientation(otherPhi) || IsInOrientation(otherPhi + Math.PI);
        }

        public static bool IsSameAngle(double phi, double gamma, double maxDelta = MAX_ANGLE_DELTA)
        {
            double delta = Math.Abs(phi - gamma) % (2.0 * Math.PI);
            if (delta > Math.PI)
            {
                delta = 2.0 * Math.PI - delta;
            }
            return delta < maxDelta;
        }

        // Returns the projection of this position on the line from {from} to {to}.
        public Position ProjectOnLine(Position from, Position to)
        {
            double dx = to.x - from.x;
            double dy = to.y - from.y;
            double numerator = (this.x - from.x) * dx + (this.y - from.y) * dy;
            double denominator = dx * dx + dy * dy;
            double mu = numerator / denominator;
            int x = (int)(from.x + mu * dx);
            int y = (int)(from.y + mu * dy);
            return new Position(x, y, 0, 0);
        }

        // Returns the projection of this position on the line through {other}.
        public Position ProjectOn(Position other)
        {
            Position furher = other.GetFurther(1000);
            return ProjectOnLine(other, furher);
        }
    }
}
