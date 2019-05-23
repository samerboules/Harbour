using Assets.script.Menu.MenuItems.Button;
using UnityEngine;
using static Assets.script.Util.Util;
using Image = UnityEngine.UI.Image;

namespace Assets.script.Menu.MenuItems {
    public class TopBar{

        private float topBarHeight = 0.0235f;

        public UiTextBox title;
        public GameObject gameObject;


        public TopBar(GameObject parent, Vector3 menuScale, Menu menu, string text) {


            Vector3 topBarScale = new Vector3(1, 1 / (menuScale.y / topBarHeight), 1);

            gameObject = new UiCanvas("topBar", new Vector2(1, 1), Layer.content).gameObject;
           SetAnchor(gameObject, AnchorAlignment.TopLeft, parent.gameObject, topBarScale, new Vector3(0, 0, 0));

            var background = gameObject.AddComponent<Image>();
            background.color = new Color32(0, 0, 0, 180);

            title = new UiTextBox("title", gameObject, new Vector2(350, 100), text, 90,TextAnchor.MiddleLeft);
           SetAnchor(title.gameObject, AnchorAlignment.BottomRight, gameObject, new Vector3(0.8f / 350, 0.01f, 1), new Vector3(0, 0, 0));

            var button = new ImageButton(gameObject, new Vector3(0.01f * (gameObject.GetComponent<RectTransform>().localScale.x / gameObject.GetComponent<RectTransform>().localScale.y), 0.01f, 1), new Vector3(0, 0, 0), "rotate", ButtonType.rotate, menu);
            SetAnchor(button.button, AnchorAlignment.TopLeft, gameObject, new Vector3(0.01f * (gameObject.GetComponent<RectTransform>().localScale.y / gameObject.GetComponent<RectTransform>().localScale.x), 0.01f, 1), new Vector3(0, 0, 0));


        }

    }
}
