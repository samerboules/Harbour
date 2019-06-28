using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSim.ConsoleApp.Middleware.Scheduling.JobPool;
using QSim.ConsoleApp.DataTypes;
using System;

namespace QSimTest.Middleware.Scheduling.JobPool
{
    [TestClass]
    public class JobPoolTests
    {
        private QSim.ConsoleApp.Middleware.Scheduling.JobPool.JobPool jobPool = QSim.ConsoleApp.Middleware.Scheduling.JobPool.JobPool.Instance;
        private string qcId = "QC01";
        private string scId = "SC01";
        private string ascId = "ASC01";
        private string containerNumber = "TEST0000001";
        private Location stowLocation = new Location(LocationType.STOWAGE, 1, 1, 1, 1);
        private Location qctpLocation = new Location(LocationType.QCTP, 2, 2, 2, 2);
        private Location wstpLocation = new Location(LocationType.WSTP, 3, 3, 3, 3);
        private Location yardLocation = new Location(LocationType.YARD, 4, 4, 4, 4);

        // Because JobPool is a singleton and unit tests have no defined order, a scenario will be tested rather then seperate unit tests. 
        [TestMethod]
        public void TestScenario()
        {
            Assert.IsTrue(jobPool.AllJobsDone);
            Assert.IsFalse(jobPool.HasDischargeContainersOnDeck(1));
            Assert.IsFalse(jobPool.HasDischargeContainersOnDeck(2));
            Assert.IsFalse(jobPool.HasDischargeContainersOnQctp(1));
            Assert.IsFalse(jobPool.HasDischargeContainersOnQctp(2));

            jobPool.AddJob(new Container(containerNumber, ContainerLength.LENGTH_40), stowLocation, LocationType.YARD);

            Assert.IsFalse(jobPool.AllJobsDone);
            Assert.IsTrue(jobPool.HasDischargeContainersOnDeck(1));
            Assert.IsFalse(jobPool.HasDischargeContainersOnDeck(2));
            Assert.IsFalse(jobPool.HasDischargeContainersOnQctp(1));
            Assert.IsFalse(jobPool.HasDischargeContainersOnQctp(2));

            // Discharge with QC
            Job resultJob = jobPool.GetDischargeQcJob(stowLocation.block, qcId);
            string jobId = resultJob.JobId;

            AssertJob(resultJob, qcId, stowLocation);
            FinishAndAssertJobStep(resultJob, qctpLocation);

            Assert.IsFalse(jobPool.HasDischargeContainersOnDeck(1));
            Assert.IsFalse(jobPool.HasDischargeContainersOnDeck(2));
            Assert.IsFalse(jobPool.HasDischargeContainersOnQctp(1));
            Assert.IsTrue(jobPool.HasDischargeContainersOnQctp(2));

            resultJob = jobPool.GetDischargeQcJob(stowLocation.block, qcId);
            Assert.IsNull(resultJob);

            // Drive with SC
            resultJob = jobPool.GetDischargeScJob(qctpLocation.block, scId);
            AssertJob(resultJob, scId, qctpLocation);
            FinishAndAssertJobStep(resultJob, wstpLocation);

            resultJob = jobPool.GetDischargeScJob(qctpLocation.block, scId);
            Assert.IsNull(resultJob);
            Assert.IsFalse(jobPool.AllJobsDone);

            // In yard with ASC
            resultJob = jobPool.GetDischargeAscJob(wstpLocation.block, ascId);
            AssertJob(resultJob, ascId, wstpLocation);
            FinishAndAssertJobStep(resultJob, yardLocation);

            Assert.IsTrue(jobPool.AllJobsDone);
        }

        private void AssertJob(Job job, string equipmentId, Location location)
        {
            Assert.AreEqual(equipmentId, job.HandledBy);
            Assert.AreEqual(location, job.CurrentLocation);
            Assert.AreEqual(LocationType.YARD, job.Destination);
            Assert.IsFalse(jobPool.AllJobsDone);
        }

        private void FinishAndAssertJobStep(Job job, Location location)
        {
            bool resultBool = jobPool.CompleteJobStep(job.JobId, location);
            Assert.AreEqual(true, resultBool);
            Assert.AreEqual("", job.HandledBy);
        }
    }
}
