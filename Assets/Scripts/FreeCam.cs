using System.Collections;
using UnityEngine;

public class FreeCam : MonoBehaviour
{
    public Transform cameraTransform;
    public float sensitivity;
    public float zoomSensitivity;
    public Vector3 axis;
    public Vector3 lowerLimits = new Vector3(-200, 0, 5);
    public Vector3 upperLimits = new Vector3(-106, 0, 45);
    public AnimationCurve zoomHeight;

    private Vector3 workerVec;
    private float timer = 0;

    void Update()
    {
        if (Universe.paused || Universe.loading)
            return;
        transform.localEulerAngles += axis * Input.GetAxis("Horizontal") * Time.deltaTime * sensitivity;
        timer = Time.deltaTime / (Universe.dayLengthInMinutes * 60f);
        transform.localEulerAngles += timer * 360f * axis.normalized;

        workerVec = cameraTransform.localPosition;
        workerVec.x += Input.GetAxis("Vertical") * Time.deltaTime * sensitivity;
        workerVec.z -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomSensitivity;
        workerVec.x -= (Input.GetAxis("Mouse ScrollWheel") > 0 ? Input.GetAxis("Mouse ScrollWheel") : 0) * Time.deltaTime * zoomSensitivity;
        /*
        workerVec.x = Mathf.Clamp(workerVec.x, Mathf.Lerp(lowerLimits.x, upperLimits.x, zoomHeight.Evaluate((workerVec.z - lowerLimits.z) / (upperLimits.z - lowerLimits.z))), upperLimits.x);
        workerVec.y = 0;
        workerVec.z = Mathf.Clamp(workerVec.z, lowerLimits.z, upperLimits.z);
        */
        cameraTransform.localPosition = workerVec;
    }
}