using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class PropButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text PropCountText;
        [SerializeField] private Button btn;

        [SerializeField] private TMP_Text effectText;

        [SerializeField] private Image PropIcon;

       // [SerializeField] private Image effectImg;

        private GameItem _itemData;

        public Button Btn{
            get { return this.btn; }
        }

        public async void SetData(GameItem item)
        {
            //btn.interactable = clickable;
            _itemData = item;
            PropIcon.sprite = await item.GetIcon();
            if(item.GetPlayerCount()<=0)
                PropCountText.text = $"<color=red>{ item.GetPlayerCount()}</color>";
            else
                PropCountText.text = $"<color=white>{ item.GetPlayerCount()}</color>";

            /*if(effectText == null){
                this.effectText.gameObject.SetActive(false);
            }
            else{
                this.effectText.gameObject.SetActive(true);
                this.effectText.text = effectText;
            }*/

           
        }

        
        private void OnClick()
        {
            
        }
    }
}