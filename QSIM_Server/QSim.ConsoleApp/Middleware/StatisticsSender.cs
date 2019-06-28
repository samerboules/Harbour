using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Middleware.Scheduling;
using QSim.ConsoleApp.Middleware.Scheduling.JobPool;
using QSim.ConsoleApp.Middleware.StackingSystem;
using QSim.ConsoleApp.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Middleware
{
    public class StatisticsSender
    {
        const int UPDATE_DELAY = 3000;
        VisualizationBridge _bridge = VisualizationBridge.Instance;
        Stacking _stacking = Stacking.Instance;
        JobPool _jobPool = JobPool.Instance;
        MainScheduler _scheduler;

        public StatisticsSender(MainScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task Send()
        {
            while (!_jobPool.AllJobsDone)
            {
                await UpdateScStats();
                await UpdateQcStats();
                await UpdateAscStats();
                await UpdateShip();
                await Task.Delay(UPDATE_DELAY);
            }
        }

        public async Task UpdateRandomContainerDetails()
        {
            var containers = _stacking.GetContainers();

            foreach (var container in containers)
            {
                await _bridge.Status(container.Number, GetHeader(container.Number) + container.GetStatistics());
            }
        }

        private async Task UpdateScStats()
        {
            foreach (var sc in _scheduler.ScList)
            {
                await _bridge.Status(sc.Id, GetHeader(sc.Id) + sc.Status.ToString());
                await Task.Delay(5);
            }
        }

        private async Task UpdateAscStats()
        {
            foreach (var asc in _scheduler.AscList)
            {
                await _bridge.Status(asc.Id, GetHeader(asc.Id) + _stacking.GetAscStats(asc.NumericId) + asc.Status.ToString());
                await Task.Delay(5);
            }
        }

        private async Task UpdateQcStats()
        {
            foreach (var qc in _scheduler.QcList)
            {
                await _bridge.Status(qc.Id, GetHeader(qc.Id) + _stacking.GetQcStats(qc.NumericId, qc.BayId) + _jobPool.GetQctpStatistics(qc.NumericId) + qc.Status.ToString());
                await Task.Delay(5);
            }
        }

        private async Task UpdateShip()
        {
            await _bridge.Status("ship", GetHeader("Jobs") + _jobPool.GetStatistics());
        }

        private string GetHeader(string id)
        {
            return $"<b>{id}</b>:\n";
        }
    }
}
