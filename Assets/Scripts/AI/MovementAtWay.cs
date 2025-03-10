using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweener;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using PlayerDescription;
using System.Collections;
using static Unity.VisualScripting.Member;

namespace AIInput
{
    [Serializable]
    public class MovementAtWay : IInputCharacter
    {
        private MovementAtWay()
        { 
            ToComplited.AddListener(DeInitialize);
        }

        private NavMeshPath path;

        private IEnumerator PointsPath;

        private Vector3 TargetPositionPath;

        [SerializeField] private CharacterMediator charMediator;

        private CharacterMotor sourse => charMediator.Motor;

        private AnimatorCharacterInput animator;

        public AnimatorCharacterInput AnimatorCharacter 
        {
            get
            {
                if(animator == null)
                    animator = sourse.gameObject.GetComponent<AnimatorCharacterInput>();
                return animator;
            }
        }

        private Transform transform => sourse.transform;

        private BezierWay way;
        [Range(0, 1)]
        [SerializeField] private float step, minDistanceNextPoint;
        
        private float stepAtWay, currentStepAtWay;

        public float GetCurrentStep => currentStepAtWay;

        private Vector3 currentTargetPosition, direction;

        [SerializeField] private UnityEvent ToComplited = new();

        public static MovementAtWay Create(CharacterBody sourse ,BezierWay way, float _step, float minDistanceNextPoint)
        {
            MovementAtWay movement = new MovementAtWay();
            movement.step = _step;
            movement.stepAtWay = _step / way.DistanceWay;
            movement.minDistanceNextPoint = minDistanceNextPoint;
            movement.currentStepAtWay = 0;
            movement.charMediator = sourse.GetComponent<CharacterMediator>();
            return movement;
        }
        public static MovementAtWay Clone(MovementAtWay sourse, BezierWay way)
        {
            MovementAtWay movement = new MovementAtWay();
            movement.step = sourse.step;
            movement.stepAtWay = sourse.step / way.DistanceWay;
            movement.minDistanceNextPoint = sourse.minDistanceNextPoint;
            movement.currentStepAtWay = 0;
            movement.charMediator = sourse.charMediator;
            return movement;
        }
        public void Initialize(BezierWay _way)
        {
            if (_way == null)
            {
                Debug.LogError("Null BezierWay");
            }
            path = new();
            way = _way;
            SetStepWay(0);
            stepAtWay = step / way.DistanceWay;
            sourse.SetInputCharacter(this, setStack: true);
        }
        public void ReversePlay()
        {
            way.ReverseWay();
            Initialize(way);
            AddToCompleted(way.ReverseWay);
        }
        public void DeInitialize()
        {
            sourse.SetInputCharacter(null);
            ClearToComplited();
        }
        public void AddToCompleted(Action completed)
        {
            ToComplited.AddListener(completed.Invoke);
        }
        public void ClearToComplited()
        {
            ToComplited.RemoveAllListeners();
            ToComplited.AddListener(DeInitialize);
        }
        [field: SerializeField] public bool IsRun { get; set; }

        public bool IsCrouch => false;

        public bool Space()
        {
            return false;
        }
        public void SetStepWay(float step)
        {
            currentStepAtWay = step;
            CalculatePathNextPoint();
        }
        private void NextStepWay()
        {
            currentStepAtWay += stepAtWay;
            CalculatePathNextPoint();
        }
        private void CalculatePathNextPoint()
        {
            currentTargetPosition = BezierWay.GetPositionInProgress(way, currentStepAtWay + stepAtWay).Item1;
            if (sourse.isSwim)
            {
                TargetPositionPath = currentTargetPosition;
                return;
            }
            bool isCast = Physics.Raycast(new Ray(sourse.transform.position + 0.1F * Vector3.up, Vector3.down), out RaycastHit hit, 50F, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
            bool isCast2 = Physics.Raycast(new Ray(currentTargetPosition + 0.1F * Vector3.up, Vector3.down), out RaycastHit hit2, 50F, MasksProject.RigidObject, QueryTriggerInteraction.Ignore);
            NavMesh.CalculatePath(isCast ? hit.point : sourse.transform.position, isCast2 ? hit2.point : currentTargetPosition, NavMesh.AllAreas, path);
            PointsPath = path.corners.GetEnumerator();
            if(PointsPath.MoveNext())
                TargetPositionPath = (Vector3)PointsPath.Current;
        }
        public Vector3 Move(Transform source, out bool isMove)
        {
            if (way == null || currentStepAtWay >= 1f)
            {
                isMove = false;
                return Vector3.zero;
            }
            isMove = true;

            if(Time.frameCount % 10 == 0)
            {
                CalculatePathNextPoint();
            }
            if (Vector3.Distance(new Vector3(source.position.x, 0, source.position.z), new Vector3(currentTargetPosition.x, 0, currentTargetPosition.z)) < minDistanceNextPoint)
                NextStepWay();
            if (Vector3.Distance(new Vector3(source.position.x, 0, source.position.z), new Vector3(TargetPositionPath.x, 0, TargetPositionPath.z)) < minDistanceNextPoint)
            {
                if (PointsPath.MoveNext())
                    TargetPositionPath = (Vector3)PointsPath.Current;
            }

            direction = Quaternion.LookRotation(transform.position - TargetPositionPath, Vector3.up).eulerAngles;
            charMediator.Body.RotateBody(Quaternion.Euler(new Vector3(0, direction.y - 180, 0)));

            if (currentStepAtWay >= 1F)
            {
                ToComplited?.Invoke();
                isMove = false;
                return Vector3.zero;
            }
            return transform.forward;
        }

        public void OnWrawGizmos()
        {
            if (path == null)
                return;
            for(int i = 0; i < path.corners.Length - 1; i += 2)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(path.corners[i], path.corners[i+1]);
            }
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentTargetPosition, 0.1F);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(TargetPositionPath, 0.1F);
        }

    }
}
