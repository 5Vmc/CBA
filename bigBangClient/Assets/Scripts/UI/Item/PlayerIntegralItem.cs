using Babu;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class PlayerIntegralItem : MonoBehaviour
    {
        [SerializeField] private Image rankImg;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private Image playerImg;
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private ClubIconItem clubIcon;
        [SerializeField] private TMP_Text clubName;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text position1;
        [SerializeField] private TMP_Text position2;
        [SerializeField] private RectTransform star;
        [SerializeField] private RectTransform pos1;
        [SerializeField] private RectTransform pos2;
        [SerializeField] private RectTransform pos3;
        [SerializeField] private GameObject particle;
        [SerializeField] private PeakImage peakImage = null;

        [SerializeField] private Color MyNameColor = new();
        [SerializeField] private Color EnemyNameColor = new();

        private Sequence sequence = null;
        private List<Tween> tweens = new List<Tween>();

        private void Start()
        {
            sequence = DOTween.Sequence();
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
        }

        private void OnDestroy()
        {
            sequence?.Kill();
            tweens.ForEach(item => item?.Kill());
        }

        public async void SetData(int rank, LeagueCardRankData data, int value)
        {
            if (data == null) return;
            // 名次、球员头像和名字及擅长位置、球队队徽和名字、进球数（助攻榜等则显示对应的XX数）。
            // 设置排名
            rankImg.gameObject.SetActive(true);
            rankText.gameObject.SetActive(false);
            particle.SetActive(rank == 1);
            if (rank <= 3)
            {
                rankImg.sprite = await SpriteProxy.GetRank(rank);
            }
            else
            {
                rankText.gameObject.SetActive(true);
                rankText.text = rank.ToString();
                rankImg.gameObject.SetActive(false);
            }
            var cardCfg = Configs.CardModel.GetConfig(data.CardId);
            // 设置球员头像
            playerImg.sprite = await SpriteProxy.GetPlayerPortrait(cardCfg.Portrait);
            // 设置球员姓名
            playerName.text = cardCfg.Name;
            // 设置品质
            playerName.color = CBAColorUtil.Instance.GetColor(cardCfg.Quality);
            // 设置俱乐部图标
            clubIcon.SetIcon(data.Team.TeamIcon);
            // 设置俱乐部名称
            clubName.text = data.Team.TeamName;
            clubName.color = data.Team.TeamId == Player.GbId ? MyNameColor : EnemyNameColor;
            // 设置值
            valueText.text = value.ToString();
            // ⚠设置擅长位置
            var cfg = Configs.CardModel.GetConfig(data.CardId);
            if (cfg != null)
            {
                position1.text = Configs.SeparatedPosition.GetConfig(cfg.AdaptPosition[0]).Abbreviation;
                position1.transform.parent.gameObject.SetActive(true);
                position2.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                position1.transform.parent.gameObject.SetActive(false);
                position2.transform.parent.gameObject.SetActive(false);
            }
            peakImage.SetData(cardCfg);
        }

        //public void PlayFlash(float delay)
        //{
        //    float sourceValue = backgroundImg.color.a;
        //    tweens.Add(backgroundImg.DOFade(sourceValue - 0.1f, 0.15f).SetDelay(delay).OnComplete(() =>
        //    {
        //        tweens.Add(backgroundImg.DOFade(sourceValue, 0.15f));
        //    }));
        //}

        [SerializeField] private Image bgDarkImage = null;
        [SerializeField] private Image bgLightImage = null;
        // 设置背景颜色
        public void SetBackground(bool isLight)
        {
            bgLightImage.gameObject.SetActive(isLight);
            bgDarkImage.gameObject.SetActive(!isLight);
        }
    }
}
