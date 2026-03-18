using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using GameConfig;
using System;
using UnityTimer;
using System.Linq;
using Utils;

namespace BigBang.Animation
{
    public class WorldMapUIAnim : AnimBase
    {
        [SerializeField] RawImage worldMapImg;
        [SerializeField] CanvasGroup bottomUi;
        [SerializeField] private CanvasGroup topBar = null;
        //[SerializeField] WorldMapItem worldMapItem;
        [SerializeField] Image mask;

        public Transform xuanZhuanTrans = null;
        public bool useIdleAni = false;

        private Camera _camera;
        private new Camera camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = CameraManager.Instance.GetCamera(CameraID.WorldMap);
                }
                return _camera;
            }
        }
        private Vector3 cameraLocalPos = new Vector3(0, 0.95f, -17.84f);

        // 镜头拉近动画
        public Tween MoveCameraIn(WorldMap3DItem worldMap3DItem)
        {
            Vector3 camPos = camera.transform.position;
            Vector3 itemPos = worldMap3DItem.transform.position;
            Vector3 midPos = (camPos + itemPos) / 2;
            return camera.transform.DOMove(midPos, 0.5f).AddTo(this.gameObject);
        }

        // 镜头拉远动画
        //public Tween MoveCameraOut(WorldMap3DItem worldMap3DItem)
        //{
        //    camera.transform.localPosition = camera.transform.position;
        //    Vector3 camPos = camera.transform.position;
        //    Vector3 itemPos = worldMap3DItem.transform.position;
        //    Vector3 midPos = (camPos + itemPos) / 2;
        //    camera.transform.position = midPos;
        //    return camera.transform.DOMove(camPos, 0.5f);
        //}

        public void PlayEnterAnim(WorldMap3DItem worldMap3DItem, Action callback = null)
        {
            mask.SetAlpha(0.5f);
            mask.DOFade(0, 0.5f).SetEase(Ease.Linear).AddTo(this.gameObject);
            //MoveCameraOut(worldMap3DItem).OnComplete(() =>
            //{
            // 文本线段动画
            //worldMapItem.Anim.ShowName();

            //if (Player.ChallengeManager.IsNewMap())
            //{
            //    float orgine = camera.fieldOfView;
            //    camera.fieldOfView = orgine + 5;
            //    worldMapImg.color = new Color(1, 1, 1, 0);
            //    worldMapImg.DOFade(1, 0.5f);
            //    camera.DOFieldOfView(orgine, 0.5f).SetEase(Ease.OutCubic);
            //}
            //else
            //{
            ShowCloseBtn();
            //}
            //worldMapItem.Anim.PlayAppearAnim();
            callback?.Invoke();
            //});
        }

        public Action idleUpdateCallBack = null;
        private Tween idleAni = null;
        public void StartIdle()
        {
            if (useIdleAni == false) return;
            if (idleAni != null) return;
            Debug.Log("StartIdle");
            StopIdle();
            Vector3 StartRot = xuanZhuanTrans.rotation.eulerAngles;
            Sequence sequence = DOTween.Sequence();
            sequence.AddTo(this.gameObject);
            sequence.Append(xuanZhuanTrans.DOLocalRotate(StartRot + new Vector3(0, 5, 0), 3.2f));
            sequence.Append(xuanZhuanTrans.DOLocalRotate(StartRot, 3.2f));
            sequence.OnUpdate(() =>
            {
                idleUpdateCallBack?.Invoke();
            });
            sequence.SetLoops(-1);
            idleAni = sequence;
        }
        public void StopIdle()
        {
            Debug.Log("StopIdle");
            idleAni?.Kill();
            idleAni = null;
        }

        private void ShowCloseBtn()
        {
            PlayEnter();
        }

        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [SerializeField] private RectTransform left;
        [SerializeField] private RectTransform right;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            top.SetAnchoredPositionY(UICommon.TopBarHideY);
            bottom.SetAnchoredPositionY(-300);
            left.SetAnchoredPositionX(-422f);
            right.SetAnchoredPositionX(458.4f);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 顶部栏下移
            tweens.Add(top.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
            // 底部栏上移
            tweens.Add(bottom.DOAnchorPosY(164.5f, 0.3f).OnComplete(() =>
            {
                topBar.interactable = true;
                bottomUi.interactable = true;
            }));
            // 左栏右移
            tweens.Add(left.DOAnchorPosX(-295, 0.3f));
            // 右栏左移
            tweens.Add(right.DOAnchorPosX(352, 0.3f));
        }

    }
}