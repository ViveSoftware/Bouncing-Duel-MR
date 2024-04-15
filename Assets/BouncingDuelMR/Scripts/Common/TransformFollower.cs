using UnityEngine;

public class TransformFollower : MonoBehaviour
{

    [SerializeField]
    private Transform target;

    public bool smoothRotate;
    public float smoothness = 1f;

    [Header("Rotation locking")]
    public bool lockX;
    public bool lockY;
    public bool lockZ;

    private float xv, yv, zv;

    void Start()
    {
        if (target == null) return;
        transform.position = target.transform.position;

        Vector3 followRotation = target.transform.eulerAngles;
        Vector3 newRotation = transform.eulerAngles;

        if (!lockX) newRotation.x = followRotation.x;
        if (!lockY) newRotation.y = followRotation.y;
        if (!lockZ) newRotation.z = followRotation.z;

        transform.eulerAngles = newRotation;
    }

    void Update()
    {

        if (target == null) return;

        transform.position = target.position;

        Vector3 followRotation = target.eulerAngles;
        Vector3 newRotation = transform.eulerAngles;

        if (smoothRotate)
        {
            if (!lockX) newRotation.x = Mathf.SmoothDampAngle(transform.eulerAngles.x, followRotation.x, ref xv, smoothness);
            if (!lockY) newRotation.y = Mathf.SmoothDampAngle(transform.eulerAngles.y, followRotation.y, ref yv, smoothness);
            if (!lockZ) newRotation.z = Mathf.SmoothDampAngle(transform.eulerAngles.z, followRotation.z, ref zv, smoothness);

        }
        else
        {
            if (!lockX) newRotation.x = followRotation.x;
            if (!lockY) newRotation.y = followRotation.y;
            if (!lockZ) newRotation.z = followRotation.z;
        }

        transform.eulerAngles = newRotation;
    }
}