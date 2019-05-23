using Assets.script.jsonElements;
using Assets.script.Menu.MenuItems;
using Assets.script.Menu.MenuItems.Button;
using UnityEngine;
using static Assets.script.Util.Util;
using Image = UnityEngine.UI.Image;

namespace Assets.script.Menu.Widgets {

    public enum WidGetType { text, image }

    public class Widget {

        public UiCanvas widget;
        public TextWidget holder;
        public PictureWidget pictureW;
        public TextButton button;

        public WidGetType widgetType;

        public Widget(string name, GameObject parent, Vector3 scale, Vector3 position, AnchorAlignment alignment) {
            widget = new UiCanvas(name, new Vector2(1, 1), Layer.content);
            Util.Util.SetAnchor(widget.gameObject, alignment, parent,scale, position);

            widget.gameObject.AddComponent<Image>();
            widget.gameObject.GetComponent<Image>().color = new Color32(113, 147, 147, 160);
                button = new TextButton(widget.gameObject, new Vector3(1, 1, 0), new Vector3(0, 0, 0), "", ButtonType.WidgetState, parent.GetComponent<Menu>(), this);
                SetAnchor(button.button, AnchorAlignment.TopLeft, widget.gameObject, new Vector3(1, 1, 0), position);
                button.button.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                button.button.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                button.button.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                button.overlay.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                button.button.GetComponent<BoxCollider>().size = new Vector3(1, 1, 0.005f);

            widgetType = WidGetType.text;

        }

        public void InitWidget(MenuData data, Side side) {
            holder = new TextWidget(widget.gameObject, data, side);
            pictureW = new PictureWidget(widget.gameObject, data, side);

            pictureW.holder.transform.localScale = new Vector3(0, 0, 0);
            if (side == Side.back) {
                widget.gameObject.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 180, 0);
            }


            if (data.graph.graph.Count == 0) {
                button.button.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
            }
        }

        public void Update(MenuData data) {
            holder.Update(data);

            if (data.graph.graph.Count > 1) {
                pictureW.Update(data.graph);
            }
            if (widgetType == WidGetType.image) {
                pictureW.DrawImage();
            }

        }

    }
}
