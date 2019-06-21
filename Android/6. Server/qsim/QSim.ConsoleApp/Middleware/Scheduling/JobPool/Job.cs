using QSim.ConsoleApp.DataTypes;
using System;

namespace QSim.ConsoleApp.Middleware.Scheduling.JobPool
{
    public class Job
    {
        public string JobId { get; private set; }
        public Container Container { get; private set; }
        public string HandledBy;
        public Location CurrentLocation;
        public LocationType Destination { get; private set; }

        public Job (string jobId, Container container, Location location, LocationType destination)
        {
            JobId = jobId;
            Container = container;
            CurrentLocation = location;
            Destination = destination;
            HandledBy = "";
        }

        public bool IsFinished
        {
            get { return CurrentLocation.locationType == Destination; }
        }

        public bool Handling
        {
            get { return !string.IsNullOrEmpty(HandledBy); }
        }

        public override string ToString()
        {
            return $"({JobId}) {Container.Number}: {CurrentLocation} => {Destination} {(Handling ? "<-- " + HandledBy : "")}";
        }
    }
}
