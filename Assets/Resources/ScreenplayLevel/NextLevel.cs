﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerDescription;


public class NextLevel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out CharacterBody trigger))
        {
            GameState.StartGame(GameState.TypeGame, SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
