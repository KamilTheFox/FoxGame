using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Globals.GlobalUpdates))]
public class GlobalUpdatesEditor : Editor
{
    public override void OnInspectorGUI()
    {

        Globals.GlobalUpdates globalUpdates = (Globals.GlobalUpdates)target;

        EditorGUILayout.LabelField("N DEVELOPMENT!!! DONT WORKING");

        globalUpdates.debugMode = EditorGUILayout.Toggle("Debug Mode", globalUpdates.debugMode);

        if (GUILayout.Button("Print Detailed Report"))
        {
            globalUpdates.PrintDetailedReport();
        }

        if (GUILayout.Button("Print Summary by Type"))
        {
            globalUpdates.PrintSummaryByType();
        }

        if (GUILayout.Button("Print Summary by Method"))
        {
            globalUpdates.PrintSummaryByMethod();
        }
    }
}
#endif

public interface IGlobalUpdates 
{
    GameObject gameObject { get; }

    bool enabled { get; }

    void Update() { }

    void EndUpdate() { }

    void FixedUpdate() { }

    void EndFixedUpdate() { }

    void LateUpdate() { }

    void EndLateUpdate() { }


}
namespace Globals
{
    public class GlobalUpdateDebugger
    {
        private Dictionary<Type, PerformanceData> performanceData = new Dictionary<Type, PerformanceData>();

        public class PerformanceData
        {
            public string GameObjectName;
            public string NameMethid;
            public double TotalExecutionTime;
            public int ExecutionCount;
            public double LastExecutionTime;

            public void AddExecution(double executionTime)
            {
                TotalExecutionTime += executionTime;
                ExecutionCount++;
                LastExecutionTime = executionTime;
            }
        }

        public void RecordPerformance(IGlobalUpdates global, string nameMethod, long executionTime)
        {
            Type type = global.GetType();
            if (!performanceData.ContainsKey(type))
            {
                performanceData[type] = new PerformanceData();
            }

            performanceData[type].AddExecution(executionTime);
            performanceData[type].GameObjectName = global.gameObject.name;
            performanceData[type].NameMethid = nameMethod;
        }

        public double GetTotalExecutionTimeForType(Type type)
        {
            return performanceData[type].TotalExecutionTime;
        }

        public int GetTotalExecutionCountForType(Type type)
        {
            return performanceData[type].ExecutionCount;
        }

        public void PrintPerformanceReport()
        {
            foreach (var kvp in performanceData)
            {
                UnityEngine.Debug.Log($"  Type: {kvp.Key.Name}, GameObject: {kvp.Value.GameObjectName}");
                UnityEngine.Debug.Log($"  Total Execution Time: {kvp.Value.TotalExecutionTime}ms");
                UnityEngine.Debug.Log($"  Execution Count: {kvp.Value.ExecutionCount}");
                UnityEngine.Debug.Log($"  Average Execution Time: {kvp.Value.TotalExecutionTime / (float)kvp.Value.ExecutionCount}ms");
                UnityEngine.Debug.Log($"  Last Execution Time: {kvp.Value.LastExecutionTime}ms");
            }
        }
        public void PrintSummaryByType()
        {
            var summaryByType = performanceData
                .GroupBy(kvp => kvp.Key)
                .Select(g => new
                {
                    Type = g.Key,
                    TotalTime = g.Sum(x => x.Value.TotalExecutionTime),
                    Count = g.Sum(x => x.Value.ExecutionCount)
                })
                .OrderByDescending(x => x.TotalTime);

            foreach (var summary in summaryByType)
            {
                UnityEngine.Debug.Log($"Type: {summary.Type.Name}");
                UnityEngine.Debug.Log($"  Total Execution Time: {summary.TotalTime}ms");
                UnityEngine.Debug.Log($"  Total Execution Count: {summary.Count}");
                UnityEngine.Debug.Log($"  Average Execution Time: {summary.TotalTime / (float)summary.Count} ms");
            }
        }
        public void PrintSummaryByMethod()
        {
            var summaryByMethod = performanceData
                .GroupBy(kvp => kvp.Value.NameMethid)
                .Select(g => new
                {
                    Method = g.Key,
                    TotalTime = g.Sum(x => x.Value.TotalExecutionTime),
                    Count = g.Sum(x => x.Value.ExecutionCount)
                })
                .OrderByDescending(x => x.TotalTime);

            foreach (var summary in summaryByMethod)
            {
                UnityEngine.Debug.Log($"Method: {summary.Method}");
                UnityEngine.Debug.Log($"  Total Execution Time: {summary.TotalTime}ms");
                UnityEngine.Debug.Log($"  Total Execution Count: {summary.Count}");
                UnityEngine.Debug.Log($"  Average Execution Time: {summary.TotalTime / (float)summary.Count}ms");
            }
        }
    }
    public class GlobalUpdates : MonoBehaviour
    {
        public static GlobalUpdates instance;
        public static GlobalUpdates Instance
        {
            get
            {
                if (!instance)
                {
                    instance = new GameObject(nameof(GlobalUpdates)).AddComponent<GlobalUpdates>();
                }
                return instance;
            }
        }
        private List<IGlobalUpdates> globalUpdates = new();

        private GlobalUpdateDebugger debugger = new GlobalUpdateDebugger();
        public bool debugMode = false;


        private void Update()
        {
            GlobalUpdatesExtension.NextUpdate?.Invoke();
            GlobalUpdatesExtension.NextUpdate = null;
            for (int i = 0; i < globalUpdates.Count; i++)
            {
                if (CheckDontDestroyOrActiveObject(ref i))
                {
                    Diagnostic(globalUpdates[i], "Update", globalUpdates[i].Update);
                }
            }
            for (int i = 0; i < globalUpdates.Count; i++)
            {
                if (CheckDontDestroyOrActiveObject(ref i))
                {
                    Diagnostic(globalUpdates[i], "EndUpdate", globalUpdates[i].EndUpdate);
                }
            }
        }
        private void FixedUpdate()
        {
            GlobalUpdatesExtension.NextFixedUpdate?.Invoke();
            GlobalUpdatesExtension.NextFixedUpdate = null;
            for (int i = 0; i < globalUpdates.Count; i++)
            {
                if (CheckDontDestroyOrActiveObject(ref i))
                {
                    Diagnostic(globalUpdates[i], "FixedUpdate", globalUpdates[i].FixedUpdate);
                }
            }
            for (int i = 0; i < globalUpdates.Count; i++)
            {
                if (CheckDontDestroyOrActiveObject(ref i))
                {
                    Diagnostic(globalUpdates[i], "EndFixedUpdate", globalUpdates[i].EndFixedUpdate);
                }
            }
        }

        private void LateUpdate()
        {
            GlobalUpdatesExtension.NextLateUpdate?.Invoke();
            GlobalUpdatesExtension.NextLateUpdate = null;
            for (int i = 0; i < globalUpdates.Count; i++)
            {
                if (CheckDontDestroyOrActiveObject(ref i))
                {
                    Diagnostic(globalUpdates[i], "LateUpdate", globalUpdates[i].LateUpdate);
                }
            }
            for (int i = 0; i < globalUpdates.Count; i++)
            {
                if (CheckDontDestroyOrActiveObject(ref i))
                {
                    Diagnostic(globalUpdates[i], "EndLateUpdate", globalUpdates[i].EndLateUpdate);
                }
            }
        }
        private void Diagnostic(IGlobalUpdates global, string nameMethod , Action action)
        {
            if (global.gameObject.activeSelf == false) return;

            if (global.enabled == false) return;

            if (debugMode)
            {
                long startTime = Stopwatch.GetTimestamp();
                action();
                long endTime = Stopwatch.GetTimestamp();

                long elapsedTicks = endTime - startTime;
                long elapsedMicroseconds = (elapsedTicks * 1000000) / Stopwatch.Frequency;
                debugger.RecordPerformance(global, nameMethod, elapsedMicroseconds);
            }
            else
                action();
        }
        public void PrintDetailedReport()
        {
            debugger.PrintPerformanceReport();
        }

        public void PrintSummaryByType()
        {
            debugger.PrintSummaryByType();
        }

        public void PrintSummaryByMethod()
        {
            debugger.PrintSummaryByMethod();
        }
        public static void AddListner(IGlobalUpdates global)
        {
            Instance.globalUpdates.Add(global);
        }
        
        public static void InsertFirstListnerUpdate(IGlobalUpdates global)
        {
            Instance.globalUpdates.Insert(0, global);
        }
        private bool CheckDontDestroyOrActiveObject(ref int index)
        {
            if (globalUpdates[index] == null || !globalUpdates[index].gameObject.activeSelf)
            {
                globalUpdates.RemoveAt(index);
                index--;
                return false;
            }
            return true;
        }
        private void OnDestroy()
        {
            globalUpdates.Clear();
            instance = null;
        }
    }
}

public static class GlobalUpdatesExtension
{
    public static Action NextUpdate, NextFixedUpdate, NextLateUpdate;

    public static void AddListnerNextUpdate(this Action action)
    {
        NextUpdate += action;
    }
    public static void AddListnerNextFixedUpdate(this Action action)
    {
        NextFixedUpdate += action;
    }
    public static void AddListnerNextLateUpdate(this Action action)
    {
        NextLateUpdate += action;
    }

    public static void AddListnerUpdate(this IGlobalUpdates global)
    {
        Globals.GlobalUpdates.AddListner(global);
    }
    public static void InsertFirstListnerUpdate(this IGlobalUpdates global)
    {
        Globals.GlobalUpdates.InsertFirstListnerUpdate(global);
    }
}
