using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motorcycle : MonoBehaviour
{
    public WheelCollider frontWheel;
    public WheelCollider backWheel;

    public float test = 0;

    private void nValidate()
    {
        test = frontWheel.steerAngle;
    }

    void FixedUpdate()
    {
        // Для теста - простое вращение колёс
        backWheel.motorTorque = 10;

        frontWheel.steerAngle = test;

        frontWheel.transform.parent.parent.localRotation = Quaternion.Euler(270, test, 0);

        // Обновление визуального положения колёс
        UpdateWheel(frontWheel);
        UpdateWheel(backWheel);
    }

    void UpdateWheel(WheelCollider col)
    {
        // Получаем позицию и поворот колеса
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);

        // Обновляем визуальную модель
        Transform visualWheel = col.transform.GetChild(0);
        visualWheel.position = position;
        visualWheel.rotation = rotation;
    }
}
