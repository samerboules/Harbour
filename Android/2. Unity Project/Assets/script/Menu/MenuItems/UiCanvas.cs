using UnityEngine;

namespace Assets.script.Menu.MenuItems {

    public class UiCanvas {

        public GameObject gameObject;
        public Canvas canvas;

        public UiCanvas(string name, Vector2 size, Layer layer) {
            gameObject = new GameObject(name);
            canvas = gameObject.AddComponent<Canvas>();
            gameObject.GetComponent<RectTransform>().sizeDelta = size;
            gameObject.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);

            canvas.renderMode = RenderMode.WorldSpace;

            canvas.sortingOrder = (int)layer;
        }

        

    }
}
