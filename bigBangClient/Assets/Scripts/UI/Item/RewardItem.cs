using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class RewardItem : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] TMP_Text countText;

        public void SetData(RewardItemData data)
        {
            //Debug.Log(">>>>>>>>>Email SetData:"  +data.type);
            GameItem gameItem = GameItemUtils.CreateGameItem(data.type, data.id, data.count);
            if(gameItem == null)
            {
                Debug.LogErrorFormat("gameItem == null , type = {0} , id = {1} , count = {2}", data.type, data.id, data.count);
            }
            countText.text = data.count.ToString();
            UpdateUI(gameItem);
        }

        public async void UpdateUI(GameItem data)
        {
            icon.sprite = await data.GetIcon();
            countText.text = data.CountString();
        }

       
    }
}
