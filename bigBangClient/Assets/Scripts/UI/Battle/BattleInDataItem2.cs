using System.Collections.Generic;
using BigBang;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using FightCard = Protocol.FightCard;

public class BattleInDataItem2 : MonoBehaviour
{
    [HideInInspector] public bool isFirst = false;

    [SerializeField] public RectTransform thisTrans;
    [SerializeField] public GameObject DarkImage;
    [SerializeField] public GameObject FirstImage;
    [SerializeField] public GameObject InjureImage;
    [SerializeField] public GameObject MvpImage;
    [SerializeField] public List<TMP_Text> NumTextList;
    [SerializeField] public List<GameObject> LightImageList;

    PlayerStat playerStat = null;
    FightCard fightCard = null;
    bool useAni = false;
    public void SetData(PlayerStat playerStat, FightCard fightCard, bool useAni = false)
    {
        this.playerStat = playerStat;
        this.fightCard = fightCard;
        this.useAni = useAni;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (this.playerStat == null)
        {
            RefreshUINone();
        }
        else
        {
            RefreshUINormal();
        }
    }
    private void RefreshUINone()
    {
        InjureImage.SetActive(false);
        foreach (var item in NumTextList)
        {
            item.text = "/";
        }
        NumTextList[1].gameObject.SetActive(true);
        peakYearPanel.gameObject.SetActive(false);
        SetColorZero();
    }
    [SerializeField] private PeakImage peakImage = null;
    [SerializeField] private RectTransform peakYearPanel = null;
    [SerializeField] private TMP_Text peakNameText = null;
    [SerializeField] private TMP_Text peakYearText = null;
    private void RefreshUINormal()
    {

        CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(fightCard.CardId);
        bool isPeak = PlayerCard.IsPeak(cardModelConfig);
        peakImage.SetData(cardModelConfig);

        NumTextList[1].gameObject.SetActive(!isPeak);
        peakYearPanel.gameObject.SetActive(isPeak);
        if (!isPeak)
        {
            SetText(NumTextList[1], fightCard.Name.ToString());
        }
        else
        {
            SetText(peakNameText, cardModelConfig.Name);
            SetText(peakYearText, cardModelConfig.PeakYear);
        }

        //序号姓名受伤，得分篮板助攻，抢断盖帽失误犯规，投篮三分罚球

        SetText(NumTextList[0], fightCard.Number.ToString());
        SetText(NumTextList[1], fightCard.Name.ToString());

        InjureImage.SetActive(playerStat.Hurt != 0);
        MvpImage.SetActive(false);

        SetText(NumTextList[2], playerStat.Point.ToString());
        SetText(NumTextList[3], playerStat.Rebound.ToString());
        SetText(NumTextList[4], playerStat.Assist.ToString());

        SetText(NumTextList[5], playerStat.Steal.ToString());
        SetText(NumTextList[6], playerStat.Turnover.ToString());
        SetText(NumTextList[7], playerStat.Block.ToString());
        SetText(NumTextList[8], playerStat.Foul.ToString());

        SetText(NumTextList[9], "{0}/{1}".SafeFormat(playerStat.FgCount, playerStat.FgTotal));
        SetText(NumTextList[10], "{0}/{1}".SafeFormat(playerStat.TpCount, playerStat.TpTotal));
        SetText(NumTextList[11], "{0}/{1}".SafeFormat(playerStat.FtCount, playerStat.FtTotal));

        SetText(NumTextList[12], "{0}".SafeFormat(playerStat.Time > 0 ? playerStat.Time + "分" : "--"));
        if (playerStat.IsMvp)
        {
            InjureImage.SetActive(false);
            MvpImage.SetActive(false);
        }

        RefreshColor();

    }

    private void SetText(TMP_Text text, string str)
    {
        bool needSetText = text.text != str;
        if (needSetText == true)
        {
            text.text = str;
            if (useAni) PlayStageBigAni(text);
        }
    }
    private readonly float bigTime = 0.1f;
    private readonly float smallTime = 0.3f;
    private readonly float bigScale = 1.3f;
    private HashSet<Sequence> seqSet = new();
    public void ClearStageBigAni()
    {
        foreach (var seq in seqSet)
        {
            seq.Kill();
        }
        seqSet.Clear();
        foreach (var numText in NumTextList)
        {
            numText.transform.localScale = Vector3.one;
        }
    }
    private void PlayStageBigAni(TMP_Text text)
    {
        DOTween.Kill(text?.transform);
        Sequence seq = DOTween.Sequence();
        seq.Append(text.transform.DOScale(bigScale, bigTime));
        seq.Append(text.transform.DOScale(1f, smallTime));
        seqSet.Add(seq);
    }

    [SerializeField] public Color firstZeroColor;
    [SerializeField] public Color firstNotZeroColor;
    [SerializeField] public Color otherZeroColor;
    [SerializeField] public Color otherNotZeroColor;
    private void RefreshColor()
    {
        Color zeroColor = isFirst ? firstZeroColor : otherZeroColor;
        Color notZeroColor = isFirst ? firstNotZeroColor : otherNotZeroColor;

        NumTextList[0].color = zeroColor;
        NumTextList[1].color = zeroColor;
        peakNameText.color = zeroColor;
        NumTextList[2].color = playerStat.Point > 0 ? notZeroColor : zeroColor;
        NumTextList[3].color = playerStat.Rebound > 0 ? notZeroColor : zeroColor;
        NumTextList[4].color = playerStat.Assist > 0 ? notZeroColor : zeroColor;
        NumTextList[5].color = playerStat.Steal > 0 ? notZeroColor : zeroColor;
        NumTextList[6].color = playerStat.Block > 0 ? notZeroColor : zeroColor;
        NumTextList[7].color = playerStat.Turnover > 0 ? notZeroColor : zeroColor;
        NumTextList[8].color = playerStat.Foul > 0 ? notZeroColor : zeroColor;
        NumTextList[9].color = playerStat.FgTotal > 0 ? notZeroColor : zeroColor;
        NumTextList[10].color = playerStat.TpTotal > 0 ? notZeroColor : zeroColor;
        NumTextList[11].color = playerStat.FtTotal > 0 ? notZeroColor : zeroColor;
        NumTextList[11].color = playerStat.Time > 0 ? notZeroColor : zeroColor;
    }
    public void SetColorZero()
    {
        NumTextList[0].color = otherZeroColor;
        NumTextList[1].color = otherZeroColor;
        peakNameText.color = otherZeroColor;
        NumTextList[2].color = otherZeroColor;
        NumTextList[3].color = otherZeroColor;
        NumTextList[4].color = otherZeroColor;
        NumTextList[5].color = otherZeroColor;
        NumTextList[6].color = otherZeroColor;
        NumTextList[7].color = otherZeroColor;
        NumTextList[8].color = otherZeroColor;
        NumTextList[9].color = otherZeroColor;
        NumTextList[10].color = otherZeroColor;
        NumTextList[11].color = otherZeroColor;
        NumTextList[11].color = otherZeroColor;
    }
}
