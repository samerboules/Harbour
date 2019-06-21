using QSim.ConsoleApp.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QSim.ConsoleApp.Simulators.SCRouterSystem
{
    public class RoutePoint
    {
        public Position Position;

        public RoutePoint(Position position)
        {
            Position = position;
        }
    }
}
