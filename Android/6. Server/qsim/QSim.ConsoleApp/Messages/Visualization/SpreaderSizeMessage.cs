using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QSim.ConsoleApp.DataTypes;

namespace QSim.ConsoleApp.Messages.Visualization
{
    class SpreaderSizeMessage
    {
        public string equipId;
        [JsonConverter(typeof(StringEnumConverter))]
        public SpreaderSize spreaderSize;
        public int dt;
    }
}
