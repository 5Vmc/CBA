using GameConfig;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class GuidePlayerIconItem : MonoBehaviour
    {
        [SerializeField] Image icon;

        public void SetIcon(Sprite portrait)
        {
            icon.sprite = portrait;
        }
    }
}
