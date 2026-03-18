using System;
using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{


    //球队数据
    public class BattleInDataPad1 : MonoBehaviour
    {
        #region 初始化

        [SerializeField] private Button hideButton;
        [SerializeField] private Image BgImage;
        [SerializeField] private List<BattleInDataItem1> BattleInDataItemList = new();
        [SerializeField] private Image EndImage;


        private void OnEnable()
        {
            hideButton.onClick.AddListener(OnClickHideButton);
        }

        private void OnDisable()
        {
            hideButton.onClick.RemoveListener(OnClickHideButton);
        }

        private void OnClickHideButton()
        {
            this.gameObject.SetActive(false);
        }

        #endregion

        #region UI显示
        public void InitUI()
        {
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItemList[i].TitleText.text = Configs.FightStat[i].Name;
                bool isNormal = i < 9;
                bool isDark = i % 2 != 0;

                BattleInDataItemList[i].DarkBgImage.gameObject.SetActive(isDark);

                BattleInDataItemList[i].SliderFgGrayImage.gameObject.SetActive(false);
                BattleInDataItemList[i].SliderFgImage.gameObject.SetActive(false);

                BattleInDataItemList[i].ScoreTextBlue.gameObject.SetActive(isNormal);
                BattleInDataItemList[i].ScoreTextRed.gameObject.SetActive(isNormal);
                BattleInDataItemList[i].ScoreTextPercent1Blue.gameObject.SetActive(!isNormal);
                BattleInDataItemList[i].ScoreTextPercent2Blue.gameObject.SetActive(!isNormal);
                BattleInDataItemList[i].ScoreTextPercent1Red.gameObject.SetActive(!isNormal);
                BattleInDataItemList[i].ScoreTextPercent2Red.gameObject.SetActive(!isNormal);

                BattleInDataItemList[i].ScoreTextBlue.text = "0";
                BattleInDataItemList[i].ScoreTextRed.text = "0";
                BattleInDataItemList[i].ScoreTextPercent1Blue.text = "0.00%";
                BattleInDataItemList[i].ScoreTextPercent2Blue.text = "(0/0)";
                BattleInDataItemList[i].ScoreTextPercent1Red.text = "0.00%";
                BattleInDataItemList[i].ScoreTextPercent2Red.text = "(0/0)";

                BattleInDataItemList[i].SliderFgImageBlue.fillAmount = 0;
                BattleInDataItemList[i].SliderFgImageRed.fillAmount = 0;
            }
        }
        public void RefreshUI()
        {
            RefreshUI(Player.BattleManager.battleTeamData.teamStatBlue, Player.BattleManager.battleTeamData.teamStatRed);
        }
        public void RefreshUIFast()
        {
            RefreshUI(Player.BattleManager.battleTeamData.teamStatBlue, Player.BattleManager.battleTeamData.teamStatRed, false, 0);
        }
        public void RefreshUI(TeamStat teamStatBlue, TeamStat teamStatRed, bool useAni = true, float aniTime = 0.4f)
        {
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem1 battleInDataItem = BattleInDataItemList[i];
                FightStatConfig fightStatConfig = Configs.FightStat[i];

                switch (fightStatConfig.Id)
                {
                    case 1:
                        SetItemData(battleInDataItem, teamStatBlue.Point, teamStatRed.Point, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 2:
                        SetItemData(battleInDataItem, teamStatBlue.Rebound, teamStatRed.Rebound, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 3:
                        SetItemData(battleInDataItem, teamStatBlue.Assist, teamStatRed.Assist, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 4:
                        SetItemData(battleInDataItem, teamStatBlue.Steal, teamStatRed.Steal, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 5:
                        SetItemData(battleInDataItem, teamStatBlue.Block, teamStatRed.Block, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 6:
                        SetItemData(battleInDataItem, teamStatBlue.Turnover, teamStatRed.Turnover, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 7:
                        SetItemData(battleInDataItem, teamStatBlue.FtCount, teamStatRed.FtCount, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 8:
                        SetItemData(battleInDataItem, teamStatBlue.TpCount, teamStatRed.TpCount, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 9:
                        SetItemData(battleInDataItem, teamStatBlue.Foul, teamStatRed.Foul, fightStatConfig.ProMax, useAni, aniTime);
                        break;
                    case 10:
                        SetItemData(battleInDataItem, teamStatBlue.FgCount, teamStatBlue.FgTotal, teamStatRed.FgCount, teamStatRed.FgTotal, useAni, aniTime);
                        break;
                    case 11:
                        SetItemData(battleInDataItem, teamStatBlue.FtCount, teamStatBlue.FtTotal, teamStatRed.FtCount, teamStatRed.FtTotal, useAni, aniTime);
                        break;
                    case 12:
                        SetItemData(battleInDataItem, teamStatBlue.TpCount, teamStatBlue.TpTotal, teamStatRed.TpCount, teamStatRed.TpTotal, useAni, aniTime);
                        break;
                    default:
                        break;
                }
            }
        }

        private HashSet<Tween> sliderTweensSet = new();
        private void SetItemData(BattleInDataItem1 battleInDataItem, int bluePoint, int redPoint, int proMax, bool useAni, float aniTime = 0.4f)
        {
            float bluePercent = bluePoint / (float)proMax;
            float redPercent = redPoint / (float)proMax;
            bool isBlueHigher = bluePoint > redPoint;
            bool isRedHigher = bluePoint < redPoint;
            if (bluePoint == redPoint)
            {
                isBlueHigher = true;
                isRedHigher = true;
            }
            battleInDataItem.ScoreTextBlue.text = bluePoint.ToString();
            battleInDataItem.ScoreTextRed.text = redPoint.ToString();
            battleInDataItem.SliderFgImageBlue.sprite = isBlueHigher ? battleInDataItem.SliderFgImage.sprite : battleInDataItem.SliderFgGrayImage.sprite;
            battleInDataItem.SliderFgImageRed.sprite = isRedHigher ? battleInDataItem.SliderFgImage.sprite : battleInDataItem.SliderFgGrayImage.sprite;
            if (useAni == false)
            {
                battleInDataItem.SliderFgImageBlue.fillAmount = bluePercent;
                battleInDataItem.SliderFgImageRed.fillAmount = redPercent;
            }
            else
            {
                sliderTweensSet.Add(battleInDataItem.SliderFgImageBlue.DOFillAmount(bluePercent, aniTime));
                sliderTweensSet.Add(battleInDataItem.SliderFgImageRed.DOFillAmount(redPercent, aniTime));
            }
        }
        private void SetItemData(BattleInDataItem1 battleInDataItem, int blueCount, int blueTotal, int redCount, int redTotal, bool useAni, float aniTime = 0.4f)
        {
            float bluePercent = blueTotal == 0 ? 0 : blueCount / (float)blueTotal;
            float redPercent = redTotal == 0 ? 0 : redCount / (float)redTotal;
            bool isBlueHigher = bluePercent > redPercent;
            bool isRedHigher = bluePercent < redPercent;
            if (bluePercent == redPercent)
            {
                isBlueHigher = true;
                isRedHigher = true;
            }
            battleInDataItem.ScoreTextPercent1Blue.text = "{0:N0}%".SafeFormat(bluePercent * 100);
            battleInDataItem.ScoreTextPercent1Red.text = "{0:N0}%".SafeFormat(redPercent * 100);
            battleInDataItem.ScoreTextPercent2Blue.text = "({0}/{1})".SafeFormat(blueCount, blueTotal);
            battleInDataItem.ScoreTextPercent2Red.text = "({0}/{1})".SafeFormat(redCount, redTotal);
            battleInDataItem.SliderFgImageBlue.sprite = isBlueHigher ? battleInDataItem.SliderFgImage.sprite : battleInDataItem.SliderFgGrayImage.sprite;
            battleInDataItem.SliderFgImageRed.sprite = isRedHigher ? battleInDataItem.SliderFgImage.sprite : battleInDataItem.SliderFgGrayImage.sprite;
            if (useAni == false)
            {
                battleInDataItem.SliderFgImageBlue.fillAmount = bluePercent;
                battleInDataItem.SliderFgImageRed.fillAmount = redPercent;
            }
            else
            {
                sliderTweensSet.Add(battleInDataItem.SliderFgImageBlue.DOFillAmount(bluePercent, aniTime));
                sliderTweensSet.Add(battleInDataItem.SliderFgImageRed.DOFillAmount(redPercent, aniTime));
            }
        }

        #endregion

        #region 动画

        public void PrepareAni()
        {
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem1 battleInDataItem = BattleInDataItemList[i];
                battleInDataItem.transform.localScale = new Vector3(1, 0, 1);
            }
            this.gameObject.SetAlpha(0);
        }

        Sequence uiSequence = null;
        public void DoUIAni(Action aniEndCallBack = null)
        {
            uiSequence = DOTween.Sequence();
            uiSequence.AppendInterval(0.3f);
            uiSequence.Append(this.gameObject.DOFade(1, 0.5f));
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem1 battleInDataItem = BattleInDataItemList[i];
                if (i < 6) uiSequence.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
                uiSequence.Append(battleInDataItem.transform.DOScaleY(1f, 0.06f));
            }
            uiSequence.AppendInterval(0.2f);
            uiSequence.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_REG); RefreshUI(Player.BattleManager.battleTeamData.teamStatBlue, Player.BattleManager.battleTeamData.teamStatRed, true, 0.6f); });
            uiSequence.AppendInterval(0.6f);
            uiSequence.AppendCallback(() =>
            {
                aniEndCallBack?.Invoke();
            });
        }
        public void ClearAni()
        {
            uiSequence?.Kill();
            uiSequence = null;
            foreach (var sliderTween in sliderTweensSet)
            {
                sliderTween?.Kill();
            }
            sliderTweensSet.Clear();
        }

        #endregion

        #region 让pad适配不同的界面
        public enum BattleInDataPad1BgState
        {
            Normal,
            Light,
        }

        [SerializeField] private GameObject BgImageLight;
        public void SetBgState(BattleInDataPad1BgState battleInDataPad1BgState)
        {
            BgImage.gameObject.SetActive(false);
            BgImageLight.SetActive(false);
            EndImage.gameObject.SetActive(true);

            switch (battleInDataPad1BgState)
            {
                case BattleInDataPad1BgState.Normal: BgImage.gameObject.SetActive(true); break;
                case BattleInDataPad1BgState.Light: BgImageLight.SetActive(true); EndImage.gameObject.SetActive(false); break;
            }
        }
        #endregion

    }
}