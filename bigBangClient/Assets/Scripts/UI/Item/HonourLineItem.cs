using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using UnityEngine;

public class HonourLineItem : MonoBehaviour
{
    [SerializeField] private List<HonourItem> honourItemList = new();

    public void SetData(HonourLineItemData lineData, int index)
    {
        for (int i = 0; i < honourItemList.Count; i++)
        {
            HonourItem honourItem = honourItemList[i];
            if (i < lineData.honourGroupDataList.Count)
            {
                honourItem.SetData(lineData.honourGroupDataList[i]);
            }
            else
            {
                honourItem.SetData(null);
            }
        }
    }
}
