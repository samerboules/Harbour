using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class LoadGUI : MonoBehaviour
{
    public string productVersion = "1.0";
    public string hostString = "127.0.0.1:8096";
    public int buttonWidth = 90;
    public int buttonHeight = 20;
    public int buttonSpacing = 1;
    public int objectListTopMargin = 80;
    public int objectListLeftMargin = 10;
    public int objectListHeight = 400;
    private int objectListScrollbarWidth = 20;
    private int defaultButtonHeight = 20;
    private LoadObjects loadObjects;
    private string tutorial;
    private bool showTutorial = true;
    private bool showContainers = false;
    private bool showObjectList = false;
    private float currentScrollHeight = 0;

    void Start()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Controls:");
        sb.AppendLine();
        sb.AppendLine("WASD - movement");
        sb.AppendLine("QE - up/down or zoom");
        sb.AppendLine("(hold) Shift - fly and walking speed x10");
        sb.AppendLine("Right Mouse Button - turn/pan camera");
        sb.AppendLine("F1 - F12 - preset views. Long press for saving");
        sb.AppendLine("1, 2, 3 - predefined cameras");
        sb.AppendLine("Shift + 1-9 - Straddle carrier cameras");
        sb.AppendLine("Alt + 1-9 - Straddle carrier cameras");
        sb.AppendLine("O - toggle object list");
        sb.AppendLine("C - toggle claiming");
        sb.AppendLine("H - show this window");
        tutorial = sb.ToString();
    }

    private void Update()
    {
        currentScrollHeight -= Input.mouseScrollDelta.y * 12;

        if (Input.GetKeyDown(KeyCode.H))
        {
            showTutorial = true;
        }
        else if (showTutorial && Input.anyKeyDown)
        {
            showTutorial = false;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            showObjectList = !showObjectList;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            LoadObjects.DrawClaims = !LoadObjects.DrawClaims;
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 250, 60), "Network connection");


        if (showTutorial)
        {
            GUI.Box(new Rect(10, 80, 300, 160), tutorial);
        }

        if (LoadObjects.Connected)
        {
            GUI.Label(new Rect(20, 35, 130, defaultButtonHeight), hostString);
            if (GUI.Button(new Rect(165, 35, 90, defaultButtonHeight), "Disconnect"))
            {
                LoadObjects.DisconnectClient();
            }

            if (showObjectList)
            {
                DrawObjectList();
            }
        }
        else
        {
            hostString = GUI.TextField(new Rect(20, 35, 150, defaultButtonHeight), hostString);
            if (GUI.Button(new Rect(185, 35, 70, defaultButtonHeight), "Connect"))
            {
                LoadObjects.ConnectToTcpServer(hostString);
            }
        }
    }

    private void DrawObjectList()
    {
        if (GUI.Button(
            new Rect(
                objectListLeftMargin + buttonWidth + objectListScrollbarWidth + 10,
                objectListTopMargin,
                showContainers ? 200 : 170,
                defaultButtonHeight),
            (showContainers ? "Do not show" : "Show") + " containers in list"))
        {
            showContainers = !showContainers;
        }

        var renderObjects = LoadObjects.GetObjectList();
        if (!showContainers)
            renderObjects.RemoveAll(x => x.objectType.ToString().Contains("CONTAINER"));
        renderObjects.Sort((a, b) => 10 * a.objectType.CompareTo(b.objectType) + a.id.CompareTo(b.id));
        int maxScroll = renderObjects.Count * (buttonHeight + buttonSpacing) - objectListHeight;
        currentScrollHeight = maxScroll < 0 ? 0 : Mathf.Clamp(currentScrollHeight, 0, maxScroll);

        using (var scrollView = new GUI.ScrollViewScope(
            new Rect(
                objectListLeftMargin,
                objectListTopMargin,
                buttonWidth + objectListScrollbarWidth,
                objectListHeight),
            new Vector2(0, currentScrollHeight),
            new Rect(0, 0, buttonWidth, renderObjects.Count * (buttonHeight + buttonSpacing))))
        {
            for (int i = 0; i < renderObjects.Count; i++)
            {
                var renderObject = renderObjects[i];
                if (GUI.Button(new Rect(0, i * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), renderObject.objectType.ToString() + " " + renderObject.id))
                {
                    GameObject.Find("Main Camera").transform.LookAt(renderObject.transform);
                }
            }
        }
    }

    private LoadObjects LoadObjects
    {
        get
        {
            if (loadObjects == null)
                loadObjects = GetComponent<LoadObjects>();

            return loadObjects;
        }
    }
}
