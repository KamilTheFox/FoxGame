using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMenu
{
    public class DebugAnimation : MainGroup
    {
        public override TypeMenu TypeMenu => TypeMenu.DebugAnimation;
        public static Animator Animator { get; set; }

        public static List<string> Animations = new List<string> { "Idle", "Sits", "Idle_Sits", "Run", "Run_Fast" };
        static MenuUI<Text> Text;
        protected override void Activate()
        {
            Text.SetText(Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.GetLText().GetTextUI());
        }
        protected override void Start()
        {
            Text = MenuUI<Text>.Create("Animation", GetTransform() , "None".GetTextUI(), true);
        }
    }
}
