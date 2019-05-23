using UnityEngine;
using System.Collections;
using System;

public class OrthoCam : MonoBehaviour
{
    public float mainSpeed = 0.05f; //regular speed
    public float mouseSpeed = 1f;
    public float shiftAdd = 250.0f; //multiplied by how long shift is held.
    public float maxShift = 1000.0f; //Maximum speed when holdin gshift
    public float zoomSpeed = 50.0f;

    private float totalRun = 1.0f;
    private float sizeMin = 3;
    private float sizeMax = 250;

    private Camera cam;
    private float size = 250;
    private Vector3 lastMouse = new Vector3(255, 255, 255);

    void Update()
    {
        if (!cam.enabled)
            return;

        size = cam.orthographicSize;

        HandleKeyboardInput();
        HandleMouseInput();
        HandleZoom();
    }

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        lastMouse = Input.mousePosition;
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMouse = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.x * size * .00158f, 0, -lastMouse.y * size * .00158f);
            transform.Translate(lastMouse);
            lastMouse = Input.mousePosition;
        }

        int scroll = GetScroll();
        if (scroll != 0)
        {
            float newSize = Mathf.Clamp(size + (zoomSpeed * -scroll * Time.deltaTime), sizeMin, sizeMax);
            cam.orthographicSize = newSize;
        }
    }

    private void HandleKeyboardInput()
    {
        //Keyboard commands
        Vector3 p = GetBaseInput();
        float sizeNormalized = (size - sizeMin) / (sizeMax - sizeMin);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * (mainSpeed * sizeNormalized);
        }

        p = p * Time.deltaTime;
        transform.Translate(p);
    }


    private Vector3 GetBaseInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }

        return p_Velocity;
    }

    private void HandleZoom()
    {
        // Zoom
        bool isZooming = false;

        if (Input.GetKey(KeyCode.Q) || Input.mouseScrollDelta.x > 0)
        {
            size += zoomSpeed * Time.deltaTime;
            isZooming = true;
        }
        if (Input.GetKey(KeyCode.E))
        {
            size -= zoomSpeed * Time.deltaTime;
            isZooming = true;
        }

        if (isZooming)
            cam.orthographicSize = Mathf.Clamp(size, sizeMin, sizeMax);
    }

    private int GetScroll()
    {
        float s = Input.GetAxis("Mouse ScrollWheel");

        if (s == 0f)
            return 0;
        if (s > 0f)
            return 1;
        else
            return -1;
    }
}