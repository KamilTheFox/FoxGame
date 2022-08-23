using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GroupMenu;

public class SceneMenu : MonoBehaviour
{
    private void Start()
    {
        Menu.ActivateMenu(new MainMenu());
    }
    public static void LoadLevel(int level)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(level);
    }
   
}
