using UnityEngine;
using UnityEngine.UI;

namespace Assets.script.Menu.MenuItems {
    public class UiText : Text {

        public void Initialise(GameObject parent, string text, TextAnchor alignment) {
            font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            fontSize = 90;
            color = new Color32(255,255,255,255);
            this.alignment = alignment;
            this.text = text;
        }

    }
}
