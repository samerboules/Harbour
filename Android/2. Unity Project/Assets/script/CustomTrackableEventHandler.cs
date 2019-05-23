using Assets.script.Util;
using script.Vumark;
using UnityEngine;
using Vuforia;

namespace script {
    public class CustomTrackableEventHandler : MonoBehaviour, ITrackableEventHandler {
        private TrackableBehaviour _mTrackableBehaviour;

        protected virtual void Start() {
            _mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            if (_mTrackableBehaviour) {
                _mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }
        }

        protected virtual void OnDestroy() {
            if (_mTrackableBehaviour) {
                _mTrackableBehaviour.UnregisterTrackableEventHandler(this);
            }
        } 


        public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus,
            TrackableBehaviour.Status newStatus) {
            switch (newStatus) {
                case TrackableBehaviour.Status.DETECTED:
                OnTrackingFound();
                break;
                case TrackableBehaviour.Status.TRACKED:
                OnTrackingFound();
                break;
                case TrackableBehaviour.Status.EXTENDED_TRACKED:
                OnTrackingFound();
                break;
                default:
                OnTrackingLost();
                break;

            }
        }

       
        protected virtual void OnTrackingFound() {
            foreach (var behaviour in VuMarkSource.getVuMarkBehaviours()) {
                if (GetComponent<TrackableBehaviour>() == behaviour) {
                        gameObject.transform.GetChild(0).name = Util.InstanceIdToString(behaviour.VuMarkTarget.InstanceId);
                    //GameObject.Find("InputManager").GetComponent<MenuManager>().Add(Util.InstanceIdToString(behaviour.VuMarkTarget.InstanceId));
                }
            }
        

            var rendererComponents = GetComponentsInChildren<Renderer>(true);
            var colliderComponents = GetComponentsInChildren<Collider>(true);
            var canvasComponents = GetComponentsInChildren<Canvas>(true);

            // Enable rendering:
            foreach (var component in rendererComponents) {
                component.enabled = true;
            }

            // Enable colliders:
            foreach (var component in colliderComponents) {
                component.enabled = true;
            }

            // Enable canvas':
            foreach (var component in canvasComponents) {
                component.enabled = true;
            }
        }


        protected virtual void OnTrackingLost() {
            var rendererComponents = GetComponentsInChildren<Renderer>(true);
            var colliderComponents = GetComponentsInChildren<Collider>(true);
            var canvasComponents = GetComponentsInChildren<Canvas>(true);

            // Disable rendering:
            foreach (var component in rendererComponents) {
                component.enabled = false;
            }

            // Disable colliders:
            foreach (var component in colliderComponents) {
                component.enabled = false;
            }

            // Disable canvas':
            foreach (var component in canvasComponents) {
                component.enabled = false;
            }
        }



    }


}