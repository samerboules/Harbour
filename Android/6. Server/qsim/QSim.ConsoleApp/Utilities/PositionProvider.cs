using System;
using QSim.ConsoleApp.DataTypes;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using QSim.ConsoleApp.Simulators;

namespace QSim.ConsoleApp.Utilities
{
    /// <summary>
    /// Provides functions to map physical positions to logical positions. Also provides the terminal mapping.
    /// </summary>
    public static class PositionProvider
    {
        public const double EAST = Position.EAST;
        public const double SOUTH = Position.SOUTH;
        public const double WEST = Position.WEST;
        public const double NORTH = Position.NORTH;

        private const int z = 0;
        private const int yardBlockOffset = 34125;
        private const int SC_PARK_Y_OFFSET = 138820;

        public const int ShipBayCount = 16;
        public const int ShipRowCount = 12;
        public const int ShipTierCount = 6;

        public const int QcCount = 3;
        public const int AscCount = 10;
        public const int ScCount = 5;
        public const int QcBayIncrement = 5;

        public static int[] Bays = new int[] { 1, 5, 12 };
        public static int[] UnusableBays = new int[] { 10, 11,15 };

        private static readonly Dictionary<int, int> AscYardBlocks = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> QcLanePositions = new Dictionary<int, int>
        {
            { 1, 32820 },
            { 2, 39670 }
        };

        private static readonly Dictionary<int, Highway> HighwayMap = new Dictionary<int, Highway>
        {
            { 1, new Highway(1, new Position(506425,  54667, 0, 0), new Position(163650,  54667, 0, 0), WEST) },
            { 2, new Highway(2, new Position(163650,  61667, 0, 0), new Position(506425,  61667, 0, 0), EAST) },
            { 3, new Highway(3, new Position(163650, 263000, 0, 0), new Position(163650,  54667, 0, 0), NORTH) },
            { 4, new Highway(4, new Position(169150,  54667, 0, 0), new Position(169150, 263000, 0, 0), SOUTH) }
        };

        private static readonly ConcurrentDictionary<Location, Position> LocationMap = new ConcurrentDictionary<Location, Position>();
        private static readonly ConcurrentDictionary<string, Position> EquipmentMap = new ConcurrentDictionary<string, Position>();

        static PositionProvider()
        {
            InitStowagePositions();
            InitYardBlocks();
            InitQCPositions();
            InitASCPositions();
            InitSCPositions();
            InitQCTPPositions();
            InitWSTPPositions();
            InitASCBlockPositions();
        }

        private static void InitStowagePositions()
        {
            int xOffset = 216410;
            int yOffset = -15120;
            int zOffset = 13004;

            int xIncrement = 14670;
            int yIncrement = 2509;
            int zIncrement = Container.DefaultHeight;

            for (int bay = 0; bay < ShipBayCount; bay++)
            {
                if (UnusableBays.Contains(bay+1))
                    continue;

                for (int row = 0; row < ShipRowCount; row++)
                {
                    if (bay == 14 && (row == 0 || row == ShipRowCount - 1))
                        continue;

                    for (int tier = 0; tier < ShipTierCount; tier++)
                    {
                        if (bay == 0 && tier > ShipTierCount - 2)
                            continue;

                        LocationMap[new Location(LocationType.STOWAGE, 0, bay + 1, row + 1, tier)] =
                            new Position(
                                xOffset + (bay > 8 ? 3850 : 0) + (bay > 14 ? 3300 : 0) + bay * xIncrement,
                                yOffset - row * yIncrement,
                                zOffset + tier * zIncrement,
                                EAST);
                    }
                }
            }
        }

        private static void InitYardBlocks()
        {
            for (int i = 0; i < AscCount; i++)
            {
                AscYardBlocks.Add(i + 1, 177300 + i * yardBlockOffset);
            }
        }

        private static void InitQCPositions()
        {
            for (int i = 0; i < QcCount; i++)
            {
                int xPos = GetPosition(new Location(LocationType.STOWAGE, 0, Bays[i], 1, 1)).x;
                EquipmentMap[IndexToId("QC", i)] = new Position(xPos, 10000, z, EAST);
            }
        }

        private static void InitASCPositions()
        {
            for (int i = 0; i < AscCount; i++)
            {
                EquipmentMap[IndexToId("ASC", i)] = new Position(189850 + i * yardBlockOffset, 120000, z, EAST);
            }
        }

        private static void InitSCPositions()
        {
            int minimumHighwayX = HighwayMap.Values.Where(h => h.from.IsNorthSouth).Select(h => h.from.x).Min();
            int offsetX = minimumHighwayX - (SC.TURNING_RADIUS + SC.EQUIPMENT_LENGTH + 500);
            int a = SC.TURNING_RADIUS + SC.EQUIPMENT_WIDTH / 2;
            int b = SC.EQUIPMENT_LENGTH / 2;
            int deltaY = (int)Math.Sqrt(a * a + b * b) + 500;

            for (int i = 0; i < ScCount; i++)
            {
                Position position = new Position(offsetX, SC_PARK_Y_OFFSET + i * deltaY, z, EAST);
                LocationMap[new Location(LocationType.SCPARK, IndexToNumericId(i), 0, 0, 0)] = position;
                EquipmentMap[IndexToId("SC", i)] = position;
            }
        }

        private static void InitQCTPPositions()
        {
            foreach (var qc in EquipmentMap.Where(qc => qc.Key.StartsWith("QC")))
            {
                int qcId = 0;
                Int32.TryParse(qc.Key.Substring(2), out qcId);  // Bleh...
                foreach (var laneEntry in QcLanePositions)
                {
                    for (int floor = 0; floor < 3; floor++)
                    {
                        LocationMap[new Location(LocationType.QCTP, qcId, 0, laneEntry.Key, floor)] =
                            new Position(0, laneEntry.Value, Container.DefaultHeight * floor, EAST);
                    }
                }
            }
        }

        private static void InitWSTPPositions()
        {
            Dictionary<int, int> lanes = new Dictionary<int, int>
            {
                { 1, 22000 },
                { 2, 17500 },
                { 3, 13000 },
                { 4, 8500 },
                { 5, 4000 }
            };

            Dictionary<int, int> slots = new Dictionary<int, int>
            {
                {1, 92303},
                {2, 88585},
                {3, 85845},
                {4, 82016},
                {5, 78987},
                {6, 75595},
                {7, 72529}
            };

            foreach (var asc in AscYardBlocks)
            {
                foreach (var lane in lanes)
                {
                    foreach (var slot in slots)
                    {
                        int x = asc.Value + lane.Value;
                        int y = slot.Value;
                        LocationMap[new Location(LocationType.WSTP, asc.Key, slot.Key, lane.Key, 0)] =
                            new Position(x, y, z, SOUTH);
                    }
                }
            }
        }

        private static void InitASCBlockPositions()
        {
            Dictionary<int, int> lanes = new Dictionary<int, int>
            {
                { 1, 24710 },
                { 2, 21826 },
                { 3, 18930 },
                { 4, 16040 },
                { 5, 13150 },
                { 6, 10266 },
                { 7, 7370 },
                { 8, 4486 },
                { 9, 1590 }
            };

            Dictionary<int, int> stacks = new Dictionary<int, int>
            {
                { 1, 45346 },
                { 3, 51846 },
                { 5, 58346 },
                { 7, 64829 },
                { 10, 74612 },
                { 14, 87395 },
                { 18, 100195 },
                { 22, 113012 },
                { 26, 125795 },
                { 29, 135546 },
                { 31, 142029 },
                { 34, 151795 },
                { 38, 166545 },
                { 42, 178912 },
              //{ 46, 191712 },  Reefer stacking section in some stacks. For now not relevant
                { 49, 201446 },
                { 51, 207929 },
                { 53, 214429 },
                { 55, 220929 },
                { 57, 227446 },
                { 60, 237212 },
                { 102, 192012 },
              //{ 103, 203829 },
              //{ 104, 206895 },
              //{ 105, 210329 },
              //{ 107, 218846 },
              //{ 108, 221912 },
              //{ 109, 225329 }
            };

            foreach (var asc in AscYardBlocks)
            {
                foreach (var lane in lanes)
                {
                    foreach (var stack in stacks)
                    {
                        for (int floor = 0; floor < 5; floor++)
                        {
                            LocationMap[new Location(LocationType.YARD, asc.Key, stack.Key, lane.Key, floor)] =
                                new Position(asc.Value + lane.Value, 58400 + stack.Value, floor * Container.DefaultHeight, SOUTH);
                        }
                    }
                }
            }
        }

        public static void UpdateQctpPositions(int qcId, int xPosition)
        {
            LocationMap.
                Where(p => p.Key.locationType == LocationType.QCTP && p.Key.block == qcId).
                ToList().
                ForEach(p => p.Value.x = xPosition);
        }

        public static Location GetNearestLocation(Position position, LocationType locationType)
        {
            Location result = null;
            long minimalDistance = long.MaxValue;

            var locations = GetLocationsOfType(locationType);

            foreach (var location in locations)
            {
                var distance = position.DistanceTo(location.Value);
                if (distance < minimalDistance)
                {
                    result = location.Key;
                    minimalDistance = distance;
                }
            }

            return result;
        }

        public static Position GetPosition(Location location)
        {
            return LocationMap.GetValueOrDefault(location, new Position(0, 0, 0, 0));
        }

        public static Position GetEquipmentPosition(string EquipmentId)
        {
            return EquipmentMap.GetValueOrDefault(EquipmentId, new Position(0, 0, 0, 0));
        }

        public static List<Highway> GetHighways()
        {
            return HighwayMap.Values.ToList();
        }

        // Returns the highway that contains the position.  The orientation of the
        // position must have the same or opposite orientation as the highway, and
        // the position must be on the highway between its start and end position.
        // Returns null if the position is not on a highway.
        public static Highway GetHighway(Position position)
        {
            foreach (var highway in HighwayMap.Values)
            {
                if (highway.IsOnHighway(position))
                {
                    return highway;
                }
            }
            return null;
        }

        public static Highway GetHighwayInOrientation(double orientation)
        {
            foreach (var highway in HighwayMap.Values)
            {
                if (highway.IsInOrientation(orientation))
                {
                    return highway;
                }
            }
            return null;
        }

        public static int GetQctpYPosition(int laneNumber)
        {
            return QcLanePositions[laneNumber];
        }

        public static IEnumerable<KeyValuePair<Location, Position>> GetLocationsOfType(LocationType locationType)
        {
            return LocationMap.Where(yardLoc => yardLoc.Key.locationType == locationType);
        }

        public static IEnumerable<KeyValuePair<Location, Position>> GetLocations()
        {
            return LocationMap;
        }

        public static string IndexToId(string prefix, int id)
        {
            return $"{prefix}{(id + 1).ToString("00")}";
        }

        public static int IndexToNumericId(int id)
        {
            return id + 1;
        }

        public static bool IsInQCArea(Position position)
        {
            int minimumHighwayY = HighwayMap.Values.Where(h => h.IsInOrientation(EAST) || h.IsInOrientation(WEST)).Select(h => h.from.y).Min();
            return position.y < minimumHighwayY;
        }

        public static bool IsInWstpArea(Position position)
        {
            var wstpLocations = LocationMap.Where(kvp => kvp.Key.locationType == LocationType.WSTP);
            int minX = wstpLocations.Select(kvp => kvp.Value.x).Min();
            int maxX = wstpLocations.Select(kvp => kvp.Value.x).Max();
            int minY = wstpLocations.Select(kvp => kvp.Value.y).Min();
            int maxY = wstpLocations.Select(kvp => kvp.Value.y).Max();
            return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
        }

        public static bool IsInSCParkArea(Position position)
        {
            int minimumHighwayX = HighwayMap.Values.Where(h => h.from.IsNorthSouth).Select(h => h.from.x).Min();
            return position.y >= SC_PARK_Y_OFFSET && position.x < minimumHighwayX;
        }
    }
}
