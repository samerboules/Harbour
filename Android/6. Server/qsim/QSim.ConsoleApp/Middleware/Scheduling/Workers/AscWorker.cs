using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Middleware.Scheduling.JobPool;
using QSim.ConsoleApp.Simulators;
using QSim.ConsoleApp.Utilities;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Middleware.Scheduling.Workers
{
    class AscWorker : Worker
    {
        private readonly ASC asc;
        private int ascNumericId;
        private Rectangle wstpArea;

        public AscWorker(ASC _asc)
        {
            asc = _asc;
            ascNumericId = asc.NumericId;
            wstpArea = new Rectangle(asc.Position.x - asc.EquipmentLength / 2, 88585 - asc.EquipmentWidth / 2, asc.EquipmentLength, asc.EquipmentWidth);
        }

        public override async Task Run()
        {
            Job currentJob = _jobPool.GetDischargeAscJob(ascNumericId, asc.Id);

            while (!_jobPool.AllJobsDone)
            {
                while (currentJob == null)
                {
                    currentJob = _jobPool.GetDischargeAscJob(ascNumericId, asc.Id);
                    await Task.Delay((int)(5000 / _multiplier));

                    if (_jobPool.AllJobsDone)
                        break;
                }

                if (_jobPool.AllJobsDone)
                    break;

                Location yardLocation = null;
                while (yardLocation == null)
                {
                    yardLocation = _stacking.GetAscStackingLocation(currentJob.Container.Length, ascNumericId);
                }

                Guid? id = await _areaControl.RequestAccess(wstpArea, asc.Id);
                await PickupByAsc(currentJob.Container, currentJob.CurrentLocation);
                _areaControl.RelinquishAccess(id.Value);
                await PutDownByAsc(currentJob.Container, yardLocation);
                _jobPool.CompleteJobStep(currentJob.JobId, yardLocation);
                currentJob = _jobPool.GetDischargeAscJob(ascNumericId, asc.Id);
            }

            _log.Info($"No more jobs for {asc.Id}");
        }

        public async Task PickupByAsc(Container container, Location from)
        {
            _log.Info($"{asc.Id}: {container.Number} picked up from {from}");
            await asc.PickUp(from, container);
        }

        public async Task PutDownByAsc(Container container, Location to)
        {
            _log.Info($"{asc.Id}: {container.Number} => {to}");
            await asc.PutDown(to, container);
        }

        public async Task PositionAsc(Position to)
        {
            _log.Info($"{asc.Id}: drive to {to}.");
            await asc.DriveTo(to);
        }
    }
}
