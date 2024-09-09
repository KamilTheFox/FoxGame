using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweener;
using UnityEngine;
using GroupMenu;
using UnityEngine.AI;
using AIInput;

public class MovingAvatarAndCamera : MonoBehaviour
{
    [Serializable]
   private class StartAnimation
    {
        [SerializeField] Animator animator;
        [SerializeField] string NameAnimation;

        public void Start()
        {
            animator.Play(NameAnimation);
        }
    }

    private static MovingAvatarAndCamera instance;

    [SerializeField] private MovementAtWay movementAtWay;

    [SerializeField] private StartAnimation[] startAnimation;

    private IExpansionBezier moveCamera;

    [SerializeField] private BezierWay[] WaysAvatar;
    [SerializeField] private BezierWay[] WaysCamera;

    private int currentIndex = -999;

    private static readonly Dictionary<string, int> keyWayName = new()
    {
        ["Back"] = -1,
        ["Settings"] = 0,
        ["Play"] = 1,
    };
    public void Start()
    {
        instance = this;
        startAnimation.ToList().ForEach(t => t.Start());
    }
    public static void MoveWayMenu(string nameWay, Action action)
    {
        instance.MoveToNameWayMenu(nameWay);
        instance.movementAtWay.AddToCompleted(action);
    }

    public void OnDrawGizmos()
    {
        movementAtWay.OnWrawGizmos();
    }
    public void MoveToNameWayMenu(string nameWay)
    {
        movementAtWay.ClearToComplited();
        MainGroup.InteractableHat(false);
        movementAtWay.AddToCompleted(() => MainGroup.InteractableHat(true));
        int index = keyWayName[nameWay];
        if (index == -1)
        {
            if (moveCamera != null)
            {
                if(currentIndex == 0)
                {
                    movementAtWay.AnimatorCharacter.PlayForced(PlayerDescription.TypeAnimation.Idle01);
                }
                movementAtWay.AnimatorCharacter.applyRootMotion = false;
                movementAtWay.ReversePlay();

                movementAtWay.IsRun = true;

                moveCamera.ReverseProgress();
                Tween.Start((IExpansionTween)moveCamera);
            }

            return;
        }
        currentIndex = index;
        movementAtWay.AnimatorCharacter.applyRootMotion = false;
        movementAtWay.Initialize(WaysAvatar[currentIndex]);
        movementAtWay.IsRun = true;
        
        movementAtWay.AddToCompleted(() => {
            if (currentIndex == 0)
            {
                movementAtWay.AnimatorCharacter.Animator.Play("Macarena");
                movementAtWay.AnimatorCharacter.applyRootMotion = true;
            }
        });


        var currWayCamera = WaysCamera[currentIndex];

        moveCamera = Tween.GoWay(CameraControll.instance.transform, currWayCamera, 2 + index * 2, true);
        moveCamera.ChangeEase(index == 1? Ease.CubicRoot : Ease.FourthRoot);
        moveCamera.onUpdate.AddListener(() =>
        {
            CameraControll.instance.transform.position = moveCamera.CurrentPosition;
            CameraControll.instance.transform.rotation = Quaternion.Lerp(currWayCamera[0]._Point.rotation, currWayCamera[currWayCamera.pointsBezier.Count - 1]._Point.rotation, moveCamera.Timer);
        });
    }
}
