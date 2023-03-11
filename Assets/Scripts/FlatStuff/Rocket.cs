using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rocket : MonoBehaviour
{
    [Range(0,1)]
    public float throttle;
    public float thrust;
    public float rcs;

    void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(transform.up * thrust * throttle);
        GetComponent<Rigidbody>().AddTorque(transform.forward * Input.GetAxis("Horizontal") * rcs);
    }
}
