using UnityEngine;

namespace Assets.script.Menu.MenuItems {
    public class UiCube {

        public GameObject gameObject;

        public UiCube (GameObject parent, Vector3 scale) {
            gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = "cube";
            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.localScale = scale;
            gameObject.transform.localPosition = new Vector3(0, 0, 0);
            gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
            gameObject.GetComponent<Renderer>().material = Resources.Load("CubeMaterial") as Material;
        }

    }
}
