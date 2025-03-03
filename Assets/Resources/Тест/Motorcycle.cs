using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motorcycle : MonoBehaviour
{
    public WheelCollider frontWheel;
    public WheelCollider backWheel;

    private Rigidbody rb;

    public Transform WheelViewFront, WheelViewBack;

    public float test,force, turnInput = 0;

    [SerializeField] private float leanForce = 500f;
    [SerializeField] private float leanAngleMax = 45f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnValidate()
    {
        frontWheel.steerAngle = test;
        backWheel.motorTorque = force;
    }

    void FixedUpdate()
    {
        frontWheel.transform.parent.localRotation = Quaternion.Euler(270, test, 0);

        UpdateFrontWheel(frontWheel);
        UpdateWheel(backWheel);

        float turnInput = Input.GetAxis("Horizontal");
        float speed = rb.velocity.magnitude;

        float targetLean = -turnInput * leanAngleMax * (speed / 20f); // 20f - скорость для максимального наклона

        float currentLean = Vector3.SignedAngle(Vector3.up, transform.up, transform.forward);

        float leanTorque = (targetLean - currentLean) * leanForce;

        rb.AddTorque(transform.forward * leanTorque);

        if (Mathf.Abs(turnInput) < 0.1f)
        {
            float stabilizationTorque = -currentLean * leanForce * 0.5f;
            rb.AddTorque(transform.forward * stabilizationTorque);
        }
    }

    void UpdateFrontWheel(WheelCollider col)
    {
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);

        Transform visualWheel = WheelViewFront;
        visualWheel.parent.position = position; //GetProjectedPosition(new Vector3(0F,1.2F,1F),Vector3.up * (position.y - visualWheel.parent.position.y));
        visualWheel.rotation = rotation;
    }

    void UpdateWheel(WheelCollider col)
    {
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);

        Transform visualWheel = WheelViewBack;
        visualWheel.position = position;
        visualWheel.rotation = rotation;
    }

    public Vector3 GetProjectedPosition(Vector3 direction, Vector3 axisMask)
    {
        Vector3 normalizedDir = direction.normalized;
        Vector3 result = Vector3.zero;

        if (axisMask.x != 0)
        {
            float angleX = Vector3.Angle(normalizedDir, Vector3.right);
            float angleRadX = angleX * Mathf.Deg2Rad;
            float projectedDistanceX = axisMask.x / Mathf.Cos(angleRadX);
            result += normalizedDir * projectedDistanceX;
        }

        if (axisMask.y != 0)
        {
            float angleY = Vector3.Angle(normalizedDir, Vector3.up);
            float angleRadY = angleY * Mathf.Deg2Rad;
            float projectedDistanceY = axisMask.y / Mathf.Cos(angleRadY);
            result += normalizedDir * projectedDistanceY;
        }

        if (axisMask.z != 0)
        {
            float angleZ = Vector3.Angle(normalizedDir, Vector3.forward);
            float angleRadZ = angleZ * Mathf.Deg2Rad;
            float projectedDistanceZ = axisMask.z / Mathf.Cos(angleRadZ);
            result += normalizedDir * projectedDistanceZ;
        }

        return result;
    }
}
