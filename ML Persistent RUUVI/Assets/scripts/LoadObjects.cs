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
    private bool drawClaims = true;
    private bool updatedStatus = false;

    private readonly System.Object lockArray = new System.Object();
    private readonly System.Object lockStaticArray = new System.Object();

    private Dictionary<string, RenderObject> objectList = new Dictionary<string, RenderObject>();
    private ConcurrentDictionary<string, string> statusList = new ConcurrentDictionary<string, string>();

    void Start()
    {
        Application.runInBackground = true;
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
            SpreaderType spreaderType = (SpreaderType)Enum.Parse(typeof(SpreaderType), msg.spreaderSize);
            renderObject.spreader.spreaderContentUpdate = true;
            renderObject.spreader.spreaderType = spreaderType;
            renderObject.spreader.spreaderContents = msg.containerId;
        }
    }

    private void HandlePutDownMessage(JsonPutDownMessage msg)
    {
        RenderObject renderObject;
        if (objectList.TryGetValue(msg.equipId, out renderObject) && renderObject.hasSpreader)
        {
            renderObject.spreader.spreaderContentUpdate = true;
            renderObject.spreader.spreaderType = SpreaderType.SPREADER_EMPTY;
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

        if (jsonObject.type == "CONTAINER_40")
        {
            color = GetColorFromLong(ContainerColorer.GetHexColorFromPrefix(jsonObject.id.Substring(0, 4))); 
        }

        if (jsonObject.type == "CONTAINER")
        {
            // For now, magenta containers
            color = Color.magenta;

            if (jsonObject.l < 9)
            {
                jsonObject.type = ObjectType.CONTAINER_20.ToString();
                scale = new Vector3(jsonObject.l / 6.058f, jsonObject.h / 2.591f, jsonObject.w / 2.438f).ToMeter();       
            }
            else
            {
                jsonObject.type = ObjectType.CONTAINER_40.ToString();
                scale = new Vector3(jsonObject.l / 12.192f, jsonObject.h / 2.591f, jsonObject.w / 2.438f).ToMeter();
            }
        }
        else
        {
            scale = new Vector3(jsonObject.l, jsonObject.h, jsonObject.w).ToMeter();
        }

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
        bool timeReached = false;
        // 5ms seems to be enough to render multiple objects at the same time, but not inhibit movement too much
        int maxTimeInMilliseconds = 5; 
        var timer = new Timer(state => timeReached = true, new object(), maxTimeInMilliseconds, -1);
        foreach (var objectToUpdate in GetObjectList())
        {
            if (timeReached)
            {
                timer.Dispose();
                yield return null;
                timeReached = false;
                timer = new Timer(state => timeReached = true, new object(), maxTimeInMilliseconds, -1);
            }

            if (objectToUpdate.isRemoved)
            {
                if (objectToUpdate.transform != null)
                {
                    FindObjectOfType<CameraCycler>().RemoveCamera(objectToUpdate.transform);
                    Destroy(objectToUpdate.transform.gameObject);
                }

                lock (lockArray)
                    objectList.Remove(objectToUpdate.id);

            }
            else if (objectToUpdate.transform == null)
            {
                InstantiateNewObject(objectToUpdate.id);
            }

            if (objectToUpdate.transform == null)
                continue;

            if (objectToUpdate.transform.parent != null)
                continue;

            if (objectToUpdate.hasSpreader && objectToUpdate.spreader.spreaderContentUpdate)
            {
                objectToUpdate.spreader.spreaderContentUpdate = false;
                SetSpreader(objectToUpdate.transform, objectToUpdate.spreader.spreaderType, objectToUpdate.spreader.spreaderContents);
            }

            if (objectToUpdate.hasSpreader && objectToUpdate.spreader.desiredTransform.moveSpeed != 0f)
            {
                Transform trolley = objectToUpdate.transform.GetChild(0).GetChild(0);
                Transform spreader = trolley.GetChild(0).GetChild(0);

                switch (objectToUpdate.objectType)
                {
                    case ObjectType.ASC:
                    case ObjectType.QC:
                        UpdateCraneSpreader(objectToUpdate.spreader.desiredTransform, trolley, spreader);
                        break;
                    case ObjectType.AUTOSTRAD:
                        UpdateAutoStradSpreader(objectToUpdate.spreader.desiredTransform, spreader);
                        break;
                    default:
                        break;
                }
            }

            var currentPosition = objectToUpdate.transform.position;
            var desiredPosition = objectToUpdate.desiredTransform.position;

            if (currentPosition != desiredPosition)
            {
                float maxMoveDistance = objectToUpdate.desiredTransform.moveSpeed * Time.deltaTime;
                if (maxMoveDistance == 0f)
                { // move like The Flash, because at zero speed it will never reach its desired position and just keep eating CPU
                    maxMoveDistance = 30f * Time.deltaTime;
                }

                if (objectToUpdate.objectType == ObjectType.AUTOSTRAD)
                {
                    SetWheelSpeed(
                        objectToUpdate.transform.gameObject,
                        Vector3.Distance(currentPosition, desiredPosition),
                        objectToUpdate.desiredTransform.moveSpeed);
                }

                Vector3 newPos = Vector3.MoveTowards(objectToUpdate.transform.position, objectToUpdate.desiredTransform.position, maxMoveDistance);

                if (newPos.IsValid())
                {
                    objectToUpdate.transform.position = newPos;
                }
            }

            if (objectToUpdate.statusUpdate)
            {
                SetStatusText(objectToUpdate.transform.gameObject, objectToUpdate.status);
                objectToUpdate.statusUpdate = false;
            }

            var currentRotation = objectToUpdate.transform.rotation;
            var desiredRotation = objectToUpdate.desiredTransform.rotation;

            if (currentRotation != desiredRotation)
            {
                objectToUpdate.transform.rotation = Quaternion.RotateTowards(currentRotation, desiredRotation, objectToUpdate.desiredTransform.rotateSpeed * Time.deltaTime);
            }
        }
        timer.Dispose();
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

    private void UpdateCraneSpreader(DesiredTransform desiredTransform, Transform trolley, Transform spreader)
    {
        Vector3 desiredTrolleyPosition = new Vector3(0, 0, desiredTransform.position.z);
        Vector3 desiredSpreaderPosition = new Vector3(0, desiredTransform.position.y);
        Vector3 newTrolleyPosition = Vector3.MoveTowards(trolley.localPosition, desiredTrolleyPosition, desiredTransform.moveSpeed * Time.deltaTime);

        Vector3 dummy = Vector3.zero;
        Vector3 newSpreaderPosition = Vector3.SmoothDamp(spreader.localPosition, desiredSpreaderPosition, ref dummy, 0.12f, desiredTransform.moveSpeed * 3f);


        if (!newTrolleyPosition.IsValid() || !newSpreaderPosition.IsValid())
            return;

        trolley.localPosition = newTrolleyPosition;
        spreader.localPosition = newSpreaderPosition;
    }

    private void UpdateAutoStradSpreader(DesiredTransform desiredTransform, Transform spreader)
    {
        Vector3 desiredSpreaderPosition = new Vector3(0, desiredTransform.position.y);
        Vector3 dummy = Vector3.zero;
        Vector3 newPos = Vector3.SmoothDamp(spreader.localPosition, desiredSpreaderPosition, ref dummy, 0.12f, desiredTransform.moveSpeed * 3f);

        if (!newPos.IsValid())
            return;

        spreader.localPosition = newPos;
    }

    private void InstantiateNewObject(string objectId, bool isStatic = false)
    {
        RenderObject renderObject;

        objectList.TryGetValue(objectId, out renderObject);

        if (renderObject == null)
            return;

        var objectBase = GameObject.Find(renderObject.objectType.ToString());

        GameObject newObject = Instantiate(objectBase);

        newObject.name = renderObject.id;
        newObject.transform.localScale = renderObject.scale;
        newObject.transform.rotation = renderObject.desiredTransform.rotation;
        newObject.SetActive(true);
        newObject.transform.position = renderObject.desiredTransform.position;
        SetSpreader(newObject.transform, SpreaderType.SPREADER_EMPTY);
        renderObject.desiredTransform.moveSpeed = 0;

        SetString(newObject.transform, renderObject.id);

        if (renderObject.objectType == ObjectType.CLAIM &&
            renderObject.claim != null)
        {
            PolyClaim claim = newObject.GetComponent<PolyClaim>();
            claim.SetClaim(renderObject.claim);
        }

        if (renderObject.IsContainer())
        {
            var containerParent = GameObject.Find("RootContainerHolder").transform;
            newObject.transform.SetParent(containerParent);
        }

        if (renderObject.color != Color.clear)
        {
            if (renderObject.IsContainer())
                SetContainerColor(newObject.transform, renderObject.color);
            else
            {
                SetObjectColor(newObject.transform, renderObject.color);
            }
        }

        if (renderObject.objectType == ObjectType.QC ||
            renderObject.objectType == ObjectType.AUTOSTRAD)
        {
            var cam = newObject.GetComponentInChildren<Camera>();
            var cycler = FindObjectOfType<CameraCycler>();
            cam.name = CameraCycler.CamNamePrefix + renderObject.id;
            cycler.AddCamera(cam);
        }

        lock (lockArray)
            objectList[objectId].transform = newObject.transform;

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

    private void SetSpreader(Transform transform, SpreaderType spreaderType, string containerId = "")
    {
        List<GameObject> spreaders = transform.FindObjectsWithTag("SPREADER");
        foreach (GameObject spreader in spreaders)
        {
            bool isRightSpreader = spreader.name == spreaderType.ToString();
            SetVisibility(spreader, isRightSpreader);

            if (!isRightSpreader)
                continue;

            if (spreaderType == SpreaderType.SPREADER_TWIN_20 &&
                containerId.Contains(","))
            {
                string[] containers = containerId.Split(',');
                Transform container = spreader.transform.Find("CONTAINER_20_LEFT");
                SetString(container, containers[0]);
                container = spreader.transform.Find("CONTAINER_20_RIGHT");
                SetString(container, containers[1]);
            }

            if (containerId != "")
            {
                if (spreaderType == SpreaderType.SPREADER_EMPTY)
                {
                    var currentContainer = GameObject.Find(containerId);
                    if (currentContainer == null)
                    {
                        Debug.Log("No container found for " + containerId);
                        continue;
                    }

                    currentContainer.transform.SetParent(null);
                    SetVisibility(currentContainer, true);
                }
                else
                {
                    var newParent = spreader.transform.FindObjectsWithTag("CONTAINER_HOLDER").FirstOrDefault().transform;
                    var currentContainer = GameObject.Find(containerId);

                    if (newParent == null || currentContainer == null)
                    {
                        Debug.Log("No holder found for " + transform.gameObject.name);
                        continue;
                    }

                    currentContainer.transform.SetParent(newParent);
                    currentContainer.transform.localPosition = Vector3.zero;
                }
            }
        }
    }

    public List<RenderObject> GetObjectList()
    {
        lock (lockArray)
        {
            return objectList.Values.ToList();
        }
    }

    private void SetVisibility(GameObject gameObject, bool isVisible)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isVisible;
        }
    }

    private void SetWheelSpeed(GameObject gameObject, float distance, float speed, bool reversed = false)
    {
        const float wheelSpeed = 1.963f;

        var anim = gameObject.GetComponentInChildren<Animator>();
        //anim.speed = speed / wheelSpeed * (reversed ? -1 : 1);
        anim.speed = 1;
    }

    private static void SetString(Transform currentObject, string text)
    {
        var meshes = currentObject.GetComponentsInChildren<TextMesh>();

        foreach (TextMesh textMesh in meshes)
            if (textMesh.name.StartsWith("id-"))
                textMesh.text = text;
    }

    private static void SetContainerColor(Transform currentObject, Color color)
    {
        var colorObjects = currentObject.transform.FindObjectsWithTag("CONTAINER_COLOR");
        foreach (var colorObject in colorObjects)
        {
            SetObjectColor(colorObject.transform, color);
        }
    }

    private static void SetObjectColor(Transform currentObject, Color color)
    {
        var renderers = currentObject.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
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
