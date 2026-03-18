using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeActivityTabPoint : MonoBehaviour
{
    [SerializeField] private GameObject lightGo;
    [SerializeField] private GameObject darkGo;

    private bool isLight;
    public void SetLight(bool isLight)
    {
        this.isLight = isLight;
        lightGo.SetActive(isLight);
        darkGo.SetActive(!isLight);
    }
}
