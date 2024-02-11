using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPrimaLie : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponentInChildren<Animator>().Play("Lie");
    }
}
