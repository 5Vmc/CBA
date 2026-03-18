using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using BigBang.UI;

namespace BigBang.Animation
{
    public class SelectedSkillPadAnim : AnimBase
    {
        [SerializeField] private RectTransform condition;
        [SerializeField] private RectTransform trainBtn;
        [SerializeField] private SkillIcon skillIcon;
        [SerializeField] private RectTransform unlockBtn;
        [SerializeField] private GameObject costObj;
        [SerializeField] private RectTransform skillPad;
        

        private void InitUnlockAnim()
        {
            // 启用解锁按钮
            unlockBtn.gameObject.SetActive(true);
            // 启用前往学习按钮
            trainBtn.gameObject.SetActive(true);
            // 启用解锁条件
            condition.gameObject.SetActive(true);
            // 初始化缩放
            trainBtn.transform.localScale = Vector3.one * 0.8f;
            // 初始化透明度
            condition.gameObject.SetAlpha(1);
            unlockBtn.gameObject.SetAlpha(1);
            trainBtn.gameObject.SetAlpha(0);
        }
        private void InitSkillPad()
        {
            skillPad.gameObject.SetAlpha(0);
            skillPad.localScale = Vector3.one * 0.7f;
        }

        // 播放解锁动画
        public void PlayUnlockAnim(Action callback)
        {
            TouchManager.Instance.DisableTouch();
            InitUnlockAnim();

            // 隐藏解锁条件
            condition.gameObject.SetAlpha(0);
            costObj.SetAlpha(0);
            // 显示学习按钮
            trainBtn.gameObject.DOFade(1, 0.3f);
            // 隐藏解锁按钮
            unlockBtn.gameObject.DOFade(0, 0.3f);
            trainBtn.DOScale(1, 0.3f).OnComplete(() =>
            {
                trainBtn.transform.localScale = Vector3.one;
                condition.gameObject.SetAlpha(1);
                costObj.SetAlpha(1);
                unlockBtn.gameObject.SetAlpha(1);
                trainBtn.gameObject.SetAlpha(1);
                callback?.Invoke();
                TouchManager.Instance.EnableTouch();
            });
        }
        //特技框放大淡入动画
        public void PlayEnterAnim()
        {
            InitSkillPad();
            tweens.Add(skillPad.DOScale(1,0.4f));
            tweens.Add(skillPad.gameObject.DOFade(1, 0.4f));
        }
        
    }
}