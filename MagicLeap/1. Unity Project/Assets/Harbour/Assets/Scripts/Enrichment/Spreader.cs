using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spreader : MonoBehaviour
{
    private const float MINIMUM_LENGTH = 6f;
    private const float MAXIMUM_LENGTH = 15f;
    public Transform leftBeam;
    public Transform rightBeam;
    public float Offset = 0;
    private Vector3 leftInitialPosition;
    private Vector3 rightInitialPosition;
    private float initialLength = 0;
    private float lastLength;

    public float DesiredLength;
    private float dt = 2f;

    void Start ()
    {
        leftInitialPosition = leftBeam.localPosition;
        rightInitialPosition = rightBeam.localPosition;
        initialLength = GetCurrentLength();
        lastLength = initialLength;
    }

    void Update ()
    {
        if (lastLength != DesiredLength)
        {
            AdjustSpreaderSize();
        }
	}

    private void AdjustSpreaderSize()
    {
        var finalDelta = DesiredLength - initialLength;
        var halfDelta = finalDelta / 2;

        var travel = Math.Abs(DesiredLength - lastLength) / 2;

        var newLeftFinalPosition = leftInitialPosition;
        var newRightFinalPosition = rightInitialPosition;
        newLeftFinalPosition.x += halfDelta;
        newRightFinalPosition.x -= halfDelta;

        Vector3 newLeftPosition =  Vector3.MoveTowards(leftBeam.localPosition,  newLeftFinalPosition, (travel / dt) * Time.deltaTime);
        Vector3 newRightPosition = Vector3.MoveTowards(rightBeam.localPosition, newRightFinalPosition, (travel / dt) * Time.deltaTime);

        if (!newLeftPosition.IsValid() || !newRightPosition.IsValid())
            return;

        leftBeam.localPosition = newLeftPosition;
        rightBeam.localPosition = newRightPosition;

        if (Mathf.Approximately(GetCurrentLength(), DesiredLength))
            lastLength = DesiredLength + Offset;
    }

    private float GetCurrentLength()
    {
        return Math.Abs(rightBeam.localPosition.x - leftBeam.localPosition.x) + Offset;
    }

    public void SetSpreader(float desired, float deltaTime)
    {
        DesiredLength = Mathf.Clamp(desired, MINIMUM_LENGTH, MAXIMUM_LENGTH);
        dt = deltaTime;
    }
}
