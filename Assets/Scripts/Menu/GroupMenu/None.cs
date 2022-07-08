using System;
using UnityEngine;

namespace GroupMenu
{
    public class None : IActivatable
    {
        public TypeMenu TypeMenu => TypeMenu.None;

        public void Activate()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void Deactivate()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        public void Start()
        {
        }
    }
}
