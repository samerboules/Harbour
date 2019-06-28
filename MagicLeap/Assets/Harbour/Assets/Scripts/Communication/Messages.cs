using System;
using UnityEngine;

namespace DataObjects
{
    [Serializable]
    public class JsonMessage<T>
    {
        public string type;
        public T content;
    }

    [Serializable]
    public class JsonUpdateMessage
    {
        public string id;
        public string type;
        public int x;
        public int y;
        public int z;
        public int p;
        public int l;
        public int w;
        public int h;
        public int dt;
        public long c;
    }

    [Serializable]
    public class JsonDeleteMessage
    {
        public string id;
    }

    [Serializable]
    public class JsonSpreaderMessage
    {
        public string id;
        public int x;
        public int y;
        public int z;
        public int p;
        public int dt;
    }

    [Serializable]
    public class JsonSpreaderSizeMessage
    {
        public string equipId;
        public string spreaderSize;
        public int dt;
    }

    [Serializable]
    public class JsonPickupMessage
    {
        public string equipId;
        public string containerId;
    }

    [Serializable]
    public class JsonPutDownMessage
    {
        public string equipId;
    }

    [Serializable]
    public class JsonClaimMessage
    {
        public string id;
        public long color;
        public IntPoint[] points;
    }

    [Serializable]
    public class IntPoint
    {
        public int x;
        public int y;
    }

    [Serializable]
    public class JsonStatusMessage
    {
        public string id;
        public string status;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
}
