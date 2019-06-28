using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Middleware.Scheduling.JobPool;
using QSim.ConsoleApp.Simulators;
using QSim.ConsoleApp.Utilities;
using System;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Middleware.Scheduling.Workers
{
    class QcWorker : Worker
    {
        private readonly QC qc;
        private int qcNumericId;
        private int bayId;

        public QcWorker(QC _qc)
        {
            qc = _qc;
            bayId = qc.BayId;
            qcNumericId = qc.NumericId;
        }

        public override async Task Run()
        {
            int maxBay = PositionProvider.Bays[qcNumericId - 1] + PositionProvider.QcBayIncrement;

            while (true)
            {
                while (!_jobPool.HasDischargeContainersOnDeck(bayId) &&
                        bayId < maxBay)
                {
                    bayId++;
                }

                if (bayId >= maxBay)
                    break;

                bool isQctpOccupied = true;

                while (isQctpOccupied)
                {
                    isQctpOccupied = _jobPool.HasDischargeContainersOnQctp(qcNumericId);
                    await Task.Delay((int)(5000 / _multiplier));
                }

                await DischargeBay(bayId);
            }

            _log.Info($"No more jobs for {qc.Id}");
        }

        private async Task DischargeBay(int bayId)
        {
            Job currentJob = _jobPool.GetDischargeQcJob(bayId, qc.Id);
            _log.Info($"{qc.Id}: Starting discharge on bay {bayId}");

            await qc.SetBayId(bayId);

            while (currentJob != null)
            {
                await PickupByQc(currentJob.Container, currentJob.CurrentLocation);

                var claimArea = qc.GetQctpClaim();

                Guid? id = await _areaControl.RequestAccess(claimArea, qc.Id);

                bool alreadyAcces = true;
                Location qctpLocation = _stacking.GetQCTPStackingLocation(currentJob.Container.Length, qcNumericId);

                while (qctpLocation == null)
                {
                    if (alreadyAcces)
                    {
                        _areaControl.RelinquishAccess(id.Value);
                        alreadyAcces = false;
                    }
                    await Task.Delay((int)(10000 / _multiplier));
                    qctpLocation = _stacking.GetQCTPStackingLocation(currentJob.Container.Length, qcNumericId);
                }

                if (!alreadyAcces)
                {
                    id = await _areaControl.RequestAccess(claimArea, qc.Id);
                }

                await PutDownByQc(currentJob.Container, qctpLocation);
                _areaControl.RelinquishAccess(id.Value);
                _jobPool.CompleteJobStep(currentJob.JobId, qctpLocation);

                currentJob = _jobPool.GetDischargeQcJob(bayId, qc.Id);
            }
            _log.Info($"{qc.Id}: Finished discharge on bay {bayId}");
        }

        public async Task PutDownByQc(Container container, Location to)
        {
            _log.Info($"{qc.Id}: {container.Number} => {to}");
            await qc.PutDown(to, container);
            await qc.SpreaderSafeHeight();
        }

        public async Task PickupByQc(Container container, Location from)
        {
            _log.Info($"{qc.Id}: {container.Number} pickup from {from}");
            await qc.PickFromStowage(from, container);
        }
    }
}
