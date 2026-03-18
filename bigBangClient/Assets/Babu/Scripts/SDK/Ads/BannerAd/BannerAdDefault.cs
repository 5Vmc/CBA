using UnityEngine;

namespace Babu.SDK
{
    public class BannerAdDefault : BannerAd
    {
        public override bool IsReady()
        {
            return true;
        }

        public override void Init(Vector2 adPos, Vector2 adSize = new Vector2())
        {
            Debug.Log($"Init: adSzie: {adSize}, adPos: {adPos}");
            LoadAd();
        }

        public override void UpdatePos(Vector2 adPos)
        {
            Debug.Log($"Update Pos: {adPos}");
        }

        public override Rect GetLayout()
        {
            return new Rect(0, 0, 350, 50);
        }

        protected override void LoadAd()
        {
            Debug.Log("LoadAd");
            OnAdPrepareSucc();
        }

        public override void ShowAd()
        {
            Debug.Log("ShowAd");
        }

        public override void HideAd()
        {
            Debug.Log("Hide");
        }
    }
}
