using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGaze : MonoBehaviour, IFocusable
{
    public void OnFocusEnter()
    {
        Transform StatusWindo = transform.Find("StatusWindow");
        StatusWindo.localScale = new Vector3(6f, 6f, 6f);
    }

    public void OnFocusExit()
    {
        Transform StatusWindo = transform.Find("StatusWindow");
        StatusWindo.localScale = new Vector3(6f, 6f, 6f);
    }
}
