using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CameraCycler : MonoBehaviour
{
    [SerializeField]
    private Camera[] cameras = new Camera[6]; // Set them in the editor

    private const int alphaBase = (int)KeyCode.Alpha1;
    public const string CamNamePrefix = "CAM_";

    private List<Camera> cameraList = new List<Camera>();

    // Use this for initialization
    void Start()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        bool pressedShift = Input.GetKey(KeyCode.LeftShift);
        bool pressedAlt = Input.GetKey(KeyCode.LeftAlt);

        for (int i = 0; i <= (int)KeyCode.Alpha9 - alphaBase; i++)
        {
            if (Input.GetKeyDown((KeyCode) i + alphaBase))
            {
                if (pressedShift)
                {
                    ActivateCam(CamNamePrefix + "SC" + (i + 1).ToString("00"));
                }
                else if (pressedAlt)
                {
                    ActivateCam(CamNamePrefix + "QC" + (i + 1).ToString("00"));
                }
                else
                {
                    if (i < cameras.Where(c => c != null).Count())
                        ActivateCam(cameraList.ElementAt(i).name);
                }
            }
        }
    }

    public void AddCamera(Camera newCamera)
    {
        newCamera.enabled = false;
        cameraList.Add(newCamera);
    }

    public void RemoveCamera(Transform transform)
    {
        Camera cam = transform.GetComponentInChildren<Camera>();

        if (cam != null && cameraList.Contains(cam))
        {
            cameraList.Remove(cam);
            ActivateCam("MainCamera");
        }
    }

    public void Reset()
    {
        FindObjectsOfType<Camera>().ToList().ForEach(c => c.enabled = false);
        cameraList.Clear();
        cameraList.AddRange(cameras.Where(c => c != null));
        ActivateCam("MainCamera");
    }

    private void CycleForward()
    {
        var cam = cameraList.Where(c => c.enabled).FirstOrDefault();
        ActivateCam(cameraList[(cameraList.IndexOf(cam) + 1) % cameraList.Count].name);
    }

    private void ActivateCam(string name)
    {
        var cam = cameraList.Where(c => c.name == name).FirstOrDefault();
        bool orthographic = false;

        if (cam != null)
        {
            orthographic = cam.orthographic;
        }
        else
        {
            Debug.Log("Unknown camera " + name);
            return;
        }

        // Enable correct camera
        cameraList.ForEach(c => c.enabled = c.name == name);

        GameObject.FindObjectOfType<Light>().shadowStrength = orthographic ? 0 : 1;
    }
}