using System;
using Babu;
using Babu.BigNumber;
using BigBang.Animation;
using GameConfig.Config;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class RegularTrainItem : MonoBehaviour
    {
        [SerializeField] private RegularTrainItemComponent com;
        [SerializeField] private RegularTrainItemAnim anim;
        [SerializeField] private RegularTrainItemCountAnim countAnim;
        [SerializeField] private RegularTrainItemUnlockAnim unlockAnim;
        [SerializeField] private RegularTrainItemLevelAnim levelAnim;
        [SerializeField] private RegularTrainItemBreakThroughAnim breakAnim;

        private int itemID;
        private TrainConfig cfg;
        private bool isInit = false;

        public PlayerTrainItem Item;

        private RectTransform selfRect;

        private float selfY = 0;
        private RectTransform canvasRect;

        private bool isInEyeArea = false; //是否在可视范围内

        private CameraID cameraID;
        private Camera renderCamera;

        private void Awake() {
            
        }

        private void OnEnable()
        {
            Babu.EventManager.Instance.Register(EventID.OnSpeedChange, OnSpeedChange);
            Babu.EventManager.Instance.Register(EventID.OnExpChanged, OnExpChanged);
            com.UpgradeBtn.onClick.AddListener(OnUpgradeBtn);
            com.UnlockBtn.onClick.AddListener(OnUnlock);
            com.BreakThroughBtn.onClick.AddListener(OnOpenBreakDetail);
        }

        private void OnOpenBreakDetail()
        {

            Item = Player.TrainManager.GetTrainItem(itemID);
            var trainItem = new BigBreakthroughDIANJIUIProperties(cfg.Name, Item.Level, Item.SetBreakLevelData(), this, cfg);
            cfg = Item.GetConfig();
            trainItem.Name = cfg.Name;
            trainItem.Level = Item.Level;
            trainItem.IntsList = Item.SetBreakLevelData();
            UIController.Instance.OpenWindow<BigBreakthroughDIANJIUI>(trainItem);
        }

        private void OnDisable()
        {
            Babu.EventManager.Instance.Unregister(EventID.OnSpeedChange, OnSpeedChange);
            Babu.EventManager.Instance.Unregister(EventID.OnExpChanged, OnExpChanged);
            com.UpgradeBtn.onClick.RemoveListener(OnUpgradeBtn);
            com.UnlockBtn.onClick.RemoveListener(OnUnlock);
            com.BreakThroughBtn.onClick.RemoveListener(OnOpenBreakDetail);
            // 禁用相机渲染
            if (renderCamera != null && renderCamera.gameObject.activeInHierarchy)
            {
                DisableCameraRender();
            }
        }

        private void SetGroupActivity(bool flag)
        {
            foreach (var item in com.ActivityGroup)
            {
                item.SetActive(flag);
            }
        }

        private void SetActivityLeftGroup(bool flag)
        {
            foreach (var item in com.ActivityLeftGroup)
            {
                item.SetActive(flag);
            }
        }

        private void OnSpeedChange(object[] args)
        {
            com.CountImgCopy.sprite = com.CountImg.sprite;
            SpriteManager.GetSprite(AtlasNames.TrainUI, SpriteNames.TrainUI.UpgradeCount[(int)Player.TrainManager.UpLevelType], s => com.CountImg.sprite = s);
            countAnim.Play();
        }

        // 启用相机渲染
        private void EnableCameraRender()
        {
            // 获得相机
            var c = CameraManager.Instance.GetCamera(cameraID);
            // 启用相机
            c.gameObject.SetActive(true);
            // 获得临时渲染纹理
            var temporary = RenderTexture.GetTemporary(688, 336, 24);
            // 设置训练纹理
            com.TrainImg.texture = temporary;
            // 设置相机目标渲染纹理
            c.targetTexture = temporary;
            // 静态背景渲染
            if (com.TrainBackground.texture == null)
            {
                com.TrainBackground.gameObject.SetActive(true);
                var c2 = Instantiate(c);
                c2.clearFlags = CameraClearFlags.Skybox;
                c2.transform.SetParent(c.transform.parent, false);
                c2.transform.SetPositionAndRotation(c.transform.position, c.transform.rotation);
                c2.cullingMask = 1 << LayerMask.NameToLayer("Stadium");
                c2.targetTexture = RenderTexture.GetTemporary(688, 336, 24);
                com.TrainBackground.texture = c2.targetTexture;
                // 下一帧执行关闭相机
                Babu.DelayTaskService.Instance.Run(this.gameObject, () => c2.enabled = false);
            }
        }

        // 禁用相机渲染
        private void DisableCameraRender()
        {
            // 获得相机
            var c = CameraManager.Instance.GetCamera(cameraID);
            // 设置相机目标渲染纹理为空
            c.targetTexture = null;
            // 禁用相机
            c.gameObject.SetActive(false);
            // 释放临时渲染纹理
            RenderTexture.ReleaseTemporary(com.TrainImg.texture as RenderTexture);
            // 设置训练纹理为空
            com.TrainImg.texture = null;
        }

        // 训练条目由不可见变为可见
        private void OnVisibleEnter()
        {
            if (Item.IsUnlock())
            {
                EnableCameraRender();
            }
        }

        // 训练条目由可见变为不可见
        private void OnVisibleExit()
        {
            DisableCameraRender();
        }

        private void SetUpgradeBtnData()
        {
            com.UnlockBtn.gameObject.SetActive(!Item.IsUnlock());
            com.LockTrainImg.gameObject.SetActive(!Item.IsUnlock());
            SetGroupActivity(Item.IsUnlock());
            //第一次突破后显示加成条                       
            if (Item.ItemGetAbility() == 0)
            {
                SetActivityLeftGroup(false);
            }
            else
            {
                SetActivityLeftGroup(true);
            }
            //设置按钮启用状态（随时间变化）
            int upgradeLevel = Player.TrainManager.GetUpgradeLevel(Item);
            if (upgradeLevel < 1) upgradeLevel = 1;
            //升级和解锁消耗（随时间变化）
            BigNumber cost = Item.GetUpLevelCost(upgradeLevel);

            com.UpgradeBtnText.text = cost.ToFormatString();
            
            if (Player.TrainManager.Exp > cost)
            {
                com.UnlockText.text = cost.ToFormatString();
                SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.YellowBtnImg, s => com.UpgradeBtn.image.sprite = s);
                //按钮字体颜色 黄色
                com.UpgradeText.text = ColorString.GetColorString("#946408", Lang.Get(LangID.UpgradeText));
                com.UpgradeBtnText.text = ColorString.GetColorString("#946408", cost.ToFormatString());
            }
            else
            {
                com.UnlockText.text = ColorString.GetColorString("#bb3031", cost.ToFormatString());
                SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.GrayBtnImg, s => com.UpgradeBtn.image.sprite = s);
                //按钮字体颜色 灰色
                com.UpgradeText.text = ColorString.GetColorString("#263646", Lang.Get(LangID.UpgradeText));
                com.UpgradeBtnText.text = ColorString.GetColorString("#263646", cost.ToFormatString());
            }
        }

        private void Update()
        {
            // 判断当前训练条目是否在屏幕显示范围内
            if (cameraID != CameraID.None)
            {
                if(this.selfY != selfRect.position.y){
                    this.selfY = selfRect.position.y;
                    this.isInEyeArea = selfRect.IsOverlap(canvasRect);
                }
                // 如果在屏幕显示范围内，并且该训练条目解锁了，则开启相机进行训练动画的渲染
               
                // // 如果在显示范围，并且相机之前没有渲染
                if (this.isInEyeArea && !renderCamera.gameObject.activeInHierarchy)
                {
                    OnVisibleEnter();
                }
                // 如果不在显示范围，并且相机正在渲染
                if (!this.isInEyeArea && renderCamera.gameObject.activeInHierarchy)
                {
                    OnVisibleExit();
                }
            }
        }

        //经验改变事件
        private void OnExpChanged(object[] args)
        {
            //如果已经初始化完成，则进行以下操作
            if (!isInit) return;
            SetUpgradeBtnData();
        }

        private void OnUpgradeBtn()
        {
            if (Player.TrainManager.CanUpgrade(itemID))
            {
                //升级按钮动画
                com.UpgradeBtn.GetComponent<ButtonAnim>().Play(() =>
                {
                    Player.TrainManager.DoUpgrade(itemID);
                    //项目等级(随升级变化)
                    com.ProjectLevelText.text = Item.Level.ToString() + Lang.Get(LangID.LvTxt);
                    //能力值（随升级变化）
                    com.ValueText.text = "+" + Item.ItemGetAbility().ToString();
                    //收入（随升级变化）
                    RefreshFillIncomeText();
                    //突破进度条动画
                    breakAnim.Play();
                    Player.TrainManager.ShowMessage();

                }, playAudio: false);
                AudioManager.Instance.PlaySound(AudioNames.BTN_UPGRADE);
                //升级动画
                com.ProjectLevelText.DOLight(36, 43, 0.35f);
                levelAnim.Play(com.FillIncomeText, 20, 22);


            }
            else
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_NULL);
                Tips.PopError(ErrorID.ExpNotEnough);
            }

            BigBreakthroughUIAnim.sourcePosition = RectTransformUtility.WorldToScreenPoint(UIController.Instance.GetCamera(), com.BreakProgress.transform.position);
            BigBreakthroughUIAnim.source = com.BreakProgress.transform.parent.gameObject;
            BreakthroughUIAnim.sourcePosition = RectTransformUtility.WorldToScreenPoint(UIController.Instance.GetCamera(), com.BreakProgress.transform.position);
            BreakthroughUIAnim.source = com.BreakProgress.transform.parent.gameObject;
        }
        private void RefreshFillIncomeText()
        {
            //获得进度条开始位置
            var timeUnit = Item.GetInComeTimeUnit();
            if (timeUnit > 0.2f)
            {
                com.FillIncomeText.text = (Item.GetInComePerSecond() * Item.GetInComeTimeUnit()).ToFormatString();
            }
            else
            {
                com.FillIncomeText.text = Item.GetInComePerSecond().ToFormatString() + "/" + Lang.Get(LangID.SecondTxt);
            }
        }

        private void OnUnlock()
        {
            //判断能否解锁
            if (Player.TrainManager.CanUpgrade(itemID))
            {
                //禁用触摸
                TouchManager.Instance.DisableTouch();
                unlockAnim.PlayUnlock(() =>
                {
                    Player.TrainManager.DoUpgrade(itemID);
                    //启用触摸
                    TouchManager.Instance.EnableTouch();
                    //项目等级(随升级变化)
                    com.ProjectLevelText.text = Item.Level.ToString() + Lang.Get(LangID.LvTxt);
                    //能力值（随升级变化）
                    com.ValueText.text = "+" + Item.ItemGetAbility().ToString();
                    Player.TrainManager.ShowMessage();
                    //todo:
                    

                });
                // 解锁音效
                AudioManager.Instance.PlaySound(AudioNames.BTN_UNLOCK);
                AudioManager.Instance.PlaySound(AudioNames.EVENT_UNLOCKTRAINING);
            }
            else
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_NULL);
                Tips.PopError(ErrorID.ExpNotEnough);
            }
            BigBreakthroughUIAnim.sourcePosition = RectTransformUtility.WorldToScreenPoint(UIController.Instance.GetCamera(), com.BreakProgress.transform.position);
            BigBreakthroughUIAnim.source = com.BreakProgress.transform.parent.gameObject;
            BreakthroughUIAnim.sourcePosition = RectTransformUtility.WorldToScreenPoint(UIController.Instance.GetCamera(), com.BreakProgress.transform.position);
            BreakthroughUIAnim.source = com.BreakProgress.transform.parent.gameObject;
        }

        public void SetItem(int itemId)
        {
            this.itemID = itemId;
            Item = Player.TrainManager.GetTrainItem(itemId);
            cfg = Item.GetConfig();
            //项目名称
            com.ProjectNameText.text = cfg.Name;
            //升级按钮
            com.UpgradeText.text = Lang.Get(LangID.UpgradeText);
            //收入名称
            com.ProjectValueText.text = cfg.Name + Lang.Get(LangID.PropertiesTxt);
            //项目等级(随升级变化)
            com.ProjectLevelText.text = Item.Level.ToString() + Lang.Get(LangID.LvTxt);
            //能力值（随升级变化）
            com.ValueText.text = "+" + Item.ItemGetAbility().ToString();
            //设置突破进度条
            com.BreakProgress.fillAmount = Item.GetBreakThroughProgress();
            SetUpgradeBtnData();
            com.LockTrainImg.gameObject.SetActive(!Item.IsUnlock());
            com.TrainImg.gameObject.SetActive(Item.IsUnlock());
            isInit = true;
            cameraID = (CameraID)Enum.Parse(typeof(CameraID), cfg.Cameraid);
            renderCamera = CameraManager.Instance.GetCamera(cameraID);
            if(renderCamera == null) Debug.LogWarning("renderCamera == null , cameraID" + cameraID);
            selfRect = GetComponent<RectTransform>();
            canvasRect = UIController.Instance.GetComponent<RectTransform>();

            // 放到下一帧去执行
            Babu.DelayTaskService.Instance.Run(this.gameObject, () =>
            {
                this.selfY = selfRect.position.y;
                this.isInEyeArea = selfRect.IsOverlap(canvasRect);
                // 启用相机渲染
                if (Item.IsUnlock() && this.isInEyeArea)
                {
                    EnableCameraRender();
                }
            });
        }
    }
}