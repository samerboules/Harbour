using UnityEngine;

namespace DataObjects
{
    public class RenderSpreader
    {
        public bool spreaderContentUpdate = false;
        public bool spreaderSizeUpdate = false;
        public SpreaderSize desiredSpreaderSize = SpreaderSize.SPREADER_40;
        public float deltaTime = 10;
        public string spreaderContents = "";
        public DesiredTransform desiredTransform = new DesiredTransform(Vector3.zero, Quaternion.AngleAxis(9f, Vector3.up), 0f, 0f);
    }

    public enum ObjectType
    {
        ASC,
        QC,
        AUTOSTRAD,
        CONTAINER_20,
        CONTAINER_40,
        CLAIM
    }

    public enum MessageType
    {
        UPDATE,
        DELETE,
        PICKUP,
        PUTDOWN,
        SPREADER,
        SPREADER_SIZE,
        CLAIM,
        STATUS
    }

    public enum SpreaderSize
    {
        SPREADER_20,
        SPREADER_40,
        SPREADER_TWIN_20
    }

    public class DesiredTransform
    {
        public DesiredTransform(Vector3 posArg, Quaternion rotArg, float moveSpeedArg, float rotateSpeedArg)
        {
            position = posArg;
            rotation = rotArg;
            moveSpeed = moveSpeedArg;
            rotateSpeed = rotateSpeedArg;
        }

        public Vector3 position;
        public Quaternion rotation;
        public float moveSpeed;
        public float rotateSpeed;
    }
}