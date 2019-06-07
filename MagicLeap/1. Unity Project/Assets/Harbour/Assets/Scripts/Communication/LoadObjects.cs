using DataObjects;
using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class LoadObjects : MonoBehaviour {

    private bool connected = false;
    private bool drawClaims = false;
    private bool updatedStatus = false;
    private Transform rootObject;

    private readonly System.Object lockArray = new System.Object();

    private Dictionary<string, RenderObject> objectList = new Dictionary<string, RenderObject>();
    private ConcurrentDictionary<string, string> statusList = new ConcurrentDictionary<string, string>();

    void Start()
    {
        Application.runInBackground = true;
        rootObject = GameObject.Find("TerminalVisualization").transform;
    }

    void Update () {
        StartCoroutine(UpdateRenderObjects());
        StartCoroutine(UpdateStatusObjects());
    }

    public Socket Client { get; set; }
    public bool DrawClaims
    {
        get { return drawClaims; }
        set
        {
            drawClaims = value;

            if (drawClaims)
                return;

            lock (lockArray)
            {
                objectList.Where(o => o.Value.objectType == ObjectType.CLAIM).ToList().ForEach(p => p.Value.isRemoved = true);
            }
        }
    }
    private SocketAsyncEventArgs _socketEventArgs;
    private AutoResetEvent _autoReset = new AutoResetEvent(false);


    public bool Connected
    {
        get
        {
            return connected;
        }

        set
        {
            if (connected != value)
            {
                connected = value;
                if (!connected)
                { //disconnected
                    Debug.Log("Connection lost.");

                    RemoveObjects(GetObjectList());
                }
            }
        }
    }

    public void ConnectToTcpServer(string hostString)
    {
        Debug.Log("Connecting to " + hostString);
        try
        {
            var clientReceiveThread = new Thread(new ThreadStart(() => ListenForData(hostString)));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
            Connected = false;
        }
    }

    private void ListenForData(string hostString)
    {
        try
        {
            string[] hostInfo = hostString.Split(':');
            using (var tcpClient = new TcpClient(hostInfo[0], int.Parse(hostInfo[1])))
            {
                Debug.Log("Connected to " + hostString);
                Client = tcpClient.Client;

                var stream = tcpClient.GetStream();
                Byte[] bytes = new Byte[46750];
                var buffer = new List<Byte>();
                int length;

                Connected = true;
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    buffer.AddRange(bytes.Take(length));
                    InterpretData(buffer);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception " + hostString + " :" + socketException);
        }
        catch (System.IO.IOException ioException)
        {
            Debug.Log("IO exception " + hostString + " :" + ioException);
        }
        catch (Exception ex)
        {
            Debug.Log("Unexpected exception " + hostString + " :" + ex);
        }
        finally
        {
            Connected = false;
            if (Client != null)
            {
                Client.Disconnect(false);
                Client.Close();
                Client = null;
            }
        }
    }

    public void DisconnectClient()
    {
        _socketEventArgs = new SocketAsyncEventArgs() { DisconnectReuseSocket = false };
        Client.DisconnectAsync(_socketEventArgs);
        _socketEventArgs.Completed += _socketEventArgs_Completed;

        _autoReset.WaitOne(TimeSpan.FromSeconds(2));

        _socketEventArgs.Completed -= _socketEventArgs_Completed;
        Client.Close();
        Client = null;
        _socketEventArgs = null;
        _autoReset.Reset();
    }

    private void _socketEventArgs_Completed(object sender, SocketAsyncEventArgs e)
    {
        _autoReset.Set();
    }

    private void InterpretData(List<Byte> buffer)
    {
        while (true)
        {
            int msgSizeEndIndex = buffer.IndexOf((byte)':');
            if (msgSizeEndIndex < 1)
            {
                //check for unexpected characters
                RemoveNonDigitCharacters(buffer);
                return;
            }

            string msgSizeString = Encoding.ASCII.GetString(buffer.ToArray(), 0, msgSizeEndIndex);
            int msgStartIndex = msgSizeEndIndex + 1;
            int msgSize;
            if (int.TryParse(msgSizeString, out msgSize))
            {
                if (buffer.Count >= msgStartIndex + msgSize)
                {
                    byte[] msg = buffer.Skip(msgStartIndex).Take(msgSize).ToArray();
                    ThreadPool.QueueUserWorkItem(_ => ParseMessage(Encoding.UTF8.GetString(msg)));
                    buffer.RemoveRange(0, msgStartIndex + msgSize);
                }
                else
                {
                    //full message length not received yet, keep reading the stream
                    return;
                }
            }
            else
            {
                Debug.Log("Stream has an invalid message size: " + msgSizeString);

                //remove the invalid message size
                buffer.RemoveRange(0, msgStartIndex);
            }
        }
    }

    /// <summary>
    /// Returns true if at least one non-digit character was found and removed from the buffer.
    /// </summary>
    private bool RemoveNonDigitCharacters(List<Byte> buffer)
    {
        for (int index = 0; index < buffer.Count; index++)
        {
            if (!char.IsDigit((char)buffer[index]))
            {
                Debug.Log("Received unexpected characters: " + buffer.ToString());

                //remove all unexpected characters
                while (index < buffer.Count && !char.IsDigit((char)buffer[index]))
                    index++;
                buffer.RemoveRange(0, index);
                return true;
            }
        }
        return false;
    }

    private void ParseMessage(string msg)
    {
        var message = JsonUtility.FromJson<JsonMessage<JsonUpdateMessage>>(msg);

        MessageType? messageType;
        try
        {
            messageType = (MessageType)Enum.Parse(typeof(MessageType), message.type, true);
        }
        catch (SystemException)
        {
            messageType = null;
            Debug.Log("Could not parse MessageType: " + message.type);
            return;
        }

        switch (messageType)
        {
            case MessageType.UPDATE:
                HandleUpdateMessage(message.content);
                break;
            case MessageType.DELETE:
                HandleDeleteMessage(JsonUtility.FromJson<JsonMessage<JsonDeleteMessage>>(msg).content);
                break;
            case MessageType.SPREADER:
                HandleSpreaderMessage(JsonUtility.FromJson<JsonMessage<JsonSpreaderMessage>>(msg).content);
                break;
            case MessageType.SPREADER_SIZE:
                HandleSpreaderSizeMessage(JsonUtility.FromJson<JsonMessage<JsonSpreaderSizeMessage>>(msg).content);
                break;
            case MessageType.PICKUP:
                HandlePickupMessage(JsonUtility.FromJson<JsonMessage<JsonPickupMessage>>(msg).content);
                break;
            case MessageType.PUTDOWN:
                HandlePutDownMessage(JsonUtility.FromJson<JsonMessage<JsonPutDownMessage>>(msg).content);
                break;
            case MessageType.CLAIM:
                HandleClaimMessage(JsonUtility.FromJson<JsonMessage<JsonClaimMessage>>(msg).content);
                break;
            case MessageType.STATUS:
                HandleStatusMessage(JsonUtility.FromJson<JsonMessage<JsonStatusMessage>>(msg).content);
                break;
            default:
                Debug.Log("Unknown message received: " + msg);
                break;
        }
    }

    private void HandleStatusMessage(JsonStatusMessage msg)
    {
        if (objectList.ContainsKey(msg.id))
        {
            lock (lockArray)
            {
                objectList[msg.id].status = msg.status;
                objectList[msg.id].statusUpdate = true;
            }
        }
        else
        {
            statusList.AddOrUpdate(msg.id, msg.status, (key, oldValue) => oldValue = msg.status);
            updatedStatus = true;
        }
    }

    private void HandleUpdateMessage(JsonUpdateMessage msg)
    {
        if (objectList.ContainsKey(msg.id))
        {
            lock (lockArray)
                UpdateObject(msg);
        }
        else
        {
            AddNewObject(msg);
        }
    }

    private void HandleClaimMessage(JsonClaimMessage msg)
    {
        if (!DrawClaims)
            return;

        lock (lockArray)
        {
            objectList.Add(
                msg.id, new RenderObject(
                    msg.id,
                    ObjectType.CLAIM,
                    null,
                    new DesiredTransform(Vector3.zero, Quaternion.identity, 0f, 0f),
                    Vector3.one,
                    GetColorFromLong(msg.color),
                    false
                )
            );

            var list = new List<Vector3>();

            for (int i = 0; i < msg.points.Count(); i++)
            {
                list.Add(new Vector3(msg.points[i].x, msg.points[i].y).ToWorld());
            }

            objectList[msg.id].claim = list;
        }

    }

    private void HandleDeleteMessage(JsonDeleteMessage msg)
    {
        RenderObject renderObject;
        if (objectList.TryGetValue(msg.id, out renderObject))
        {
            lock (lockArray)
                renderObject.isRemoved = true;
        }
    }

    private void HandlePickupMessage(JsonPickupMessage msg)
    {
        RenderObject renderObject;
        if (objectList.TryGetValue(msg.equipId, out renderObject) && renderObject.hasSpreader)
        {
            renderObject.spreader.spreaderContentUpdate = true;
            renderObject.spreader.spreaderContents = msg.containerId;
        }
    }

    private void HandlePutDownMessage(JsonPutDownMessage msg)
    {
        RenderObject renderObject;
        if (objectList.TryGetValue(msg.equipId, out renderObject) && renderObject.hasSpreader)
        {
            renderObject.spreader.spreaderContentUpdate = true;
            renderObject.spreader.spreaderContents = "";
        }
    }

    private void HandleSpreaderMessage(JsonSpreaderMessage msg)
    {
        RenderObject renderObject;
        if (!objectList.TryGetValue(msg.id, out renderObject))
        {
            return;
        }

        if (!renderObject.hasSpreader)
        {
            return;
        }

        Vector3 position = new Vector3(msg.x, msg.y, msg.z).ToLocal();
        renderObject.spreader.desiredTransform.moveSpeed = Vector3.Distance(renderObject.spreader.desiredTransform.position, position) / (msg.dt * .001f);
        renderObject.spreader.desiredTransform.position = position;
    }

    private void HandleSpreaderSizeMessage(JsonSpreaderSizeMessage msg)
    {
        RenderObject renderObject;
        if (!objectList.TryGetValue(msg.equipId, out renderObject))
        {
            return;
        }

        if (!renderObject.hasSpreader)
        {
            return;
        }

        SpreaderSize spreaderSize = (SpreaderSize)Enum.Parse(typeof(SpreaderSize), msg.spreaderSize);

        renderObject.spreader.spreaderSizeUpdate = true;
        renderObject.spreader.desiredSpreaderSize = spreaderSize;
        renderObject.spreader.deltaTime = msg.dt * .001f;
    }

    private void AddNewObject(JsonUpdateMessage jsonObject)
    {
        Vector3 position = new Vector3(jsonObject.x, jsonObject.y, jsonObject.z).ToWorld();
        Vector3 scale;
        Color color;

        if (jsonObject.c == 0)
        {
            color = Color.clear;
        }
        else
        {
            color = GetColorFromLong(jsonObject.c);
        }

        if (jsonObject.type == "CONTAINER_40" || jsonObject.type == "CONTAINER_20")
        {
            color = GetColorFromLong(ContainerColorer.GetHexColorFromPrefix(jsonObject.id.Substring(0, 4))); 
        }

        scale = new Vector3(jsonObject.l, jsonObject.h, jsonObject.w).ToMeter();

        if (scale.magnitude < 1)
            scale = Vector3.one;

        float desiredRotationDegrees = jsonObject.p.ToLocalRotation();
        var desiredRotation = Quaternion.AngleAxis(desiredRotationDegrees, Vector3.up);

        lock (lockArray)
        {
            ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), jsonObject.type);
            objectList.Add(
                jsonObject.id, new RenderObject(
                    jsonObject.id,
                    objectType,
                    null,
                    new DesiredTransform(position, desiredRotation, 0f, 0f),
                    scale,
                    color,
                    (objectType == ObjectType.ASC || objectType == ObjectType.QC || objectType == ObjectType.AUTOSTRAD)
                )
            );
        }
    }

    private void UpdateObject(JsonUpdateMessage jsonObject)
    {
        RenderObject objectToUpdate;
        lock (lockArray)
        {
            objectToUpdate = objectList[jsonObject.id];
        }

        Vector3 position = new Vector3(jsonObject.x, jsonObject.y, jsonObject.z).ToWorld();
        float desiredRotationDegrees = jsonObject.p.ToLocalRotation();
        var desiredRotation = Quaternion.AngleAxis(desiredRotationDegrees, Vector3.up);
        var moveSpeed = Vector3.Distance(objectToUpdate.desiredTransform.position, position) / (jsonObject.dt * .001f);
        var rotateSpeed = Quaternion.Angle(objectToUpdate.desiredTransform.rotation, desiredRotation) / (jsonObject.dt * .001f);

        if (float.IsNaN(rotateSpeed) || rotateSpeed < 1f)
        {
            rotateSpeed = 90f;
        }

        objectToUpdate.desiredTransform.moveSpeed = moveSpeed;
        objectToUpdate.desiredTransform.rotateSpeed = rotateSpeed;

        objectToUpdate.desiredTransform.position = position;
        objectToUpdate.desiredTransform.rotation = desiredRotation;

    }

    private IEnumerator UpdateRenderObjects()
    {
        foreach (var objectToUpdate in GetObjectList())
        {
            if (objectToUpdate.isRemoved)
            {
                if (objectToUpdate.transform != null)
                {
                    Destroy(objectToUpdate.transform.gameObject);
                }

                objectList.Remove(objectToUpdate.id);
            }
            else if (objectToUpdate.transform == null)
            {
                InstantiateNewObject(objectToUpdate.id);
            }

            if (objectToUpdate.transform == null || objectToUpdate.IsContainer())
                continue;

            objectToUpdate.Update();
            yield return null;
        }
    }


    private IEnumerator UpdateStatusObjects()
    {
        if (!updatedStatus)
            yield return null;

        foreach(var status in statusList)
        {
            GameObject go = GameObject.Find(status.Key);

            if (go == null)
                continue;

            SetStatusText(go, status.Value);
        }
        updatedStatus = false;
        yield return null;
    }

    private void SetStatusText(GameObject transform, string status)
    {
        StatusUpdate updateWindow = transform.transform.GetComponentInChildren<StatusUpdate>();
        if (updateWindow != null)
        {
            updateWindow.SetStatus(status);
        }
    }

    private void InstantiateNewObject(string objectId, bool isStatic = false)
    {
        RenderObject renderObject;

        objectList.TryGetValue(objectId, out renderObject);

        if (renderObject == null)
            return;

        var objectBase = GameObject.Find(renderObject.objectType.ToString());

        GameObject newObject = Instantiate(objectBase);
        newObject.transform.SetParent(rootObject);

        if (renderObject.IsContainer())
        {
            var containerParent = GameObject.Find("RootContainerHolder").transform;
            newObject.transform.SetParent(containerParent);
        }

        lock (lockArray)
        {
            renderObject.Initiate(newObject);
        }

        Debug.Log("Added new " + renderObject.objectType.ToString());
    }

    private void RemoveObjects(List<RenderObject> objects)
    {
        foreach (var item in objects)
        {
            item.isRemoved = true;
        }
        Debug.Log(objects.Count + " objects flagged for destruction.");
    }

    public List<RenderObject> GetObjectList()
    {
        lock (lockArray)
        {
            return objectList.Values.ToList();
        }
    }

    void OnDestroy()
    {
        if (Client != null)
        {
            Client.Dispose();
            Client = null;
        }
    }

    private static Color32 GetColorFromLong(long color)
    {
        return new Color32(
            (byte)((color >> 16) & 0xFF),
            (byte)((color >> 8) & 0xFF),
            (byte)(color & 0xFF),
            (byte)((color >> 24) & 0xFF));
    }
}
