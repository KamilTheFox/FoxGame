using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Tweener;
using PlayerDescription;
using GroupMenu;
using AIInput;

public class MovementAgentScripst : MonoBehaviour
{

    [SerializeField] private AIMovement movement;

    public bool moveble;


    public void Start()
    {
        GetComponent<CharacterInput>().IntroducingCharacter = movement;
        {
            //if (Movements2.Length > 0)
            //{
            //    Movements2[0].Character.CharacterInput.IntroducingCaracter = Movements2[0];
            //    Movements2[0].FindHunted();
            //    return;
            //}
            //for (int i = 0; i < Movements.Length; i++)
            //{
            //    if (i < Movements.Length - 1)
            //    {
            //        int t = i + 1;
            //        Movements[i].AddToCompleted(() =>
            //        {
            //            Movements[t].Initialize(BezierWays[0]);
            //        });
            //    }
            //}
            //if (MovementScriptLoop)
            //{
            //    Movements[Movements.Length - 1].AddToCompleted(Start);
            //}
            //Movements[0].Initialize(BezierWays[0]);
        }

    }
    private void Update()
    {
        if (!moveble) return;
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            movement.SetDestination(Camera.main.transform.position + Camera.main.transform.forward * 1.3F);
        }
    }
    private void OnValidate()
    {
        
    }
    private void OnDrawGizmos()
    {
        movement?.OnDrawGizmos();
    }

}
