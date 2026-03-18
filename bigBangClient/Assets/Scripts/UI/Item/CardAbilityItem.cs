using System;
using BigBang.Animation;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CardAbilityItem : MonoBehaviour
    {
        public TMP_Text ItemNameText;
        public TMP_Text Value1Text;
        public TMP_Text Value2Text;
        public Image BlackImage;
        public Image BlueImgImage;
        public BabuButton HelpButton;

        [HideInInspector] public int Ability;

        private void OnEnable()
        {
            HelpButton.OnClick += OnClickHelpButton;
        }
        private void OnDisable()
        {
            HelpButton.OnClick -= OnClickHelpButton;
        }
        private void OnClickHelpButton(BabuButton button)
        {
            AbilitytipsUIProperties abilitytipsUIProperties = new AbilitytipsUIProperties(cardAbilityConfig);
            abilitytipsUIProperties.SetPos(this.transform, new Vector3(0, -20f, 0));
            UIController.Instance.OpenWindow<AbilitytipsUI>(abilitytipsUIProperties);
        }

        private CardAbilityConfig cardAbilityConfig;
        private void SetData(int index)
        {
            cardAbilityConfig = Configs.CardAbility.GetConfig(Ability);
            // 设置训练名称
            ItemNameText.text = cardAbilityConfig.Name;

            // 设置条纹颜色
            float alpha = ((index % 2) == 0) ? 50 / 255f : 25 / 255f;
            BlackImage.SetAlpha(alpha);
            BlueImgImage.SetAlpha(alpha);
        }

        public void SetDataShow(PlayerCard card, int index)
        {
            SetData(index);
            // 设置训练属性值
            Value1Text.text = card.GetAbility(Ability).ToString();
            Value2Text.gameObject.SetActive(false);
        }

        public void SetDataCmp(PlayerCard card, int index, bool showAddedValue)
        {
            int addedValue = 0;
            SetData(index);
            Value1Text.gameObject.SetActive(true);
            // 设置训练属性值
            //Debug.LogWarning(Ability.ToString() + ":" + card.GetAbility(Ability).ToString());
            addedValue = card.GetAbility(Ability) - int.Parse(Value1Text.text);
            //Value1Text.text = card.GetAbility(Ability).ToString();

            // 设置升星后加多少属性值
            if (addedValue <= 0)  //  card.GetAbilityStarUpgradeAdd(Ability) == 0)
            {
                Value2Text.text = "+0";
                Value2Text.gameObject.SetActive(false);
            }
            else
            {
                Value2Text.text = "+" + addedValue;
                Value2Text.gameObject.SetActive(true);
                Value2Text.rectTransform.SetAnchoredPositionY(0);
                Value2Text.GetComponent<LoomAnim>().PlayText(0.5f, 0.5f, 0.3f);

            }

            if (showAddedValue == false)
            {
                Value2Text.gameObject.SetActive(false);
            }
        }
    }
}