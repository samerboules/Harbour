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
    public string hostString = "192.168.137.1:8096";

    private LoadObjects LoadObjects;
    
    void Start()
    {
        LoadObjects = GetComponent<LoadObjects>();
        InvokeRepeating("ConnectToServer", 0, 2.0f);
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
        }
    }
}
