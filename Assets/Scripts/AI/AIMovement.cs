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
    public class AIMovement : BehaviourInput
    {
        private const float SECOND_WAIT_UPDATE = 0.5f;

        private Vector3? targetVector;

        private Vector3? targetPath;

        private NavMeshPath navPath;

        private List<NavMeshLink> meshLinks;

        private List<IEnumerator<Vector3>> navMeshPathsCalculate;

        private IEnumerator updateAICoroutine;

        [field: SerializeField] public int AgentTypeID { get; private set; }

        [field: SerializeField] public float DistanceOfCalmJump { private get; set; }

        [field: SerializeField] public float DistanceStopToTarget { private get; set; }

        [field: SerializeField] private float MinDistanceToOtherCharacters { get; set; }

        [field: SerializeField] public float DistanceSaveFall { get; set; }

        [Range(0, 1)]
        [SerializeField] private float minDistanceNextPoint = 0.5F;

        private IEnumerator<Vector3> path;

        [field: SerializeField] protected bool IsAvoidOther { private get; set; }

        private Vector3 CurrentPosition => GetPointForFloor(Character.CharacterInput.BoundsCollider.center);

        public AIMovement(CharacterBody body) : base(body)
        {

        }

        private AISeeWorld aISeeWorld;

        public AISeeWorld AISeeWorld
        {
            get
            {
                if (aISeeWorld == null)
                {
                    aISeeWorld = Character.GetComponent<AISeeWorld>();
                    if (aISeeWorld == null)
                    {
                        aISeeWorld = Character.gameObject.AddComponent<AISeeWorld>();
                    }
                }
                return aISeeWorld;
            }
        }

        private IEnumerator UpdateAI()
        {
            WaitForSeconds wait = new WaitForSeconds(SECOND_WAIT_UPDATE);
            while(true)
            {
                Update();
                yield return wait;
            }
        }
        private void Update()
        {
            if (Character.IsDie) return;
            if (targetVector == null)
            {
                AISeeWorld.TargetLookEyes.position = new Ray(Character.Head.position, Character.Transform.forward).GetPoint(4f);
                return;
            }
            else
                AISeeWorld.TargetLookEyes.position = targetVector.Value + Vector3.up;

            CalculationOfShortPath();
            CalculationPathNavMeshAndLink();

        }

        private void CalculationOfShortPath()
        {
            IEnumerator<Vector3> enumerator;

            navMeshPathsCalculate.Clear();

            NavMeshPathStatus status ;

            navMeshPathsCalculate.Add(CalculatePath(CurrentPosition, targetVector.Value, out status));

            List<RaycastHit> hits = new();

            hits.AddRange(AISeeWorld.ViewPointsWater);

            hits.AddRange(AISeeWorld.ViewPointsFloor);

            foreach (RaycastHit hit in hits)
            {
                float rangeYdistance = hit.point.y - Character.Transform.position.y;
                if (rangeYdistance > 0.3f || rangeYdistance < -DistanceSaveFall)
                    continue;

                enumerator = CalculatePath(hit.point + hit.normal * 0.1f, targetVector.Value, out status);
                if (status == NavMeshPathStatus.PathComplete)
                    navMeshPathsCalculate.Add(enumerator);
            }

            var distanse = navMeshPathsCalculate.Select(x => DistancePath(x)).Where(dist => dist > 1f);

            if (distanse.Count() == 0)
                return;

            int IndexMin = distanse.ToList().IndexOf(distanse.Min());

            path = navMeshPathsCalculate[IndexMin];

            if (path.MoveNext())
            {
                targetPath = path.Current;
            }
        }
        
        private void CalculationPathNavMeshAndLink()
        {
            NavMeshPathStatus status;

            List<RaycastHit> hits = new();

            hits.AddRange(AISeeWorld.ViewPointsWater);

            hits.AddRange(AISeeWorld.ViewPointsFloor);

            foreach (RaycastHit hit in hits)
            {
                float rangeYdistance = hit.point.y - Character.Transform.position.y;
                if (rangeYdistance > 0.3f || rangeYdistance < -DistanceSaveFall)
                    continue;
                NavMeshLink link = AILinkPool.GetFreeLink();

                link.agentTypeID = AgentTypeID;

                link.width = 1f;

                link.startPoint = CurrentPosition;

                link.endPoint = GetPointForFloor(hit.point + hit.normal);

                link.UpdateLink();

                meshLinks.Add(link);
            }

            CalculateClimbingPathAddLink();

            path = CalculatePath(CurrentPosition, targetVector.Value, out status);

            meshLinks.ForEach(link => AILinkPool.PushLink(link));

            meshLinks.Clear();

            if (status == NavMeshPathStatus.PathInvalid)
            {
                return;
            }

            if (path.MoveNext())
            {
                targetPath = path.Current;
            }
        }

        private void CalculateClimbingPathAddLink()
        {
            if (!Character.CharacterInput.isSwim)
                return;

            var array = AISeeWorld.ViewPoints.Select(hit => hit.point).ToArray();

            if (array.Length == 0)
                return;

            Vector3 median = GetMedianVector3(array);

            array = AISeeWorld.ViewPoints.Select(hit => hit.normal).ToArray();

            if (array.Length == 0)
                return;

            Vector3 medianNormal = GetMedianVector3(array);

            median = new Vector3(median.x, Character.CharacterInput.BoundsCollider.center.y, median.z);

            Vector3 ForwardSide = medianNormal * -1f;

            if (Physics.Raycast(new Ray(median + ForwardSide + Vector3.up * 1.5f, Vector3.down), out RaycastHit hit, 2f, MasksProject.RigidObject, QueryTriggerInteraction.Ignore))
            {
                NavMeshLink link = AILinkPool.GetFreeLink();

                link.agentTypeID = AgentTypeID;

                link.width = 1f;

                link.startPoint = GetPointForFloor(median + medianNormal);

                link.endPoint = GetPointForFloor(hit.point + hit.normal);

                link.UpdateLink();

                meshLinks.Add(link);
            }
        }

        public static Vector3 GetMedianVector3(Vector3[] sourceNumbers)
        {
            if (sourceNumbers == null)
                Debug.LogError($"sourceNumbers is Null");

            return new Vector3(GetMedian(sourceNumbers.Select(x => x.x).ToArray()), GetMedian(sourceNumbers.Select(y => y.y).ToArray()), GetMedian(sourceNumbers.Select(z => z.z).ToArray()));
        }

        public static float GetMedian(float[] sourceNumbers)
        {
            if (sourceNumbers == null)
                Debug.LogError($"sourceNumbers is Null");
            if (sourceNumbers.Length == 0)
                return 0;

            float[] sortedPNumbers = (float[])sourceNumbers.Clone();

            Array.Sort(sortedPNumbers);

            int size = sortedPNumbers.Length;
            int mid = size / 2;
            float median = (size % 2 != 0) ? sortedPNumbers[mid] : (sortedPNumbers[mid] + sortedPNumbers[mid - 1]) / 2f;
            return median;
        }

        public void SetDestination(Vector3 target)
        {
            this.targetVector = target;
        }
        public IEnumerator<Vector3> CalculatePath(Vector3 startPointPath, Vector3 endPointPath, out NavMeshPathStatus status)
        {
            NavMesh.CalculatePath(GetPointForFloor(startPointPath), GetPointForFloor(endPointPath), NavMesh.AllAreas, navPath);
            status = navPath.status;
            var list = navPath.corners.ToList();
            for(int i = 0; i < list.Count() - 1; i++)
            {
                if(Vector3.Distance(Character.transform.position, list[i]) > Vector3.Distance(Character.transform.position, list[i + 1]))
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            return list.GetEnumerator();
        }
        private float DistancePath(IEnumerator<Vector3> path)
        {
            float distance = 0f;
            path.MoveNext();
            Vector3 currentVector = path.Current;
            while (path.MoveNext())
            {
                distance += Vector3.Distance(currentVector, path.Current);
            }
            path.Reset();
            return distance;
        }
        private Vector3 GetPointForFloor(Vector3 PointView) 
        {
            if(Physics.Raycast(new Ray(PointView + Vector3.up * 0.1f, Vector3.down), out RaycastHit hit, 50F, MasksProject.RigidObject, QueryTriggerInteraction.Ignore))
                return hit.point + hit.normal * 0.01F;
            return PointView;
        }

        private static Vector3 ZeroY(Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }
        public override void Disable()
        {
            GameObject.Destroy(AISeeWorld.TargetLookEyes.gameObject);
            Character.AnimatorInput.OnCompletedClimbing -= OnCompletedClimbing;
            Character.StopCoroutine(updateAICoroutine);
        }

        public override void Enable()
        {
            navPath = new NavMeshPath();
            meshLinks = new();
            var obj = new GameObject("TargetLookEyesAI");
            obj.transform.SetParent(Character.Transform);
            Character.AnimatorInput.OnCompletedClimbing += OnCompletedClimbing;
            AISeeWorld.TargetLookEyes = obj.transform;
            navMeshPathsCalculate = new();
            updateAICoroutine = UpdateAI();
            Character.StartCoroutine(updateAICoroutine);
        }
        private void OnCompletedClimbing()
        {
            CalculateClimbingPathAddLink();
        }
        public override bool Space()
        {
            bool isSpase = CurrentSpace;
            CurrentSpace = false;
            return isSpase;
        }
        protected override Vector3 Move(Transform source)
        {
            Vector3 velosity = Vector3.zero;
            if (targetVector == null || path == null || targetPath == null)
                goto Continue;
            if (CurrentSpace)
                return Character.Transform.forward;
            if (Character.CharacterInput.isSwim && Character.CharacterInput.IsEdgePlaneClimbing)
            {
                CurrentSpace = true;
                return Character.Transform.forward;
            }

            if (Vector3.Distance(ZeroY(CurrentPosition), ZeroY(targetPath.Value)) < minDistanceNextPoint)
            {
                if (path.MoveNext())
                {
                    targetPath = (Vector3)path.Current;
                }
            }
            if (Vector3.Distance(ZeroY(CurrentPosition), ZeroY(targetVector.Value)) < DistanceStopToTarget)
            {
                ClearAllTargets();
                goto Continue;
            }

            Vector3 direction = Quaternion.LookRotation(source.position - targetPath.Value, Vector3.up).eulerAngles;
            Character.RotateBody(Quaternion.Euler(new Vector3(0, direction.y - 180, 0)));

            velosity = source.forward;

Continue:
            Vector3 VectorRejection = AvoidOthers();
            Vector3 сorrection = Vector3.Lerp(velosity, VectorRejection, 0.5F);
            velosity += сorrection;

            return velosity;
        }

        private void ClearAllTargets()
        {
            targetPath = null;
            targetVector = null;
            path = null;
        }
        private Vector3 AvoidOthers()
        {
            Vector3 velosity = Vector3.zero;
            if (IsAvoidOther == false) return velosity;
            foreach (RaycastHit hit in AISeeWorld.ViewPoints)
            {
                if(hit.collider == null) continue;
                CharacterBody other;
                if ((other = hit.collider.GetComponent<CharacterBody>()) != null)
                    if (!other.IsDie)
                    {
                        if (Vector3.Distance(CurrentPosition, other.Transform.position) < MinDistanceToOtherCharacters)
                        {
                            velosity += hit.normal;
                        }
                    }
            }
            return velosity;
        }

        public void OnDrawGizmos()
        {
            if (targetPath == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPath.Value, 0.1f);
        }
    }
}
