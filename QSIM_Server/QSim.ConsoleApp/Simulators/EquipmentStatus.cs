using System;

namespace QSim.ConsoleApp.Simulators
{
    public class EquipmentStatus
    {
        public long DistanceDriven { get; private set; } = 0;     // in m
        public long TimeOperational { get; private set; } = 0;    // in s
        public long ContainersHandled { get; private set; } = 0;

        private DateTime startTime;

        public EquipmentStatus()
        {
            startTime = DateTime.UtcNow;
        }

        public void Drive(long distance, long time)
        {
            DistanceDriven += distance / 1000;
            UpdateTime();
        }

        public void PutDownContainer()
        {
            ContainersHandled++;
            UpdateTime();
        }

        public int GetContainerRate(double multiplier)
        {
            UpdateTime();
            return (int)(ContainersHandled / TimeOperational / 3600 / multiplier);
        }

        public int GetAverageSpeed(double multiplier)
        {
            UpdateTime();
            return (int)(DistanceDriven / TimeOperational / multiplier);
        }

        private void UpdateTime()
        {
            TimeOperational = (long)(DateTime.UtcNow - startTime).TotalSeconds;
        }

        public override string ToString()
        {
            UpdateTime();
            return $"<b>Total distance:</b> {((double)DistanceDriven / 1000).ToString("0.00")} km\n" +
                   $"<b>Time oper:</b> {TimeSpan.FromSeconds(TimeOperational).ToString()}\n" +
                   $"<b>Containers handled:</b> {ContainersHandled}\n" +
                   $"";
        }
    }
}
