using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using GameConfig.Config;
using Babu;
using static BigBang.ClassicManager;
using Protocol;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;
using System.Linq;
using System;
using DG.Tweening;

namespace BigBang.UI
{
    public class CardUpStarItemData1
    {
        public string abilityName;
        public int abilityValue;
        public int abilityToValue;
        public bool isMax;

        public CardUpStarItemData1(string _name, int value, int tovalue, bool ismax)
        {
            abilityName = _name;
            abilityValue = value;
            abilityToValue = tovalue;
            isMax = ismax;
        }
    }

    public class CardUpStarItem1 : MonoBehaviour
    {
        [SerializeField] private Image imgMax;
        [SerializeField] private Image imgBg1;
        [SerializeField] private Image imgBg2;
        [SerializeField] private Image imgArrow;
        [SerializeField] private TMP_Text txtTitle;
        [SerializeField] private TMP_Text txtValue;
        [SerializeField] private TMP_Text txtToValue;


        private CardUpStarItemData1 data;
        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
        public void SetData(CardUpStarItemData1 _data, int index = 0)
        {
            imgBg1.gameObject.SetActive(index % 2 == 0);
            imgBg2.gameObject.SetActive(index % 2 != 0);

            data = _data;
            txtTitle.text = _data.abilityName + "能力";
            txtValue.text = "+" + _data.abilityValue.ToString() + "%";
            if (!_data.isMax)
            {
                txtToValue.text = "+" + _data.abilityToValue.ToString() + "%";
                imgArrow.gameObject.SetActive(true);
                imgMax.gameObject.SetActive(false);
                txtToValue.gameObject.SetActive(true);
            }
            else
            {
                imgArrow.gameObject.SetActive(false);
                imgMax.gameObject.SetActive(true);
                txtToValue.gameObject.SetActive(false);
            }

            if (_data.abilityToValue > _data.abilityValue)
            {
                ColorUtility.TryParseHtmlString("#13B237", out Color color1);
                txtToValue.color = color1;
                imgArrow.gameObject.SetActive(true);
            }
            else
            {
                ColorUtility.TryParseHtmlString("#A6B1B9", out Color color1);
                txtToValue.color = color1;
                imgArrow.gameObject.SetActive(false);
            }


        }
    }
}
