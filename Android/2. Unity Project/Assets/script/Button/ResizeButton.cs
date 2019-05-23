using Assets.script.Menu;
using HoloToolkit.Unity.InputModule;
using script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.script.Button {
    class ResizeButton : ScriptableriptableButton,  IInputClickHandler{

        public int increment;
        public Menu.Menu target;
        public Vector3 oldScale;

        public void OnInputClicked(InputClickedEventData eventData) {
            var scale = target.transform.localScale;
            scale = new Vector3(scale.x + 0.05f * increment, scale.y + 0.05f * increment, 1);
            target.transform.localScale = scale;
            var menuScale = target.menuScale;
            target.transform.Translate(Vector3.up * ((oldScale.y * scale.y / 2f) - (menuScale.y / 2f)));

            target.menuScale = new Vector3(oldScale.x * scale.x, oldScale.y * scale.y, 1);

           
            if (!target._fixedRot) {
                target._destination.position = target.transform.position;
            }
   
        }

        
        public override void SetTarget(Menu.Menu target) {
            this.target = target;
            this.oldScale = target.menuScale;
        }
    }
}

