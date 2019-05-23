using Assets.script.jsonElements;
using Assets.script.Menu.MenuItems;
using Assets.script.Menu.Widgets;
using script.Vumark;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Vuforia;

namespace Assets.script.Menu {

    public enum Side { front = -1, back = 1, None = -1 }
    public enum Layer { background = -1, content = 0, button = 1, text = 2, overlay = 3 }

    public class Menu : MonoBehaviour {

        string id;
        public bool _fixedRot;

        public CubeTransform _destination;
        public CubeTransform _destanationVer;
        public CubeTransform _destanationHor;

        private Widgetmanager widgetmanager;
        public UiContentPanel front;
        public UiContentPanel back;


        public Vector3 menuScale;

        public void StartUp(VuMarkBehaviour mark) {
            id = Util.Util.InstanceIdToString(mark.VuMarkTarget.InstanceId);
            widgetmanager = new Widgetmanager(this);
            StartCoroutine(GetData(mark));
            InvokeRepeating(nameof(GetDataForUpdate), 10, 10);
            if (SystemInfo.deviceType == DeviceType.Handheld) {
                _fixedRot = true;
            }
        }

        public IEnumerator GetData(VuMarkBehaviour mark) {
            UnityWebRequest request = UnityWebRequest.Get("http://hololensmartijn.azurewebsites.net/api/Todo/" + "{ \"id\" : \"" + id + "\"}");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) {
                Debug.Log(request.error);
            } else {
                var data = JsonUtility.FromJson<JsonData>(request.downloadHandler.text);
                widgetmanager.SetData(data);
                CreateMenu(mark, data.title);
                widgetmanager.InitialiseWigets(Side.front);
                widgetmanager.InitialiseWigets(Side.back);


            }
        }

        public void CreateMenu(VuMarkBehaviour mark, string title) {
            name = (id + "_menus"); //temp

            int input;
            Int32.TryParse(id, out input);
            menuScale = widgetmanager.GetMenuScale();
            var cube = new UiCube(this.gameObject, menuScale);
            front = new UiContentPanel(cube.gameObject, menuScale, this, Side.front, title);
            back = new UiContentPanel(cube.gameObject, menuScale, this, Side.back, title);

            var target = GameObject.Find(id);
            transform.position = target.transform.position;
        }

        public void GetDataForUpdate() {
            StartCoroutine(nameof(UpdateMenu));
        }

        public IEnumerator UpdateMenu() {
            UnityWebRequest request = UnityWebRequest.Get("http://hololensmartijn.azurewebsites.net/api/Todo/" + "{ \"id\" : \"" + id + "\"}");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) {
                Debug.Log(request.error);
            } else {
                var data = JsonUtility.FromJson<JsonData>(request.downloadHandler.text);
                widgetmanager.UpdateWidgets(data);
            }
        }


        private void Update() {
            switch (SystemInfo.deviceType) {
                case DeviceType.Handheld: HandHeldUpdate(); break;
                case DeviceType.Desktop: HoloLensUpdate(); break;
                case DeviceType.Unknown: HoloLensUpdate(); break;
            }
        }

        public void HoloLensUpdate() {
            if (_destination != null) {
                if (!CheckDiffer()) {
                    transform.position = Vector3.Lerp(transform.position, _destination.position, 1f * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, _destination.rotation, 1f * Time.deltaTime);
                }
            }
        }

        public void HandHeldUpdate() {
            if (VuMarkSource.VumarkExists(id)) {
                transform.position = _destination.position;
                transform.rotation = _destination.rotation;
                transform.localScale = new Vector3(1, 1, 1);
            } else {
                transform.localScale = new Vector3(0, 0, 0);
            }
        }

        public void UpdatePosition() {
            // move and rotate menu to match the vumark

            if (VuMarkSource.VumarkExists(id)) {
                var target = GameObject.Find(id);
                target.transform.Translate(Vector3.up * 0.01f);
                _destanationHor = new CubeTransform(target.transform.GetChild(0).transform);
                target.transform.Translate(Vector3.down * 0.01f);

                target.transform.Translate(Vector3.up * (menuScale.y / 2));
                _destanationVer = new CubeTransform(target.transform);
                target.transform.Translate(Vector3.down * (menuScale.y / 2));

                if (!_fixedRot) {
                    _destination = _destanationVer;
                } else {
                    _destination = _destanationHor;
                }
            }
        }

        private bool CheckDiffer() {

            if (Vector3.Distance(transform.position, _destination.position) > 0.005 || Vector3.Distance(transform.rotation.eulerAngles, _destination.rotation.eulerAngles) > 1.8f) {
                return false;
            } else {
                return true;
            }

        }

        public void TogglePin() {
            if (!_fixedRot) {
                _fixedRot = true;
                _destination = _destanationHor;
            } else {
                _fixedRot = false;
                _destination = _destanationVer;
            }
        }
    }
}
