using BigBang.Animation;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class DevelopGoodsItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button addButton;

        private GoodsConfig data;

        public string GoodsName
        {
            get
            {
                return data.Name;
            }
            private set { }

        }
        public void SetData(GoodsConfig data)
        {
            nameText.text = data.Name;
            this.data = data;
        }

        void OnDisable()
        {
            addButton.onClick.RemoveListener(OnClickAddBtn);
        }

        void OnEnable()
        {
            addButton.onClick.AddListener(OnClickAddBtn);
        }

        private void OnClickAddBtn()
        {
            int count = DevelopUI.Instance.GetInputInt();

            DevelopUI.Instance.SendCommand(DevelopCommand.AddGameItem, data.Id < 100 ? ((int)GameItemType.Resource).ToString() : ((int)GameItemType.Goods).ToString(), data.Id.ToString(), count.ToString());
        }
    }
}