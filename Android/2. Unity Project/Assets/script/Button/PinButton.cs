using Assets.script.Menu;
using HoloToolkit.Unity.InputModule;
using script;
using System;

namespace Assets.script.Button {
    class PinButton : ScriptableriptableButton, IInputClickHandler {

        Menu.Menu menu;

        public void OnInputClicked(InputClickedEventData eventData) {
            menu.TogglePin();
        }

        public override void SetTarget(Menu.Menu target) {
            this.menu = target;
        }
    }
}
