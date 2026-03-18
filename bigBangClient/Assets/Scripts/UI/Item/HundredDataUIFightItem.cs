using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HundredDataUIFightItem : MonoBehaviour
{
    [SerializeField] public BabuButton hundredDataUIFightItem = null;
    [SerializeField] private TMP_Text titleText = null;

    public int index = -1;
    public string fightId = "";
    public void SetData(string fightId, int index)
    {
        this.index = index;
        this.fightId = fightId;
        titleText.text = (index + 1).ToString();
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        hundredDataUIFightItem.OnClick += OnClick;
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        hundredDataUIFightItem.OnClick -= OnClick;
    }

    private void OnClick(BabuButton _)
    {
        EventManager.Instance.Dispatch(EventID.OnClickHundredDataUIFightItem, this);
    }
}
