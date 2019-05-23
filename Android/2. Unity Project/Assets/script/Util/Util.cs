using UnityEngine;
using Vuforia;

namespace Assets.script.Util {

    public static class Util {
    public enum AnchorAlignment { TopLeft, BottomRight, TopCenter, None, PolygonBottomLeft, TopRight, BottomLeft }
        public static string InstanceIdToString(InstanceId instanceId) {
            switch (instanceId.DataType) {
                case InstanceIdType.NUMERIC: return instanceId.NumericValue.ToString();
                case InstanceIdType.STRING: return instanceId.StringValue;
                default: return "Mark type not yet implemented. Use string or numeric";
            }
        }


        public static void SetAnchor(GameObject target, AnchorAlignment alignment, GameObject parent, Vector3 scale, Vector3 offset) {
     
            target.transform.SetParent(parent.transform);
            target.GetComponent<RectTransform>().localScale = scale;
            var rectTranform = target.GetComponent<RectTransform>();
            var parentTransform = parent.GetComponent<RectTransform>();
            var newPosition = new Vector3();
            newPosition.z = offset.z;

            target.GetComponent<RectTransform>().localRotation = new Quaternion(0, 0, 0, 0);


            switch (alignment) {
                case AnchorAlignment.TopLeft: {
                        if (parent.GetComponent<Menu.Menu>() != null) {
                            Menu.Menu menu = parent.GetComponent<Menu.Menu>();
                            newPosition.x = -(menu.menuScale.x / 2) + (rectTranform.sizeDelta.x * rectTranform.localScale.x) / 2 + offset.x;
                            newPosition.y = (menu.menuScale.y / 2) - (rectTranform.sizeDelta.y * rectTranform.localScale.y) / 2 + offset.y;
                            target.GetComponent<RectTransform>().localPosition = newPosition;
                            break;
                        } else {
                            newPosition.x = (-0.5f + (rectTranform.sizeDelta.x * rectTranform.localScale.x) / 2 + offset.x);
                            newPosition.y = 0.5f - (rectTranform.sizeDelta.y * rectTranform.localScale.y) / 2 + offset.y;
                            target.GetComponent<RectTransform>().localPosition = newPosition;
                            break;
                        }

                    }
               
                case AnchorAlignment.BottomRight: {
                        newPosition.x = 0.5f - (rectTranform.sizeDelta.x * rectTranform.localScale.x) / 2 + offset.x;
                        newPosition.y = -0.5f + (rectTranform.sizeDelta.y * rectTranform.localScale.y) / 2 + offset.y;
                        target.GetComponent<RectTransform>().localPosition = newPosition;
                        break;
                    }
                case AnchorAlignment.BottomLeft: {
                        newPosition.x = -0.5f + (rectTranform.sizeDelta.x * rectTranform.localScale.x) / 2 + offset.x;
                        newPosition.y = -0.5f + (rectTranform.sizeDelta.y * rectTranform.localScale.y) / 2 + offset.y;
                        target.GetComponent<RectTransform>().localPosition = newPosition;
                        break;
                    }
                case AnchorAlignment.TopRight: {
                        newPosition.x = (0.5f - (rectTranform.sizeDelta.x * rectTranform.localScale.x) / 2) + offset.x;
                        newPosition.y = (0.5f - (rectTranform.sizeDelta.y * rectTranform.localScale.y) / 2) + offset.y;
                        target.GetComponent<RectTransform>().localPosition = newPosition;
                        break;
                    }
                case AnchorAlignment.TopCenter: {
                        newPosition.x = 0 + offset.x;
                        newPosition.y = 0.5f - (rectTranform.sizeDelta.y * rectTranform.localScale.y) / 2 + offset.y;
                        target.GetComponent<RectTransform>().localPosition = newPosition;
                        break;
                    }
                case AnchorAlignment.None: {
                        target.GetComponent<RectTransform>().localPosition = offset;
                        break;
                    }



            }
        }
    }
}
