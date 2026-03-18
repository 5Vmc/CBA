using BigBang;
using BigBang.Animation;
using BigBang.UI;
using DG.Tweening;
using GameConfig;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class CardUpAnim : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private SkeletonGraphic spineTuPo;
    [SerializeField] private Image bgImg1;
    [SerializeField] private Image bgImg;
    [SerializeField] private Button btnSpine;
    [SerializeField] private TMP_Text txtContinue;

    public void OnEnable()
    {
        btnSpine.onClick.AddListener(closeTip);
    }

    private void closeTip()
    {
        btnSpine.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        btnSpine.onClick.RemoveListener(closeTip);
    }

    public void clear()
    {
        btnSpine.gameObject.SetActive(true);
        spineTuPo.gameObject.SetActive(false);
    }

    [SerializeField] private InventoryBaseItem skIcon;
    [SerializeField] private TMP_Text skillNameText = null;
    [SerializeField] private TMP_Text skillDetailText = null;
    public async void PlayAni(PlayerCard playerCard)
    {
        clear();
        TouchManager.Instance.DisableTouch();

        List<SkillGiftItemData> list = Player.CardManager.GetGiftSkill(playerCard.CardId);
        SkillGiftItemData skillGiftItemData = list
            .Where(item => item.isUnLock)
            .OrderByDescending(item => item.unLockGrade)
            .FirstOrDefault();
        bool isUnlockGift = skillGiftItemData != null && skillGiftItemData.unLockGrade == playerCard.EquipGrade;
        if (isUnlockGift)
        {
            var _skCfg = skillGiftItemData.cfg;
            var _fireSection = PlayerCard.GetSkillFireSection(_skCfg);
            var sp = await SpriteProxy.GetGiftSkillImg(_skCfg);
            skIcon.SetData(_skCfg.Name, _skCfg.Desc, sp, _skCfg.Sklv, true, true, true, _skCfg.Fire > 0, _fireSection);
            skIcon.SetText("");
            skillNameText.text = "获得天赋：{0}".SafeFormat(_skCfg.Name);
            skillDetailText.text = _skCfg.Desc;
        }
        skIcon.gameObject.SetActive(isUnlockGift);
        skIcon.gameObject.SetAlpha(0);
        txtContinue.SetAlpha(0);

        Sequence seq = DOTween.Sequence();
        seq.Append(bgImg.DOFade(0f, 0.25f).From());
        seq.Append(bgImg1.DOFade(0f, 0.2f).From());

        seq.AppendCallback(() =>
        {
            spineTuPo.gameObject.SetActive(true);
            spineTuPo.Initialize(true);
            spineTuPo.AnimationState.SetAnimation(0, "play2", false);

            if (isUnlockGift)
            {
                skIcon.gameObject.DOFade(1f, 0.2f).SetDelay(1.0f).OnComplete(() =>
                {

                });
            }

            txtContinue.DOFade(1f, 0.2f).SetDelay(isUnlockGift ? 2.0f : 1.2f).OnComplete(() =>
            {
                TouchManager.Instance.EnableTouch();
            });

        });
    }

}
