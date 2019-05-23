using UnityEngine;

namespace Assets.script.Menu.MenuItems {
    public class UiTextBox {

        public UiText text;
        public GameObject gameObject;

        public UiTextBox(string name, GameObject parent, Vector2 size, string value, int textsize, TextAnchor textAlignment) {
            gameObject = new UiCanvas(name, size, Layer.button).gameObject;
            text = gameObject.AddComponent<UiText>();
            text.resizeTextForBestFit = true;
            text.resizeTextMaxSize = textsize;
            text.fontSize = textsize;
            text.Initialise(gameObject, value, textAlignment);

      
        }

        public void SetScale(Vector3 scale) {
            gameObject.GetComponent<RectTransform>().localScale = scale;
        }

        public void SetText(string newText) {
            text.text = newText;
        }

        
    }
}
