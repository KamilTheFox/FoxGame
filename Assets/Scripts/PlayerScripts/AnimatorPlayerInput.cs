using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerInput))]
public class AnimatorPlayerInput : MonoBehaviour
{
    private Animator Animator { get; set; }
    private PlayerBody PBody { get; set; }

    private PlayerInput InputC => PBody.PlayerInput;

    private InterceptionOnIK interceptionOnIK;

    private class InterceptionOnIK : MonoBehaviour
    {
        public UnityEvent onAnimatorIK = new UnityEvent();
        private void OnAnimatorIK()
        {
            onAnimatorIK.Invoke();
        }
    }
    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
        //interceptionOnIK = Animator.gameObject.AddComponent<InterceptionOnIK>();
        PBody = GetComponent<PlayerBody>();
        InputC.eventInput.EventMovement += MoveAnimate;
        InputC.eventInput[TypeAnimation.Jump].AddListener(() => Animator.SetTrigger("Jump"));
        InputC.eventInput[TypeAnimation.Fall].AddListener(() =>
        {
            if(!Animator.GetCurrentAnimatorStateInfo(0).IsName("Fall"))
                Animator.SetTrigger("Fall");
        });
        InputC.eventInput[TypeAnimation.Landing].AddListener(() => Animator.SetBool("Landing", InputC._isGrounded));
        InputC.AddFuncStopMovement(() =>
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName("Landing");
        });
        //interceptionOnIK.onAnimatorIK.AddListener(OnAnimatorIK);
    }
    [SerializeField] Vector3Int vect;
    [SerializeField] Vector3 farvardLo, farvardNorm;
    [SerializeField] bool isIKHead;

    [SerializeField] Vector3 vector;

    private Vector2 velositySmoothAnimation = Vector2.zero;
    private void MoveAnimate(Vector3 forward, bool move)
    {
        if (!move || forward == Vector3.zero)
        {
            Animator.SetInteger("Speed", 0);
            return;
        }
        int Speed = 2;
        if(InputC.isRun)
        {
            Speed = 5;
        }
        vect = Vector3Int.RoundToInt(transform.InverseTransformDirection(forward));

        Animator.SetInteger("Speed", Speed);

        Animator.SetFloat("RunForward", velositySmoothAnimation.x);
        Animator.SetFloat("RunRight", velositySmoothAnimation.y);
        velositySmoothAnimation = Vector2.Lerp(velositySmoothAnimation,new Vector2(vect.z, vect.x), Time.fixedDeltaTime * 5);
    }
    /*
    private void OnAnimatorIK()
    {
        if (!isIKHead) return;
        if (!CameraControll.instance.IsPlayerControll(PBody)) return;
        Animator.SetLookAtWeight(1F);
        Transform head = Animator.GetBoneTransform(HumanBodyBones.Head);
        Animator.SetBoneLocalRotation(HumanBodyBones.Head, head.localRotation);
    }
    */
}
