using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlayerDescription;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;

namespace AIInput
{
    public class AISeeWorld : MonoBehaviour, IAIOrchestrated
    {
        private CharacterMediator character;

        private CharacterMediator Character
        {
            get
            {   if(character == null)
                    character = GetComponent<CharacterMediator>();
                return character;
            }
        }

        [field: SerializeField, Min(4)] private int CountRay { get; set; } = 15;

        [field: SerializeField] private float DistanceSee { get; set; } = 10f;
        [field: SerializeField] private float AngleSee { get; set; } = 120f;

        [field: SerializeField] private TackingIK tackingIK;


        public List<RaycastHit> viewPoints = new();
        public List<RaycastHit> viewPointsWater = new();
        public List<RaycastHit> viewPointsFloor = new();

        public List<Vector3> viewPointEmpty = new();

        public RaycastHit[] ViewPoints { get; private set; }
        public RaycastHit[] ViewPointsWater { get; private set; }
        public RaycastHit[] ViewPointsFloor { get; private set; }

        public Vector3[] ViewPointEmpty { get; private set; }

        NativeArray<RaycastCommand> raycastCammands;

        NativeArray<RaycastHit> raycastHits;

        public Transform TargetLookEyes
        {
            get
            {
                return tackingIK.Target;
            }
            set
            {
                tackingIK.Target = value;
            }
        }

        private Ray[] getRays
        {
            get
            {
                List<Ray> rays = new List<Ray>();
                int dublCount = CountRay / 2;
                for (int i = -dublCount; i < dublCount + 1; i++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        Vector3 direction = Quaternion.Euler(0, AngleSee / CountRay * i, 0) * Character.Body.Head.forward;
                        direction.Normalize();
                        Vector3 axis = Vector3.Cross(direction, Vector3.up);
                        if(axis == Vector3.zero) axis = Vector3.right;
                        rays.Add(new Ray(Character.Body.Head.position, Quaternion.AngleAxis(30 * x, axis) * direction));
                    }
                }
                return rays.ToArray();
            }
        }

        public bool CanThink => true;

        private void Start()
        {
            Character.Body.OnFell += Lost;
            Character.Body.OnDied += Lost;
            Animator animator = Character.AnimatorInput.AnimatorHuman;
            if (tackingIK == null)
            {
                tackingIK = new TackingIK(animator);
            }
            else
                tackingIK.Animator = animator;
            Character.AnimatorInput.AddListenerIK(tackingIK);
            //updateCanSee = UpdateCanSee();
            //StartCoroutine(updateCanSee);

            raycastCammands  = new NativeArray<RaycastCommand>(getRays.Length, Allocator.Persistent);

            raycastHits = new NativeArray<RaycastHit>(getRays.Length, Allocator.Persistent);

            ViewPointsFloor = new RaycastHit[0];
            ViewPointsWater = new RaycastHit[0];
            ViewPointEmpty = new Vector3[0];
            ViewPoints = new RaycastHit[0];
        }
        private void Lost()
        {
            tackingIK.Target = null;
        }
        private void OnEnable()
        {
            AIOrchestrator.AddOrchestrated(this);
        }
        private void OnDisable()
        {
            AIOrchestrator.RemoveOrchestrated(this);
        }
        private void OnDestroy()
        {
            Character.AnimatorInput.RemoveListenerIK(tackingIK);
            raycastCammands.Dispose();
            raycastHits.Dispose();
            //StopCoroutine(updateCanSee);
        }
        private IEnumerator UpdateCanSee()
        {
            var wait = new WaitForSeconds(0.1f);
            while (true)
            {
                yield return wait;
                UpdateSee();
            }
        }

        private void UpdateSee()
        {
            //foreach (Ray ray in getRays)
            //{
            //    if (Physics.Raycast(ray, out RaycastHit hit, DistanceSee, MasksProject.RigidObject | MasksProject.SkinPlayer))
            //    {
            //        if (Vector3.Angle(Vector3.up, hit.normal) <= CharacterBody.CharacterInput.MaxAngleMove)
            //        {
            //            viewPointsFloor.Add(hit);
            //        }
            //        else
            //        {
            //            viewPoints.Add(hit);
            //        }
            //    }
            //    else
            //    {
            //        viewPointEmpty.Add(ray.GetPoint(DistanceSee));
            //    }
            //    if (Physics.Raycast(ray, out RaycastHit hit2, DistanceSee, MasksProject.RigidObject | MasksProject.Water))
            //    {
            //        if (MasksProject.Water == 1 << hit2.collider.gameObject.layer)
            //            viewPointsWater.Add(hit2);
            //    }
            //}

            //ViewPointsWater = viewPointsWater.ToArray();

            //ViewPointsFloor = viewPointsFloor.ToArray();

            //ViewPoints = viewPoints.ToArray();

            //ViewPointEmpty = viewPointEmpty.ToArray();
        }

        [SerializeField] private bool onDrawViewLine;
        private void OnDrawGizmos()
        {
            if (!onDrawViewLine) return;
            if (ViewPoints == null) return;
            //foreach (Vector3 hit in ViewPointEmpty)
            //{
            //    Gizmos.DrawLine(CharacterBody.Head.position, hit);
            //}
            foreach (RaycastHit hit in ViewPoints)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(Character.Body.Head.position, hit.point);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(hit.point, hit.normal);
            }
            Gizmos.color = Color.green;
            foreach (RaycastHit hit in ViewPointsFloor)
            {
                Gizmos.DrawLine(Character.Body.Head.position, hit.point);
                Gizmos.DrawRay(hit.point, hit.normal);
            }
            Gizmos.color = Color.blue;
            foreach (RaycastHit hit in ViewPointsWater)
            {
                Gizmos.DrawLine(Character.Body.Head.position, hit.point);
            }
        }

        JobHandle IAIOrchestrated.ScheduleJob()
        {
            Ray[] getRays = this.getRays;

            for (int i = 0; i < getRays.Length; i++)
            {
                raycastCammands[i] = new RaycastCommand(getRays[i].origin, getRays[i].direction, DistanceSee, MasksProject.RigidObject | MasksProject.Water | MasksProject.Player);
            }
            return RaycastCommand.ScheduleBatch(raycastCammands, raycastHits, 64);
            
        }

        void IAIOrchestrated.OnJobComplete()
        {
            List<RaycastHit> hits = raycastHits.Where(hit => hit.collider != null).ToList();

            List<RaycastHit> viewPointsWater = new List<RaycastHit>();

            List<RaycastHit> viewPointsFloor = new List<RaycastHit>();

            List<RaycastHit> viewPoints = new List<RaycastHit>();

            foreach (RaycastHit hit in hits)
            {
                if (MasksProject.Water == 1 << hit.collider.gameObject.layer)
                    viewPointsWater.Add(hit);
                else
                {
                    if (Vector3.Angle(Vector3.up, hit.normal) <= Character.Motor.MaxAngleMove)
                    {
                        viewPointsFloor.Add(hit);
                    }
                    else
                    {
                        viewPoints.Add(hit);
                    }
                }
            }

            ViewPointsWater = viewPointsWater.ToArray();

            ViewPointsFloor = viewPointsFloor.ToArray();

            ViewPoints = viewPoints.ToArray();

        }
    }
}
