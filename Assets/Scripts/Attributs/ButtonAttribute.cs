using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : Attribute
{
    public string ButtonName;

    public int Order { get; private set; }

    public ButtonAttribute(string buttonName = "", int order = 0)
    {
        ButtonName = buttonName;
        Order = order;
    }
}
