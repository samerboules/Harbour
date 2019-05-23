using Assets.script.jsonElements;
using Assets.script.Util;
using script.Vumark;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

namespace script {
    public class MenuUpdate : MonoBehaviour {

        private VuMarkBehaviour vuMark;
        private string id;
        private Vector3 scale;
        private CubeTransform _destination;
        private CubeTransform _destanationVer;
        private CubeTransform _destanationHor;
        private bool _fixedRot;

      
        public void StartUp(VuMarkBehaviour mark) {
            id = Util.InstanceIdToString(mark.VuMarkTarget.InstanceId);
            // link menu to vumark
            vuMark = mark;
            name = "UI_" + id;
          // save scale for open close mechinic (not yet implemented)
            scale = transform.localScale; 
            this.transform.position = mark.transform.position;
            this.transform.rotation = mark.transform.rotation;

            var temp = mark.transform.rotation.eulerAngles;
            if (temp.x > 45 && temp.x < 135) _fixedRot = true;
            // update data from UI every 15 seconds
            StartCoroutine(GetData());

            InvokeRepeating(nameof(UpdateUI), 0, 14);
        }

        public void ChangeScale(float increment) {
            var scale = transform.localScale;
            transform.localScale = new Vector3(scale.x + (0.0002f * increment), scale.y + (0.0002f * increment), scale.z);
            UpdatePosition();
        }

        private void Update() {
            if (_destination != null) {
                if (!CheckDiffer()) {  
                    transform.position = Vector3.Lerp(transform.position, _destination.position, 1f * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, _destination.rotation, 1f * Time.deltaTime);
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

        public void UpdatePosition() {
            scale = transform.localScale;
            // move and rotate menu to match the vumark
            var target = GameObject.Find(id);
            if (target != null) {
                target.transform.Translate(Vector3.up * ((10) * scale.x));
                _destanationHor = new CubeTransform(target.transform.GetChild(0).transform);
                target.transform.Translate(Vector3.down * ((10) * scale.x));

                target.transform.Translate(Vector3.up * (transform.GetChild(0).GetComponent<RectTransform>().rect.height / 2) * scale.y);
                _destanationVer = new CubeTransform(target.transform);
                target.transform.Translate(Vector3.down * (transform.GetChild(0).GetComponent<RectTransform>().rect.height / 2) * scale.y);
                if (!_fixedRot) {
                    _destination = _destanationVer;
                } else {
                    _destination = _destanationHor;
                }
            }
        }

       
        public IEnumerator GetData() {
            UnityWebRequest request = UnityWebRequest.Get("http://hololensmartijn.azurewebsites.net/api/Todo/" + "{ \"id\" : \"" + id + "\"}");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) {
                Debug.Log(request.error);
            } else {
                var data = JsonUtility.FromJson<JsonData>(request.downloadHandler.text);

                FillMenu(data, transform.GetChild(0).gameObject);
                FillMenu(data, transform.GetChild(1).gameObject);

            }
        }

        public void FillMenu(JsonData data, GameObject menu) {
            menu.GetComponentsInChildren<Image>()[0].GetComponentsInChildren<Text>()[0].text = data.title;
            for (var i = 0; i < data.menus.Count; i++) {
                menu.transform.GetChild(i + 1).GetComponentsInChildren<Text>()[0].text = data.menus[i].header;
                menu.transform.GetChild(i + 1).GetComponentsInChildren<Text>()[1].text = data.menus[i].data;
            }
        }

        void UpdateUI() {
            StartCoroutine(GetData());
        }
    }

    public class CubeTransform {
        public Vector3 position;
        public Quaternion rotation;

        public CubeTransform(Transform transform) {
            position = transform.position;
            rotation = transform.rotation;
        }
    }
}