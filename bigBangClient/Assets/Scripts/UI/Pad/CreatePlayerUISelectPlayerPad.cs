using Babu;
using BigBang.Animation;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class CreatePlayerUISelectPlayerPad : MonoBehaviour
    {
        [SerializeField] private BabuButton nextBtn;
        [SerializeField] private BabuButton previousBtn;
        [SerializeField] private BabuButton randomBtn;
        [SerializeField] private CreatePlayerUISelectClothesPad previousPad;
        [SerializeField] private GameObject nextPad;
        [SerializeField] private List<CardItem> cardItems;
        [SerializeField] private CreatePlayerUIAnim nextPadAnim;

        [SerializeField] public SelectPlayerPadAnim Anim;

        private UIEffect randomEffect;
        // private const int resultCount = 8;

        private System.Random rnd;

        public List<int> Result { get; private set; } = new List<int>();
        //随机出来的最好的一张紫卡
        public int starCardId;
        public event Action OnFinished;

        private void Awake()
        {
            randomBtn.Anim = null;
            randomBtn.Sound = AudioNames.BTN_RESET;
            randomEffect = randomBtn.GetComponent<UIEffect>();
        }

        private void OnEnable()
        {
            nextBtn.OnClick += OnNext;
            previousBtn.OnClick += OnPrevious;
            randomBtn.OnClick += OnRandom;
        }

        private void OnDisable()
        {
            nextBtn.OnClick -= OnNext;
            previousBtn.OnClick -= OnPrevious;
            randomBtn.OnClick -= OnRandom;
        }

        private bool lockAnim = false;

        // 下一步
        private void OnNext(BabuButton sender)
        {
            if (lockAnim) return;
            lockAnim = true;
            TouchManager.Instance.DisableTouch();
            Anim.PlayExit(() =>
            {
                TouchManager.Instance.EnableTouch();
                lockAnim = false;
                gameObject.SetActive(false);
                nextPad.SetActive(true);
                OnFinished?.Invoke();
                nextPadAnim.PlayResult();
            });
        }

        // 上一步
        private void OnPrevious(BabuButton sender)
        {
            if (lockAnim) return;
            lockAnim = true;
            TouchManager.Instance.DisableTouch();
            Anim.PlayExit(() =>
            {
                TouchManager.Instance.EnableTouch();
                lockAnim = false;
                gameObject.SetActive(false);
                previousPad.gameObject.SetActive(true);
                previousPad.ToNormal();
                previousPad.Anim.PlayEnter();
                CreatePlayerUISelectClothesPad.IsInit = true;
            });
        }

        private void OnRandom(BabuButton sender)
        {
            DOTween.To(value => randomEffect.colorFactor = value, 0, 1, 0.1f).OnComplete(() =>
            {
                DOTween.To(value => randomEffect.colorFactor = value, 1, 0, 0.1f);
            });
            Lottery();
        }
        private void Shuffle<T>(IList<T> list)
        {
            if (rnd == null) rnd = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        private void SelectPlayerInOneColor(List<CardModelConfig> seedList, int selectedCount, ref Dictionary<PositionSeparatedType, int> ownedPosDict)
        {
            Shuffle<CardModelConfig>(seedList);

            int playSelectCount = 0;
            List<CardModelConfig> delList = new List<CardModelConfig>();
            foreach (CardModelConfig card in seedList)
            {
                if (playSelectCount >= selectedCount) break;
                PositionSeparatedType posType = (PositionSeparatedType)card.AdaptPosition[0];
                if (ownedPosDict.ContainsKey(posType))
                {
                    continue;
                }

                if (Result.Contains(card.Id)) 
                {
                    continue;
                }

                ownedPosDict.Add(posType, 1);
                Result.Add(card.Id);
                playSelectCount++;
                delList.Add(card);
            }
            foreach (CardModelConfig card in delList)
            {
                seedList.Remove(card);
            }

            if (playSelectCount < selectedCount)
            {
                for (int i = 0; i < selectedCount - playSelectCount; i++)
                {
                    CardModelConfig card = seedList[i];
                    Result.Add(card.Id);
                }
            }

        }

        /// <summary>
        /// 添加特殊卡
        /// </summary>
        /// <param name="_cardid"></param>
        /// <param name="posDict"></param>
        /// <param name="seedList"></param>
        private void AddSpecialCard(int _cardid, ref Dictionary<PositionSeparatedType, int> posDict, ref List<CardModelConfig> seedList)
        {
            seedList.Add(Configs.CardModel.GetConfig(_cardid));
            Result.Add(_cardid);
            PositionSeparatedType posType = (PositionSeparatedType)seedList[0].AdaptPosition[0];
            posDict[posType] = _cardid;
        }

        /// <summary>
        ///出生给8个球员，PG, SG, SF, PF, C, PG, SG,C
        ///规则：
        /// 保证一张紫卡
        /// 随机蓝绿卡[2, 4]
        /// 其余是绿卡
        /// 其中紫卡是球星
        /// </summary>
        public void Lottery()
        {
            int totalPlayerCount = 8;
            Result.Clear();
            List<CardModelConfig> seedList = new();
            Dictionary<PositionSeparatedType, int> posDict = new Dictionary<PositionSeparatedType, int>();
            // 3名固定球员（确保剧情模式的3个基础球员）
            AddSpecialCard(101001, ref posDict, ref seedList);
            AddSpecialCard(101002, ref posDict, ref seedList);
            AddSpecialCard(101003, ref posDict, ref seedList);
            int specialPlayerCount = 3;
            // 蓝色球员的数量
            int bluePlayerCount = Utils.Utility.GetRandomInt(2, 4);

            int purplePlayerCount = 1;

            // 绿色球员的数量
            int greenPlayerCount = totalPlayerCount - bluePlayerCount - purplePlayerCount - specialPlayerCount;

            seedList = Configs.CardModel.GetConfigList().Where(item => item.Quality == QualityType.Green).ToList();
            SelectPlayerInOneColor(seedList, greenPlayerCount, ref posDict);

            seedList = Configs.CardModel.GetConfigList().Where(item => item.Quality == QualityType.Blue).ToList();
            SelectPlayerInOneColor(seedList, bluePlayerCount, ref posDict);

            seedList = Configs.CardModel.GetConfigList().Where(item => item.Quality == QualityType.Purple).ToList();
            SelectPlayerInOneColor(seedList, purplePlayerCount, ref posDict);

            starCardId = Result.Last();
            Result.Reverse();
            Result.RandomSortSelf();

            RefreshUI();

#if !RELEASE
            // 调试输出
            DebugPutput();
#endif
        }

        // private IEnumerable<GameConfig.Config.CardModelConfig> ExceptPlayer(GameConfig.Config.CardModelConfig card, IEnumerable<GameConfig.Config.CardModelConfig> lotteryList, ref StateValue typeCount)
        // {
        //     if (card == null)
        //     {
        //         Debug.LogError("没有卡可以抽了");
        //         return lotteryList;
        //     }
        //     // 将抽中的球员添加到结果列表中
        //     Result.Add(card.Id);
        //     // 将抽中的球员排除再抽奖列表中
        //     lotteryList = lotteryList.Except(new[] { card });
        //     // 对应球员类型数量+1
        //     typeCount[card.AdaptPosition[0]]++;
        //     // 如果前控球后卫满了
        //     if (typeCount[(int)PositionSeparatedType.KongQiuHouWei] >= 2)
        //     {
        //         // 排除所有控球后卫
        //         lotteryList = lotteryList.Where(item => item.AdaptPosition[0] != (int)PositionSeparatedType.KongQiuHouWei);
        //     }
        //     // 如果得分后卫抽满了
        //     if (typeCount[(int)PositionSeparatedType.DeFenHouWei] >= 2)
        //     {
        //         // 排除所有得分后卫
        //         lotteryList = lotteryList.Where(item => item.AdaptPosition[0] != (int)PositionSeparatedType.DeFenHouWei);
        //     }
        //     // 如果小前锋抽满了
        //     if (typeCount[(int)PositionSeparatedType.XiaoQianFeng] >= 1)
        //     {
        //         // 排除所有小前锋
        //         lotteryList = lotteryList.Where(item => item.AdaptPosition[0] != (int)PositionSeparatedType.XiaoQianFeng);
        //     }
        //     // 如果大前锋抽满了
        //     if (typeCount[(int)PositionSeparatedType.DaQianFeng] >= 1)
        //     {
        //         // 排除所有大前锋
        //         lotteryList = lotteryList.Where(item => item.AdaptPosition[0] != (int)PositionSeparatedType.DaQianFeng);
        //     }
        //     // 如果中锋抽满了
        //     if (typeCount[(int)PositionSeparatedType.ZhongFeng] >= 2)
        //     {
        //         // 排除所有中锋
        //         lotteryList = lotteryList.Where(item => item.AdaptPosition[0] != (int)PositionSeparatedType.ZhongFeng);
        //     }
        //     return lotteryList;
        // }

        private void RefreshUI()
        {
            for (int i = 0; i < Result.Count; i++)
            {
                cardItems[i].SetData(new PlayerCard(Result[i]), false);
            }
        }

        [EditorButton("抽100次")]
        private void LotteryOneHundred()
        {
            for (int i = 0; i < 100; i++)
            {
                Lottery();
            }
        }

        // 不满住条件会用红色字体显示
        private void DebugPutput()
        {
            // string pg = $"控卫:{typeCount[(int)PositionSeparatedType.KongQiuHouWei]} ";
            // string sg = $"分卫:{typeCount[(int)PositionSeparatedType.DeFenHouWei]} ";
            // string sf = $"小前:{typeCount[(int)PositionSeparatedType.XiaoQianFeng]} ";
            // string pf = $"大前:{typeCount[(int)PositionSeparatedType.DaQianFeng]}";
            // string c = $"中锋:{typeCount[(int)PositionSeparatedType.ZhongFeng]}";
            // if (typeCount[(int)PositionSeparatedType.KongQiuHouWei] > 2 || typeCount[(int)PositionSeparatedType.KongQiuHouWei] < 2)
            // {
            //     pg = $"<color=red>控卫:{typeCount[(int)PositionSeparatedType.KongQiuHouWei]}</color> ";
            // }
            // if (typeCount[(int)PositionSeparatedType.DeFenHouWei] > 2 || typeCount[(int)PositionSeparatedType.DeFenHouWei] < 2)
            // {
            //     sg = $"<color=red>分卫:{typeCount[(int)PositionSeparatedType.DeFenHouWei]}</color> ";
            // }
            // if (typeCount[(int)PositionSeparatedType.XiaoQianFeng] > 1 || typeCount[(int)PositionSeparatedType.XiaoQianFeng] < 1)
            // {
            //     sf = $"<color=red>小前:{typeCount[(int)PositionSeparatedType.XiaoQianFeng]}</color> ";
            // }
            // if (typeCount[(int)PositionSeparatedType.DaQianFeng] > 1 || typeCount[(int)PositionSeparatedType.DaQianFeng] < 1)
            // {
            //     pf = $"<color=red>大前:{typeCount[(int)PositionSeparatedType.DaQianFeng]}</color>";
            // }
            // if (typeCount[(int)PositionSeparatedType.ZhongFeng] > 2 || typeCount[(int)PositionSeparatedType.ZhongFeng] < 2)
            // {
            //     c = $"<color=red>中锋:{typeCount[(int)PositionSeparatedType.ZhongFeng]}</color>";
            // }
            // HashSet<int> ids = new HashSet<int>();
            // Result.ForEach(item => ids.Add(item));
            // if (ids.Count == 8)
            // {
            //     Debug.Log(pg + sg + sf + pf + c + $" 总数:{ids.Count}");

            // }
            // else
            // {
            //     Debug.Log(pg + sg + sf + pf + c + $" <color=red>总数:{ids.Count}</color>");
            // }
        }
    }
}