using deVoid.UIFramework;
using UnityEngine;
using Utils;
using BigBang.Animation;

namespace BigBang.UI
{
    public class UnlockTrainItemUIProperties : WindowProperties
    {
        public int TrainItemConfigId { get; set; }

        public UnlockTrainItemUIProperties(int trainItemConfigId)
        {
            TrainItemConfigId = trainItemConfigId;
        }
    }

    public class UnlockTrainItemUI : AWindowController<UnlockTrainItemUIProperties>
    {
        [SerializeField] private UnlockTrainItemComponent com;
        [SerializeField] private UnlockTrainAnim anim;

        protected override void AddListeners()
        {
            com.CloseBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            com.CloseBtn.onClick.RemoveListener(OnClose);
        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<UnlockTrainItemUI>();
            //显示下一条消息
            Player.TrainManager.ShowMessage();
        }

        protected override void OnPropertiesSet()
        {
            var item = Player.TrainManager.GetTrainItem(Properties.TrainItemConfigId);
            if (item == null) return;
            com.DescText.text = Lang.Get(LangID.UnlockTrainText).Replace("{TrainName}", item.GetConfig().Name);
            anim.Play();
            com.TrainImgs.ForEach(async img => img.sprite = await SpriteProxy.GetUnlockTrain(item.ConfigId));

            Player.CalFightPoint(true);
            //AudioManager.Instance.PlaySound(AudioNames.EVENT_UNLOCKTRAINING);
        }
    }
}