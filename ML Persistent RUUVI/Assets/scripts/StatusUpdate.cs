using UnityEngine;
using ExtensionMethods;
using UnityEngine.UI;

public class StatusUpdate : MonoBehaviour {

    private Canvas canvas;
    private Text text;
    private string status = "";

    void Start()
    {
        if (canvas == null)
            canvas = gameObject.GetComponentInChildren<Canvas>();

        if (text == null)
            text = GetComponentInChildren<Text>();
    }

    private void OnMouseUp()
    {
        if (canvas != null)
            canvas.enabled = !canvas.enabled;

        if (text != null)
            text.text = GetStatus();
    }

    void Update()
    {
        if (text != null)
            text.text = GetStatus();
    }

    private string GetStatus()
    {
        return (status + "\n<b>Position:</b>\n" + transform.position.UnityToPort().InverseAxes().ToMeter() + "\n<b>Heading:</b> " + transform.rotation.eulerAngles.y.ToString());
    }

    public void SetStatus(string _status)
    {
        status = _status;
    }
}
