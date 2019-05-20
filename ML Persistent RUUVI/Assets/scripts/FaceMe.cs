using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceMe : MonoBehaviour {

    public Camera cam;
    public Vector3 offset;
    private Canvas canvas;


    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }

	// Update is called once per frame
	void Update ()
    {
        if (!canvas.enabled || cam == null)
            return;

        var dist = Vector3.Distance(transform.position, cam.transform.position);
        transform.LookAt(cam.transform.position, Vector3.up);
        transform.rotation = transform.rotation * Quaternion.AngleAxis(180f, Vector3.up);
        transform.localScale = Vector3.one * (dist / 30);
        transform.localPosition = offset + new Vector3(0, dist / 30, 0);
    }
}
