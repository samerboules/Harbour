using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour {

    public GameObject originObject;
    public Vector3 offsetFromOrigin = new Vector3();
    public GameObject targetObject;
    public Vector3 offsetFromTarget = new Vector3();
    public float diameter = 0.01f;

    private GameObject rope;
    private GameObject ropeHolder;

    // Use this for initialization
	void Start () {
        string ropeName = "rope_from_" + originObject.name + "_to_" + targetObject.name;

        RemoveOldRopeObject(ropeName);
        ropeHolder = new GameObject();
        ropeHolder.name = ropeName;
        ropeHolder.transform.parent = transform;

        rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "rope";
        rope.GetComponent<Renderer>().material.color = Color.grey;
        rope.transform.parent = ropeHolder.transform;
        rope.transform.localEulerAngles = new Vector3(90, 0, 0);
    }

	// Update is called once per frame
	void Update () {
        if (ropeHolder == null)
            return;

        Vector3 originOffset = (Quaternion.AngleAxis(-90, Vector3.up) * originObject.transform.rotation) * offsetFromOrigin;
        Vector3 targetOffset = (Quaternion.AngleAxis(-90, Vector3.up) * targetObject.transform.rotation) * offsetFromTarget;
        Vector3 origin = originObject.transform.position + originOffset;
        Vector3 target = targetObject.transform.position + targetOffset;
        float   distance = Vector3.Distance(origin, target);

        ropeHolder.transform.position = (origin + target) / 2;
        ropeHolder.transform.LookAt(target, Vector3.back);
        rope.transform.localScale = new Vector3(diameter, distance / 2, diameter);
    }

    private void RemoveOldRopeObject(string name)
    {
        Transform oldRope = transform.Find(name);
        if (oldRope != null)
        {
            GameObject.Destroy(oldRope.gameObject);
        }
    }
}
