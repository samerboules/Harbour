using QSim.ConsoleApp.DataTypes;

namespace QSim.ConsoleApp.Middleware.StackingSystem
{
    public class StackEntry
    {
        public Container Container;
        public ContainerLength Length;
        public bool IsReserved;
        public bool IsOccupied
        {
            get { return Container != null;  }
        }

        public bool FreeToPick
        {
            get { return !IsReserved && IsOccupied; }
        }

        public bool FreeToReserve
        {
            get { return !IsReserved && !IsOccupied; }
        }

        public bool IsLength(ContainerLength length)
        {
            return Length == ContainerLength.UNKNOWN || Length == length;
        }

        public StackEntry(ContainerLength length)
        {
            Container = null;
            IsReserved = false;
            Length = length;
        }

        public override string ToString()
        {
            return $"Container: {Container} Reserverd:{IsReserved} Occupied:{IsOccupied}";
        }
    }
}
