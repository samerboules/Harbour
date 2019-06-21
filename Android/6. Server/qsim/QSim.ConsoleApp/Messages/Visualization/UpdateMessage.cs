using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QSim.ConsoleApp.DataTypes;

namespace QSim.ConsoleApp.Messages.Visualization
{
    public class UpdateMessage
    {
        public string id;
        [JsonConverter(typeof(StringEnumConverter))]
        public ObjectType type;
        public int x, y, z, p, l, w, h, dt;
        public long c;
    }
}
