using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HundredFight2DropdownItem : MonoBehaviour
{
    [SerializeField] public BabuButton dropdownPrefab = null;
    [SerializeField] public Image itemBackground = null;
    [SerializeField] public TMP_Text itemLabel = null;
    [SerializeField] public Image itemFireImage = null;
    [HideInInspector] public int index = 0;
}
