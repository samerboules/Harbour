using Assets.script.jsonElements;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.script.Util.Util;

namespace Assets.script.Menu.Widgets {
    public class Widgetmanager {

        public List<Widget> placeholders;
        Menu menu;
        Vector3 menuScale;

        float topBarheight = 0.0235f;
        float margin = 0.005f;
        float widgetHeight = 0.06f;
        float widgetWidth = 0.12f;

        int cols;
        int rows;

        JsonData data;

        public Widgetmanager(Menu menu) {
            this.menu = menu;
            placeholders = new List<Widget>();

        }


        public void SetData(JsonData data) {


            this.data = data;
            cols = (int)Math.Ceiling(Math.Sqrt(data.menus.Count) / 2);
            rows = (int)Math.Ceiling(data.menus.Count / (float)cols);
            CalculateMenuScale();
        }

        public void InitialiseWigets(Side side) {
            for (int i = 0; i < data.menus.Count; i++) {
                var widget = new Widget(data.menus[i].header, menu.gameObject, new Vector3(widgetWidth, widgetHeight, 1), CalculatePosition(i, side), AnchorAlignment.TopLeft);
                    widget.InitWidget(data.menus[i], side);
                    placeholders.Add(widget);

            }
        }

        public Vector3 CalculatePosition(int place, Side side) {
            if (side == Side.back) {
                Vector3 position = new Vector3(0, 0, 0);
                position.x = menuScale.x - widgetWidth - (margin + ((margin + widgetWidth) * (place % cols)));
                position.y = -topBarheight - margin + ((-margin - widgetHeight) * (place / cols));
                position.z = 0.011f * (int)side;
                return position;
            } else {
                Vector3 position = new Vector3(0, 0, 0);
                position.x = margin + ((margin + widgetWidth) * (place % cols));
                position.y = -topBarheight - margin + ((-margin - widgetHeight) * (place / cols));
                position.z = 0.011f * (int)side;
                return position;
            }

        }

        public void CalculateMenuScale() {
            menuScale = new Vector3(margin + ((widgetWidth + margin) * cols), margin + topBarheight + ((widgetHeight + margin) * rows), 0.02f);
        }

        public Vector3 GetMenuScale() {
            return menuScale;
        }

        public void UpdateWidgets(JsonData data) {
            foreach (MenuData menuData in data.menus) {
                foreach (Widget widget in placeholders) {
                    if (widget.widget.gameObject.name == menuData.header) {
                        widget.Update(menuData);
                    }
                }
            }
        }
    }

}