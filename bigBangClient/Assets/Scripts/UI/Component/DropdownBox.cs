using System.Collections;
using System.Collections.Generic;
using BigBang;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownBox : TMP_Dropdown
{

    public bool UseClickSound { set; get; } = true;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        CheckPlayClickSound();
    }

    private void CheckPlayClickSound()
    {
        AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
    }

}
