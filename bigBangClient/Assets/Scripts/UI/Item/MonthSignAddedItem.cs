using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameConfig;
using UnityEngine.EventSystems;
using Utils.GameItem;
using System.Linq;
using Utils;
using BigBang.Animation;
using DG.Tweening;
using Coffee.UIExtensions;
using UnityTimer;

namespace BigBang.UI
{
    public class RewardAddData
    {
        public int id;
        public int index;
        public RectTransform rect;
        public RewardStates rewardStates;

        public void SetValue(int itemIndex, int index, RectTransform itemRect, RewardStates states)
        {
            id = itemIndex;
            rect = itemRect;
            rewardStates = states;
            this.index = index;
        }
    }

    public class MonthSignAddedItem : MonoBehaviour
    {
        [SerializeField] private RectTransform obtain;
        [SerializeField] private List<BabuButton> boxs;
        [SerializeField] private List<TMP_Text> dayList;
        [SerializeField] private List<Image> itemsIcon;
        [SerializeField] private List<RectTransform> itemPos;
        [SerializeField] private List<InventoryItem> inventoryList;
        [SerializeField] private List<UIParticle> particles;
        private Dictionary<BabuButton, RewardAddData> btnKeys = new Dictionary<BabuButton, RewardAddData>();

        private List<Tweener> tweens = new List<Tweener>();
        private List<Vector2> boxsPos = new List<Vector2>();
        private Dictionary<int, Sequence> sequences = new Dictionary<int, Sequence>();

        private void Awake()
        {
            btnKeys.Clear();
            var list = Configs.MoonSiginAddedReward.GetConfigList();
            int index = 0;
            foreach (var btn in boxs)
            {
                btn.Anim = null;
                RewardAddData reward = new RewardAddData();
                reward.SetValue(list[index].Id, index, itemPos[index], RewardStates.UNLOGIN);
                if (list[index] != null)
                    btnKeys.Add(btn, reward);
                index++;
                boxsPos.Add(btn.image.rectTransform.anchoredPosition);
            }
        }

        private void OnEnable()
        {
            boxs.ForEach(item => item.OnClick += OnReward);
        }

        private void OnDisable()
        {
            boxs.ForEach(item => item.OnClick -= OnReward);
        }

        private void OnReward(BabuButton sender)
        {
            var thisBtn = EventSystem.current.currentSelectedGameObject.GetComponent<BabuButton>();
            var rewardAdd = btnKeys[thisBtn];
            var connent = Configs.MoonSiginAddedReward.GetDataDictionary()[rewardAdd.id].Content;
            if (rewardAdd.rewardStates == RewardStates.COLLECT)
            {
                AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);
                //领取奖励
                NetworkManager.Instance.ReceiveMonthSignReward(rewardAdd.id, (int)RewardType.AddedSigin, response =>
                {
                    if (response.ReceiveSucceed)
                    {
                        // 打开通用收益界面
                        var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(connent).ToList());
                        UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                        foreach (var item in Player.ActivityManager.AddedSignMonth)
                        {
                            if (item.GetID() == rewardAdd.id)
                            {
                                item.SetState((int)RewardStates.RECEIVED);
                            }
                        }
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Open, s => itemsIcon[rewardAdd.index].sprite = s);
                        //刷新
                        Babu.EventManager.Instance.Dispatch(EventID.OnRefreshMonthSiginUI);
                    }
                    else
                    {
                        //领取失败
                        Debug.LogError("领取失败");
                    }
                });
            }
            else
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
                //tips弹出
                SetBox(rewardAdd);
            }
        }

        // 初始化宝箱位置
        private void InitBoxsPos()
        {
            for (int i = 0; i < boxs.Count; i++)
            {
                boxs[i].image.rectTransform.anchoredPosition = boxsPos[i];
            }
        }

        public void SetData(List<ActivityManager.RewardState> data)
        {
            InitBoxsPos();
            int index = 0;
            foreach (var item in data)
            {
                int i = index;
                if (item != null)
                {
                    dayList[index].text = Lang.Get(LangID.DayTxt).Replace("{value}", item.GetID().ToString());
                    if (item.GetState() == (int)RewardStates.RECEIVED)
                    {
                        particles[index].Stop();
                        StopShakeAnim(index);
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Open, s => itemsIcon[i].sprite = s);
                    }
                    else if (item.GetState() == (int)RewardStates.COLLECT)
                    {
                        int t = index;
                        Timer.Register(this.gameObject, 0.3f, () =>
                        {
                            // 粒子特效
                            particles[t].Play();
                            PlayShakeAnim(t);
                        });
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Obtain, s => itemsIcon[i].sprite = s);
                    }
                    else
                    {
                        particles[index].Stop();
                        StopShakeAnim(index);
                        SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.Close, s => itemsIcon[i].sprite = s);
                    }
                    ++index;
                }
            }
            index = 0;
            foreach (var item in btnKeys)
            {
                item.Value.rewardStates = (RewardStates)data[index].GetState();
                ++index;
            }
        }

        // 播放抖动动画
        private void PlayShakeAnim(int index)
        {
            StopShakeAnim(index);
            Sequence sequence = DOTween.Sequence();
            sequences.Add(index, sequence);
            sequence.Append(boxs[index].image.rectTransform.DOSpin(10, 3, 0.1f));
            sequence.AppendInterval(3);
            sequence.SetLoops(-1);
        }

        // 停止抖动动画
        private void StopShakeAnim(int index)
        {
            if (sequences.ContainsKey(index))
            {
                sequences[index]?.Kill();
                sequences.Remove(index);
            }
            boxs[index].image.rectTransform.anchoredPosition = boxsPos[index];
            boxs[index].image.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
        }

        public void SetBox(RewardAddData rewardAdd)
        {
            obtain.gameObject.SetActive(true);
            obtain.transform.SetParent(rewardAdd.rect);
            obtain.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            obtain.gameObject.SetAlpha(0);
            // 宝箱Tips动画
            tweens.ForEach(item => item?.Kill());
            tweens.Clear();
            tweens.Add(obtain.DOAnchorPosY(20, 0.3f));
            tweens.Add(obtain.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                tweens.Add(obtain.gameObject.DOFade(0, 0.3f).SetDelay(5).OnComplete(() =>
                {
                    obtain.gameObject.SetActive(false);
                }));
            }));
            //宝箱内容
            var connent = Configs.MoonSiginAddedReward.GetDataDictionary()[rewardAdd.id].Content;
            var gameItem = GameItemUtils.CreateGameItems(connent).ToArray();
            int index = 0;
            foreach (var item in inventoryList)
            {
                item.gameObject.SetActive(true);
                item.SetData(gameItem[index]);
                index++;
            }
        }
    }
}
