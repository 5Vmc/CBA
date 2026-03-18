using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class PropItem : MonoBehaviour
    {
        [SerializeField] public TMP_Text ValueText;
        [SerializeField] private Button btn;
        [SerializeField] private Image propIcon;

        [SerializeField] private Image backgroundImg;

        private bool _ownerEnough = false;

        private GameItem item;


        private void OnEnable()
        {
            btn.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            btn.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(item));
        }

        public async void SetData(GameItem item, bool clickable = false)
        {
            this.item = item;
            // 设置道具图片
            propIcon.sprite = await item.GetIcon();
            //设置可点击状态
            btn.interactable = clickable;
            if (item.GetPlayerCount() < item.Count)
            {
                ValueText.text = $"<color=red>{ item.GetPlayerCount()}</color>/{item.Count}";
                _ownerEnough = false;
            }
            else
            {
                ValueText.text = $"<color=green>{ item.GetPlayerCount()}</color>/{item.Count}";
                _ownerEnough = true;
            }

            backgroundImg.sprite = await SpriteProxy.GetInvetoryQuality(item.GetQuality());
        }

        public bool ownerEnough()
        {
            return _ownerEnough;
        }

        public GameItem ItemData
        {
            get {return this.item;}
            private set{}
        }

        /*public void SetData2(GameItem item, bool clickable = false)
        {
            this.item = item;
            // 设置道具图片
            propIcon.sprite = item.GetIcon();
            //设置可点击状态
            btn.interactable = clickable;
            ValueText.text = item.Count.ToString();
        }*/
    }
}