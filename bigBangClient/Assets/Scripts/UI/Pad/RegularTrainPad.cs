using System.Collections.Generic;
using BigBang.Animation;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class RegularTrainPad : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private List<GameObject> items;
        [SerializeField] private ScrollRect scroll;

        private void Awake()
        {
            items.ForEach(item => item.SetActive(false));
        }

        //设置数据
        public void SetData()
        {
            var list = Player.TrainManager.TrainList();
            for (int i = 0; i < list.Count; i++)
            {
                items[i].GetComponent<RegularTrainItem>().SetItem(list[i].ConfigId);
            }
        }

        // 视图移动到顶部
        public void ScrollToTop()
        {
            scroll.verticalNormalizedPosition = 1;
        }

        //动画初始化
        public void InitAnim()
        {
            items.ForEach(item => item.GetComponent<RegularTrainItemAnim>().Init());
        }

        //播放动画
        public void PlayAnim()
        {
            for (int i = 0; i < items.Count; i++)
            {
                var anim = items[i].GetComponent<RegularTrainItemAnim>();
                anim.Play(i * 0.1f);
                items[i].SetActive(true);
            }
            //播放进度条动画
            PlayProgressAnim();
            //播放未解锁状态动画
            PlayUnlockAnim();
            //Timer.Register(this.gameObject, 1, () => guide.CheckGuide());
        }

        //播放进度条动画
        public void PlayProgressAnim()
        {
            items.ForEach(item => item.GetComponent<RegularTrainItemProgressAnim>().StartPlay());
        }

        //初始化未解锁状态动画
        public void InitUnlockAnim()
        {
            items.ForEach(item => item.GetComponent<RegularTrainItemUnlockAnim>().InitCanLockPlay());
        }
        //播放未解锁状态动画
        public void PlayUnlockAnim()
        {
            items.ForEach(item => item.GetComponent<RegularTrainItemUnlockAnim>().OnExpChanged(null));
        }
    }
}
