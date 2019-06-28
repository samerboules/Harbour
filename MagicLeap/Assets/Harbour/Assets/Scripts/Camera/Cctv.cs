using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cctv : MonoBehaviour {

    // Time it takes to do a full sweep
    public int sweepTime = 30;

    private float angle = 0;
    private float speed = 0;
    private bool control = false;

	// Use this for initialization
	void Start () {
        speed = (float)(Math.PI) / sweepTime;
	}
	
	// Update is called once per frame
	void Update () {
        if (GetComponentInChildren<Camera>().enabled && Input.GetKeyDown(KeyCode.Space))
        {
            control = !control;
            Vector3 currentCamRotation = GetComponentInChildren<Camera>().transform.rotation.eulerAngles;
            GetComponentInChildren<Camera>().transform.rotation = Quaternion.Euler(20f, currentCamRotation.y, currentCamRotation.z);
        }

        if (control)
        {
            processUserInput();
        }
        else
        {
            angle += speed * Time.deltaTime;
            angle %= (float)(2 * Math.PI);
            transform.rotation = Quaternion.Euler(0, (float)(Math.Sin(angle) * (180 / Math.PI)), 0);
        }
    }

    void processUserInput()
    {
        Vector3 currentPanRotation = transform.rotation.eulerAngles;
        Camera cam = GetComponentInChildren<Camera>();
        Vector3 currentCamRotation = GetComponentInChildren<Camera>().transform.rotation.eulerAngles;
        float factor = 0.005f * cam.fieldOfView;
        if (Input.GetKey(KeyCode.W))
        {
            cam.transform.rotation = Quaternion.Euler(currentCamRotation.x -= factor, currentCamRotation.y, currentCamRotation.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            cam.transform.rotation = Quaternion.Euler(currentCamRotation.x += factor, currentCamRotation.y, currentCamRotation.z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Euler(currentPanRotation.x, currentPanRotation.y -= factor, currentPanRotation.z);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.Euler(currentPanRotation.x, currentPanRotation.y += factor, currentPanRotation.z);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            cam.fieldOfView = Math.Min(100f, cam.fieldOfView + factor);
        }
        if (Input.GetKey(KeyCode.E))
        {
            cam.fieldOfView = Math.Max(2f, cam.fieldOfView - factor);
        }
    }
}
