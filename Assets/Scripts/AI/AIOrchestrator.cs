using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Jobs;
using System.Collections;
using Unity.Collections;
using UnityEngine.AI;

namespace AIInput
{
    public class AIOrchestrator : MonoBehaviour
    {
        private static AIOrchestrator instance;
        public static AIOrchestrator Instance 
        { 
            get
            {
                if(instance == null)
                {
                    instance = new GameObject(nameof(AIOrchestrator)).AddComponent<AIOrchestrator>();
                }
                return instance;
            }
            
        }
        private List<IAIOrchestrated> aIOrchestrateds = new List<IAIOrchestrated>();

        private List<IAIOrchestrated> aICalculateNavMesh = new List<IAIOrchestrated>();

        public static void AddOrchestrated(IAIOrchestrated orchestrated)
        {
            Instance.aIOrchestrateds.Add(orchestrated);
        }
       
        public static void RemoveOrchestrated(IAIOrchestrated orchestrated)
        {
            if (instance == null) return;
            instance.aIOrchestrateds.Remove(orchestrated);
        }

        public static void AddCalculateMove(IAIOrchestrated orchestrated, Vector3 target)
        {
            Instance.aIOrchestrateds.Add(orchestrated);
        }

        public static void RemovedCalculateMove(IAIOrchestrated orchestrated)
        {
            if (instance == null) return;
            instance.aIOrchestrateds.Remove(orchestrated);
        }

        private JobHandle jobHandle;

        private void Start()
        {
            base.StartCoroutine(ExpectationJobHandle());
            jobHandles = new NativeArray<JobHandle>(0, Allocator.Temp);
        }
        NativeArray<JobHandle> jobHandles;
        private IEnumerator ExpectationJobHandle()
        {
            WaitForSeconds wait = new WaitForSeconds(0.07f);
            while(true)
            {
                yield return wait;
                {
                    IAIOrchestrated[] currentOrchestrateds = Instance.aIOrchestrateds.ToArray();
                    bool[] isRanning = Instance.aIOrchestrateds.Select(run => run.IsRunning).ToArray();
                    jobHandles = new NativeArray<JobHandle>(currentOrchestrateds.Length, Allocator.TempJob);
                    for (int i = 0; i < currentOrchestrateds.Length; i++)
                    {
                        if (isRanning[i] && currentOrchestrateds[i] != null)
                            jobHandles[i] = currentOrchestrateds[i].ScheduleJob();
                    }
                    jobHandle = JobHandle.CombineDependencies(jobHandles);
                    yield return new WaitUntil(() => Instance.jobHandle.IsCompleted);
                    jobHandle.Complete();
                    for (int i = 0; i < currentOrchestrateds.Length; i++)
                    {
                        if (i == currentOrchestrateds.Length / 2) yield return null;
                        if (isRanning[i] && currentOrchestrateds[i] != null)
                            currentOrchestrateds[i].OnJobComplete();
                    }
                    yield return null;
                    for (int i = 0; i < currentOrchestrateds.Length; i++)
                    {
                        if (isRanning[i] && currentOrchestrateds[i] != null)
                            currentOrchestrateds[i].OnJobCompleteNextFrame();
                    }
                    jobHandles.Dispose();
                }
            }
        }

        private void OnDestroy()
        {
            if (aIOrchestrateds != null)
            {
                aIOrchestrateds.Clear();
            }
            jobHandle.Complete();
            jobHandles.Dispose();
        }
    }
}
