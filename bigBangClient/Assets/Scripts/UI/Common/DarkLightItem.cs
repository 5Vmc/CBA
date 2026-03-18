using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DarkLightItem : MonoBehaviour
{
    [SerializeField] private List<GameObject> darkList = new();
    [SerializeField] private List<GameObject> LightList = new();

    [SerializeField] private bool isLight = false;
    public void SetLight(bool isLight)
    {
        this.isLight = isLight;
        RefreshLight();
    }

    private void RefreshLight()
    {
        foreach (var item in darkList)
        {
            item?.SetActive(!isLight);
        }
        foreach (var item in LightList)
        {
            item?.SetActive(isLight);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        RefreshLight();
    }
#endif

}
