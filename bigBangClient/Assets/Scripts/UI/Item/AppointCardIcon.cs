using GameConfig.Config;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class AppointCardIcon : CardIconBase
    {
        [SerializeField] protected GameObject cardInfo;
        [SerializeField] protected Image border;
        [SerializeField] protected ParticleSystem starParticle;

        public void SetData(CardModelConfig config, bool isHit = false)
        {
            if (config == null)
            {
                cardInfo.SetActive(false);

                var main = starParticle.main;
                main.playOnAwake = true;
                starParticle.Play();
            }
            else
            {
                base.SetData(config);

                cardInfo.SetActive(true);
                border.gameObject.SetActive(isHit);

                var main = starParticle.main;
                main.playOnAwake = false;
                starParticle.Stop();
            }
        }
    }
}