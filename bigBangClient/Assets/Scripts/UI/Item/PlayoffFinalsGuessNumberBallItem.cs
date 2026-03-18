using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayoffFinalsGuessNumberBallItem : MonoBehaviour
{
    [SerializeField] public RectTransform luckyNumberBallItem = null;
    [SerializeField] public Image shadowImage = null;
    [SerializeField] public BabuButton selectedLuckyNumberButton = null;
    [SerializeField] public TMP_Text selectedLuckyNumberText = null;
    [SerializeField] public BabuButton normalLuckyNumberButton = null;
    [SerializeField] public TMP_Text normalLuckyNumberText = null;

    public int luckyNumber = 0;

    public void SetLuckyNumber(int _luckyNumber)
    {
        luckyNumber = _luckyNumber;
        selectedLuckyNumberText.text = luckyNumber.ToString();
        normalLuckyNumberText.text = luckyNumber.ToString();
    }

    public void SetSelected(bool _selected)
    {
        selectedLuckyNumberButton.gameObject.SetActive(_selected);
        normalLuckyNumberButton.gameObject.SetActive(!_selected);
    }

    private void OnEnable()
    {
        selectedLuckyNumberButton.OnClick += OnClickSelectedLuckyNumberButton;
        normalLuckyNumberButton.OnClick += OnClickNormalLuckyNumberButton;
    }
    private void OnDisable()
    {
        selectedLuckyNumberButton.OnClick -= OnClickSelectedLuckyNumberButton;
        normalLuckyNumberButton.OnClick -= OnClickNormalLuckyNumberButton;
    }

    private void OnClickSelectedLuckyNumberButton(BabuButton _)
    {
        OnClickLuckyNumberButton();
    }
    private void OnClickNormalLuckyNumberButton(BabuButton _)
    {
        OnClickLuckyNumberButton();
    }
    private void OnClickLuckyNumberButton()
    {
        EventManager.Instance.Dispatch(EventID.OnSelectPlayoffFinalsGuessNumberBallItem, this);
    }
}
