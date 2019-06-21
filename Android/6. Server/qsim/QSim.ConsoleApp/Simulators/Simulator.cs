using log4net;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Middleware;
using QSim.ConsoleApp.Middleware.StackingSystem;
using QSim.ConsoleApp.Utilities;
using QSim.ConsoleApp.Utilities.Clipper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Simulators
{
    abstract public class Simulator
    {
        protected VisualizationBridge _bridge;
        protected Stacking _stacking;
        protected AreaControl _areaControl;
        protected ILog _log;
        protected double _multiplier = 1;
        protected Position _lastPosition;
        protected Position _lastSpreaderPosition;
        protected SpreaderSize _lastSpreaderSize = SpreaderSize.SPREADER_40;
        protected Guid     _lastClaimedArea;

        public abstract Task<bool> DriveTo(Position position);
        public abstract Task MoveSpreader(int trolley, int height);
        public abstract Task<bool> PickUp(Location location, Container container);
        public abstract Task<bool> PutDown(Location location, Container container);

        protected Simulator()
        {
            _log = LogManager.GetLogger(GetType());
            _bridge = VisualizationBridge.Instance;
            _stacking = Stacking.Instance;
            _areaControl = AreaControl.Instance;
            Status = new EquipmentStatus();
        }

        public async Task DriveTo(Location location)
        {
            await DriveTo(GetPosition(location));
        }

        public virtual void SetMultiplier(double multiplier)
        {
            _multiplier = multiplier;
        }

        public virtual async Task SetSpreaderSize(SpreaderSize spreaderSize, int dt = 10)
        {
            if (spreaderSize == _lastSpreaderSize)
                dt = 10;

            await _bridge.SpreaderSizeUpdate(Id, spreaderSize, dt);
            _lastSpreaderSize = spreaderSize;
        }

        public List<IntPoint> GetOccupiedAreaAt(Position position, int extraLength = 0)
        {
            return GetOccupiedArea(position, extraLength);
        }

        protected Position GetPosition(Location location)
        {
            Position result = PositionProvider.GetPosition(location);
            if (result.x == 0 && result.y == 0 && location.locationType != LocationType.STOWAGE)
            {
                _log.Error($"Unknown location {location}. Result position is {result}");
            }
            return result;
        }

        protected List<IntPoint> GetOccupiedArea(Position center, int extraLength = 0)
        {
            var result = new List<IntPoint>();
            var halfLength = EquipmentLength / 2;
            var halfWidth = EquipmentWidth / 2;

            var minX = center.x - halfLength + (extraLength < 0 ? extraLength : 0);
            var maxX = center.x + halfLength + (extraLength > 0 ? extraLength : 0);
            var minY = center.y - halfWidth;
            var maxY = center.y + halfWidth;

            var rectangle = new List<IntPoint>
            {
                new IntPoint(minX, minY),
                new IntPoint(maxX, minY),
                new IntPoint(maxX, maxY),
                new IntPoint(minX, maxY),
            };

            foreach (var point in rectangle)
            {
                result.Add(RotatePosition(point, center, center.phi));
            }

            return result;
        }

        private IntPoint RotatePosition(IntPoint position, Position center, double phi)
        {
            var ox = position.X - center.x;
            var oy = position.Y - center.y;

            var x = center.x + ((ox * Math.Cos(phi)) - (oy * Math.Sin(phi)));
            var y = center.y + ((ox * Math.Sin(phi)) + (oy * Math.Cos(phi)));

            return new IntPoint((int)x, (int)y);
        }

        protected Rectangle GetBoundingBox(List<IntPoint> polygon)
        {
            long xMin = Int32.MaxValue;
            long yMin = Int32.MaxValue;
            long xMax = Int32.MinValue;
            long yMax = Int32.MinValue;

            foreach (var point in polygon)
            {
                xMin = Math.Min(xMin, point.X);
                yMin = Math.Min(yMin, point.Y);
                xMax = Math.Max(xMax, point.X);
                yMax = Math.Max(yMax, point.Y);
            }

            return Rectangle.FromLTRB((int)xMin, (int)yMin, (int)xMax, (int)yMax);
        }

        public Position Position { get { return _lastPosition; } }
        public string Id { get; protected set; }
        public int NumericId { get; protected set; }
        public int EquipmentWidth { get; protected set; }
        public int EquipmentLength { get; protected set; }
        public EquipmentStatus Status { get; protected set; }
        public Rectangle OccupiedArea { get { return GetBoundingBox(GetOccupiedArea(Position)); } }
    }
}
