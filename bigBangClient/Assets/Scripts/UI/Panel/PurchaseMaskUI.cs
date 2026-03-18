using deVoid.UIFramework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class PurchaseMaskUI : AWindowController
    {
        [SerializeField] private Image loadSprite;

        [SerializeField] private List<Sprite> spriteSequence;

        private bool startAnim = false;
        private float startTime = 0;
        private int spriteIndex = 0;

        protected override void AddListeners()
        {

        }
        protected override void RemoveListeners()
        {
            StopAnim();
        }
        protected override void OnPropertiesSet()
        {
            PlayAnim();
        }

        private async void PlayAnim()
        {
            int closeTime = 180;

            startAnim = true;
            this.startTime = Time.time;
            while (startAnim)
            {
                loadSprite.sprite = spriteSequence[spriteIndex];
                spriteIndex = (spriteIndex + 1) % spriteSequence.Count;
                if (Time.time - this.startTime > closeTime)
                {
                    UIController.Instance.CloseWindow<PurchaseMaskUI>(false);
                }
                await Task.Delay(50);
            }
        }

        private void StopAnim()
        {
            startAnim = false;
        }
    }
}