using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweener;
using UnityEngine;
using VulpesTool;

namespace MoveablePlatform
{
    [AddComponentMenu("Factory/ConveyorBelt")]
    [RequireComponent(typeof(BoxCollider))]
    public class ConveyorBelt : VulpesMonoBehaviour
    {
        private const string TEXTURE_OFFSET = "_MainTex";

        [field: SerializeField] public Vector2 Velocity { get; private set; }

        [SerializeField] private float speed;

        private BoxCollider boxCollider;

        private HashSet<Rigidbody> objectsOnBelt = new HashSet<Rigidbody>();

        private Material material;

        [ButtonField(nameof(ReadHalfExtentsBoxCollider), buttonText: "Collider", position: ButtonPosition.Left)]
        [ButtonField(nameof(ReadHalfExtentsScale), buttonText: "Scale", position: ButtonPosition.Left)]
        [SerializeField] private Vector3 halfExtents;

        private void ReadHalfExtentsBoxCollider()
        {
            halfExtents = GetComponent<BoxCollider>().size * 0.5F;
        }
        private void ReadHalfExtentsScale()
        {
            halfExtents = transform.localScale * 0.5F;
        }

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
            if (TryGetComponent<Renderer>(out var renderer))
            {
                material = renderer.material;
            }
        }
        private void FixedUpdate()
        {
            CastBoxToMoveObjects();
        }

        private void CastBoxToMoveObjects()
        {
            objectsOnBelt.Clear();

            Quaternion orientation = transform.rotation;

            float castDistance = 0.1f;

            Vector3 center = transform.TransformPoint(boxCollider.center);
            center += Vector3.up * castDistance;
            Collider[] overlappedColliders = Physics.OverlapBox(
                center,
                halfExtents,
                orientation,
                MasksProject.EntityPlayer
            );
            
            foreach (var collider in overlappedColliders)
            {
                if (collider.attachedRigidbody != null)
                {
                    objectsOnBelt.Add(collider.attachedRigidbody);
                }
            }
        }

        private void Update()
        {
            MoveObjects();
            MoveTexture();
        }
        private void MoveObjects()
        {
            foreach (var rb in objectsOnBelt)
            {
                if (rb.gameObject.activeSelf == false) continue;

                if (rb != null)
                {
                    Vector3 movement = new Vector3(Velocity.x, 0, Velocity.y) * speed * Time.deltaTime;
                    rb.MovePosition(rb.position + movement);
                }
            }
        }
        private void MoveTexture()
        {
            if (material == null) return;

            Vector2 offset = material.GetTextureOffset(TEXTURE_OFFSET);
            offset += Velocity * speed * Time.deltaTime;
            material.SetTextureOffset(TEXTURE_OFFSET, offset);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, new Vector3(Velocity.x, 0, Velocity.y));

            if (boxCollider == null) return;

            Vector3 center = transform.TransformPoint(boxCollider.center);

            Quaternion orientation = transform.rotation;

            float castDistance = 0.1f;
            Gizmos.DrawCube(center + Vector3.up * castDistance, halfExtents * 2);
        }

    }
}
