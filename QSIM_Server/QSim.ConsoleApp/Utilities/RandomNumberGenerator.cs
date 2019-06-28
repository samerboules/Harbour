using System;
using System.Collections.Generic;
using System.Text;

namespace QSim.ConsoleApp.Utilities
{
    class RandomNumberGenerator
    {
        private static Object lockObject = new Object();
        private static Random _random = new Random();

        public static int NextNumber(int range)
        {
            lock (lockObject)
            {
                return _random.Next(range);
            }
        }

        public static int NextNumber(int min, int max)
        {
            return NextNumber(max - min + 1) + min;
        }
    }
}
