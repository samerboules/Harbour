using System;
using System.Collections.Generic;
using System.Text;

namespace QSim.ConsoleApp.DataTypes
{
    public class Dimension
    {
        public int width, length, height;

        public Dimension(int _width, int _lenght, int _height)
        {
            width = _width;
            length = _lenght;
            height = _height;
        }

        public override string ToString()
        {
            return $"({length}, {width}, {height})";
        }
    }
}
