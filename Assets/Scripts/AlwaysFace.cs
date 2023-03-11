using UnityEngine;

[ExecuteInEditMode]
public class AlwaysFace : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Update()
    {
        transform.LookAt(target);
        transform.localEulerAngles += offset;
    }
}
