using log4net;
using QSim.ConsoleApp.Middleware.StackingSystem;
using System;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Middleware.Scheduling.Workers
{
    abstract public class Worker
    {
        protected ILog _log;
        protected JobPool.JobPool _jobPool;
        protected AreaControl _areaControl;
        protected Stacking _stacking;
        protected double _multiplier = 1;

        protected Worker()
        {
            _log = LogManager.GetLogger(GetType());
            _jobPool = JobPool.JobPool.Instance;
            _stacking = Stacking.Instance;
            _areaControl = AreaControl.Instance;
        }

        public abstract Task Run();

        public void SetMultiplier(double multiplier)
        {
            this._multiplier = multiplier;
        }
    }
}
