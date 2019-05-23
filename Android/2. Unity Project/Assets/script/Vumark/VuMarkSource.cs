using Assets.script.Util;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

namespace script.Vumark{
    public static class VuMarkSource {
        private static VuMarkBehaviour Behaviour;
        private static VuMarkManager Manager;

        static VuMarkSource() {
            Behaviour = GameObject.Find("VuMark").GetComponent<VuMarkBehaviour>();
            Manager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        }

        public static bool VumarkExists(string id) {
            foreach (var mark in VuMarkSource.getVuMarkBehaviours()) {
                if (Util.InstanceIdToString(mark.VuMarkTarget.InstanceId) == id) {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<VuMarkTarget> getVuMarkTargets() {
            Manager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
            return Manager.GetActiveVuMarks();
        }

        public static IEnumerable<VuMarkBehaviour> getVuMarkBehaviours() {
            Manager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
            return Manager.GetActiveBehaviours();
        }

        public static VuMarkTarget GetVuMarkTarget() {
            foreach (var bhvr in Manager.GetActiveBehaviours()) {
                return bhvr.VuMarkTarget;
            }
            return null;
        }

        public static string GetCurrentVuMarkId() {
            return GetVuMarkTarget().InstanceId.StringValue;
        }

        public static VuMarkTarget GetLastVuMark() {
            VuMarkTarget output = null;
            foreach (var target in getVuMarkTargets()) {
                output = target;
            }
            return output;
        }

        public static string GetLastVuMarkId() {
            VuMarkTarget output = null;
            foreach(var target in getVuMarkTargets()) {
                output = target;
            }
            return output.InstanceId.StringValue;
        }

        public static VuMarkBehaviour GetVuMarkBehaviour() {
            return Behaviour;
        }

        public static GameObject GetVuMark(){
            return GameObject.Find("VuMark");
        }

        public static Transform GetVuMarkTransform() {
            return GameObject.Find("VuMark").transform;
        }

        public static Vector3 GetVuMarkPosition(){
            return GameObject.Find("VuMark").transform.position;
        }

        public static Quaternion GetVuMarkRotation() {
            return GameObject.Find("VuMark").transform.rotation;
        }

    }
}