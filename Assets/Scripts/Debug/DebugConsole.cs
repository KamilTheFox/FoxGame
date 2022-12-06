using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GroupMenu;

public class DebugConsole : MonoBehaviour
{
    public static DebugConsole instance { get; private set; }

    private void Start()
    {
        instance = this;
        Application.logMessageReceived += SendDebug;
    }
    private void ActivateConsole(string title,string text)
    {
        ConsoleMenu.SetStackTrace(title, text);
        Menu.ActivateMenu<ConsoleMenu>();
    }
    public void SendDebug(string logString, string stackTrace, LogType type)
    {
        Action action = () => ActivateConsole(type.ToString() + " " + logString, stackTrace);
        string logString2 = logString + "\nOk - Open Console";
        switch (type)
        {
            case LogType.Error:
                if (Settings.LogWisible[0]) 
                    MessageBox.Error(logString2, action);
                break;
            case LogType.Warning:
               if (Settings.LogWisible[1]) 
                    MessageBox.Warning(logString2, action);
                break;
            case LogType.Log:
               if (Settings.LogWisible[2]) 
                    MessageBox.Info(logString2, action);
                break;
        };
    }
}




