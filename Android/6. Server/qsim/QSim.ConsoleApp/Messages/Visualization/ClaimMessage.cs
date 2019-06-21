using System.Collections.Generic;

namespace QSim.ConsoleApp.Messages.Visualization
{
    public class ClaimMessage
    {
        public string id;
        public long color;
        public List<JsonIntPoint> points;
    }

    public class JsonIntPoint
    {
        public int x, y;

        public JsonIntPoint(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }
}
