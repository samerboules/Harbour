using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct ViewPreset
{
    public Vector3 position;
    public Quaternion rotation;

    public ViewPreset(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}

public class LongPress
{
    public KeyCode key = KeyCode.None;
    public float seconds;

    public void setKey(KeyCode key)
    {
        this.key = key;
        seconds = Time.realtimeSinceStartup;
    }
}

public class FlyCamera : MonoBehaviour
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
    public AudioClip setPresetSound;

    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    public List<ViewPreset> presets = new List<ViewPreset>();
    private Int32 basePresetButton = (Int32)KeyCode.F1;
    private LongPress longPress = new LongPress();
    
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        presets.Insert(0, new ViewPreset(new Vector3(   0, 83,   5), Quaternion.Euler(30,   0, 0)));
        presets.Insert(1, new ViewPreset(new Vector3(-231, 37, 175), Quaternion.Euler(12,  87, 0)));
        presets.Insert(2, new ViewPreset(new Vector3( 231, 37, 175), Quaternion.Euler(12, -87, 0)));
        for (int i = 3; i < 11; i++)
        {
            presets.Insert(i, new ViewPreset(new Vector3(0, 83, 5), Quaternion.Euler(30, 0, 0)));
        }
    }

    void Update()
    {
        if (!GetComponentInChildren<Camera>().enabled)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            lastMouse = Input.mousePosition;
        }

        if (!rotateOnlyIfMousedown ||
            (rotateOnlyIfMousedown && Input.GetMouseButton(1)))
        {
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;
            //Mouse  camera angle done.  
        }

        //Keyboard commands
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
       
        if (Input.GetKey(KeyCode.Space) || movementStaysFlat)
        { //If player wants to move on X and Z axis only
            Vector3 newPosition = transform.position;
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
            Vector3 newPosition = transform.position;
            newPosition.y = Math.Max(transform.position.y, 1f);
            transform.position = newPosition;
        }

        if (!movementStaysFlat)
        {
            for (int i = 0; i < 11; i++)
            {
                KeyCode key = (KeyCode)basePresetButton + i;

                if (Input.GetKey(key))
                {
                    if (longPress.key == key)
                    {
                        if (Time.realtimeSinceStartup - longPress.seconds > longPressTimeout)
                        {
                            presets[i] = new ViewPreset(transform.position, transform.rotation);
                            audioSource.PlayOneShot(setPresetSound, 0.7F);
                            longPress.setKey(KeyCode.None);
                        }
                    }
                    else
                    {
                        longPress.setKey(key);
                    }
                }

                if (Input.GetKeyUp(key))
                {
                    transform.SetPositionAndRotation(presets[i].position, presets[i].rotation);
                    longPress.setKey(KeyCode.None);
                }
            }
        }
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
        if (Input.GetKey(KeyCode.Q))
        {
            p_Velocity += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            p_Velocity += new Vector3(0, 1, 0);
        }

        return p_Velocity;
    }
}