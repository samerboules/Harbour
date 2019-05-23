using Assets.script.jsonElements;
using Assets.script.Menu.MenuItems;
using UnityEngine;
using static Assets.script.Util.Util;

namespace Assets.script.Menu.Widgets {
    public class TextWidget{

        public GameObject holder;

        public UiTextBox headerBox;
        public UiTextBox valueBox;

        public TextWidget(GameObject parent, MenuData data, Side side) {
            var scale1 = new Vector3(0.0012f, 0.0015f, 1);
            holder = new GameObject("text");
            holder.transform.SetParent(parent.transform);
            holder.transform.localPosition = new Vector3(0, 0, 0);
            holder.transform.localRotation = new Quaternion(0, 0, 0, 0);
            holder.transform.localScale = new Vector3(1,1,1);
            string header = data.header;
            if (data.unit != "none") header = header + " (" + data.unit + ")";

            headerBox = new UiTextBox("header", holder, new Vector2(750, 280), header, 100, TextAnchor.MiddleLeft);
            headerBox.SetScale(new Vector3(0.00133f, 0.0015f, 1));
            SetAnchor( headerBox.gameObject, AnchorAlignment.TopLeft, holder,scale1, new Vector3(0,0,0));
   
            valueBox = new UiTextBox("data", holder, new Vector2(750, 500), data.data, 200, TextAnchor.MiddleRight);
            valueBox.SetScale(new Vector3(0.00133f, 0.00115f, 1));
           SetAnchor(valueBox.gameObject, AnchorAlignment.BottomRight, holder,scale1, new Vector3(0, 0, 0));
        }

        public void Update(MenuData menuData) {
            valueBox.text.text = menuData.data;

        }



    }
}
