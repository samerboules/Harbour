using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Middleware.Scheduling.JobPool;
using QSim.ConsoleApp.Simulators;
using QSim.ConsoleApp.Utilities;
using System;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Middleware.Scheduling.Workers
{
    class ScWorker : Worker
    {
        private const int MAX_LOOK_FOR_WORK_DELAY = 5000;
        private const int LOOK_FOR_WSTP_DELAY = 1250;

        private readonly SC sc;
        private Job currentJob = null;

        public ScWorker(SC _sc)
        {
            sc = _sc;
        }

        public override async Task Run()
        {
            int lastUnreachableQc = -1;
            int tries = 0;

            while (!_jobPool.AllJobsDone)
            {
                // Find closest QC
                int qcId = await FindWorkAtAQc(lastUnreachableQc);

                // No more work available
                if (qcId == -1)
                    break;

                if (!_jobPool.ReserveContainerOnQctp(qcId))
                {
                    await Task.Delay((int)(10000 / _multiplier));
                    continue;
                }

                // Work found! Now drive to it
                if (!await PositionSc(SC.GetQctpPrePosition(qcId)))
                {
                    // Unreachable destination
                    _jobPool.UnreserveContainerOnQctp(qcId);
                    lastUnreachableQc = qcId;
                    tries++;

                    if (tries == 25)
                    {
                        await ParkAtWSTP();
                        await Task.Delay((int)(60000 / _multiplier));
                        tries = 0;
                    }

                    continue;
                }

                Guid? id = await _areaControl.RequestAccess(GetQctpClaim(qcId));

                currentJob = _jobPool.GetDischargeScJob(qcId, sc.Id);

                if (currentJob == null)
                {
                    // Somebody stole our work :'(
                    _areaControl.RelinquishAccess(id.Value);
                    _jobPool.UnreserveContainerOnQctp(qcId);
                    continue;
                }

                await Pickup(currentJob.Container, currentJob.CurrentLocation);
                _jobPool.UnreserveContainerOnQctp(qcId);
                _areaControl.RelinquishAccess(id.Value);

                Location wstpLocation = null;
                while (wstpLocation == null)
                {
                    wstpLocation = await ReachWstp();
                }

                await PutDownAtWstp(wstpLocation);

                _jobPool.CompleteJobStep(currentJob.JobId, wstpLocation);
                currentJob = null;
            }

            _log.Info($"No more jobs for {sc.Id}, parking.");
            await Park();
        }

        public async Task<bool> Pickup(Container container, Location from)
        {
            _log.Info($"{sc.Id}: {container.Number} {from}");
            await sc.PickUp(from, container);

            if (from.locationType == LocationType.QCTP)
            {
                Position parkPosition = SC.GetQctpPostPosition(from.block, from.minor);

                bool hasReachedPostQctp = false;

                while (!hasReachedPostQctp)
                {
                    hasReachedPostQctp = await sc.DriveTo(parkPosition, false);
                }
            }

            return true;
        }

        public async Task<bool> PutDown(Container container, Location to)
        {
            _log.Info($"{sc.Id}: {container.Number} => {to}");
            return await sc.PutDown(to, container);
        }

        public async Task<bool> PositionSc(Position to)
        {
            _log.Info($"{sc.Id}: drive to {to}.");
            return await sc.DriveTo(to, true);
        }

        private async Task<Location> ReachWstp()
        {
            Location wstpLocation = await GetWstpLocation();
            Position parkScPosition = GetParkPosition(wstpLocation);

            int tries = 0;

            while (!await PositionSc(parkScPosition))
            {
                // Undriveable route. Try another.
                _stacking.ResetReservation(wstpLocation);
                wstpLocation = await GetWstpLocation();
                parkScPosition = GetParkPosition(wstpLocation);

                tries++;

                if (tries == 25)
                    break;
            }

            if (tries == 25)
            {
                await ParkAtWSTP();
                return null;
            }

            return wstpLocation;
        }

        private async Task PutDownAtWstp(Location wstpLocation)
        {
            Guid? id = await _areaControl.RequestAccess(GetClaim(wstpLocation));
            await PutDown(currentJob.Container, wstpLocation);
            await PositionSc(GetParkPosition(wstpLocation));
            _areaControl.RelinquishAccess(id.Value);
        }

        private async Task Park()
        {
            Position parkPosition = PositionProvider.GetPosition(new Location(LocationType.SCPARK, sc.NumericId, 0, 0, 0));
            while (!await PositionSc(parkPosition))
            {
                Position wstpPosition = await GetNearestWstpParkPosition();
                await PositionSc(wstpPosition);
                await Task.Delay((int)(20000 / _multiplier));
            }
        }

        private async Task ParkAtWSTP()
        {
            Position parkScPosition = await GetNearestWstpParkPosition();
            while (!await PositionSc(parkScPosition))
            {
                await Task.Delay(10000);
                parkScPosition = await GetNearestWstpParkPosition();
            }
        }

        private async Task<int> FindWorkAtAQc(int lastQc)
        {
            int qcId = GetClosestQc();

            if (qcId == lastQc)
            {
                qcId = RandomNumberGenerator.NextNumber(1, PositionProvider.QcCount);
            }

            // Check for work
            bool qcHasWork = _jobPool.HasAvailableDischargeContainersOnQctp(qcId);
            int tries = 0;

            while (!qcHasWork)
            {
                await Task.Delay((int)(RandomNumberGenerator.NextNumber(MAX_LOOK_FOR_WORK_DELAY / 2, MAX_LOOK_FOR_WORK_DELAY) / _multiplier));
                qcId = RandomNumberGenerator.NextNumber(1, PositionProvider.QcCount);
                qcHasWork = _jobPool.HasAvailableDischargeContainersOnQctp(qcId);

                if (_jobPool.AllJobsDone)
                    return -1;

                tries++;

                if (tries == 100)
                    return -1;
            }

            return qcId;
        }

        private async Task<Location> GetWstpLocation(int stackId = -1)
        {
            try
            {
                Location wstpLocation = _stacking.GetWstpStackingLocation(currentJob.Container.Length, stackId);
                while (wstpLocation == null)
                {
                    await Task.Delay((int)(LOOK_FOR_WSTP_DELAY / _multiplier));
                    wstpLocation = _stacking.GetWstpStackingLocation(currentJob.Container.Length, stackId);
                }

                return wstpLocation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        private async Task<Location> GetWstpLocation(ContainerLength length, int stackId = -1)
        {
            try
            {
                Location wstpLocation = _stacking.GetWstpStackingLocation(length, stackId);
                while (wstpLocation == null)
                {
                    await Task.Delay((int)(LOOK_FOR_WSTP_DELAY / _multiplier));
                    wstpLocation = _stacking.GetWstpStackingLocation(length, stackId);
                }

                return wstpLocation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private async Task<Position> GetNearestWstpParkPosition()
        {
            var nearestLocation = PositionProvider.GetNearestLocation(sc.Position, LocationType.WSTP);
            Location loc;
            if (currentJob == null)
            {
                loc = await GetWstpLocation(ContainerLength.LENGTH_40,nearestLocation.block);
            }
            else
            {
                loc = await GetWstpLocation(nearestLocation.block);
            }
            _stacking.ResetReservation(loc);
            return GetParkPosition(loc);
        }

        private int GetClosestQc()
        {
            return PositionProvider.GetNearestLocation(sc.Position, LocationType.QCTP).block;
        }

        private Position GetParkPosition(int ascId, int lane)
        {
            return PositionProvider.GetPosition(new Location(LocationType.WSTP, ascId, 7, lane, 0));
        }

        private Position GetParkPosition(Location location)
        {
            if (location.locationType != LocationType.WSTP)
                return null;

            return GetParkPosition(location.block, location.minor);
        }

        private Area GetQctpClaim(int qcId)
        {
            return GetClaim(new Location(LocationType.QCTP, qcId, 0, 1, 0));
        }

        private Area GetClaim(Location location)
        {
            return new Area(sc.GetOccupiedAreaAt(PositionProvider.GetPosition(location)), sc.Id);
        }
    }
}
