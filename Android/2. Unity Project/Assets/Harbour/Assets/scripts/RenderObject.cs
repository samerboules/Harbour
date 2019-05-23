using DataObjects;
using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;

public class RenderObject
{
    public string id;
    public string status;
    public bool statusUpdate;
    public ObjectType objectType;
    public Vector3 scale;
    public Transform transform = null;
    public PlacedObject placedObject = null;
    public DesiredTransform desiredTransform;
    public Color color;
    public bool isRemoved = false;
    public bool hasSpreader = false;
    public RenderSpreader spreader;
    public List<Vector3> claim = null;

    public RenderObject(string id, ObjectType objectType, Transform transform, DesiredTransform desiredTransform, Vector3 scale, Color color, bool hasSpreader = false)
    {
        this.id = id;
        this.scale = scale;
        this.objectType = objectType;
        this.transform = transform;
        this.desiredTransform = desiredTransform;
        this.hasSpreader = hasSpreader;
        this.color = color;
        if (hasSpreader)
        {
            spreader = new RenderSpreader();
        }
        status = "";
        statusUpdate = false;
    }

    public void SetTransform(Transform newTransform)
    {
        transform = newTransform;
    }

    public void Initiate(GameObject go)
    {
        SetTransform(go.transform);
        placedObject = go.GetComponent<PlacedObject>();
        go.SetActive(true);
        go.name = id;
        go.transform.localScale = scale;
        go.transform.localRotation = desiredTransform.rotation;
        go.transform.localPosition = desiredTransform.position;
        desiredTransform.moveSpeed = 0;

        if (objectType == ObjectType.CLAIM &&
            claim != null)
        {
            PolyClaim polyClaim = go.GetComponent<PolyClaim>();
            polyClaim.SetClaim(claim);
            polyClaim.SetColor(color);
            return;
        }

        if (placedObject != null)
        {
            placedObject.SetName(id);
            placedObject.SetColor(color);
        }

        Update();
    }

    public void Update()
    {
        UpdateSpreader();
        UpdatePosition();
        UpdateRotation();
        UpdateStatus();
    }

    public void UpdateSpreader()
    {
        if (!hasSpreader)
            return;

        if (spreader.spreaderContentUpdate)
        {
            spreader.spreaderContentUpdate = false;
            placedObject.SetSpreaderContent(spreader.spreaderContents);
        }

        if (spreader.spreaderSizeUpdate)
        {
            spreader.spreaderSizeUpdate = false;
            placedObject.SetSpreaderSize(spreader.desiredSpreaderSize, spreader.deltaTime);
        }

        if (spreader.desiredTransform.moveSpeed != 0f)
        {
            placedObject.UpdateSpreaderPosition(spreader.desiredTransform);
        }
    }

    public void UpdatePosition()
    {
        var currentPosition = transform.localPosition;
        var desiredPosition = desiredTransform.position;

        if (currentPosition == desiredPosition)
            return;

        float maxMoveDistance = desiredTransform.moveSpeed * Time.deltaTime;
        if (maxMoveDistance == 0f)
        { // move like The Flash, because at zero speed it will never reach its desired position and just keep eating CPU
            maxMoveDistance = 30f * Time.deltaTime;
        }

        placedObject.SetWheelSpeed(Vector3.Distance(currentPosition, desiredPosition), desiredTransform.moveSpeed);

        Vector3 newPosition = Vector3.MoveTowards(transform.localPosition, desiredTransform.position, maxMoveDistance);

        if (!newPosition.IsValid())
            return;

        transform.localPosition = newPosition;

        if (newPosition.IsApproximately(desiredPosition))
            newPosition = desiredPosition;
    }

    public void UpdateRotation()
    {
        var currentRotation = transform.localRotation;
        var desiredRotation = desiredTransform.rotation;

        if (currentRotation != desiredRotation)
        {
            transform.localRotation = Quaternion.RotateTowards(currentRotation, desiredRotation, desiredTransform.rotateSpeed * Time.deltaTime);
        }
    }

    public void UpdateStatus()
    {
        if (statusUpdate)
        {
            placedObject.SetStatusText(status);
            statusUpdate = false;
        }
    }

    public bool IsContainer()
    {
        return this.objectType == ObjectType.CONTAINER_20 || this.objectType == ObjectType.CONTAINER_40;
    }
}
