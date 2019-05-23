using UnityEngine;

namespace Assets.script.Menu.MenuItems {
    public class CubeTransform {
        public Vector3 position;
        public Quaternion rotation;

        public CubeTransform(Transform transform) {
            position = transform.position;
            rotation = transform.rotation;
        }
    }
}
