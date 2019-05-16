using System;
using System.Collections.Generic;
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
    public class JsonPickupMessage
    {
        public string equipId;
        public string containerId;
        public string spreaderSize;
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

    public class RenderObject
    {
        public string id;
        public string status;
        public bool statusUpdate;
        public ObjectType objectType;
        public Vector3 scale;
        public Transform transform = null;
        public DesiredTransform desiredTransform;
        public Color color;
        public bool isRemoved = false;
        public bool hasSpreader = false;
        public RenderSpreader spreader;
        public List<Vector3> claim = null;

        public RenderObject(string id, ObjectType objectType, Transform transform, DesiredTransform desiredTransform, Vector3 scale, Color color, bool hasSpreader = false)
        {
            this.id = id;
            this.scale = scale;
            this.objectType = objectType;
            this.transform = transform;
            this.desiredTransform = desiredTransform;
            this.hasSpreader = hasSpreader;
            this.color = color;
            if (hasSpreader)
            {
                spreader = new RenderSpreader();
            }
            status = "";
            statusUpdate = false;
        }

        public bool IsContainer()
        {
            return this.objectType == ObjectType.CONTAINER_20 || this.objectType == ObjectType.CONTAINER_40;
        }
    }

    public class RenderSpreader
    {
        public bool spreaderContentUpdate = false;
        public SpreaderType spreaderType = SpreaderType.SPREADER_EMPTY;
        public string spreaderContents = "";
        public DesiredTransform desiredTransform = new DesiredTransform(Vector3.zero, Quaternion.AngleAxis(9f, Vector3.up), 0f, 0f);
    }

    public enum ObjectType
    {
        ASC,
        QC,
        AUTOSTRAD,
        CONTAINER_20,
        CONTAINER_40,
        CLAIM
    }

    public enum MessageType
    {
        UPDATE,
        DELETE,
        PICKUP,
        PUTDOWN,
        SPREADER,
        CLAIM,
        STATUS
    }

    public enum SpreaderType
    {
        SPREADER_20,
        SPREADER_40,
        SPREADER_TWIN_20,
        SPREADER_EMPTY
    }

    public class DesiredTransform
    {
        public DesiredTransform(Vector3 posArg, Quaternion rotArg, float moveSpeedArg, float rotateSpeedArg)
        {
            position = posArg;
            rotation = rotArg;
            moveSpeed = moveSpeedArg;
            rotateSpeed = rotateSpeedArg;
        }

        public Vector3 position;
        public Quaternion rotation;
        public float moveSpeed;
        public float rotateSpeed;
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