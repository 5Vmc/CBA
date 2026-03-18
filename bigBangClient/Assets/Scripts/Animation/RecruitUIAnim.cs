using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using BigBang.UI;
using static Babu.DelayTaskService;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class RecruitUIAnim : AnimBase
    {
        // 顶部栏
        [SerializeField] private RectTransform top;
        // 信息栏
        [SerializeField] private List<GameObject> fadeInList = new();
        // 招募按钮
        [SerializeField] private RectTransform oneBtn;
        [SerializeField] private RectTransform tenBtn;
        // [SerializeField] private RectTransform superOneBtn;
        // [SerializeField] private RectTransform superTenBtn;
        // 返回按钮
        //[SerializeField] private RectTransform bottomBar;

        private Material blackPlane;

        public bool isActivity = false;

        private void Awake()
        {
            Babu.EventManager.Instance.Register(EventID.InitRecruitUIModelAnim, InitModelAnim);
        }

        private void OnDestroy()
        {
            Babu.EventManager.Instance.Unregister(EventID.InitRecruitUIModelAnim, InitModelAnim);
        }

        public override void Init()
        {
            base.Init();
            // 初始化位置
            top.SetAnchoredPositionY(200);
            oneBtn.SetAnchoredPositionY(-57.5f);
            tenBtn.SetAnchoredPositionY(-57.5f);
            // superOneBtn.SetAnchoredPositionY(-426);
            // superTenBtn.SetAnchoredPositionY(-426);
            // 初始化透明度
            foreach (var item in fadeInList)
            {
                item.SetAlpha(0);
            }
            // 初始化黑幕
            blackPlane = GameObjectManager.Instance.GetComponentAtPath<MeshRenderer>(GameObjectID.RecruitModel, "BlackPlane").material;
            blackPlane.SetVector("_Color", Vector4.zero);
        }

        // 初始化模型动画
        public void InitModelAnim(object[] args)
        {
            PlayIdleAnim();
            // 初始化黑幕
            MeshRenderer meshRenderer = GameObjectManager.Instance.GetComponentAtPath<MeshRenderer>(GameObjectID.RecruitModel, "BlackPlane");
            if (meshRenderer == null)
            {
                return;
            }
            blackPlane = meshRenderer.material;
            if (blackPlane == null)
            {
                return;
            }
            blackPlane.SetVector("_Color", Vector4.zero);
            if (isActivity) InitActivityAnim();
        }

        [EditorButton("播放UI进入动画")]
        public override void PlayEnter()
        {
            base.PlayEnter();
            // 顶部栏下移
            tweens.Add(top.DOAnchorPosY(0, 0.3f));
            //tweens.Add(bottomBar.DoRelativeAnchorPosY(-200, 0.3f).From());
            // 信息栏淡入
            foreach (var item in fadeInList)
            {
                tweens.Add(item.gameObject.DOFade(1, 0.6f));
            }
            // 招募按钮上浮出现
            tweens.Add(oneBtn.DOAnchorPosY(142.5f, 0.3f));
            tweens.Add(tenBtn.DOAnchorPosY(142.5f, 0.3f));
            // tweens.Add(superOneBtn.DoRelativeAnchorPosY(-200, 0.3f).From());
            // tweens.Add(superTenBtn.DoRelativeAnchorPosY(-200, 0.3f).From());

        }

        // 招募动画(打开书本，时长3s)
        public void PlayRecruit(TaskCallback callback)
        {
            GameObjectManager.Instance.GetComponent<RecruitlistUI>(GameObjectID.RecruitlistUI)?.StopIllusion();
            if (isActivity)
            {
                PlayActivityAnim();
            }
            else
            {
                PlayBookAnim();
            }
            blackPlane.SetVector("_Color", Vector4.zero);
            // 黑幕出现
            DOTween.To(value => blackPlane.SetVector("_Color", new Vector4(0, 0, 0, value)), 0, 0.7f, 0.5f).SetDelay(0.8f);
            // 延迟打开下一个面板
            Babu.DelayTaskService.Instance.Run(this.gameObject, 1.4f + (isActivity ? 0.5f : 0f), callback);
        }

        // 心愿单动画
        public void PlayWish(TaskCallback callback = null)
        {
            GameObjectManager.Instance.GetComponent<RecruitlistUI>(GameObjectID.RecruitlistUI)?.StopIllusion();
            PlayWishAnim();
            blackPlane.SetVector("_Color", Vector4.zero);
            // 黑幕出现
            // DOTween.To(value => blackPlane.SetVector("_Color", new Vector4(0, 0, 0, value)), 0, 0.7f, 0.5f).SetDelay(0.5f);
            Babu.DelayTaskService.Instance.Run(this.gameObject, 1f, () =>
            {
                PlayIdleAnim();
                callback?.Invoke();
            });
        }

        [EditorButton("模型动画初始化")]
        private void PlayIdleAnim()
        {
            bookMoveTween?.Kill();
            paperMoveTween?.Kill();
            ShowAllUI();
            GameObject recruitModelGo = GameObjectManager.Instance.GetGameObject(GameObjectID.RecruitModel);
            if (recruitModelGo == null) return;
            Transform recruitModelTrans = recruitModelGo.transform;
            Transform kongbaiTrans = recruitModelTrans.Find("空白");
            kongbaiTrans.Find("文件夹底").SetLocalPosition(bookMidPos);
            kongbaiTrans.Find("白纸").SetLocalPosition(paperMidPos);
            Animator animator = recruitModelTrans.GetComponent<Animator>();
            if (animator != null) animator.enabled = true;
            if (animator != null) animator.speed = 1;
            if (animator != null) animator.Play("Idle");
        }

        [EditorButton("播放书本模型动画")]
        private void PlayBookAnim()
        {
            PlayHideAllUIAnim(0.2f);
            Animator animator = GameObjectManager.Instance.GetComponent<Animator>(GameObjectID.RecruitModel);
            if (animator != null) animator.enabled = true;
            if (animator != null) animator.speed = 1;
            if (animator != null) animator.Play("Book");
        }

        [EditorButton("播放心愿单模型动画")]
        private void PlayWishAnim()
        {
            Animator animator = GameObjectManager.Instance.GetComponent<Animator>(GameObjectID.RecruitModel);
            if (animator != null) animator.enabled = true;
            if (animator != null) animator.speed = 1;
            if (animator != null) animator.Play("Wish");
        }

        [EditorButton("播放照片模型动画")]
        private void PlayPhotoAnim()
        {
            Animator animator = GameObjectManager.Instance.GetComponent<Animator>(GameObjectID.RecruitModel);
            if (animator != null) animator.enabled = true;
            if (animator != null) animator.speed = 1;
            if (animator != null) animator.Play("Photo");
        }

        Vector3 bookMidPos = new Vector3(-19.49553f, 1.463896f, 15.15973f);
        Vector3 paperMidPos = new Vector3(-20.62655f, -0.4188809f, 13.93907f);
        Vector3 bookFarPos = new Vector3(-61.5f, 1.5f, 14.5f);
        Vector3 paperFarPos = new Vector3(-60.3f, -0.8f, 12.9f);

        [EditorButton("准备播放活动招募书本动画")]
        public void InitActivityAnim()
        {
            ShowAllUI();
            GameObject recruitModelGo = GameObjectManager.Instance.GetGameObject(GameObjectID.RecruitModel);
            if (recruitModelGo == null) return;
            Transform recruitModelTrans = recruitModelGo.transform;
            Transform kongbaiTrans = recruitModelTrans.Find("空白");
            kongbaiTrans.Find("文件夹底").SetLocalPosition(bookFarPos);
            kongbaiTrans.Find("白纸").SetLocalPosition(paperFarPos);
            Animator animator = recruitModelTrans.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            if (animator != null) animator.speed = 0;
        }

        private Tween bookMoveTween = null;
        private Tween paperMoveTween = null;
        [EditorButton("播放活动招募书本动画")]
        public void PlayActivityAnim()
        {
            PlayHideAllUIAnim(0.3f);
            GameObject recruitModelGo = GameObjectManager.Instance.GetGameObject(GameObjectID.RecruitModel);
            if (recruitModelGo == null) return;
            Transform recruitModelTrans = recruitModelGo.transform;
            Transform kongbaiTrans = recruitModelTrans.Find("空白");

            bookMoveTween = kongbaiTrans.Find("文件夹底").DOLocalMove(bookMidPos, 0.5f);
            paperMoveTween = kongbaiTrans.Find("白纸").DOLocalMove(paperMidPos, 0.5f).OnComplete(() =>
            {
                Animator animator = recruitModelTrans.GetComponent<Animator>();
                if (animator != null) animator.enabled = true;
                if (animator != null) animator.speed = 1;
                if (animator != null) animator.Play("Book");
            });
        }

        public override void PlayExit()
        {
            base.PlayExit();
        }

        [SerializeField] private RectTransform bookHidePanel = null;
        private Sequence sequence = null;
        private void PlayHideAllUIAnim(float duration = 0.5f)
        {
            sequence?.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(bookHidePanel.gameObject.DOFade(0, duration));
        }
        private void ShowAllUI()
        {
            sequence?.Kill();
            sequence = DOTween.Sequence();
            bookHidePanel.gameObject.SetAlpha(1);
        }
    }
}

