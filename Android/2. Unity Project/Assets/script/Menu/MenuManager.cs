using Assets.script.Menu;
using Assets.script.Util;
using script.Vumark;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace script {
    public class MenuManager : MonoBehaviour {

        private static Dictionary<string, GameObject> menus = new Dictionary<string, GameObject>();
        private void Update() {

            foreach (var mark in VuMarkSource.getVuMarkBehaviours()) {
                var id = Util.InstanceIdToString(mark.VuMarkTarget.InstanceId);
                if (menus.Keys.Contains(id)) {
                    menus[id].GetComponent<Menu>().UpdatePosition();
                } else {
                    menus.Add(id, new GameObject("init"));
                    var menu = menus[id].AddComponent<Menu>();
                    menu.StartUp(mark);
                }
            }
        }

        public static void Remove(string key) {
            menus.Remove(key);
        }
    }

}