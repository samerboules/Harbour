using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.script.Util {
    public class DrawShape{

        private int width;
        private int height = 200;

        public Sprite DrawPolygon2D(List<float> values, float min, float max, Color color) {



            if (values.Count == 1) {
                values.Add(values[0]);
            }

            if (values.Count <= 5) {
                width = (values.Count - 1) * 80;
            } else if (values.Count > 5 && values.Count <= 15) {
                width = (values.Count - 1) * 40;
            } else if (values.Count > 15 && values.Count <= 30) {
                width = (values.Count - 1) * 20;
            } else if (values.Count > 30 && values.Count <= 50) {
                width = (values.Count - 1) * 10;
            } else {
                width = (values.Count - 1) * 5;
            }
            Texture2D texture = new Texture2D(width, 200);

            // set offsets for graph
            float pixelOffsetX = width / (values.Count - 1);
            float pixelOffsetY = height / (max - min);

            // draw graph
            for (int timer = 0; timer < values.Count - 1; timer++) {
                float value1 = values[timer];
                float value2 = values[timer + 1];

                float precentStart = ((value1 - min) / (max - min));
                float precentEnd = ((value2 - min) / (max - min));
                // amount of y change every x
                float formula = (precentEnd - precentStart) / pixelOffsetX * 200;

                for (int i = 0; i < pixelOffsetX; i++) {
                    var graphHeight = ((precentStart * 200) + (i * formula));
                    for (int j = 0; j < 200; j++) {
                        if (j < graphHeight) {
                            texture.SetPixel(i + (timer * (int)Math.Ceiling(pixelOffsetX)), j, color);
                        } else {
                            texture.SetPixel(i + (timer * (int)Math.Ceiling(pixelOffsetX)), j, Color.clear);
                        }

                    }
                }
            }
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, 200), Vector2.zero, 1); //create a sprite with the texture we just created and colored in


            return sprite;

        }


    }
}
