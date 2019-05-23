using Assets.script.Menu.Widgets;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Assets.script.Menu.MenuItems.Button {
    class ImageButton : UiButton {

        public ImageButton(GameObject parent, Vector3 scale, Vector3 position, string imageUrl, ButtonType buttonType,  Menu menu, Widget widget = null) : base(parent, scale, position, buttonType, menu, widget) {
            var image = button.AddComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>(imageUrl);
            image.sprite = sprite;
        }

    }
}
