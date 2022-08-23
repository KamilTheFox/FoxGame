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
        static Dropdown dropdown;
        protected override void Activate()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(Animations);
            string name = Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            dropdown.value = Animations.IndexOf(name);
        }
        protected override void Start()
        {
            dropdown = MenuUI<Dropdown>.Create("Animation", GetTransform() , "None".GetTextUI(), true).Component;
            dropdown.AddOptions(Animations);
            dropdown.onValueChanged.AddListener((countAnim) => { Menu.PopMenu(true); Animator.Play(Animations[countAnim]);  });
        }
    }
}
