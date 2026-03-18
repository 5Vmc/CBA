using BigBang;
using UnityEngine;
using UnityEngine.UI;

public class DebrisIconItem : MonoBehaviour
{
    [SerializeField] private Image playerImg;
    [SerializeField] private Image debrisEdgeImg;
    [SerializeField] private Image backgroundImg;

    public async void SetData(int portraitID, int quality)
    {
        // 设置球员头像
        playerImg.sprite = await SpriteProxy.GetPlayerPortrait(portraitID);
        // 设置边缘光图片
        debrisEdgeImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.DebrisEdge, quality);
        // 设置背景图片
        backgroundImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.DebrisBackground, quality);
    }
}
