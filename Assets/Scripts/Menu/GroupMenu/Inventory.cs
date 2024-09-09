using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GroupMenu
{
    internal class Inventory : MainGroup
    {
        public override TypeMenu TypeMenu => TypeMenu.Inventory;

        public static Inventory Instance { get; private set; }

        protected override void Start()
        {
            Instance = this;
        }

        protected override void Update()
        {
            if(Input.GetKeyDown(KeyCode.V))
            {
                Menu.ActivateMenu(new Inventory());
            }
        }
    }
}
