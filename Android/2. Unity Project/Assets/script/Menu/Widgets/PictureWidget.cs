using Assets.script.jsonElements;
using Assets.script.Menu.MenuItems;
using Assets.script.Util;
using System;
using System.Linq;
using UnityEngine;
using static Assets.script.Util.Util;

namespace Assets.script.Menu.Widgets {
    public class PictureWidget {

        public GameObject holder;
        public GameObject polygon;

        public UiTextBox headerBox;
        public UiTextBox yMax;
        public UiTextBox yMin;
        public UiTextBox xMax;
        public UiTextBox xMin;

        Graph graph;

        public PictureWidget(GameObject parent, MenuData data, Side side) {
            holder = new GameObject("image");
            holder.transform.SetParent(parent.transform);
            holder.transform.localPosition = new Vector3(0, 0, 0);
            holder.transform.localRotation = new Quaternion(0, 0, 0, 0);
            holder.transform.localScale = new Vector3(1, 1, 1);
            this.graph = data.graph;

            headerBox = new UiTextBox("header", holder, new Vector2(750, 280), data.header, 100, TextAnchor.MiddleCenter);
            SetAnchor(headerBox.gameObject, AnchorAlignment.TopRight, holder,new Vector3(0.8f / 750,0.33f / 280,1), new Vector3(0, 0, 0));

            yMax = new UiTextBox("yMax", holder, new Vector2(200, 100), "", 100, TextAnchor.MiddleRight);
           SetAnchor(yMax.gameObject, AnchorAlignment.TopLeft, holder,new Vector3(0.002f, 0.0005f,1), new Vector3(-0.175f, -0.175f, 0));
            yMax.gameObject.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 90);

            yMin = new UiTextBox("yMin", holder, new Vector2(200, 100), "", 100, TextAnchor.MiddleLeft);
            SetAnchor(yMin.gameObject, AnchorAlignment.BottomLeft, holder, new Vector3(0.002f, 0.0005f, 1), new Vector3(-0.175f, 0.175f + 0.1f, 0));
            yMin.gameObject.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 90);

            var scale2 = new Vector3(0.0008f, 0.0007f, 1); ;

            xMin = new UiTextBox("xmin", holder, new Vector2(350, 200), "", 100, TextAnchor.MiddleLeft);
            SetAnchor(xMin.gameObject, AnchorAlignment.BottomLeft, holder,scale2, new Vector3(0.05f, 0, 0));

            xMax = new UiTextBox("xMax", holder, new Vector2(350, 200), "", 100, TextAnchor.MiddleRight);
            SetAnchor(xMax.gameObject, AnchorAlignment.BottomRight, holder, scale2, new Vector3(0, 0, 0));

            polygon = new GameObject("pol");
            polygon.AddComponent<RectTransform>();
            SpriteRenderer sr = polygon.AddComponent<SpriteRenderer>(); // add a sprite renderer  
            sr.sortingOrder = (int)Layer.text;

        }

        public void Update(Graph graph) {
            this.graph = graph;
        }

        public void DrawImage() {

            DrawShape drawwer = new DrawShape();

            float min = graph.graph.Min();
            float max = graph.graph.Max();

            float offset = (max - min) / 10;
            if (offset == 0) {
                offset = 1;
            }

            min = min - offset;
            max = max + offset;

            yMax.SetText(Math.Round(max, 2).ToString());
            yMin.SetText(Math.Round(min, 2).ToString());
            xMin.SetText(graph.graph_start);
            xMax.SetText(graph.graph_end);

            Sprite sprite = drawwer.DrawPolygon2D(graph.graph, min, max, new Color32(50, 105, 193, 120));
            polygon.GetComponent<SpriteRenderer>().sprite = sprite;
            polygon.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);

            Vector3 scale = new Vector3((1 / (sprite.rect.width)) * 0.95f, 0.005f * 0.85f, 1);
            polygon.GetComponent<RectTransform>().localScale = scale;
           SetAnchor(polygon, AnchorAlignment.TopRight, holder, scale, new Vector3((scale.x / 2 * -1) * sprite.rect.width, (scale.y / 2 * -1) * sprite.rect.height, 0));
        }



    }
}
