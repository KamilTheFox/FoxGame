using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerDescription;
using UnityEngine;
using UnityEngine.AI;

namespace AIInput
{
    [Serializable]
    public class Harassment : BehaviourInput
    {
        [SerializeField] private List<GameObject> gameObjects = new List<GameObject>();

        [Range(0, 1)]
        [SerializeField] private float step, minDistanceNextPoint;

        [SerializeField] private float distanceStopHarassment;

        [SerializeField] private float DistanseRun;

        private List<IHunted> IHunteds = new List<IHunted>();

        IHunted CurrentHunted;

        private NavMeshPath path;

        IEnumerator PointsPath;

        private Vector3 direction;

        public Vector3 TargetPositionPath { get; private set; }

        public Harassment(CharacterMediator body) : base(body)
        {
            FindHunted();
        }

        public override void Disable()
        {
            
        }

        public override void Enable()
        {

        }

        public void FindHunted()
        {
            path = new NavMeshPath();
            IHunteds = new List<IHunted>();
            gameObjects.ForEach(gameObject =>
            {
                if (gameObject.TryGetComponent(out IHunted hunted))
                    IHunteds.Add(hunted);
            });
        }
        
        public void OnGizmos()
        {
            if (path == null) return;
            Gizmos.color = Color.yellow;
            Vector3 previus = Vector3.zero;
            foreach(Vector3 vector in path.corners)
            {
                if(previus != Vector3.zero)
                {
                    Gizmos.DrawSphere(previus, 0.1F);
                    Gizmos.DrawSphere(vector, 0.1F);
                    Gizmos.DrawLine(previus, vector);
                }
                previus = vector;
            }
        }

        public void CalculatePath()
        {
            IHunted[] hunteds = IHunteds.Where((t) => !t.IsDie).ToArray();
            if (hunteds.Length > 0)
                CurrentHunted = hunteds[0];
            if (CurrentHunted == null)
                return;
            bool isCast = Physics.Raycast(new Ray(CurrentHunted.transform.position + Vector3.up, Vector3.down), out RaycastHit hit, 50F, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
            bool isCast2 = Physics.Raycast(new Ray(Character.MainCollider.bounds.center + 0.1F * Vector3.up, Vector3.down), out RaycastHit hit2, 50F, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
            Debug.DrawLine(Character.MainCollider.bounds.center, hit2.point + hit2.normal * 0.01F, Color.magenta);
            NavMesh.CalculatePath(isCast2 ? hit2.point + hit2.normal * 0.01F : Character.transform.position, isCast ? hit.point : CurrentHunted.transform.position, NavMesh.AllAreas, path);

            var list = path.corners.ToList();
            if (list.Count > 1)
                list.RemoveAt(0);
            PointsPath = list.GetEnumerator();
            if (PointsPath.MoveNext())
                TargetPositionPath = (Vector3)PointsPath.Current;
        }
        protected override Vector3 Move(Transform source)
        {
            if (Time.frameCount % 10 == 0)
            {
                CalculatePath();
                IsRun = Vector3.Distance(CurrentHunted.transform.position, Character.Transform.position) > DistanseRun;
            }
            if (CurrentHunted == null)
                return Vector3.zero;
            if (Vector3.Distance(new Vector3(source.position.x, 0, source.position.z), new Vector3(TargetPositionPath.x, 0, TargetPositionPath.z)) < minDistanceNextPoint ||
                distanceStopHarassment > Vector3.Distance(Character.Transform.position, TargetPositionPath))
            {
                if (PointsPath.MoveNext())
                {
                    TargetPositionPath = (Vector3)PointsPath.Current;
                        CalculatePath();
                }
                else
                {
                    CalculatePath();
                }
            }
            direction = Quaternion.LookRotation(source.position - TargetPositionPath, Vector3.up).eulerAngles;
            Character.Body.RotateBody(Quaternion.Euler(new Vector3(0, direction.y - 180, 0)));

            Vector3 velosity = source.forward;

            if (distanceStopHarassment > Vector3.Distance(Character.Transform.position, CurrentHunted.transform.position))
                velosity = Vector3.zero;

            return velosity;
        }
    }
}
