using Assets.script.Button;
using Assets.script.Menu.Widgets;
using UnityEngine;
using static Assets.script.Util.Util;
using Image = UnityEngine.UI.Image;

namespace Assets.script.Menu.MenuItems {

    public enum ButtonType { rotate, scale_up, scale_down, WidgetState }

    public class UiButton {
        public UiText text;
        public GameObject button;
        public GameObject overlay;
        public Vector2 size = new Vector2(100, 100);

        public UiButton(GameObject parent, Vector3 scale, Vector3 position, ButtonType buttonType,  Menu menu, Widget widget = null) {

            button = new UiCanvas("button", size, Layer.button).gameObject;

            overlay = new UiCanvas("overlay", size, Layer.overlay).gameObject;
            SetAnchor(overlay, AnchorAlignment.None, button, new Vector3(1, 1, 1), new Vector3(0,0,0));

            overlay.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            button.AddComponent<BoxCollider>();
            button.GetComponent<BoxCollider>().size = new Vector3(size.x, size.y, 0.005f);

            overlay.AddComponent<Image>();
            overlay.GetComponent<Image>().color = new Color32(125, 125, 125, 100);

            switch (buttonType) {
                case ButtonType.rotate: {
                        var script = button.AddComponent<PinButton>();
                        script.SetTarget(menu); break;
                    }
                case ButtonType.scale_down: {
                        var script = button.AddComponent<ResizeButton>();
                        script.increment = -1;
                        script.SetTarget(menu); break;
                    }
                case ButtonType.scale_up: {
                        var script = button.AddComponent<ResizeButton>();
                        script.increment = 1;
                        script.SetTarget(menu); break;
                    }
                case ButtonType.WidgetState: {
                        var script = button.AddComponent<ChangeWidgetType>();
                        script.SetWidget(widget, menu);
                        break;
                    }
            }


        }

        public void SetScale(Vector3 scale) {
            button.GetComponent<RectTransform>().localScale = scale;
        }

       

    }
}
