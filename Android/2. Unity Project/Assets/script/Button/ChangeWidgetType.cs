using Assets.script.Menu.Widgets;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Assets.script.Button {
    class ChangeWidgetType : ScriptableriptableButton, IInputClickHandler {

        public Menu.Menu menu;
        public Widget widget;

        public void OnInputClicked(InputClickedEventData eventData) {
            switch (widget.widgetType) {
                case WidGetType.image: {
                        widget.pictureW.holder.transform.localScale = new Vector3(0, 0, 0);
                        widget.holder.holder.transform.localScale = new Vector3(1, 1, 1);
                        widget.widgetType = WidGetType.text;
                      
                        break;
                    }

                case WidGetType.text: { 
                        widget.pictureW.holder.transform.localScale = new Vector3(1, 1, 1);
                        widget.holder.holder.transform.localScale = new Vector3(0, 0, 0);
                        widget.widgetType = WidGetType.image;
                        widget.pictureW.DrawImage();
                        break;
                    }
            }
        }

        public override void SetTarget(Menu.Menu target) {
            this.menu = target;
        }

        public void SetWidget(Widget widget, Menu.Menu menu) {
            SetTarget(menu);
            this.widget = widget;
        }
    }
}
