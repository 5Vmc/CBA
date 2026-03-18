using BigBang;
using BigBang.UI;
using GameConfig.Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillGiftItemData
{
    public GiftSkillConfig cfg;
    public bool isUnLock;
    public int unLockGrade;
    public int CardStar;
    public int cardId;
    public PlayerCard card;
    /// <summary>
    /// 技能升级表， 升级的星数list
    /// </summary>
    public List<CardUpgradeConfig> SkillStarMap { get; internal set; }

    public SkillGiftItemData(int _cardid, int _ulockgrade, GiftSkillConfig _cfg)
    {
        cardId = _cardid;
        unLockGrade = _ulockgrade;
        cfg = _cfg;
        card = Player.CardManager.GetCard(_cardid);
        isUnLock = _ulockgrade <= card.EquipGrade;
    }
}

public class SkillGiftItem : MonoBehaviour
{
    [SerializeField] public TMP_Text txtSkillName;
    [SerializeField] public TMP_Text txtSkillDesc;
    [SerializeField] public InventoryBaseItem skillIcon;
    [SerializeField] public Image bgImg;
    [SerializeField] public TMP_Text txtUnlock;

    public async void SetData(SkillGiftItemData _data, int index)
    {
        txtSkillName.text = _data.cfg.Name;
        if (!_data.isUnLock)
        {
            txtUnlock.text = string.Format("(球员{0}阶解锁)", _data.unLockGrade);
            ColorUtility.TryParseHtmlString("#5a646d", out Color color);
            txtUnlock.color = color;
            txtSkillName.color = color;
            txtSkillDesc.color = color;
            SpriteManager.GetSprite(AtlasNames.Public, "bannerActive2", s => bgImg.sprite = s);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#0a5067", out Color color);
            txtUnlock.color = color;
            ColorUtility.TryParseHtmlString("#475057", out Color color1);
            txtSkillName.color = color1;
            ColorUtility.TryParseHtmlString("#243745", out Color color2);
            txtSkillDesc.color = color2;
            SpriteManager.GetSprite(AtlasNames.Public, "bannerActive1", s => bgImg.sprite = s);
            CardUpgradeConfig nxtStar = _data.SkillStarMap.FirstOrDefault((p) => { return p.Quality == _data.card.Quality && p.Star > _data.card.Star; });
            if (nxtStar != null)
            {
                txtUnlock.text = string.Format("(球员{0}星升级)", nxtStar.Star);
            }
            else
            {
                if (nxtStar == null) nxtStar = _data.SkillStarMap.FirstOrDefault((p) => { return p.Quality == _data.card.Quality + 1; });
                if (nxtStar != null)
                {
                    if (nxtStar.Star > 0)
                        txtUnlock.text = string.Format("(球员升品后{0}星升级)", nxtStar.Star);
                    else
                        txtUnlock.text = string.Format("(球员升品后升级)", nxtStar.Star);
                }
                else
                {
                    txtUnlock.text = "";
                }
            }
        }

        txtSkillDesc.text = _data.cfg.Desc;
        var sprite = await SpriteProxy.GetGiftSkillImg(_data.cfg);
        var fireSection = PlayerCard.GetSkillFireSection(_data.cfg);

        skillIcon.SetData("", "", sprite, 2, _data.isUnLock, false, false, _data.cfg.Fire > 0, fireSection);
        skillIcon.SetFire(_data.cfg.Fire > 0 ? true : false);
        skillIcon.OpenTips = false;
        skillIcon.SetText("");
    }
}
