using Babu;
using BigBang.Animation;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class StrengthenTrainPad : MonoBehaviour
    {
        [SerializeField] private StrengthenTrainPadComponent com;
        [SerializeField] private StrengthenTrainPadAnim anim;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private RectTransform emptyPanel = null;

        public bool YoN = true;
        public bool First = false;

        //黄色
        private Color enableColor = new Color(148 / 255f, 100 / 255f, 8 / 255f, 1);
        //灰色
        private Color disableColor = new Color(40 / 255f, 50 / 255f, 58 / 255f, 1);

        private void OnEnable()
        {
            com.TrainAllBtn.onClick.AddListener(OnTrainAll);
            Babu.EventManager.Instance.Register(EventID.OnStrengthen, OnStrengthen);
        }

        private void OnDisable()
        {
            com.TrainAllBtn.onClick.RemoveListener(OnTrainAll);
            Babu.EventManager.Instance.Unregister(EventID.OnStrengthen, OnStrengthen);
        }

        private void OnStrengthen(object[] args)
        {
            SetData();
            Player.TrainManager.StrengthenController.CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshBigBangUIRedDot);
        }

        private float bottom1 = 330;
        private float bottom2 = 230;
        private float bottom3 = 100;

        private void Update()
        {
            // 一键强化按钮是否解锁
            if (Player.TrainManager.StrengthenController.UnlockStrengthenBatch())
            {
                com.TrainAllBtn.gameObject.SetActive(true);
                com.Scroll.SetBottom(bottom1);
            }
            else
            {
                com.TrainAllBtn.gameObject.SetActive(false);
                com.Scroll.SetBottom(bottom2);
                com.ViewPort.SetBottom(bottom3);
            }

            if (Player.TrainManager.StrengthenController.CanDoStrengthenBatch())
            {
                SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.YellowBtnImg, (s) => { com.TrainAllBtn.image.sprite = s; });
                com.TrainAllBtnText.color = enableColor;
            }
            else
            {
                SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.GrayBtnImg, (s) => { com.TrainAllBtn.image.sprite = s; });
                com.TrainAllBtnText.color = disableColor;
            }
        }

        private void OnTrainAll()
        {
            if ((Player.TrainManager.StrengthenController.CanDoStrengthenBatch()))
            {
                TouchManager.Instance.DisableTouch();
                com.TrainAllBtn.GetComponent<ButtonAnim>().Play(() =>
                {
                    StrengthenTrainPadAnim.isPlaying = true;
                    // 播放一键强化动画
                    anim.PlayTrainAll(() =>
                    {
                        StrengthenTrainPadAnim.isPlaying = false;
                        Babu.EventManager.Instance.Dispatch(EventID.OnTrainAllCompleted);
                        TouchManager.Instance.EnableTouch();
                        // 一键强化时，自动从消耗经验少的科技开始 

                        Player.TrainManager.StrengthenController.DoStrengthenBatch();
                        SetData();

                        Player.TrainManager.StrengthenController.CheckRedDot();
                        EventManager.Instance.Dispatch(EventID.RefreshBigBangUIRedDot);
                        //SetAllStrengthenItemData();
                    });
                });
                // 一键强化音效
                AudioManager.Instance.PlaySound(AudioNames.BTN_STRENALL);
            }
            else
            {
                // 一键强化已解锁，但是不能一键强化 
                Tips.PopError(ErrorID.NoItemStrengthen);
                com.TrainAllBtn.GetComponent<ButtonAnim>().PlayNull();
            }
        }

        //实例化预制体
        public void InstantiateItem()
        {
            com.ItemList = Player.TrainManager.StrengthenController.GetShowList();
            while (com.Content.childCount < com.ItemList.Count)
            {
                var item = Instantiate(itemPrefab, com.Content);
                item.SetActive(false);
            }
        }

        //设置数据
        public void SetData()
        {
            com.ItemList = Player.TrainManager.StrengthenController.GetShowList();
            emptyPanel.gameObject.SetActive(com.ItemList.Count <= 0);
            for (int i = 0; i < com.ItemList.Count; i++)
            {
                com.Content.GetChild(i).GetComponent<StrengthenedTrainItem>().SetItem(com.ItemList[i].ConfigId);
                com.Content.GetChild(i).gameObject.SetActive(true);
            }
        }

        //播放侧滑动画
        public void PlayAnim()
        {
            anim.Play();
        }
    }
}