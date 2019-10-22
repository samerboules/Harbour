using log4net;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSim.ConsoleApp.Middleware.StackingSystem
{
    public sealed class Stacking
    {
        private static readonly Lazy<Stacking> lazy = new Lazy<Stacking>(() => new Stacking());
        private readonly Dictionary<Location, StackEntry> containerLocations = new Dictionary<Location, StackEntry>();
        private readonly Dictionary<string, Container> containers = new Dictionary<string, Container>();
        private readonly ILog _log;

        public static Stacking Instance { get { return lazy.Value; } }

        private Stacking()
        {
            _log = LogManager.GetLogger(GetType());
            InitStackingLocations();
        }

        private void InitStackingLocations()
        {
            foreach (var location in PositionProvider.GetLocations())
            {
                var length = DetermineLength(location.Key);
                containerLocations[location.Key] = new StackEntry(length);
            }

            _log.Debug($"Added {containerLocations.Count} container locations to inventory");
        }

        public void ClearStackingLocations()
        {
            InitStackingLocations();
        }

        private ContainerLength DetermineLength(Location location)
        {
            switch (location.locationType)
            {
                case LocationType.WSTP:
                case LocationType.YARD:
                case LocationType.STOWAGE:
                    return location.major % 2 == 0 ? ContainerLength.LENGTH_40 : ContainerLength.LENGTH_20;
            }

            return ContainerLength.UNKNOWN;
        }

        #region Reserve stacking locations

        public Location GetAscStackingLocation(ContainerLength lenght, int stackId = -1)
        {
            lock (containerLocations)
            {
                var filteredByEmptyLocations = containerLocations.Where(
                    loc => loc.Key.locationType == LocationType.YARD &&
                    (stackId == -1 || loc.Key.block == stackId) &&
                    loc.Value.IsLength(lenght) &&
                    loc.Value.FreeToReserve);

                return ReserveRandomLocationFromSet(ref filteredByEmptyLocations);
            }
        }

        public Location GetWstpStackingLocation(ContainerLength lenght, int stackId = -1)
        {
            lock (containerLocations)
            {
                var filteredByEmptyLocations = containerLocations.Where(
                    loc => loc.Key.locationType == LocationType.WSTP &&
                    (stackId == -1 || loc.Key.block == stackId) &&
                    loc.Key.major < 4 &&
                    loc.Key.minor % 2 == 1 &&
                    loc.Value.IsLength(lenght) &&
                    loc.Value.FreeToReserve);

                return ReserveRandomLocationFromSet(ref filteredByEmptyLocations);
            }
        }

        public Location GetStowageStackingLocation(ContainerLength lenght, int bayId = -1)
        {
            lock (containerLocations)
            {
                var filteredByEmptyLocations = containerLocations.Where(
                    loc => loc.Key.locationType == LocationType.STOWAGE &&
                    (bayId == -1 || loc.Key.major == bayId) &&
                    loc.Value.IsLength(lenght) &&
                    loc.Value.FreeToReserve);

                return ReserveRandomLocationFromSet(ref filteredByEmptyLocations);
            }
        }

        public Location GetQCTPStackingLocation(ContainerLength lenght, int qcId = -1)
        {
            lock (containerLocations)
            {
                var filteredByEmptyLocations = containerLocations.Where(
                    loc => loc.Key.locationType == LocationType.QCTP &&
                    (qcId == -1 || loc.Key.block == qcId) &&
                    loc.Value.IsLength(lenght) &&
                    loc.Value.FreeToReserve);

                return ReserveRandomLocationFromSet(ref filteredByEmptyLocations);
            }
        }

        private Location ReserveRandomLocationFromSet(ref IEnumerable<KeyValuePair<Location, StackEntry>> filteredSet)
        {
            if (filteredSet.Count() == 0)
            {
                _log.Error("No stacking position available!");
                return null;
            }

            var groundLocation = filteredSet.ElementAt(RandomNumberGenerator.NextNumber(filteredSet.Count())).Key;
            var key = filteredSet.Where(loc => loc.Key.major == groundLocation.major && loc.Key.minor == groundLocation.minor).OrderBy(loc => loc.Key.floor).FirstOrDefault().Key;

            SetWstpReservation(key, true);
            ReserveLocation(key);
            return key;
        }

        #endregion

        #region Container queries

        public List<Container> GetContainers()
        {
            return containers.Values.ToList();
        }

        public bool HasContainer(string containerNumber)
        {
            return containers.ContainsKey(containerNumber);
        }

        public Container GetContainer(string containerNumber)
        {
            return containers[containerNumber];
        }

        public IEnumerable<KeyValuePair<Location, StackEntry>> GetStowageContainers(int bayId = -1)
        {
            lock (containerLocations)
            {
                return containerLocations.Where(
                    loc => loc.Key.locationType == LocationType.STOWAGE &&
                    (bayId == -1 || loc.Key.major == bayId) &&
                    loc.Value.IsOccupied).OrderByDescending(loc => loc.Key.floor);
            }
        }

        public IEnumerable<KeyValuePair<Location, StackEntry>> GetYardContainers(int ascId = -1)
        {
            lock (containerLocations)
            {
                return containerLocations.Where(
                    loc => loc.Key.locationType == LocationType.YARD &&
                    (ascId == -1 || loc.Key.block == ascId) &&
                    loc.Value.IsOccupied).OrderByDescending(loc => loc.Key.floor);
            }
        }

        #endregion

        #region Pick, put, reserve, unreserve

        public void PutContainer(Location location, string containerId, ContainerLength containerLength = ContainerLength.LENGTH_40)
        {
            lock (containerLocations)
            {
                if (!HasContainer(containerId))
                {
                    containers[containerId] = new Container(containerId, containerLength);
                }

                containerLocations[location].Container = containers[containerId];
                SetWstpReservation(location, true);
                ResetReservation(location);
            }
        }

        public void PickContainer(Location location, string containerId)
        {
            lock (containerLocations)
            {
                if (containerLocations[location].Container.Number == containerId)
                {
                    containerLocations[location].Container = null;
                    SetWstpReservation(location, false);
                    ResetReservation(location);
                }
                else
                {
                    _log.Error($"{containerId} NOT cleared from {location}. Location is occupied by {containerLocations[location]}");
                }
            }
        }

        public bool ReserveLocation(Location location)
        {
            lock (containerLocations)
            {
                if (!containerLocations[location].FreeToReserve)
                {
                    return false;
                }
                containerLocations[location].IsReserved = true;
                return true;
            }
        }

        public void ResetReservation(Location location)
        {
            lock (containerLocations)
            {
                containerLocations[location].IsReserved = false;
            }
        }

        private void SetWstpReservation(Location location, bool isReserved)
        {
            if (location.locationType != LocationType.WSTP)
                return;

            foreach (int slot in Enumerable.Range(1, 3))
            {
                var loc = new Location(LocationType.WSTP, location.block, slot, location.minor, location.floor);
                containerLocations[loc].IsReserved = isReserved;
            }
        }

        #endregion

        public int GetSafeHoistHeight(int id, LocationType locationType, int defaultSafeHeight)
        {
            lock (containerLocations)
            {
                    var containersLeft = containerLocations
                        .Where(loc => loc.Value.IsOccupied &&
                                      loc.Key.locationType == locationType &&
                                      ((loc.Key.major == id && locationType == LocationType.STOWAGE) ||
                                       (loc.Key.block == id && locationType == LocationType.YARD)))
                        .OrderByDescending(loc => loc.Key.floor)
                        .ToList();
                    Location topLocation = null;

                    if (containersLeft.Count > 0)
                    {
                        topLocation = containersLeft.FirstOrDefault().Key.Copy();
                    }

                    if (topLocation == null)
                    {
                        return defaultSafeHeight;
                    }

                    int height = PositionProvider.GetPosition(topLocation).z;

                    if (locationType == LocationType.STOWAGE)
                    {
                        topLocation.floor = 3;
                        height = Math.Max(height, PositionProvider.GetPosition(topLocation).z);
                    }
                    return height + (2 * Container.DefaultHeight) + 500;
            }
        }

        #region Statistics and debug

        public string GetAscStats(int ascId)
        {
            int wstpContCount = containerLocations.Where(l => l.Key.block == ascId && l.Key.locationType == LocationType.WSTP && l.Value.IsOccupied).Count();
            int yardContCount = containerLocations.Where(l => l.Key.block == ascId && l.Key.locationType == LocationType.YARD && l.Value.IsOccupied).Count();
            int wstpReservedCount = containerLocations.Where(l => l.Key.block == ascId && l.Key.locationType == LocationType.WSTP && l.Value.IsReserved && !l.Value.IsOccupied).Count();

            return $"Container on WSTP: {wstpContCount}\n" +
                   $"Container in YARD: {yardContCount}\n" +
                   $"Reserved WSTP: {wstpReservedCount}\n" +
                   $"\n";
        }

        public string GetQcStats(int qcId, int bayId)
        {
            int qctpContCount = containerLocations.Where(l => l.Key.block == qcId && l.Key.locationType == LocationType.QCTP && l.Value.IsOccupied).Count();
            int bayContCount = containerLocations.Where(l => l.Key.major == bayId && l.Key.locationType == LocationType.STOWAGE && l.Value.IsOccupied).Count();

            return $"Container on QCTP: {qctpContCount}\n" +
                   $"Container in bay: {bayContCount}\n" +
                   $"";
        }

        public void DumpStack()
        {
            _log.Info("======================");
            _log.Info(" STACK DUMP ");
            _log.Info("======================");
            lock (containerLocations)
            {
                containerLocations
                    .Where(l => l.Value.IsOccupied)
                    .ToList()
                    .ForEach(kvp => _log.Info($"{kvp.Value} at {kvp.Key}"));
            }
            _log.Info("======================");
        }

        #endregion
    }
}
