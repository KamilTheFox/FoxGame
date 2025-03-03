using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using VulpesTool;
using Random = UnityEngine.Random;

namespace PlayerDescription
{
    [Serializable]
    public class TailAnimatorDefault : IGlobalUpdates, IAdditionalAnimate
    {
        private const int CURVE_RESOLUTION = 360;
        public Transform Tail { get; private set; }
        public GameObject gameObject => Tail.gameObject;
        bool IGlobalUpdates.enabled => Tail.gameObject.activeSelf;

        [field: SerializeField] public bool Enable { get; set; } = true;
        public string Name => "Tail";

        [SerializeField] private Vector3 amplitude = new(20f, 20f, 20f);

        [SerializeField] private float rangeRandomAmplitude = 0;

        [SerializeField] private AnimationCurve curveX;
        [SerializeField] private AnimationCurve curveY;
        [SerializeField] private AnimationCurve curveZ;

        private Transform[] tailBones; 
        private Bone[] bonesDefault;
        private float time;
        [Range(0f,4f)]
        [SerializeField] private float timeScale = 0.4f;

        private bool isResetNative;

        // Добавляем поля для Job System
        private NativeArray<float> sampledCurveX;
        private NativeArray<float> sampledCurveY;
        private NativeArray<float> sampledCurveZ;
        private NativeArray<Vector3> positions;
        private NativeArray<Quaternion> rotations;
        private NativeArray<Bone> defaultBonesNative;
        private TailWaveJob tailJob;
        private JobHandle tailJobHandle;

        [BurstCompile]
        private struct TailWaveJob : IJob
        {
            public float normalizedTime; // 0-1
            [ReadOnly] public Vector3 amplitude;
            [ReadOnly] public NativeArray<float> curveX;
            [ReadOnly] public NativeArray<float> curveY;
            [ReadOnly] public NativeArray<float> curveZ;
            [ReadOnly] public NativeArray<Bone> defaultBones;
            public NativeArray<Vector3> positions;
            public NativeArray<Quaternion> rotations;

            public void Execute()
            {
                int index = (int)(normalizedTime * (curveX.Length - 1));

                float x = curveX[index];
                float y = curveY[index];
                float z = curveZ[index];

                float xRotation = Mathf.Lerp(-amplitude.x, amplitude.x, x);
                float yRotation = Mathf.Lerp(-amplitude.y, amplitude.y, y);
                float zRotation = Mathf.Lerp(-amplitude.z, amplitude.z, z);

                Quaternion additionalRotation = Quaternion.Euler(xRotation, yRotation, zRotation);

                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = defaultBones[i].position;
                    rotations[i] = defaultBones[i].rotation * additionalRotation;
                }
            }
        }

        public void Initialize(AnimatorCharacterInput arcs)
        {
            Tail = arcs.PBody.Hips.GetChilds().First(t => t.name.ToLower().Contains("tail"));
            if (Tail == null) return;

            time = UnityEngine.Random.Range(0, 1f);

            amplitude.x += Random.Range(-rangeRandomAmplitude, rangeRandomAmplitude);
            amplitude.y += Random.Range(-rangeRandomAmplitude, rangeRandomAmplitude);
            amplitude.z += Random.Range(-rangeRandomAmplitude, rangeRandomAmplitude);

            tailBones = Tail.GetComponentsInChildren<Transform>();
            bonesDefault = CloneTransformBone(tailBones);

            sampledCurveX = new NativeArray<float>(CURVE_RESOLUTION, Allocator.Persistent);
            sampledCurveY = new NativeArray<float>(CURVE_RESOLUTION, Allocator.Persistent);
            sampledCurveZ = new NativeArray<float>(CURVE_RESOLUTION, Allocator.Persistent);

            UpdateCurveForNative();

            positions = new NativeArray<Vector3>(tailBones.Length, Allocator.Persistent);
            rotations = new NativeArray<Quaternion>(tailBones.Length, Allocator.Persistent);
            defaultBonesNative = new NativeArray<Bone>(bonesDefault, Allocator.Persistent);


            tailJob = new TailWaveJob
            {
                normalizedTime = time,
                amplitude = amplitude,
                curveX = sampledCurveX,
                curveY = sampledCurveY,
                curveZ = sampledCurveZ,
                positions = positions,
                rotations = rotations,
                defaultBones = defaultBonesNative
            };

            Globals.GlobalUpdates.AddListner(this);
        }

        public void UpdateCurveForNative()
        {
            for (int i = 0; i < CURVE_RESOLUTION; i++)
            {
                float time = i / (float)(CURVE_RESOLUTION - 1);
                sampledCurveX[i] = curveX.Evaluate(time);
                sampledCurveY[i] = curveY.Evaluate(time);
                sampledCurveZ[i] = curveZ.Evaluate(time);
            }
        }

        private static Bone[] CloneTransformBone(Transform[] transformBones)
        {
            var bones = new Bone[transformBones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].rotation = transformBones[i].localRotation;
                bones[i].position = transformBones[i].localPosition;
            }
            return bones;
        }

        void IGlobalUpdates.Update()
        {
            if (!Enable) return;

            time += Time.deltaTime * timeScale;
            if (time > 1F)
            {
                time -= 1f;
            }

            tailJob.normalizedTime = time;
            tailJob.amplitude = amplitude;

            tailJobHandle = tailJob.Schedule();
        }

        void IGlobalUpdates.LateUpdate()
        {
            tailJobHandle.Complete();

            if (!Enable) return;

            for (int i = 0; i < tailBones.Length; i++)
            {
                tailBones[i].localPosition = positions[i];
                tailBones[i].localRotation = rotations[i];
            }

            if (isResetNative)
            {
                isResetNative = false;
                UpdateCurveForNative();
            }

        }

        void IAdditionalAnimate.OnDestroy()
        {
            Globals.GlobalUpdates.RemoveListner(this);
            if (positions.IsCreated) positions.Dispose();
            if (rotations.IsCreated) rotations.Dispose();
            if (defaultBonesNative.IsCreated) defaultBonesNative.Dispose();
            ClearNativeCurve();
        }
        private void ClearNativeCurve()
        {
            if (sampledCurveX.IsCreated) sampledCurveX.Dispose();
            if (sampledCurveY.IsCreated) sampledCurveY.Dispose();
            if (sampledCurveZ.IsCreated) sampledCurveZ.Dispose();
        }

        public void Reset()
        {
            isResetNative = true;
        }
    }
}
