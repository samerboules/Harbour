using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QSim.ConsoleApp.Utilities
{
    public static class JsonConversion
    {
        public static byte[] SerializeMessage(object obj)
        {
            string jsonString = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public static object DeserializeMessage(byte[] message, Type type)
        {
            string jsonString = Encoding.UTF8.GetString(message);
            return JsonConvert.DeserializeObject(jsonString, type);
        }

        public static T DeserializeMessage<T>(byte[] message)
        {
            string jsonString = Encoding.UTF8.GetString(message);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
