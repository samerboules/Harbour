using QSim.ConsoleApp.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QSim.ConsoleApp.Simulators.SCRouterSystem
{
    public static class ReorientRoute
    {
        public static void Reorient(List<RoutePoint> route)
        {
            if (route.Count < 2)
            {
                return;
            }

            for (int i = 1; i < route.Count; i++)
            {
                bool backwards = IsBackwards(route, i - 1);
                route[i].Position.phi = CalculateOrientation(route[i - 1].Position, route[i].Position, backwards);
            }
        }

        private static bool IsBackwards(List<RoutePoint> route, int index)
        {
            Position firstPoint = route[index].Position;
            Position secondPoint = route[index + 1].Position;
            return Position.InOpposingSemiCircles(firstPoint.phi, firstPoint.AngleTo(secondPoint));
        }

        private static double CalculateOrientation(Position from, Position to, bool backwards)
        {
            double orientation = from.AngleTo(to);
            if (backwards)
            {
                orientation += Math.PI;
            }
            return Position.NormaliseAngle(orientation);
        }
    }
}
