using QSim.ConsoleApp.Utilities;
using System.Collections.Generic;

namespace QSim.ConsoleApp.DataTypes
{
    public class Container
    {
        public const int DefaultHeight = 2896;
        public const int DefaultWidth = 2438;
        public const int Length40ft = 12192;
        public const int Length20ft = 6058;

        public string Number { get; private set; }
        public ContainerLength Length { get; private set; }
        public ContainerHeight Height { get; private set; }
        public Dimension Dimension { get; private set; }
        public double Weight { get; private set; }
        public string Contents { get; private set; }

        public Container(string number, ContainerLength length)
        {
            Number = number;
            Length = length;
            Height = ContainerHeight.HEIGHT_9_6;
            Weight = (RandomNumberGenerator.NextNumber(1000, 16000) + 4200) / 1000.0;
            Contents = RandomContents[RandomNumberGenerator.NextNumber(RandomContents.Count)];
            Dimension = new Dimension(
                DefaultWidth,
                length == ContainerLength.LENGTH_40 ? Length40ft : Length20ft,
                DefaultHeight);
        }

        public override string ToString()
        {
            return $"CONTAINER({Number})";
        }

        public SpreaderSize GetSpreaderSize()
        {
            return Length == ContainerLength.LENGTH_20 ? SpreaderSize.SPREADER_20 : SpreaderSize.SPREADER_40;
        }

        public string GetStatistics()
        {
            return $"Weight: {Weight.ToString(".00")} T\n" +
                   $"Contents: {Contents}\n" +
                   $"Dimension (lwh):\n{Dimension}\n" +
                   $"";
        }

        private static List<string> RandomContents = new List<string>()
        {
            "Car parts", "Airplane parts", "Clothing", "Fruit", "Bulk gear", "Electronics", "Bananas", "Clothing", "Chemicals", "Dry goods", "Cars"
            // Ideas? Please add more.
        };
    }
}
