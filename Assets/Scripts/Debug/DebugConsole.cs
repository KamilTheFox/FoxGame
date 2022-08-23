using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GroupMenu;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugConsole : MonoBehaviour
{
    public static DebugConsole instance { get; private set; }

    public List<bool> LogWisible;

    private void Start()
    {
        instance = this;
        instance.LogWisible[0] = PlayerPrefs.GetInt("Error") == 1;
        instance.LogWisible[1] = PlayerPrefs.GetInt("Warning") == 1;
        instance.LogWisible[2] = PlayerPrefs.GetInt("Info") == 1;
        Application.logMessageReceived += SendDebug;
    }
    private void ActivateConsole(string title,string text)
    {
        ConsoleMenu.SetStackTrace(title, text);
        Menu.ActivateMenu(new ConsoleMenu());
    }
    public void SendDebug(string logString, string stackTrace, LogType type)
    {
        Action action = () => ActivateConsole(type.ToString() + " " + logString, stackTrace);
        string logString2 = logString + "\nOk - Open Console";
        switch (type)
        {
            case LogType.Error:
                if (LogWisible[0]) MessageBox.Error(logString2, action); break;
            case LogType.Warning:
               if (LogWisible[1]) MessageBox.Warning(logString2, action); break;
            case LogType.Log:
               if (LogWisible[2]) MessageBox.Info(logString2, action); break;
        };
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(DebugConsole)), CanEditMultipleObjects]
public class DebugConsoleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        DebugConsole console = (DebugConsole)target;


        string[] Logs = new string[] { "Error", "Warning", "Info" };
        for (int i = 0; i < Logs.Length; i++)
        {
            if (console.LogWisible.Count < Logs.Length)
                console.LogWisible.Add(false);
            console.LogWisible[i] = EditorGUILayout.Toggle(Logs[i], console.LogWisible[i]);
        }

        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
}

#endif




