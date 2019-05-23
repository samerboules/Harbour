using UnityEngine;
using System.Collections.Generic;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static Vector3 ToWorld(this Vector3 input)
        {
            return input.InverseAxes().ToWorldOrigin().ToMeter();
        }

        public static Vector3 ToLocal(this Vector3 input)
        {
            return input.InverseAxes().ToMeter();
        }

        public static Vector3 ToWorldOrigin(this Vector3 input)
        {
            return input + new Vector3(-330000f, 0f, 220000f);
        }

        public static Vector3 UnityToPort(this Vector3 input)
        {
            return input * 1000 + new Vector3(330000f, 0f, 220000f);
        }

        public static Vector3 ToMeter(this Vector3 input)
        {
            return input / 1000f;
        }

        public static Vector3 InverseAxes(this Vector3 input)
        {
            return new Vector3(input.x, input.z, -input.y);
        }

        public static float ToLocalRotation(this int input)
        {
            return input * 0.0572957795131f; // 0.001 * PI / 180
        }

        public static bool IsValid(this Vector3 input)
        {
            return !float.IsNaN(input.x) && !float.IsNaN(input.y) && !float.IsNaN(input.z);
        }

        public static bool IsApproximately(this Vector3 input, Vector3 other)
        {
            return Mathf.Approximately(input.x, other.x) &&
                   Mathf.Approximately(input.y, other.y) &&
                   Mathf.Approximately(input.z, other.z);
        }
    }

    public static class TransformExtensions
    {
        public static List<GameObject> FindObjectsWithTag(this Transform parent, string tag)
        {
            List<GameObject> taggedGameObjects = new List<GameObject>();

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.tag == tag)
                {
                    taggedGameObjects.Add(child.gameObject);
                }
                if (child.childCount > 0)
                {
                    taggedGameObjects.AddRange(FindObjectsWithTag(child, tag));
                }
            }
            return taggedGameObjects;
        }
    }
}
