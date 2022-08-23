using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class MyInputExpansion
{
    public static void AddAction(this KeyCode key, Action action)
    {
        if(MyInput.keyValueAction.ContainsKey(key))
        {
            MyInput.keyValueAction[key] += action;
            return;
        }
        MyInput.keyValueAction.Add(key, action);
    }
}
    public class MyInput : MonoBehaviour
    {
    public static Dictionary<KeyCode, Action> keyValueAction = new Dictionary<KeyCode, Action>();

    private void Update()
    {
        foreach (KeyCode keyCode in keyValueAction.Keys)
        {
            if (Input.GetKey(keyCode))
            {
                keyValueAction[keyCode]?.Invoke();
            }
        }
    }

}

