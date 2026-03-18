using DG.Tweening;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class LeagueScoreboardItem : MonoBehaviour
    {
        [SerializeField] private Image rankTextBgImage = null;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private Image rankImg;
        [SerializeField] private ClubIconItem clubIcon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text sessionText; //轮次
        [SerializeField] private TMP_Text winText; //胜
        [SerializeField] private TMP_Text failedText; //负

        [SerializeField] private TMP_Text repeatedText; //连胜/负
        [SerializeField] private TMP_Text gainLossText; //场均得失
        [SerializeField] private GameObject particle;

        [SerializeField] private RectTransform star;
        [SerializeField] private RectTransform pos1;
        [SerializeField] private RectTransform pos2;
        [SerializeField] private RectTransform pos3;

        [SerializeField] private Image upBgImage = null;
        [SerializeField] private Image downBgImage = null;

        [SerializeField] private Color white = new Color();
        [SerializeField] private Color green = new Color();

        [SerializeField] private Color winColor = new Color();
        [SerializeField] private Color redColor = new Color();

        private void Start()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() =>
            {
                star.anchoredPosition = pos1.anchoredPosition;
                star.localScale = Vector3.one * 0.2f;
            });
            sequence.Append(star.DOAnchorPos(pos2.anchoredPosition, 1.5f).SetEase(Ease.Linear));
            sequence.Append(star.DOAnchorPos(pos3.anchoredPosition, 1.5f).SetEase(Ease.Linear));
            sequence.Insert(0, star.DOScale(0, 3).SetEase(Ease.InQuart));
            sequence.Insert(0, star.DORotate(Vector3.forward * 360, 3, RotateMode.LocalAxisAdd));
            sequence.AppendInterval(1);
            sequence.SetLoops(-1);
            sequence.AddTo(this.gameObject);
        }

        private readonly string noDataStr = "-";
        public async void SetData(int rank, LeagueScorebarTeam data)
        {
            if (data == null) return;
            if (data.BaseData.TeamId == Player.GbId)
            {
                nameText.color = green;
            }
            else
            {
                nameText.color = white;
            }
            // 设置排名
            if (data.Session == 0)
            {
                rankImg.gameObject.SetActive(false);
                rankTextBgImage.gameObject.SetActive(true);
                particle.gameObject.SetActive(false);
                upBgImage.gameObject.SetActive(false);
                downBgImage.gameObject.SetActive(false);

                rankTextBgImage.enabled = false;
                rankText.text = noDataStr;
                sessionText.text = noDataStr;
                winText.text = noDataStr;
                failedText.text = noDataStr;
                repeatedText.text = noDataStr;
                repeatedText.color = white;
                gainLossText.text = "--/--";
            }
            else
            {
                rankTextBgImage.enabled = true;
                rankImg.gameObject.SetActive(true);
                rankTextBgImage.gameObject.SetActive(false);
                particle.SetActive(rank == 1);
                upBgImage.gameObject.SetActive(rank <= 3 && Player.PVPManager.serverLeagueData.LeagueInfo.LeagueLevel < Configs.LeagueRewardRank.GetConfigList()[^1].Level);
                downBgImage.gameObject.SetActive(rank >= 18 && Player.PVPManager.serverLeagueData.LeagueInfo.LeagueLevel > Configs.LeagueRewardRank.GetConfigList()[0].Level);
                if (rank <= 3)
                {
                    rankImg.sprite = await SpriteProxy.GetRank(rank);
                }
                else
                {
                    rankTextBgImage.gameObject.SetActive(true);
                    rankText.text = rank.ToString();
                    rankImg.gameObject.SetActive(false);
                }

                // 设置轮次
                sessionText.text = data.Session.ToString();
                // 设置积，分胜3分、平1分、负0分，各队伍按照胜平负场次累计积分
                // integralText.text = (3 * data.Win + data.Deuce).ToString();
                // 胜
                winText.text = data.Win.ToString();
                // 平
                //  deuceText.text = data.Deuce.ToString();
                // 负
                failedText.text = data.Failed.ToString();

                //连胜负
                int seqCount = 0;
                int markType = GameResultType.None;
                for (int i = data.Record.Count - 1; i >= 0; i--)
                {
                    if (i == data.Record.Count - 1)
                    {
                        markType = data.Record[i];
                    }

                    if (data.Record[i] == markType)
                    {
                        seqCount++;
                    }
                    else
                    {
                        break;
                    }
                }


                if (markType == GameResultType.Win)
                {
                    repeatedText.color = winColor;
                    repeatedText.text = seqCount.ToString() + "连胜";
                }
                else
                {
                    repeatedText.color = redColor;
                    repeatedText.text = seqCount.ToString() + "连败";
                }

                //场均得失
                if (data.Session > 0)
                {
                    gainLossText.text = (data.Obtain * 1.0f / data.Session).ToString("#.#") + "/" + (data.Lost * 1.0f / data.Session).ToString("#.#"); //得失
                }
                else
                {
                    gainLossText.text = "--/--";
                }
            }
            // 设置球队图片
            clubIcon.SetIcon(data.BaseData.TeamIcon);
            // 设置球队名称
            nameText.text = data.BaseData.TeamName;




        }

        [SerializeField] private Image bgLightImage = null;
        [SerializeField] private Image bgDarkImage = null;
        public void SetBackgroundColor(bool isLight)
        {
            bgLightImage.gameObject.SetActive(isLight);
            bgDarkImage.gameObject.SetActive(!isLight);
        }
    }
}