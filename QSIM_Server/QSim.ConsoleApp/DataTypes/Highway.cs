using System;
using System.Collections.Generic;
using System.Text;

namespace QSim.ConsoleApp.DataTypes
{
    public class Highway
    {
        public int number;
        public double orientation;
        public Position from;
        public Position to;

        public Highway(int number, Position from, Position to, double orientation)
        {
            this.number = number;
            this.orientation = orientation;
            this.from = new Position(from.x, from.y, 0, orientation);
            this.to = new Position(to.x, to.y, 0, orientation);
        }

        public override string ToString()
        {
            return $"(number={number}, orientation={orientation}, from={from}, to={to})";
        }

        public bool IsInOrientation(double orientation)
        {
            return Position.IsSameAngle(this.orientation, orientation);
        }

        public bool IsOnHighway(Position position, int maxDistance = 10)
        {
            if (!position.IsInOrientationOrOpposite(orientation))
            {
                return false;
            }
            double dx = to.x - from.x;
            double dy = to.y - from.y;
            double numerator = (position.x - from.x) * dx + (position.y - from.y) * dy;
            double denominator = dx * dx + dy * dy;
            double mu = numerator / denominator;
            if (mu < 0 || mu > 1)
            {
                return false;
            }
            Position cross = new Position((int)(from.x + mu * dx), (int)(from.y + mu * dy), 0, 0);
            return cross.DistanceTo(position) < maxDistance;
        }
    }
}
