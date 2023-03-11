using UnityEngine;

public class FreeCam : MonoBehaviour
{
    public float sensitivity;
    public Vector3 axis;

    void Update()
    {
        transform.localEulerAngles += axis * Input.GetAxis("Horizontal") * Time.deltaTime * sensitivity;
    }
}
