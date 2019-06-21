using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Middleware.Scheduling.Workers;
using QSim.ConsoleApp.Middleware.StackingSystem;
using QSim.ConsoleApp.Simulators;
using QSim.ConsoleApp.Utilities;

namespace QSim.ConsoleApp.Middleware.Scheduling
{
    public class MainScheduler
    {
        private readonly ILog _log;
        public readonly List<QC> QcList;
        public readonly List<SC> ScList;
        public readonly List<ASC> AscList;

        private readonly List<QcWorker> _qcWorkerList;
        private readonly List<ScWorker> _scWorkerList;
        private readonly List<AscWorker> _ascWorkerList;

        private readonly VisualizationBridge _bridge;
        private readonly Stacking _stacking;
        private readonly JobPool.JobPool _jobPool;
        private readonly AreaControl _areaControl;

        public MainScheduler(List<QC> qcList, List<SC> scList, List<ASC> ascList)
        {
            _log = LogManager.GetLogger(GetType());
            QcList = qcList;
            ScList = scList;
            AscList = ascList;

            _scWorkerList = new List<ScWorker>();
            _qcWorkerList = new List<QcWorker>();
            _ascWorkerList = new List<AscWorker>();

            _bridge = VisualizationBridge.Instance;
            _stacking = Stacking.Instance;
            _areaControl = AreaControl.Instance;
            _jobPool = JobPool.JobPool.Instance;

            InitWorkers();
        }

        private void InitWorkers()
        {
            _qcWorkerList.AddRange(QcList.Select(qc => new QcWorker(qc)));
            _scWorkerList.AddRange(ScList.Select(sc => new ScWorker(sc)));
            _ascWorkerList.AddRange(AscList.Select(asc => new AscWorker(asc)));
        }

        public void SetMultiplier(double multiplier)
        {
            QcList.Cast<Simulator>().
                Union(ScList.Cast<Simulator>()).
                Union(AscList.Cast<Simulator>()).
                ToList().ForEach(equip => equip.SetMultiplier(multiplier));

            _qcWorkerList.Cast<Worker>().
                Union(_scWorkerList.Cast<Worker>()).
                Union(_ascWorkerList.Cast<Worker>()).
                ToList().ForEach(worker => worker.SetMultiplier(multiplier));

            _log.Info($"Multiplier is now {Math.Round(multiplier, 2)}");
        }

        public async Task RandomFillStack(int amount)
        {
            _log.Info($"Creating {amount} containers in the stack...");
            for (int i = 0; i < amount; i++)
            {
                var length = RandomNumberGenerator.NextNumber(2) == 1 ? ContainerLength.LENGTH_20 : ContainerLength.LENGTH_40;
                Location location = _stacking.GetAscStackingLocation(length);
                string containerId = ContainerGenerator.GetRandomContainerNumber();

                await _bridge.PutDown(
                    PositionProvider.IndexToId("ASC", location.block),
                    containerId,
                    PositionProvider.GetPosition(location),
                    length);
                _stacking.PutContainer(location, containerId, length);

                if (i % 200 == 0)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public async Task RandomFillStowage(int amount)
        {
            _log.Info($"Creating {amount} containers on ship");
            for (int i = 0; i < amount; i++)
            {
                var length = RandomNumberGenerator.NextNumber(2) == 1 ? ContainerLength.LENGTH_20 : ContainerLength.LENGTH_40;
                Location location = _stacking.GetStowageStackingLocation(length);
                string containerId = ContainerGenerator.GetRandomContainerNumber();

                await _bridge.PutDown(
                    PositionProvider.IndexToId("QC", 1),
                    containerId,
                    PositionProvider.GetPosition(location),
                    length);
                _stacking.PutContainer(location, containerId, length);

                if (i % 200 == 0)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public void ResetAllScs()
        {
            ScList.ForEach(async sc => await sc.MoveTo(PositionProvider.GetEquipmentPosition(sc.Id)));
        }


        public async Task DemoJobs(int dischargeJobs, int loadJobs = 0)
        {
            CreateDischargeJobs(dischargeJobs);
            try
            {
                await StartJobs();
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private async Task StartJobs()
        {
            StatisticsSender stats = new StatisticsSender(this);
            await stats.UpdateRandomContainerDetails();

            List<Task> taskList = new List<Task>();
            taskList.AddRange(_qcWorkerList.Select(worker => worker.Run()));
            taskList.AddRange(_scWorkerList.Select(worker => worker.Run()));
            taskList.AddRange(_ascWorkerList.Select(worker => worker.Run()));
            taskList.Add(stats.Send());
            await Task.WhenAll(taskList);
        }

        private void CreateDischargeJobs(int amount)
        {
            _log.Info($"Creating {amount} discharge jobs.");
            var bayContent = _stacking.GetStowageContainers();
            for (int i = 0; i < amount; i++)
            {
                var stackEntry = bayContent.ElementAtOrDefault(i);
                if (stackEntry.Value == null)
                    continue;

                _jobPool.AddJob(stackEntry.Value.Container, stackEntry.Key, LocationType.YARD);
                _log.Debug($"Added job for {stackEntry.Value.Container} on {stackEntry.Key}");
            }
        }
    }
}
