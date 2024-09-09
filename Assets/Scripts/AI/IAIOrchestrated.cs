using Unity.Jobs;
using UnityEngine;

namespace AIInput
{
    public interface IAIOrchestrated
    {
        JobHandle ScheduleJob();
        void OnJobComplete();

        void OnJobCompleteNextFrame() { }

        bool IsRunning { get; }
    }
}
