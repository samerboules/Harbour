using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Simulators.SCRouterSystem;
using QSim.ConsoleApp.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Simulators
{
    public class SC : Simulator
    {
        private readonly SCRouter _router;

        public const int EQUIPMENT_WIDTH = 5000;
        public const int EQUIPMENT_LENGTH = Container.Length40ft + 200;

        private const int SPREADER_SPEED = 1;
        private const int MAXIMUM_DRIVING_SPEED = 4;
        private const int CORNER_DRIVING_SPEED = 2;
        private const int TWISTLOCK_DELAY = 4000;
        private const int TELESCOPE_DELAY = 3500;
        private const int MAX_SLIDE_DISTANCE = 1000;
        private const int CLAIM_SIZE = 4000;
        private const int CLAIM_TIMEOUT = 10000;

        private const int SPREADER_SAFETY_MARGIN = 500;
        private const int SPREADER_MAX = Container.DefaultHeight * 3 + SPREADER_SAFETY_MARGIN;
        private const int SPREADER_DRIVE = Container.DefaultHeight + SPREADER_SAFETY_MARGIN;

        public const int TURNING_RADIUS = 7000;

        private int claimTimeout;

        public SC(int id)
        {
            Id = PositionProvider.IndexToId("SC", id);
            NumericId = PositionProvider.IndexToNumericId(id);

            EquipmentWidth = EQUIPMENT_WIDTH;
            EquipmentLength = EQUIPMENT_LENGTH;

            claimTimeout = (int)(CLAIM_TIMEOUT / _multiplier);

            _router = new SCRouter();
            _lastPosition = PositionProvider.GetEquipmentPosition(Id);
            _lastSpreaderPosition = new Position(0, 0, Container.DefaultHeight * 1, 0);

            _ = MoveSpreader(0, _lastSpreaderPosition.z);
            _ = MoveTo(_lastPosition);
            Guid? guid = _areaControl.RequestAccess(OccupiedArea, Id).Result;
            _lastClaimedArea = guid.Value;
        }

        public async Task MoveTo(Position position, int dt = 10)
        {
            await _bridge.Update(Id, ObjectType.AUTOSTRAD, position, dt);
            Status.Drive(_lastPosition.DistanceTo(position), dt);
            _lastPosition = position;
        }

        public async Task<bool> DriveTo(Position position, bool lowerSpreader)
        {
            if (lowerSpreader)
            {
                _ = SpreaderDriveHeight();
            }

            return await DriveTo(position);
        }

        public override async Task<bool> DriveTo(Position position)
        {
            _log.Debug($"{Id} drive to {position}");
            var route = _router.GetRoute(_lastPosition, position);
            route = SplitLongSegments(route, MAX_SLIDE_DISTANCE, CLAIM_SIZE);

            int nextIndex = 0;
            while (nextIndex < route.Count)
            {
                nextIndex = await DriveOneClaim(route, nextIndex);
                if (nextIndex < 0)
                {
                    // Failed to obtain claim.
                    break;
                }
            }

            Guid? finalClaimId = _areaControl.RequestAccess(OccupiedArea, Id).Result;
            _areaControl.RelinquishAccess(_lastClaimedArea);
            _lastClaimedArea = finalClaimId.Value;

            return nextIndex == route.Count;
        }

        private async Task<int> DriveOneClaim(List<RoutePoint> route, int startIndex)
        {
            //int lastIndex = GetLastCornerPointIndex(route, startIndex);

            // Only stop on a highway.
            int lastIndex = startIndex;
            if (PositionProvider.GetHighway(route[startIndex].Position) == null)
            {
                for (; lastIndex < route.Count - 1; lastIndex++)
                {
                    if (PositionProvider.GetHighway(route[lastIndex].Position) != null)
                    {
                        break;
                    }
                }
            }
            _log.Debug($"{Id} startIndex={startIndex}, lastIndex={lastIndex}");

            // Claim one ahead.
            int claimUntil = lastIndex == route.Count - 1 ? lastIndex : lastIndex + 1;

            Guid? claimId = await ClaimMultiple(route, startIndex, claimUntil);
            if (!claimId.HasValue)
            {
                _log.Debug($"{Id} failed to get claim");
                return -1;
            }
            _log.Debug($"{Id} claim id: {claimId.Value.ToString()}");

            for (int currentIndex = startIndex; currentIndex <= lastIndex; currentIndex++)
            {
                Position nextPosition = route[currentIndex].Position;
                _log.Debug($"{Id} driving to route point {currentIndex} {nextPosition}");
                bool corner = route.Count > currentIndex + 1 && !nextPosition.IsSameOrientation(route[currentIndex + 1].Position);
                int speed = corner ? CORNER_DRIVING_SPEED : MAXIMUM_DRIVING_SPEED;
                int delay = (int)(_lastPosition.DistanceTo(nextPosition) / (speed * _multiplier));
                await MoveTo(nextPosition, delay);
                await Task.Delay(delay);
            }

            _areaControl.RelinquishAccess(_lastClaimedArea);
            _lastClaimedArea = claimId.Value;

            return lastIndex + 1;
        }

        private List<RoutePoint> SplitLongSegments(List<RoutePoint> route, int maxSlideDistance, int maxDistance)
        {
            if (maxSlideDistance > maxDistance)
            {
                maxSlideDistance = maxDistance;
            }
            List<RoutePoint> stopPoints = new List<RoutePoint>();
            RoutePoint previousPoint = route[0];
            stopPoints.Add(previousPoint);
            for (int i = 1; i < route.Count; i++)
            {
                RoutePoint nextPoint = route[i];
                if (!previousPoint.Position.IsSameOrientation(nextPoint.Position) && previousPoint.Position.DistanceTo(nextPoint.Position) > maxSlideDistance)
                {
                    // Insert a route point with the final orientation to prevent driving sideways.
                    Position intermediatePosition = previousPoint.Position.GetPositionTowards(nextPoint.Position, maxSlideDistance);
                    intermediatePosition.phi = nextPoint.Position.phi;
                    RoutePoint intermediatePoint = new RoutePoint(intermediatePosition);
                    stopPoints.Add(intermediatePoint);
                    previousPoint = intermediatePoint;
                }
                while (previousPoint.Position.DistanceTo(nextPoint.Position) > maxDistance)
                {
                    Position intermediatePosition = previousPoint.Position.GetPositionTowards(nextPoint.Position, maxDistance);
                    intermediatePosition.phi = nextPoint.Position.phi;
                    RoutePoint intermediatePoint = new RoutePoint(intermediatePosition);
                    stopPoints.Add(intermediatePoint);
                    previousPoint = intermediatePoint;
                }
                stopPoints.Add(nextPoint);
                previousPoint = nextPoint;
            }

            return stopPoints;
        }

        private static int GetLastCornerPointIndex(List<RoutePoint> route, int startIndex)
        {
            int lastCornerPointIndex = startIndex;
            for (; lastCornerPointIndex + 1 < route.Count; lastCornerPointIndex++)
            {
                if (route[lastCornerPointIndex + 1].Position.IsSameOrientation(route[lastCornerPointIndex].Position))
                {
                    break;
                }
            }
            return lastCornerPointIndex;
        }

        private async Task<Guid?> ClaimMultiple(List<RoutePoint> route, int fromIndex, int toIndex)
        {
            List<Area> claimAreas = new List<Area>();
            for (int i = fromIndex; i <= toIndex; i++)
            {
                claimAreas.Add(new Area(GetOccupiedAreaAt(route[i].Position), ""));
            }
            Area area = claimAreas[0].Union(claimAreas.ToArray());
            return await _areaControl.RequestAccess(area.GetPolygon(), Id, claimTimeout);
        }

        public override async Task MoveSpreader(int trolley, int height)
        {
            int delta = Math.Abs(_lastSpreaderPosition.z - height);
            int delay = (int)(delta / (SPREADER_SPEED * _multiplier));

            await _bridge.SpreaderUpdate(Id, new Position(0, 0, height, 0), delay);
            await Task.Delay(delay);

            _lastSpreaderPosition = new Position(0, 0, height, 0);
        }

        public override async Task<bool> PickUp(Location location, Container container)
        {
            Position position = GetPosition(location);
            _log.Debug($"{Id} picking up {container.Number} from {location}");

            await SpreaderPickupHeight(location);

            if (_lastPosition.x != position.x)
            {
                Position prePosition = position.Copy();
                prePosition.x += (_lastPosition.x > position.x ? 1 : -1) * (Container.Length40ft + SC.EQUIPMENT_LENGTH) / 2;
                prePosition.z = 0;
                if (!await DriveTo(prePosition))
                {
                    return false;
                }
            }
            if (!await DriveTo(new Position(position.x, position.y, 0, position.phi)))
            {
                return false;
            }

            await SetSpreaderSize(container.GetSpreaderSize());
            await MoveSpreader(0, position.z + Container.DefaultHeight);
            await Task.Delay((int)(TWISTLOCK_DELAY / _multiplier));
            await _bridge.PickUp(Id, container.Number);
            _stacking.PickContainer(location, container.Number);
            await SpreaderPickupHeight(location);

            return true;
        }

        public override async Task SetSpreaderSize(SpreaderSize spreaderSize, int dt = TELESCOPE_DELAY)
        {
            if (spreaderSize == _lastSpreaderSize)
                dt = 10;

            var delay = (int)(dt / _multiplier);

            await base.SetSpreaderSize(spreaderSize, delay);
            await Task.Delay(delay);
        }

        public override async Task<bool> PutDown(Location location, Container container)
        {
            Position position = GetPosition(location);
            _log.Debug($"{Id} putting down {container.Number} at {location}");

            if (!await DriveTo(new Position(position.x, position.y, 0, position.phi)))
            {
                return false;
            }

            await MoveSpreader(0, position.z + Container.DefaultHeight);
            await Task.Delay((int)(TWISTLOCK_DELAY / _multiplier));
            await _bridge.PutDown(Id, container.Number, position);
            _stacking.PutContainer(location, container.Number);
            Status.PutDownContainer();
            await SpreaderDriveHeight();

            return true;
        }

        public static Position GetQctpPrePosition(int qcId)
        {
            Position lane1Position = PositionProvider.GetPosition(new Location(LocationType.QCTP, qcId, 0, 1, 0));
            Position lane2Position = PositionProvider.GetPosition(new Location(LocationType.QCTP, qcId, 0, 2, 0));

            const int HALF_CONTAINER_LENGTH = Container.Length40ft / 2;
            const int HALF_EQUIPMENT_LENGTH = EQUIPMENT_LENGTH / 2;
            int sbendLength = new SCRouter().GetSBendLength(lane2Position, lane1Position);

            Position prePosition = lane2Position.Copy();
            prePosition.x -= HALF_CONTAINER_LENGTH + HALF_EQUIPMENT_LENGTH + sbendLength;
            return prePosition;
        }

        public static Position GetQctpPostPosition(int qcId, int laneNumber)
        {
            Location qctpLocation = new Location(LocationType.QCTP, qcId, 0, laneNumber, 0);
            Position qctpPostPosition = PositionProvider.GetPosition(qctpLocation).Copy();

            const int MARGIN = 500;
            qctpPostPosition.x += (QC.EQUIPMENT_LENGTH + SC.EQUIPMENT_LENGTH) / 2 + MARGIN;

            return qctpPostPosition;
        }

        private async Task SpreaderSafeHeight()
        {
            await MoveSpreader(0, SPREADER_MAX);
        }

        private async Task SpreaderPickupHeight(Location location)
        {
            await MoveSpreader(0, SPREADER_SAFETY_MARGIN + Container.DefaultHeight * (location.floor + 1));
        }

        private async Task SpreaderDriveHeight()
        {
            await MoveSpreader(0, SPREADER_DRIVE);
        }

        public override void SetMultiplier(double multiplier)
        {
            base.SetMultiplier(multiplier);
            claimTimeout = (int)(CLAIM_TIMEOUT / multiplier);
        }
    }
}
