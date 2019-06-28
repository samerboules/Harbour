using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Utilities;
using QSim.ConsoleApp.Utilities.Clipper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Simulators
{
    public class QC : Simulator
    {
        private const float GANTRY_SPEED    = 0.4f;
        private const float HOIST_SPEED     = 2.5f;
        private const float TROLLEY_SPEED   = 3.2f;
        private const int   TROLLEY_MIN     = -50000;
        private const int   TROLLEY_MAX     = 46800;
        private const int   SPREADER_MIN    = -15000;
        private const int   SPREADER_MAX    = 32500;
        private const int   TWISTLOCK_DELAY = 5000;
        private const int   TELESCOPE_DELAY = 5000;
        private const int   MARGIN          = 7000;
        private const int   SAFE_HEIGHT     = 6 * Container.DefaultHeight;
        private const int   GANTRY_CLAIM_LENGTH = 27500;
        private const int   GANTRY_CLAIM_WIDTH  = 2000;

        public const int EQUIPMENT_WIDTH = 15000;
        public const int EQUIPMENT_LENGTH = Container.Length40ft + 300;

        public int BayId { get; private set; }

        public QC(int id)
        {
            Id = PositionProvider.IndexToId("QC", id);
            NumericId = PositionProvider.IndexToNumericId(id);
            BayId = PositionProvider.Bays[id];

            // Not the actual equipment dimensions. Only used for QCTP Area control
            EquipmentWidth = EQUIPMENT_WIDTH;
            EquipmentLength = EQUIPMENT_LENGTH;

            _lastPosition = PositionProvider.GetEquipmentPosition(Id);

            Guid? guid = _areaControl.RequestAccess(GetGantryClaimAt(_lastPosition), Id).Result;
            _lastClaimedArea = guid.Value;

            _ = MoveTo(_lastPosition);
            _ = MoveSpreaderTo(0, SPREADER_MAX);

            PositionProvider.UpdateQctpPositions(NumericId, _lastPosition.x);
        }

        private async Task MoveTo(Position position, int delay = 10)
        {
            await _bridge.Update(Id, ObjectType.QC, position, delay);
            await Task.Delay(delay);
            _lastPosition = position;
        }

        private async Task MoveSpreaderTo(int trolley, int height, int delay = 10)
        {
            Position spreaderPos = new Position(0, trolley, height, 0);
            await _bridge.SpreaderUpdate(Id, spreaderPos, delay);
            await Task.Delay(delay);
            _lastSpreaderPosition = spreaderPos;
        }

        private async Task Hoist(int height)
        {
            int hoistDelay = (int)(Math.Abs(_lastSpreaderPosition.z - height) / (HOIST_SPEED * _multiplier));
            await MoveSpreaderTo(_lastSpreaderPosition.y, height, hoistDelay);
        }

        private async Task Trolley(int trolley)
        {
            int trolleyDelay = (int)(Math.Abs(_lastSpreaderPosition.y - trolley) / (TROLLEY_SPEED * _multiplier));
            await MoveSpreaderTo(trolley, _lastSpreaderPosition.z, trolleyDelay);
        }

        public override async Task MoveSpreader(int trolley, int height)
        {
            if (!IsInMargin(trolley, _lastSpreaderPosition.y))
            {
                await Hoist(_stacking.GetSafeHoistHeight(BayId, LocationType.STOWAGE, SPREADER_MAX));
            }
            await Trolley(trolley);
            await Hoist(height);
        }

        public override async Task<bool> DriveTo(Position position)
        {
            if (position.x == _lastPosition.x)
            {
                return true;
            }
            await MoveSpreader(0, SPREADER_MAX);
            int delay = (int)(Math.Abs(_lastPosition.x - position.x) / (GANTRY_SPEED * _multiplier));
            await MoveTo(new Position(position.x, _lastPosition.y, _lastPosition.z, _lastPosition.phi), delay);

            return true;
        }

        public override async Task<bool> PickUp(Location location, Container container)
        {
            _log.Debug($"{Id} picking up {container.Number} from {location}");
            Position position = GetPosition(location);
            Position spreaderPosition = GetSpreaderPosition(location);
            await SetSpreaderSize(container.GetSpreaderSize(), (int)(TELESCOPE_DELAY / _multiplier));
            await MoveSpreader(spreaderPosition.y, spreaderPosition.z);
            await Task.Delay((int)(TWISTLOCK_DELAY / _multiplier));
            await _bridge.PickUp(Id, container.Number);
            _stacking.PickContainer(location, container.Number);

            return true;
        }

        public override async Task<bool> PutDown(Location location, Container container)
        {
            _log.Debug($"{Id} putting down {container.Number} at {location}");
            Position position = GetPosition(location);
            Position spreaderPosition = GetSpreaderPosition(location);
            await MoveSpreader(spreaderPosition.y, spreaderPosition.z);
            await Task.Delay((int)(TWISTLOCK_DELAY / _multiplier));
            await _bridge.PutDown(Id, container.Number, position);
            _stacking.PutContainer(location, container.Number);
            Status.PutDownContainer();

            return true;
        }

        public async Task PickFromStowage(Location location, Container container)
        {
            await PickUp(location, container);
            Position spreaderPosition = GetSpreaderPosition(new Location(LocationType.QCTP, NumericId, 0, 1, 0));
            await MoveSpreader(spreaderPosition.y, _stacking.GetSafeHoistHeight(BayId, LocationType.STOWAGE, SPREADER_MAX));
        }

        public async Task SpreaderSafeHeight()
        {
            await Hoist(SAFE_HEIGHT);
        }

        public Position GetSpreaderPosition(Location location)
        {
            if (location.locationType != LocationType.STOWAGE &&
                location.locationType != LocationType.QCTP)
            {
                return new Position(0, 0, SPREADER_MAX, 0);
            }

            Position result;

            if (location.locationType == LocationType.QCTP)
            {
                result = GetQctpSpreaderPosition(location);
            }
            else
            {
                result = GetStowageSpreaderPosition(location);
            }

            result.x = 0;
            result.phi = 0;
            result.y = Math.Clamp(result.y, TROLLEY_MIN, TROLLEY_MAX);
            result.z = Math.Clamp(result.z, SPREADER_MIN, SPREADER_MAX);

            return result;
        }

        private List<IntPoint> GetGantryClaimAt(Position position)
        {
            const int railYLocationOffset = 15000;
            return new Rectangle(
                position.x - GANTRY_CLAIM_LENGTH / 2,
                position.y - GANTRY_CLAIM_WIDTH / 2 + railYLocationOffset,
                GANTRY_CLAIM_LENGTH,
                GANTRY_CLAIM_WIDTH).ToIntPoints();
        }

        private Area GetGantryClaimTo(Position position)
        {
            var result = new List<IntPoint>();
            result.AddRange(GetGantryClaimAt(_lastPosition));
            result.AddRange(GetGantryClaimAt(position));
            return new Area(result, Id);
        }

        public List<IntPoint> GetQctpClaim()
        {
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            var allQctpLaneLocations = PositionProvider.GetLocationsOfType(LocationType.QCTP);
            foreach (var entry in allQctpLaneLocations)
            {
                if (entry.Key.block == NumericId)
                {
                    if (entry.Value.y < minY)
                    {
                        minY = entry.Value.y;
                    }
                    if (entry.Value.y > maxY)
                    {
                        maxY = entry.Value.y;
                    }
                }
            }
            int centerY = (minY + maxY) / 2;

            Position areaControlPosition = PositionProvider.GetPosition(new Location(LocationType.QCTP, NumericId, 0, 1, 0)).Copy();
            areaControlPosition.y = centerY;
            return GetOccupiedAreaAt(areaControlPosition);
        }

        private Position GetQctpSpreaderPosition(Location location)
        {
            Position qctpPos = PositionProvider.GetPosition(location);
            int trolley = -qctpPos.y - 9200;
            int spreader = (location.floor + 1) * Container.DefaultHeight;

            return new Position(0, trolley, spreader, 0);
        }

        private Position GetStowageSpreaderPosition(Location location)
        {
            Position qctpPos = PositionProvider.GetPosition(location);
            return new Position(0, -qctpPos.y - 9250, qctpPos.z + Container.DefaultHeight, 0);
        }

        private bool IsInMargin(int value, int goal)
        {
            return (value >= goal - MARGIN) && (value <= goal + MARGIN);
        }

        public async Task SetBayId(int bayId)
        {
            if (bayId == BayId || bayId < 0 || bayId > PositionProvider.ShipBayCount)
                return;

            Position newPosition = _lastPosition.Copy();
            newPosition.x = PositionProvider.GetPosition(new Location(LocationType.STOWAGE, 0, bayId, 1, 1)).x;

            _log.Debug($"{Id} gantry-move to bay {bayId} at {newPosition}");

            Guid? driveClaimId = await _areaControl.RequestAccess(GetGantryClaimTo(newPosition));

            await DriveTo(newPosition);

            Guid? newClaimId = await _areaControl.RequestAccess(GetGantryClaimAt(newPosition), Id);

            _areaControl.RelinquishAccess(driveClaimId.Value);
            _areaControl.RelinquishAccess(_lastClaimedArea);
            _lastClaimedArea = newClaimId.Value;

            PositionProvider.UpdateQctpPositions(NumericId, newPosition.x);

            BayId = bayId;
        }
    }
}
