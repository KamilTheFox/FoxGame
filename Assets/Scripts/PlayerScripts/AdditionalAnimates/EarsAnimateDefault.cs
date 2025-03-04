using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlayerDescription
{
    [Serializable]
    public class EarsAnimateDefault : IGlobalUpdates, IAdditionalAnimate
    {
        [Flags]
        public enum EarAnimationMode
        {
            Left = 1 << 1,
            Right = 1 << 2,
            Random = 1 << 3,
        }

        private const int CURVE_RESOLUTION = 360;

        CharacterMediator input;
        public Transform[] Ears { get; private set; }
        public GameObject gameObject => input.gameObject;
        bool IGlobalUpdates.enabled => input.gameObject.activeSelf;

        [field: SerializeField] public bool Enable { get; set; } = true;
        public string Name => "Ears";

        [SerializeField] private Vector3 amplitude = new(20f, 20f, 20f);

        [SerializeField] private float rangeRandomAmplitude = 0;

        private float cooldown;

        [SerializeField] private float cooldownMin = 4f, cooldownMax = 12f;

        [SerializeField] private AnimationCurve curveX;
        [SerializeField] private AnimationCurve curveY;
        [SerializeField] private AnimationCurve curveZ;

        private Transform[] earsBones;
        private Bone[] bonesDefault;
        private float time;

        [SerializeField] private EarAnimationMode modeEar;

        private IEnumerator waitCooldown;

        [Range(0f, 4f)]
        [SerializeField] private float timeScale = 0.4f;

        private bool isResetNative;
        private bool isSchedule;

        // Добавляем поля для Job System
        private NativeArray<float> sampledCurveX;
        private NativeArray<float> sampledCurveY;
        private NativeArray<float> sampledCurveZ;
        private NativeArray<Vector3> positions;
        private NativeArray<Quaternion> rotations;
        private NativeArray<Bone> defaultBonesNative;
        private EarsWaveJob earsJob;
        private JobHandle earJobHandle;

        [BurstCompile]
        private struct EarsWaveJob : IJob
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

                Quaternion leftRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
                Quaternion rightRotation = Quaternion.Euler(xRotation, -yRotation, -zRotation);

                int halfLength = positions.Length / 2;

                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = defaultBones[i].position;
                    rotations[i] = defaultBones[i].rotation * (i < halfLength ? leftRotation : rightRotation);
                }
            }
        }

        public void Initialize(CharacterMediator arcs)
        {
            input = arcs;
            Ears = arcs.Body.Head.GetChilds().Where(t => t.name.ToLower().Contains("ear")).ToArray();

            if (Ears == null) return;

            if (Ears.Length == 0) return;

            time = UnityEngine.Random.Range(0, 1f);

            cooldown = UnityEngine.Random.Range(cooldownMin, cooldownMax);

            amplitude.x += Random.Range(-rangeRandomAmplitude, rangeRandomAmplitude);
            amplitude.y += Random.Range(-rangeRandomAmplitude, rangeRandomAmplitude);
            amplitude.z += Random.Range(-rangeRandomAmplitude, rangeRandomAmplitude);

            List<Transform> allEarBones = new List<Transform>();
            foreach (var earBone in Ears)
            {
                allEarBones.AddRange(earBone.GetComponentsInChildren<Transform>());
            }

            earsBones = allEarBones.OrderBy(t => t.name.Contains(".R")).ToArray();

            bonesDefault = CloneTransformBone(earsBones);

            sampledCurveX = new NativeArray<float>(CURVE_RESOLUTION, Allocator.Persistent);
            sampledCurveY = new NativeArray<float>(CURVE_RESOLUTION, Allocator.Persistent);
            sampledCurveZ = new NativeArray<float>(CURVE_RESOLUTION, Allocator.Persistent);

            UpdateCurveForNative();

            positions = new NativeArray<Vector3>(earsBones.Length, Allocator.Persistent);
            rotations = new NativeArray<Quaternion>(earsBones.Length, Allocator.Persistent);
            defaultBonesNative = new NativeArray<Bone>(bonesDefault, Allocator.Persistent);


            earsJob = new EarsWaveJob
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

            waitCooldown = WaitCooldown();

            input.StartCoroutine(waitCooldown);

            Globals.GlobalUpdates.AddListner(this);
        }

        private IEnumerator WaitCooldown()
        {
            while (true)
            {
                yield return new WaitUntil(() => Enable);
                yield return new WaitForSeconds(cooldown);
                yield return new WaitUntil(() => isSchedule == false);
                isSchedule = true;
                cooldown = UnityEngine.Random.Range(cooldownMin, cooldownMax);
                if(modeEar.HasFlag(EarAnimationMode.Random))
                {
                    int i = Random.Range(0, 9);
                    if (i % 3 == 0)
                    {
                        modeEar = EarAnimationMode.Left | EarAnimationMode.Random;
                    }
                    else if (i % 3 == 1)
                    {
                        modeEar = EarAnimationMode.Right | EarAnimationMode.Random;
                    }
                    else
                    {
                        modeEar = EarAnimationMode.Right | EarAnimationMode.Random | EarAnimationMode.Left;
                    }
                }
            }
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

            if (isSchedule == false) return;

            time += Time.deltaTime * timeScale;
            if (time > 1F)
            {
                time = 0;
                isSchedule = false;
            }

            earsJob.normalizedTime = time;
            earsJob.amplitude = amplitude;

            earJobHandle = earsJob.Schedule();
        }

        void IGlobalUpdates.LateUpdate()
        {
            if (isSchedule == false) return;

            earJobHandle.Complete();

            if (!Enable) return;

            for (int i = 0; i < earsBones.Length; i++)
            {
                bool isLeftEar = i < earsBones.Length / 2;
                if (isLeftEar && (!modeEar.HasFlag(EarAnimationMode.Left))) continue;
                if (!isLeftEar && (!modeEar.HasFlag(EarAnimationMode.Right))) continue;

                earsBones[i].localPosition = positions[i];
                earsBones[i].localRotation = rotations[i];
            }

            if (isResetNative)
            {
                isResetNative = false;
                UpdateCurveForNative();
            }

        }

        void IAdditionalAnimate.OnDestroy()
        {
            input.StopCoroutine(waitCooldown);
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
