using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EquipmentCamera : MonoBehaviour
{
    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

    public float mainSpeed = 100.0f; //regular speed
    public float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    public float maxShift = 1000.0f; //Maximum speed when holdin gshift
    public float camSens = 0.25f; //How sensitive it with mouse
    public float longPressTimeout = 1;
    public bool rotateOnlyIfMousedown = true;
    public bool movementStaysFlat = true;

    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)

    private void Start()
    {

    }

    void Update()
    {
        var camera = GetComponentInChildren<Camera>();
        if (!camera.enabled)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            lastMouse = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Transform trans = camera.transform;
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(trans.eulerAngles.x + lastMouse.x, trans.eulerAngles.y + lastMouse.y, 0);
            camera.transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;
            //Mouse  camera angle done.  
        }
    }
}