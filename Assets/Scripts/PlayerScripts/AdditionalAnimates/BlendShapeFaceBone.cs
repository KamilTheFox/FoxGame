using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerDescription
{
    public class BlendShapeFaceBone : IAdditionalAnimate
    {
        public string Name => "FaceBone BlendShape";
        [field: SerializeField] public bool Enable { get; set; } = true;

        [SerializeField] private Transform[] boneFaces; // Кости лица через инспектор

        [Serializable]
        private class TransformPreset
        {
            [field: SerializeField] public string Name { get; set; }
            public List<BoneBlendData> bones = new();
            public float weight;
        }

        [Serializable]
        private struct BoneBlendData
        {
            public int boneIndex;
            public Vector3 position;
            public Quaternion rotation;
        }

        [SerializeField] private List<TransformPreset> shapes = new();

        private Dictionary<string, TransformPreset> presets = new Dictionary<string,TransformPreset>();

        [HideInInspector, SerializeField] private bool isChangeShape;

        private string defauldName = "Shape";

        public void Initialize(AnimatorCharacterInput input)
        {
            foreach(TransformPreset preset in shapes)
            {
                presets.Add(preset.Name, preset);
            }
        }
        public void OnDestroy() { Reset(); }

        public void Reset()
        {
            if (boneFaces == null || boneFaces.Length == 0)
            {
                Debug.LogWarning("Не заданы кости лица!");
                return;
            }
            if (shapes.Count == 0)
            {
                shapes.Add(CaptureDefaultShape());
                return;
            }
            if (shapes[0].bones.Count != boneFaces.Length)
            {
                Debug.LogWarning("Количество костей изменилось! Пересчитываю все шейпы...");
                ApplyDefaultPose();
                shapes[0] = CaptureDefaultShape();
            }
            ApplyDefaultPose();
        }

        public void OnGUI()
        {
            if (boneFaces == null || boneFaces.Length == 0)
            {
                GUILayout.Box("Назначьте кости лица в инспекторе!");
                return;
            }

            GUILayout.Label($"Количество костей: {boneFaces.Length}");

            if (shapes.Count == 0)
            {
                GUILayout.Box("Нажмите Reset для создания дефолтного шейпа");
                return;
            }

            isChangeShape = GUILayout.Toggle(isChangeShape, $"Edit Shapes");

            if (isChangeShape == false) return;

            defauldName = GUILayout.TextArea(defauldName);

            if (GUILayout.Button("Добавить шейп"))
            {
                var preset = new TransformPreset
                {
                    weight = 0f,
                    Name = defauldName,
                };
                CaptureBlendShape(preset);
                shapes.Add(preset);
            }

            for (int i = 1; i < shapes.Count; i++)
            {
                shapes[i].Name = GUILayout.TextArea(shapes[i].Name);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Shape {i}: Weight: {shapes[i].weight}", GUILayout.Width(140));

                shapes[i].weight = GUILayout.HorizontalSlider(shapes[i].weight, 0f, 1f);
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Захватить позу"))
                {
                    CaptureBlendShape(shapes[i]);
                }

                if (GUILayout.Button("Применить"))
                {
                    ApplyShapeForced(i);
                }
                Color old = GUI.color;
                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    shapes.RemoveAt(i);
                    i--;
                }
                GUI.color = old;
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Apply Pose"))
            {
                ApplyPose();
            }
            if (GUILayout.Button("DefaultPose"))
            {
                ApplyDefaultPose();
            }

        }

        private void CaptureBlendShape(TransformPreset preset)
        {
            preset.bones.Clear();

            for (int i = 0; i < boneFaces.Length; i++)
            {
                Vector3 defaultPos = shapes[0].bones[i].position;
                Quaternion defaultRot = shapes[0].bones[i].rotation;

                Vector3 currentPos = boneFaces[i].localPosition;
                Quaternion currentRot = boneFaces[i].localRotation;

                if ((defaultPos - currentPos).sqrMagnitude > 0.0001f ||
                    Quaternion.Angle(defaultRot, currentRot) > 0.01f)
                {
                    preset.bones.Add(new BoneBlendData
                    {
                        boneIndex = i,
                        position = currentPos - defaultPos,
                        rotation = currentRot * Quaternion.Inverse(defaultRot)
                    });
                }
            }
        }
        private TransformPreset CaptureDefaultShape()
        {
            var defaultShape = new TransformPreset();
            defaultShape.bones = new List<BoneBlendData>(boneFaces.Length);

            for (int i = 0; i < boneFaces.Length; i++)
            {
                defaultShape.bones.Add(new BoneBlendData()
                {
                    position = boneFaces[i].localPosition,
                    rotation = boneFaces[i].localRotation,
                    boneIndex = i,
                });
            }
            defaultShape.weight = 1f;
            return defaultShape;
        }
        private void ApplyPose()
        {
            ApplyDefaultPose();
            for (int shapeIndex = 1; shapeIndex < shapes.Count; shapeIndex++)
            {
                if (shapes[shapeIndex].weight > 0)
                {
                    ApplyShape(shapeIndex, shapes[shapeIndex].weight);
                }
            }
        }
        private void ApplyDefaultPose()
        {
            if (shapes.Count == 0) return;

            for (int i = 0; i < shapes[0].bones.Count; i++)
            {
                boneFaces[i].localPosition = shapes[0].bones[i].position;
                boneFaces[i].localRotation = shapes[0].bones[i].rotation;
            }
        }
        private void ApplyShape(int indexShape, float weight)
        {
            if (indexShape >= shapes.Count || weight <= 0) return;

            var shape = shapes[indexShape];
            foreach (var boneData in shape.bones)
            {
                Transform bone = boneFaces[boneData.boneIndex];
                bone.localPosition += boneData.position * weight;
                bone.localRotation = Quaternion.Slerp(
                    bone.localRotation,
                    bone.localRotation * boneData.rotation,
                    weight
                );
            }
        }
        public void ApplyShapeForced(int indexShape)
        {
            ApplyDefaultPose(); 
            ApplyShape(indexShape, 1f); 
        }

    }
}
