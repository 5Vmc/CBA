using System;
using BigBang;
using UnityEngine.UI;

public class BabuToggleGroup : ToggleGroup
{
    public BabuToggle EnableToggle;
    public event Action<BabuToggle, BabuToggle> OnValueChanged;
    /// <summary>
    /// 从 0 开始
    /// </summary>
    public int EnableIndex
    {
        get => EnableToggle.transform.GetSiblingIndex();
    }

    public int Count
    {
        get
        {
            int cnt = 0;
            for (int i = 0; i < transform.childCount; i++)
            {

                var toggle = transform.GetChild(i).GetComponent<BabuToggle>();
                if (toggle)
                    cnt++;
            }
            return cnt;
        }

    }

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < transform.childCount; i++)
        {
            var toggle = transform.GetChild(i).GetComponent<BabuToggle>();
            if (toggle)
            {
                toggle.group = this;
            }

        }
    }

    public void Switch(BabuToggle newToggle, bool falseTrig = false)
    {
        if (falseTrig == false && newToggle == EnableToggle) return;
        var oldToggle = EnableToggle;
        EnableToggle.isOn = false;
        EnableToggle = newToggle;
        EnableToggle.isOn = true;
        OnValueChanged?.Invoke(oldToggle, newToggle);
        AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
    }

    public void Switch(int index)
    {
        if (index >= transform.childCount) return;
        var newToggle = transform.GetChild(index).GetComponent<BabuToggle>();

        var oldToggle = EnableToggle;
        EnableToggle.isOn = false;
        EnableToggle = newToggle;
        EnableToggle.isOn = true;
        OnValueChanged?.Invoke(oldToggle, newToggle);
        AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
    }
}
