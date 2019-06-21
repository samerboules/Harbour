using System;
using QSim.ConsoleApp.DataTypes;
using System.Collections.Generic;
using log4net;
using QSim.ConsoleApp.Utilities;
using PP = QSim.ConsoleApp.Utilities.PositionProvider;
using System.Linq;

namespace QSim.ConsoleApp.Simulators.SCRouterSystem
{
    public class SCRouter
    {
        // This value is implicitly used in the unit tests.  Change it at your own peril.
        private const int RIGHT_ANGLE_DISTANCE = SC.TURNING_RADIUS + 500;

        private readonly ILog _log;

        public SCRouter()
        {
            _log = LogManager.GetLogger(GetType());
        }

        public List<RoutePoint> GetRoute(Position from, Position to)
        {
            List<RoutePoint> route = GetRawRoute(from, to);
            route = DeduplicateRoute(route);
            route = SmoothRoute.GetSmoothRoute(route, SC.TURNING_RADIUS);
            route = DeduplicateRoute(route);
            ReorientRoute.Reorient(route);
            return route;
        }

        private List<RoutePoint> GetRawRoute(Position from, Position to)
        {
            if (from.Equals(to))
            {
                return GetEmptyRoute(from);
            }
            if (PP.IsInQCArea(from))
            {
                return GetFromQcRoute(from, to);
            }
            if (PP.IsInWstpArea(from))
            {
                return GetFromWstpRoute(from, to);
            }
            return GetHighwayRoute(from, to);
        }

        private List<RoutePoint> GetFromQcRoute(Position from, Position to)
        {
            if (PP.IsInQCArea(to))
            {
                return GetQcToQcRoute(from, to);
            }
            if (PP.IsInWstpArea(to))
            {
                return GetQcToWstpRoute(from, to);
            }
            return GetHighwayRoute(from, to);
        }

        private List<RoutePoint> GetFromWstpRoute(Position from, Position to)
        {
            if (PP.IsInWstpArea(to))
            {
                return GetWstpToWstpRoute(from, to);
            }
            if (PP.IsInQCArea(to))
            {
                return GetWstpToQcRoute(from, to);
            }
            return GetHighwayRoute(from, to);
        }

        public List<RoutePoint> GetQcToQcRoute(Position from, Position to)
        {
            if (!from.IsEastWest || !to.IsEastWest || Math.Abs(to.x - from.x) >= 4 * RIGHT_ANGLE_DISTANCE)
            {
                return GetHighwayRoute(from, to);
            }
            return GetSBendAtFrontRoute(from, to);
        }

        public List<RoutePoint> GetQcToWstpRoute(Position from, Position to)
        {
            if (!from.IsEastWest|| !to.IsNorthSouth)
            {
                return GetHighwayRoute(from, to);
            }

            int deltaX = to.x - from.x;
            if (deltaX >= 3 * RIGHT_ANGLE_DISTANCE)
            {
                return GetHighwayRoute(from, to);
            }

            var route = new List<RoutePoint>();
            route.Add(new RoutePoint(from));

            Position corner = from.Copy();
            corner.x += RIGHT_ANGLE_DISTANCE;
            route.Add(new RoutePoint(corner));

            if (deltaX == RIGHT_ANGLE_DISTANCE)
            {
                route.Add(new RoutePoint(to));
                return route;
            }
            if (deltaX < -RIGHT_ANGLE_DISTANCE)
            {
                route.AddRange(GetHighwayRoute(corner, to));
                return route;
            }

            Position sbendStart = corner.Copy();
            sbendStart.y += RIGHT_ANGLE_DISTANCE;
            route.AddRange(GetSBendAtFrontRoute(sbendStart, to));
            return route;
        }

        public List<RoutePoint> GetWstpToQcRoute(Position from, Position to)
        {
            if (!from.IsNorthSouth || !to.IsEastWest)
            {
                return GetHighwayRoute(from, to);
            }

            int distance = Math.Abs(to.x - from.x);
            if (distance >= 3 * RIGHT_ANGLE_DISTANCE)
            {
                return GetHighwayRoute(from, to);
            }

            int sign = Math.Sign(to.x - from.x);
            Position corner = to.Copy();
            corner.x -= sign * RIGHT_ANGLE_DISTANCE;

            if (distance == RIGHT_ANGLE_DISTANCE)
            {
                return new List<RoutePoint>()
                {
                    new RoutePoint(from),
                    new RoutePoint(corner),
                    new RoutePoint(to)
                };
            }

            Position sbendEnd = corner.Copy();
            sbendEnd.y += RIGHT_ANGLE_DISTANCE;
            var sbendRoute = GetSBendAtTailRoute(from, sbendEnd);
            var route = new List<RoutePoint>();
            route.AddRange(sbendRoute);
            route.Add(new RoutePoint(corner));
            route.Add(new RoutePoint(to));
            return route;
        }

        public List<RoutePoint> GetWstpToWstpRoute(Position from, Position to)
        {
            if (!from.IsNorthSouth || !to.IsNorthSouth)
            {
                return GetHighwayRoute(from, to);
            }
            int xDistance = Math.Abs(to.x - from.x);
            if (xDistance == 0)
            {
                return GetDirectRoute(from, to);
            }

            double orientation = to.x > from.x ? Position.EAST : Position.WEST;
            Highway highway = PP.GetHighwayInOrientation(orientation);

            var entryPosition = from.Copy();
            entryPosition.y = highway.from.y;
            var exitPosition = to.Copy();
            exitPosition.y = highway.from.y;
            if (xDistance >= 2 * RIGHT_ANGLE_DISTANCE)
            {
                return new List<RoutePoint>
                {
                    new RoutePoint(from),
                    new RoutePoint(entryPosition),
                    new RoutePoint(exitPosition),
                    new RoutePoint(to)
                };
            }

            // Enter the highway in the wrong direction, then back up to the destination.
            var stabPosition = entryPosition.Copy();
            stabPosition.x += to.x > from.x ? -RIGHT_ANGLE_DISTANCE : RIGHT_ANGLE_DISTANCE;
            return new List<RoutePoint>
                {
                    new RoutePoint(from),
                    new RoutePoint(entryPosition),
                    new RoutePoint(stabPosition),
                    new RoutePoint(exitPosition),
                    new RoutePoint(to)
                };
        }

        private List<RoutePoint> GetEmptyRoute(Position from)
        {
            var route = new List<RoutePoint>();
            route.Add(new RoutePoint(from));
            return route;
        }

        // Returns a direct route between the two positions.  The orientation of the each position is expected to
        // be equal or opposite to the orientation of the line between them.
        public List<RoutePoint> GetDirectRoute(Position from, Position to)
        {
            var route = new List<RoutePoint>();
            route.Add(new RoutePoint(from));
            route.Add(new RoutePoint(to));
            return route;
        }

        // Returns the distance, in the orientation of from and to, required for an s-bend.
        public int GetSBendLength(Position from, Position to)
        {
            Position projected = from.ProjectOn(to);
            double crossDistance = from.DistanceTo(projected);
            if (crossDistance == 0)
            {
                return 0;
            }
            if (crossDistance >= 2 * RIGHT_ANGLE_DISTANCE)
            {
                return 2 * RIGHT_ANGLE_DISTANCE;
            }

            double alpha = Math.Acos(1 - crossDistance / (2 * RIGHT_ANGLE_DISTANCE));
            double distance = 2 * RIGHT_ANGLE_DISTANCE * Math.Sin(alpha);
            return (int)Math.Round(distance);
        }

        // Returns an s-bend route between the two positions.  The s-bend is made at the to position.
        // The positions are expected to have equal or opposite orientations.
        public List<RoutePoint> GetSBendAtTailRoute(Position from, Position to)
        {
            var route = GetSBendAtFrontRoute(to, from);
            route.Reverse();
            return route;
        }

        // Returns an s-bend route between the two positions.  The s-bend is made at the from position.
        // The positions are expected to have equal or opposite orientations.
        public List<RoutePoint> GetSBendAtFrontRoute(Position from, Position to)
        {
            Position projected = from.ProjectOn(to);
            double crossDistance = from.DistanceTo(projected);
            if (crossDistance == 0)
            {
                return GetDirectRoute(from, to);
            }
            if (crossDistance >= 2 * RIGHT_ANGLE_DISTANCE)
            {
                return GetStraightSBendRoute(from, to);
            }

            double alpha = Math.Acos(1 - crossDistance / (2 * RIGHT_ANGLE_DISTANCE));
            double distance = 2 * RIGHT_ANGLE_DISTANCE * Math.Sin(alpha);
            double angle = to.phi;
            if (Position.InOpposingSemiCircles(angle, from.AngleTo(to)))
            {
                angle += Math.PI;
            }
            Position to2 = projected.GetPositionAtCircle(angle, distance);
            Position halfway = from.GetPositionTowards(projected, (int)Math.Round(crossDistance / 2));
            Position mid = halfway.GetPositionAtCircle(angle, distance / 2);

            double extraR = RIGHT_ANGLE_DISTANCE * (1 / Math.Cos(alpha / 2) - 1);
            double m = (RIGHT_ANGLE_DISTANCE + extraR) * Math.Sin(alpha / 2);
            Position corner1 = from.GetPositionAtCircle(angle, m);
            Position corner2 = to2.GetPositionAtCircle(Math.PI + angle, m);

            var route = new List<RoutePoint>();
            route.Add(new RoutePoint(from));
            route.Add(new RoutePoint(corner1));
            route.Add(new RoutePoint(mid));
            route.Add(new RoutePoint(corner2));
            route.Add(new RoutePoint(to2));
            route.Add(new RoutePoint(to));
            return route;
        }

        // Not really an s-bend, but rather two 90 degrees corners.
        private List<RoutePoint> GetStraightSBendRoute(Position from, Position to)
        {
            double angle = to.phi;
            if (Position.InOpposingSemiCircles(angle, from.AngleTo(to)))
            {
                angle += Math.PI;
            }
            Position corner1 = from.GetPositionAtCircle(angle, RIGHT_ANGLE_DISTANCE);
            Position corner2 = corner1.ProjectOn(to);

            var route = new List<RoutePoint>();
            route.Add(new RoutePoint(from));
            route.Add(new RoutePoint(corner1));
            route.Add(new RoutePoint(corner2));
            route.Add(new RoutePoint(to));
            return route;
        }

        private List<RoutePoint> GetHighwayRoute(Position from, Position to)
        {
            List<RoutePoint> route = new List<RoutePoint>();

            Highway lastHighway = null;
            for (Highway highway = DetermineInitialHighway(from, to); highway != null; highway = DetermineNextHighway(highway, to))
            {
                var subroute = lastHighway == null ? EnterHighway(from, highway) : SwitchHighway(lastHighway, highway);
                route.AddRange(subroute);
                lastHighway = highway;
            }
            route.AddRange(LeaveHighway(lastHighway, to));

            return route;
        }

        private Highway DetermineInitialHighway(Position from, Position to)
        {
            double orientation;
            if (PP.IsInSCParkArea(from))
            {
                orientation = to.y > from.y ? PP.SOUTH : PP.NORTH;
            }
            else
            {
                orientation = to.x > from.x ? PP.EAST : PP.WEST;
            }
            return PP.GetHighwayInOrientation(orientation);
        }

        private Highway DetermineNextHighway(Highway fromHighway, Position to)
        {
            double orientation;
            if (PP.IsInSCParkArea(to))
            {
                if (fromHighway.IsInOrientation(PP.NORTH) || fromHighway.IsInOrientation(PP.SOUTH))
                {
                    return null;
                }
                orientation = to.y > fromHighway.from.y ? PP.SOUTH : PP.NORTH;
            }
            else
            {
                if (fromHighway.IsInOrientation(PP.EAST) || fromHighway.IsInOrientation(PP.WEST))
                {
                    return null;
                }
                orientation = to.x > fromHighway.from.x ? PP.EAST : PP.WEST;
            }
            return PP.GetHighwayInOrientation(orientation);
        }

        private List<RoutePoint> EnterHighway(Position from, Highway highway)
        {
            var route = new List<RoutePoint>();
            route.Add(new RoutePoint(from));
            if (highway.IsOnHighway(from))
            {
                return route;
            }

            // The test on IsInQCArea is to 'ensure' that the carrier does not drive through containers.
            double departureOrientation = PP.IsInQCArea(from) ? PP.EAST : highway.orientation;
            Position cross = from.ProjectOnLine(highway.from, highway.to);
            int distance = from.DistanceTo(cross);

            if (from.IsInOrientationOrOpposite(highway.orientation))
            {
                // more or less the same orientation
                if (distance > 2 * RIGHT_ANGLE_DISTANCE)
                {
                    // two straight turns
                    Position corner1 = from.GetPositionAtCircle(departureOrientation, RIGHT_ANGLE_DISTANCE);
                    Position corner2 = corner1.ProjectOnLine(highway.from, highway.to);
                    route.Add(new RoutePoint(corner1));
                    route.Add(new RoutePoint(corner2));
                }
                else
                {
                    // s-bend
                    Position entry = cross.GetPositionAtCircle(departureOrientation, 2 * RIGHT_ANGLE_DISTANCE);
                    entry.phi = highway.orientation;
                    route.AddRange(GetSBendAtFrontRoute(from, entry));
                }
            }
            else
            {
                // not the same orientation, assume right angle
                if (distance > RIGHT_ANGLE_DISTANCE)
                {
                    // straight to the highway
                    route.Add(new RoutePoint(cross));
                }
                else
                {
                    // too close to the highway, back up a bit first
                    Position corner1 = cross.GetPositionAtCircle(from.phi, RIGHT_ANGLE_DISTANCE);
                    Position corner2 = cross.GetPositionAtCircle(Math.PI + from.phi, RIGHT_ANGLE_DISTANCE);
                    Position corner = from.DistanceTo(corner1) < from.DistanceTo(corner2) ? corner1 : corner2;
                    Position entry = cross.GetPositionAtCircle(departureOrientation, RIGHT_ANGLE_DISTANCE);
                    route.Add(new RoutePoint(corner));
                    route.Add(new RoutePoint(cross));
                    route.Add(new RoutePoint(entry));
                }
            }

            route[route.Count - 1].Position.phi = highway.orientation;

            return route;
        }

        private List<RoutePoint> SwitchHighway(Highway fromHighway, Highway toHighway)
        {
            int x;
            int y;
            if (fromHighway.IsInOrientation(PP.EAST) || fromHighway.IsInOrientation(PP.WEST))
            {
                x = toHighway.from.x;
                y = fromHighway.from.y;
            }
            else
            {
                x = fromHighway.from.x;
                y = toHighway.from.y;
            }

            var route = new List<RoutePoint>();
            Position cross = new Position(x, y, 0, toHighway.orientation);
            route.Add(new RoutePoint(cross));
            return route;
        }

        private List<RoutePoint> LeaveHighway(Highway highway, Position to)
        {
            var route = new List<RoutePoint>();

            Position cross = to.ProjectOnLine(highway.from, highway.to);
            int distance = cross.DistanceTo(to);

            if (to.IsInOrientationOrOpposite(highway.orientation))
            {
                // more or less the same orientation
                if (distance > 2 * RIGHT_ANGLE_DISTANCE)
                {
                    // two straight turns
                    Position corner2 = to.GetPositionAtCircle(highway.orientation, -RIGHT_ANGLE_DISTANCE);
                    Position corner1 = corner2.ProjectOnLine(highway.from, highway.to);
                    route.Add(new RoutePoint(corner1));
                    route.Add(new RoutePoint(corner2));
                }
                else
                {
                    // s-bend
                    Position departure = cross.GetPositionAtCircle(highway.orientation, -2 * RIGHT_ANGLE_DISTANCE);
                    departure.phi = highway.orientation;
                    route.AddRange(GetSBendAtTailRoute(departure, to));
                }
            }
            else
            {
                // not the same orientation, assume right angle
                if (distance > RIGHT_ANGLE_DISTANCE)
                {
                    route.Add(new RoutePoint(cross));
                }
                else
                {
                    Position corner1 = cross.GetPositionAtCircle(to.phi, RIGHT_ANGLE_DISTANCE);
                    Position corner2 = cross.GetPositionAtCircle(Math.PI + to.phi, RIGHT_ANGLE_DISTANCE);
                    Position corner = corner1.DistanceTo(to) < corner2.DistanceTo(to) ? corner1 : corner2;
                    route.Add(new RoutePoint(cross));
                    route.Add(new RoutePoint(corner));
                }
            }

            route.Add(new RoutePoint(to));
            return route;
        }

        private List<RoutePoint> DeduplicateRoute(List<RoutePoint> route)
        {
            const double MINIMUM_DISTANCE = 10.0;

            List<RoutePoint> result = new List<RoutePoint>();
            result.Add(route[0]);
            for (int i = 1; i < route.Count - 1; i++)
            {
                if (route[i - 1].Position.DistanceTo(route[i].Position) >= MINIMUM_DISTANCE)
                {
                    result.Add(route[i]);
                }
            }
            if (route.Count > 1)
            {
                result.Add(route[route.Count - 1]);
            }
            return result;
        }

        private void DumpRoute(string title, List<RoutePoint> route)
        {
            _log.Info($"Point dump for {title} containing {route.Count} points.");
            foreach (var point in route)
            {
                _log.Info($"{point.Position.x} {point.Position.y} {point.Position.phi}");
            }
        }
    }
}
