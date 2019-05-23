using UnityEngine;

public class FaceMe : MonoBehaviour {

    private CameraCycler cameraCycler;
    public Vector3 offset;
    private Canvas canvas;


    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
        cameraCycler = FindObjectOfType<CameraCycler>();
    }

	// Update is called once per frame
	void Update ()
    {
        if (!canvas.enabled)
            return;

        Camera cam = cameraCycler.GetActiveCam();
        bool ortho = cam.orthographic;
        var dist = Vector3.Distance(transform.position, cam.transform.position);
        transform.LookAt(cam.transform.position, Vector3.up);
        transform.rotation = ortho ? Quaternion.AngleAxis(90f, Vector3.right) : transform.rotation * Quaternion.AngleAxis(180f, Vector3.up);
        transform.localScale = Vector3.one * (ortho ? (cam.orthographicSize / 15) : (dist / 30));
        transform.localPosition = offset + new Vector3(0, dist / 30, 0);
    }
}
