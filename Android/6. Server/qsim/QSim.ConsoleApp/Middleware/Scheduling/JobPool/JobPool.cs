using log4net;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace QSim.ConsoleApp.Middleware.Scheduling.JobPool
{
    public sealed class JobPool
    {
        private static readonly Lazy<JobPool> lazy = new Lazy<JobPool>(() => new JobPool());
        private const int MAX_RESERVATIONS = 2;
        private readonly List<Job> JobList;
        private readonly ConcurrentDictionary<int, int> qctpReservations = new ConcurrentDictionary<int, int>();
        private ILog _log;
        private int lastJobId = 1;

        public static JobPool Instance { get { return lazy.Value; } }

        private JobPool()
        {
            JobList = new List<Job>();
            _log = LogManager.GetLogger(GetType());

            foreach (int i in Enumerable.Range(1, PositionProvider.QcCount))
            {
                qctpReservations.TryAdd(i, 0);
            }
        }

        public bool AllJobsDone
        {
            get
            {
                return JobList.Count == 0;
            }
        }

        public void AddJob(Container container, Location from, LocationType destination)
        {
            JobList.Add(new Job(lastJobId.ToString("0000"), container, from, destination));
            lastJobId++;
        }

        #region Get discharge jobs

        public Job GetDischargeQcJob(int bayId, string equipId)
        {
            var result = JobList.Where(job => job != null &&
                job.CurrentLocation?.major == bayId &&
                job.CurrentLocation?.locationType == LocationType.STOWAGE &&
                !job.Handling).ToList().FirstOrDefault();

            return SetJobHandling(result, equipId);
        }

        public Job GetDischargeAscJob(int ascNumber, string equipId)
        {
            try
            {
                var result = JobList?.Where(job => job != null &&
                    job.CurrentLocation?.block == ascNumber &&
                    job.CurrentLocation?.locationType == LocationType.WSTP &&
                    !job.Handling).ToList().FirstOrDefault();
                if (result == null)
                {
                    var yard = JobList?.Where(job => job != null &&
                        job.CurrentLocation?.block == ascNumber &&
                        job.CurrentLocation?.locationType == LocationType.YARD &&
                        !job.Handling).ToList().FirstOrDefault();
                    if (yard != null)
                        lock (yard)
                        {
                            JobList.Remove(yard);
                        }
                }
                return SetJobHandling(result, equipId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public Job GetDischargeScJob(int qcId, string equipId)
        {
            var result = JobList.Where(job => job != null &&
                job.CurrentLocation.block == qcId &&
                job.CurrentLocation.locationType == LocationType.QCTP &&
                !job.Handling).OrderByDescending(job => job.CurrentLocation.floor).ToList().FirstOrDefault();

            return SetJobHandling(result, equipId);
        }

        public Job GetDischargeScJob(string containerId, string equipId)
        {
            var result = JobList.Where(job => job != null &&
                job.CurrentLocation.locationType == LocationType.QCTP &&
                job.Container.Number == containerId &&
                !job.Handling).ToList().FirstOrDefault();

            return SetJobHandling(result, equipId);
        }

        private Job SetJobHandling(Job job, string equipmentId)
        {
            if (job == null)
            {
                return null;
            }

            job.HandledBy = equipmentId;
            return job;
        }

        #endregion

        #region TP queries and reservations

        public bool ReserveContainerOnQctp(int qcId)
        {
            if (HasAvailableDischargeContainersOnQctp(qcId))
            {
                qctpReservations[qcId]++;
                return true;
            }
            return false;
        }

        public void UnreserveContainerOnQctp(int qcId)
        {
            qctpReservations[qcId] = Math.Max(0, qctpReservations[qcId] - 1);
        }

        public bool HasDischargeContainersOnQctp(int qcId)
        {
            return CountDischargeContainersOnQctp(qcId) > 0;
        }

        public bool HasAvailableDischargeContainersOnQctp(int qcId)
        {
            return qctpReservations[qcId] <= MAX_RESERVATIONS && CountDischargeContainersOnQctp(qcId) - qctpReservations[qcId] > 0;
        }

        public bool HasDischargeContainersOnDeck(int bayId)
        {
            try
            {
                return JobList.Where(job => job != null &&
                           job.CurrentLocation.major == bayId &&
                           job.CurrentLocation.locationType == LocationType.STOWAGE &&
                           !job.Handling).ToList().Any();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private int CountDischargeContainersOnQctp(int qcId)
        {
            try
            {
                return JobList.Where(job => job != null &&
                    job.CurrentLocation.block == qcId &&
                    job.CurrentLocation.locationType == LocationType.QCTP &&
                    !job.Handling).ToList().Count;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        #endregion

        public bool CompleteJobStep(string jobId, Location newLocation)
        {
            try
            {
                var completedJob = JobList.Where(job => job != null && job.JobId == jobId).ToList().FirstOrDefault();

                if (completedJob == null)
                    return false;

                completedJob.HandledBy = "";
                completedJob.CurrentLocation = newLocation;

                if (completedJob.IsFinished)
                    JobList.ToList().Remove(completedJob);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        #region Statistics and output

        public void DumpJobs()
        {
            _log.Info("======================");
            _log.Info(" JOB DUMP ");
            _log.Info("======================");
            JobList.ForEach(j => _log.Info(j));
            _log.Info("======================");
        }

        public string GetStatistics()
        {
            return $"Discharge jobs: {JobList.Count}\n" +
                   $"On board: {JobList.Where(j => j.CurrentLocation.locationType == LocationType.STOWAGE).Count()}\n" +
                   $"On QCTP: {JobList.Where(j => j.CurrentLocation.locationType == LocationType.QCTP).Count()}\n" +
                   $"On WSTP: {JobList.Where(j => j.CurrentLocation.locationType == LocationType.WSTP).Count()}\n";
        }

        public string GetQctpStatistics(int qcId)
        {
            return $"Reservations on QCTP: {qctpReservations[qcId]}\n\n";
        }

        #endregion
    }
}
