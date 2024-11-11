using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class Water : MonoBehaviour
{

    [SerializeField] private float speedChange;

    [SerializeField] private float density;

    [SerializeField] private float speedLimitArchimedes;

    [SerializeField] private float speedArchimedes;

    [SerializeField] private Collider triggerCollider;

    [SerializeField] private Vector2 course;

    [SerializeField] private float courseMagnitude;

    private float offset;

    private List<WaterObject> waterObjects = new List<WaterObject>();

    private class WaterObject
    {
        public WaterObject(Rigidbody rigidbody)
        {
            gameObject = rigidbody;
        }

        public WaterObject(Rigidbody rigidbody, IAtWater _atWater)
        {
            gameObject = rigidbody;
            atWater = _atWater;

        }
        public IAtWater atWater;

        public Rigidbody gameObject;

        public float time;

        public float archimedes;

        public float VolumeObject => atWater == null ? 1f : atWater.VolumeObject / gameObject.mass;
    }
    private void OnTriggerStay(Collider other)
    {
        Rigidbody rigidbody = other.gameObject.GetComponentInParent<Rigidbody>();
        WaterObject waterObject = waterObjects.Find((water) => water.gameObject == rigidbody);
        if (waterObject == null)
        {
            waterObject = new WaterObject(rigidbody, rigidbody.GetComponent<IAtWater>());
            waterObjects.Add(waterObject);
            rigidbody.drag = density;
            rigidbody.angularDrag = 0.8f;
        }
        else
        {
            waterObject.time = 0F;
        }
        if (waterObject.atWater != null)
        {
            if (!waterObject.atWater.isSwim && waterObject.gameObject.worldCenterOfMass.y <= triggerCollider.bounds.max.y)
                waterObject.atWater.EnterWater();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rigidbody = other.gameObject.GetComponentInParent<Rigidbody>();
        ExitObjectWater(rigidbody);
    }
    private void ExitObjectWater(Rigidbody rigidbody)
    {
        if (rigidbody == null) return;
        WaterObject waterObject = waterObjects.Find((water) => water.gameObject == rigidbody);
        if (waterObject == null)
        {
            waterObject = new WaterObject(rigidbody, rigidbody?.GetComponent<IAtWater>());
        }
        else waterObjects.Remove(waterObject);

        rigidbody.drag = 0;
        rigidbody.angularDrag = 0;

        waterObject.atWater?.ExitWater();
    }
    private void CalculateWaterArchimedes(WaterObject obj)
    {
        float dir =  (triggerCollider.bounds.max.y - obj.gameObject.worldCenterOfMass.y) * speedArchimedes;
        obj.archimedes = Mathf.Clamp(obj.VolumeObject * Mathf.Abs(Physics.gravity.y) * density * dir, 0 ,Mathf.Abs(Physics.gravity.y) * speedLimitArchimedes);
    }
    private void CalculateWaterForse(WaterObject obj)
    {
        obj.gameObject.AddForce(new Vector3 (0f, obj.archimedes, 0f),ForceMode.Acceleration);
    }
    private void CalculateRotateCourse(WaterObject obj)
    {
        obj.gameObject.AddForce(new Vector3(course.x * courseMagnitude * 2, 0f, course.y * courseMagnitude * 2), ForceMode.Acceleration);
        if(!obj.gameObject.freezeRotation)
            obj.gameObject.AddTorque(new Vector3(course.x * courseMagnitude * 0.5F, 0f, course.y * courseMagnitude * 0.5F), ForceMode.Acceleration);
    }

    private void FixedUpdate()
    {
        for(int i = 0; i < waterObjects.Count; i++)
        {
            var water = waterObjects[i];
            if (water.gameObject == null)
            {
                waterObjects = waterObjects.Where(obj => obj.gameObject != null).ToList();
                continue;
            }
            water.time += Time.fixedDeltaTime;
            if (water.time > 0.2f)
            {
                ExitObjectWater(water.gameObject);
                continue;
            }
            CalculateWaterArchimedes(water);
            CalculateWaterForse(water); CalculateRotateCourse(water);
            if (water.atWater != null)
            {
                if (water.atWater.isSwim && water.gameObject.worldCenterOfMass.y - 0.1F > triggerCollider.bounds.max.y)
                    water.atWater.ExitWater();
            }
        }
    }
}
