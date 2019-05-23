using Assets.script.Menu.Widgets;
using UnityEngine;
using static Assets.script.Util.Util;

namespace Assets.script.Menu.MenuItems.Button {
    public class TextButton : UiButton{

        public TextButton(GameObject parent, Vector3 scale, Vector3 position, string text, ButtonType buttonType, Menu menu, Widget widget = null) : base (parent, scale, position, buttonType, menu, widget) {
            SetAnchor(button.gameObject, AnchorAlignment.None, parent, scale, position);
        }

    }
}
