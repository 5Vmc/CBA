using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using UnityEngine;
using Utils;
using System.Linq;

namespace BigBang.UI
{
    public class CardTrainInfoItem : MonoBehaviour
    {
        [SerializeField] private List<CardAbilityItem> left = new();
        [SerializeField] private List<CardAbilityItem> right = new();

        // 设置升星数据
        public void SetDataCmp(PlayerCard card, bool showAddedValue)
        {
            int cfgIndex = 0;
            var cfgs = Configs.CardAbility.GetConfigList().ToList();
            for (int i = 0; i < left.Count; i++)
            {
                left[i].Ability = cfgs[cfgIndex++].Id;
            }
            for (int i = 0; i < right.Count; i++)
            {
                right[i].Ability = cfgs[cfgIndex++].Id;
            }

            int index = 0;
            left.ForEach(item => item.SetDataCmp(card, index++, showAddedValue));
            right.ForEach(item => item.SetDataCmp(card, index++, showAddedValue));
        }

        // 设置显示数据
        public void SetDataShow(PlayerCard card)
        {
            int cfgIndex = 0;
            var cfgs = Configs.CardAbility.GetConfigList().ToList();
            for (int i = 0; i < left.Count; i++)
            {
                left[i].Ability = cfgs[cfgIndex++].Id;
            }
            for (int i = 0; i < right.Count; i++)
            {
                right[i].Ability = cfgs[cfgIndex++].Id;
            }

            int index = 0;
            Color blue = new Color(98 / 255f, 181 / 255f, 224 / 255f, 1);
            left.ForEach(item => { item.SetDataShow(card, index++); });
            right.ForEach(item => { item.SetDataShow(card, index++); });
        }

        // 初始化翻牌动画
        public void InitTurnAnim()
        {
            left.ForEach(item => item.transform.localScale = new Vector3(1, 0, 1));
            right.ForEach(item => item.transform.localScale = new Vector3(1, 0, 1));
        }

        // 播放翻牌动画
        public void PlayTurnAnim()
        {
            for (int i = 0; i < left.Count; i++)
            {
                left[i].transform.DOTurn(Vector3.up, 0.1f).SetDelay(i * 0.1f);
            }
            for (int i = 0; i < right.Count; i++)
            {
                right[i].transform.DOTurn(Vector3.up, 0.1f).SetDelay(i * 0.1f);
            }
        }

        // 播放增加数值动画
        public void PlayAddScoreAnim()
        {
            left.ForEach(item => AddScore(item));
            right.ForEach(item => AddScore(item));
        }

        // 增加数值动画
        private void AddScore(CardAbilityItem item)
        {
            var value1 = int.Parse(item.Value1Text.text.TrimStart('+'));
            var value2 = int.Parse(item.Value2Text.text.TrimStart('+'));
            item.Value2Text.GetComponent<LoomAnim>().Stop();
            item.Value2Text.SetAlpha(1);
            DOTween.To(value => item.Value1Text.text = ((int)value).ToString(), value1, value1 + value2, 1f).SetEase(Ease.Linear).AddTo(this.gameObject);
            DOTween.To(value => item.Value2Text.text = ((int)value).ToString(), value2, 0, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                item.Value2Text.SetAlpha(0);
            }).AddTo(this.gameObject);
            //DOTween.To(value => item.Value2Text.text = "+" + ((int)value).ToString(), value2, 0, 1).SetEase(Ease.Linear);
        }
    }
}