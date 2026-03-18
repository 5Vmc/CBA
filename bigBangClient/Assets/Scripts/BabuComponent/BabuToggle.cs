using System;
using UnityEngine.UI;
using Utils;

public class BabuToggle : Toggle
{
    public new event Action OnSelect;
    public new event Action OnDeselect;

    private bool state;

    private bool statusControl = true;

    protected override void Awake()
    {
        base.Awake();
        state = isOn;
        onValueChanged.AddListener(OnValueChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onValueChanged.RemoveListener(OnValueChanged);
    }

    public void EnableStatusControl()
    {
        statusControl = true;
    }

    public void DisableStatusControl()
    {
        statusControl = false;
    }

    private void OnValueChanged(bool flag)
    {
        // 由未选中变为选中
        if (flag && !state)
        {
            transform.parent.GetComponent<BabuToggleGroup>().Switch(this);
            if (statusControl)
            {
                GetComponent<StatusControl>()?.SetStatus(true);
            }
            OnSelect?.Invoke();
        }
        // 由选中变为未选中
        if (!flag && state)
        {
            if (statusControl)
            {
                GetComponent<StatusControl>()?.SetStatus(false);
            }
            OnDeselect?.Invoke();
        }
        state = flag;
    }
}
