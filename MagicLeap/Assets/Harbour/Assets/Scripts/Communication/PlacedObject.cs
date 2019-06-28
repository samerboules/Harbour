using DataObjects;
using ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    private Camera _camera;
    private Transform _trolley;
    private Transform _spreader;
    private Spreader _spreaderEquipment;
    private string _spreaderContents = "";
    private Dictionary<SpreaderSize, float> _spreaderSizes = new Dictionary<SpreaderSize, float>()
    {
        { SpreaderSize.SPREADER_20,       6.058f },
        { SpreaderSize.SPREADER_40,      12.192f },
        { SpreaderSize.SPREADER_TWIN_20, 12.192f },
    };

    public Transform _containerRootObject;
    public ObjectType EquipmentType;
    public bool HasSpreader = false;
    public float SpreaderSmoothTime = 0.001f;

    // Use this for initialization
    void Start ()
    {
        if (HasSpreader)
        {
            _trolley = transform.GetChild(0).GetChild(0);
            _spreader = _trolley.GetChild(0).GetChild(0);
        }
    }

    void OnDestroy()
    {
    }

    #region Spreader

    public void SetSpreaderContent(string containerId)
    {
        if (containerId != "")
        {
            PickUp(containerId);
        }
        else
        {
            PutDown();
        }
    }

    public void SetSpreaderSize(SpreaderSize spreaderSize, float deltaTime)
    {
        if (_spreaderEquipment == null && _spreader != null)
        {
            _spreaderEquipment = _spreader.GetComponentInChildren<Spreader>();
        }

        if (_spreaderEquipment == null)
            return;

        _spreaderEquipment.SetSpreader(_spreaderSizes[spreaderSize], deltaTime);
    }

    public void PutDown()
    {
        var currentContainer = GameObject.Find(_spreaderContents);
        if (currentContainer == null)
        {
            Debug.Log("No container found for " + _spreaderContents);
            return;
        }

        currentContainer.transform.SetParent(_containerRootObject);
        _spreaderContents = "";
    }

    public void PickUp(string containerId)
    {
        var newParent = transform.FindObjectsWithTag("CONTAINER_HOLDER").FirstOrDefault().transform;
        var currentContainer = GameObject.Find(containerId);

        if (newParent == null || currentContainer == null)
        {
            Debug.Log("No holder found for " + containerId);
            return;
        }

        currentContainer.transform.SetParent(newParent);
        currentContainer.transform.localPosition = Vector3.zero;

        _spreaderContents = containerId;
    }

    public void UpdateSpreaderPosition(DesiredTransform desiredTransform)
    {
        if (!HasSpreader)
            return;

        switch (EquipmentType)
        {
            case ObjectType.ASC:
            case ObjectType.QC:
                UpdateCraneSpreader(desiredTransform);
                break;
            case ObjectType.AUTOSTRAD:
                UpdateAutoStradSpreader(desiredTransform);
                break;
            default:
                break;
        }
    }

    private void UpdateCraneSpreader(DesiredTransform desiredTransform)
    {
        if (desiredTransform == null || _trolley == null || _spreader == null)
            return;

        Vector3 dummy = Vector3.zero;
        Vector3 desiredTrolleyPosition = new Vector3(0, 0, desiredTransform.position.z);
        Vector3 desiredSpreaderPosition = new Vector3(0, desiredTransform.position.y);

        Vector3 newTrolleyPosition = Vector3.MoveTowards(_trolley.localPosition, desiredTrolleyPosition, desiredTransform.moveSpeed * Time.deltaTime);
        //Vector3 newSpreaderPosition = Vector3.SmoothDamp(_spreader.localPosition, desiredSpreaderPosition, ref dummy, SpreaderSmoothTime, desiredTransform.moveSpeed * 3f);
        Vector3 newSpreaderPosition = Vector3.MoveTowards(_spreader.localPosition, desiredSpreaderPosition, desiredTransform.moveSpeed * Time.deltaTime);

        if (!newTrolleyPosition.IsValid() || !newSpreaderPosition.IsValid())
        {
            Debug.Log("Invalid spreader positions based on spreader " + _spreader.localPosition + " and trolley " + _trolley.localPosition + " => " + desiredTransform.position);
            _trolley.localPosition = desiredTrolleyPosition;
            _spreader.localPosition = desiredSpreaderPosition;
            desiredTransform.moveSpeed = 0;
            return;
        }

        _trolley.localPosition = newTrolleyPosition;
        _spreader.localPosition = newSpreaderPosition;
    }

    private void UpdateAutoStradSpreader(DesiredTransform desiredTransform)
    {
        if (desiredTransform == null || _trolley == null || _spreader == null)
            return;

        Vector3 desiredSpreaderPosition = new Vector3(0, desiredTransform.position.y);
        Vector3 dummy = Vector3.zero;
        Vector3 newSpreaderPosition = Vector3.SmoothDamp(_spreader.localPosition, desiredSpreaderPosition, ref dummy, SpreaderSmoothTime, desiredTransform.moveSpeed * 3f);

        if (!newSpreaderPosition.IsValid())
            return;

        _spreader.localPosition = newSpreaderPosition;
    }

    #endregion

    #region Color and text

    public void SetName(string newName)
    {
        var texts = transform.gameObject.GetComponentsInChildren<TextMesh>();
        foreach (TextMesh textMesh in texts)
        {
            if (textMesh.name.StartsWith("id-"))
            {
                textMesh.text = newName;
            }
        }
    }

    public void SetColor(Color color)
    {
        if (color == Color.clear)
            return;

        if (IsContainer())
            SetContainerColor(color);
        else
        {
            SetObjectColor(transform, color);
        }
    }

    private void SetContainerColor(Color color)
    {
        var colorObjects = transform.FindObjectsWithTag("CONTAINER_COLOR");
        foreach (var colorObject in colorObjects)
        {
            SetObjectColor(colorObject.transform, color);
        }
    }

    private static void SetObjectColor(Transform transform, Color color)
    {
        var renderers = transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    private static void SetVisibility(GameObject gameObject, bool isVisible)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isVisible;
        }
    }

    public void SetWheelSpeed(float distance, float speed, bool reversed = false)
    {
        if (EquipmentType != ObjectType.AUTOSTRAD)
            return;

        //const float wheelSpeed = 1.963f;

        var anim = gameObject.GetComponentInChildren<Animator>();
        //anim.speed = speed / wheelSpeed * (reversed ? -1 : 1);
        anim.speed = 1;
    }

    public void SetStatusText(string status)
    {
        StatusUpdate updateWindow = transform.GetComponentInChildren<StatusUpdate>();
        if (updateWindow != null)
        {
            updateWindow.SetStatus(status);
        }
    }

    #endregion


    public bool IsContainer()
    {
        return EquipmentType == ObjectType.CONTAINER_20 || EquipmentType == ObjectType.CONTAINER_40;
    }
}
