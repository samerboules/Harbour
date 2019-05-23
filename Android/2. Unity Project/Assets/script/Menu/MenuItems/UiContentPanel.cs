using UnityEngine;
using static Assets.script.Util.Util;
using Image = UnityEngine.UI.Image;

namespace Assets.script.Menu.MenuItems {
    public class UiContentPanel {

        public TopBar topBar;
        public GameObject gameObject;

        public UiContentPanel(GameObject parent, Vector3 menuScale, Menu menu, Side side, string title) {
            gameObject = new UiCanvas(side.ToString(), new Vector2(1, 1), Layer.background).gameObject;
            SetAnchor(gameObject, AnchorAlignment.None, menu.gameObject, menuScale, new Vector3(0, 0, (menuScale.z / 2 + 0.0002f) * (int)side));

            gameObject.AddComponent<Image>();
            gameObject.GetComponent<Image>().color = new Color32(67, 78, 130, 180);

            if (side == Side.back) {
                gameObject.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 180, 0);
            }

            topBar = new TopBar(gameObject, menuScale, menu, title);

           
        }
    }
}
