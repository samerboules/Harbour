using QSim.ConsoleApp.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QSim.ConsoleApp.Messages
{
    public class JsonMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType type;
        public object content;
    }
}
