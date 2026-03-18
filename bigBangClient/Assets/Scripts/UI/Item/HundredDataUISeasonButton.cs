using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using TMPro;
using UnityEngine;
using Utils;

public class HundredDataUISeasonButton : MonoBehaviour
{
    [SerializeField] public BabuButton hundredDataUISeasonButton = null;
    [SerializeField] private TMP_Text titleText = null;

    public int index = 0;
    public string title = "";
    public void SetData(string title, int index)
    {
        this.index = index;
        this.title = title;
        HundredManager.Instance.GetYearAndSession(title, out int year, out int session);
        if (year == 0 || session == 0)
        {
            titleText.text = title;
            return;
        }
        titleText.text = "{0}第{1}届".SafeFormat(year, session.ToChinese());
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        hundredDataUISeasonButton.OnClick += OnClick;
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        hundredDataUISeasonButton.OnClick -= OnClick;
    }

    private void OnClick(BabuButton _)
    {
        EventManager.Instance.Dispatch(EventID.OnClickHundredDataUISeasonItem, this);
    }
}
