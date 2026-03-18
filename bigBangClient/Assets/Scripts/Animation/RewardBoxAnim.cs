using Coffee.UIExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class RewardBoxAnim : MonoBehaviour
    {
        [SerializeField] private List<Sprite> sprites;
        [SerializeField] private UIParticle particle;

        [EditorButton("播放动效")]
        public async void Play()
        {
            float duration = 0.1f;
            if (sprites == null) return;
            var img = GetComponent<Image>();
            particle.Play();
            for (int i = 0; i < sprites.Count; i++)
            {
                img.sprite = sprites[i];
                await Task.Delay(TimeSpan.FromSeconds(i * duration / sprites.Count));
            }
        }
    }
}
