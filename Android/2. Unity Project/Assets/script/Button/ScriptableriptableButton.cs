using HoloToolkit.Unity.InputModule;
using script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.script.Button {
    public abstract class ScriptableriptableButton  : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


        private void Start() {
            transform.GetChild(0).GetComponent<Image>().enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            transform.GetChild(0).GetComponent<Image>().enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            transform.GetChild(0).GetComponent<Image>().enabled = false;
        }

        public abstract void SetTarget(Menu.Menu target);
    }
}
