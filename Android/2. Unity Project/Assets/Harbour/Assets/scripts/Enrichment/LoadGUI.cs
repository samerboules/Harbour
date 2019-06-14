using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LoadGUI : MonoBehaviour
{
    public string productVersion = "1.0";
    string hostString;

    private LoadObjects LoadObjects;
    public Text HostStringText;
    void Start()
    {
        LoadObjects = GetComponent<LoadObjects>();
        InvokeRepeating("ConnectToServer", 0, 15f);
    }

    void ConnectToServer()
    {
        LoadObjects.ConnectToTcpServer(hostString);
    }

    private void Update()
    {
        // Cancel all Invoke calls
        if (LoadObjects.Connected)
        {
            CancelInvoke();
            HostStringText.text = "Connected";
        }
        else//Not Connected
        {
            GameObject WelcomeScreenManager = GameObject.Find("WelcomeScreenManager");
            WelcomeScreenScript welcomescreenscript = WelcomeScreenManager.GetComponent<WelcomeScreenScript>();
            hostString = welcomescreenscript.IPAddress + ":8096";
            HostStringText.text = "Connecting to: " + hostString;
        }
    }
}
