using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class OptimizedRenderer : MonoBehaviour, IGlobalUpdates
{
    static HashSet<IPropertysRendererUpdate> PropertyesRenderer = new();

    static List<IPropertysRendererUpdate> propertyCopy = new();

    static OptimizedRenderer instance;

    public static void AddRendererBuffer(IPropertysRendererUpdate body)
    {
        if(instance == null)
        {
            instance = new GameObject(nameof(OptimizedRenderer)).AddComponent<OptimizedRenderer>();
        }
        if(!PropertyesRenderer.Contains(body))
            PropertyesRenderer.Add(body);
    }
    public static void RemoveRendererBuffer(IPropertysRendererUpdate body)
    {
        if (PropertyesRenderer == null)
            return;
        PropertyesRenderer.Remove(body);
    }


    NativeArray<InputJobPropertyData> positionOrder;

    NativeArray<ResultsJobProperty> result;

    private JobHandle WorkerDistanceCalculate;

    private static List<InputJobPropertyData> vectors = new();

    private void Start()
    {
        this.AddListnerUpdate();
    }

    private void OnDestroy()
    {
        WorkerDistanceCalculate.Complete();
        propertyCopy.Clear();
        PropertyesRenderer.Clear();
        if (result.IsCreated)
            result.Dispose();
        if (positionOrder.IsCreated)
            positionOrder.Dispose();
    }
    void IGlobalUpdates.FixedUpdate()
    {
        if (!WorkerDistanceCalculate.IsCompleted || positionOrder.IsCreated || result.IsCreated) return;

        vectors.Clear();

        propertyCopy.AddRange(PropertyesRenderer.ToArray());

        var array = propertyCopy.ToArray();

        foreach (IPropertysRendererUpdate property in array)
        {
            if (property.Transform != null && property.Transform.gameObject.activeSelf)
            {
                vectors.Add(property.inputJobProperty.SetTransform(property.Transform));
            }
            else
                propertyCopy.Remove(property);
        }
        if (vectors.Count != propertyCopy.Count) Debug.LogError($"{vectors.Count}\\{propertyCopy.Count}");
        positionOrder = new NativeArray<InputJobPropertyData>(vectors.ToArray(), Allocator.Persistent);
        result = new NativeArray<ResultsJobProperty>(vectors.Count, Allocator.Persistent);
        CalculatePropertyRendererJob job = new CalculatePropertyRendererJob(positionOrder, CameraControll.instance.Transform.position, result);
        WorkerDistanceCalculate = job.Schedule(vectors.Count, 256);
    }
    void IGlobalUpdates.EndFixedUpdate()
    {
        if (!WorkerDistanceCalculate.IsCompleted) return;
        WorkerDistanceCalculate.Complete();
        for (int i = 0; i < propertyCopy.Count; i++)
        {
            if(PropertyesRenderer.Contains(propertyCopy[i]))
                if(result.Length > i && propertyCopy.Count > i)
                propertyCopy[i].SetPropertiesJob(result[i]);
        }
        propertyCopy.Clear();
        positionOrder.Dispose();
        result.Dispose();
    }
}
