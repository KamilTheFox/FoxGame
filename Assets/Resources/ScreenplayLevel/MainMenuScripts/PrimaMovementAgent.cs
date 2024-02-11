using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Tweener;
using PlayerDescription;
using GroupMenu;

public class PrimaMovementAgent : MonoBehaviour, IInputAI
{
    private const float SmoothDelay = 3F;

    [SerializeField] private BezierWay[] Ways;

    [SerializeField] private CharacterInput playerInput;

    private BezierWay current;

    private Vector3 direction, smoothDir;

    private float lerpWay, currentState;

    private void Start()
    {
        playerInput.InputAI = this;
        AnimatorCharacterInput Animator = playerInput.GetComponent<AnimatorCharacterInput>();
        Animator.Animator.Play("Macarena");

    }

    public void ActivateWayInButton(int indexWay)
    {
        //current = Ways[indexWay];
        //lerpWay = 1F / current.DistanceWay;
        //currentState = 0;
    }
    Vector3 vector3;

    public bool Jump()
    {
        return false;
    }

    public Vector3 Move(Transform source, out bool isMove)
    {
        if (current == null)
        {
            isMove = false;
            return Vector3.zero;
        }
        isMove = true;

        var value = BezierWay.GetPositionInProgress(current, currentState+ lerpWay);
        vector3 = value.Item1;

        if (Vector3.Distance(new Vector3(source.position.x, 0, source.position.z), new Vector3(value.Item1.x, 0, value.Item1.z)) < 0.5F)
            currentState += lerpWay;
        direction = Quaternion.LookRotation(source.position, value.Item1).eulerAngles;
        direction = new Vector3(direction.x, 0, direction.z);
        playerInput.Rigidbody.MoveRotation(Quaternion.LookRotation(direction));
        return direction;
    }
    private void OnDrawGizmos()
    {
        lerpWay = 1F / Ways[0].DistanceWay;
        for (float i = 0; i < 1;)
        {
            var value = BezierWay.GetPositionInProgress(Ways[0], i);
            var value2 = BezierWay.GetPositionInProgress(Ways[0], i+lerpWay);
            Gizmos.DrawLine(value.Item1, value2.Item1);
            i += lerpWay;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(vector3,0.5F);
        Gizmos.DrawRay(transform.position, direction);
    }
    private void Update()
    {
        
    }
}
