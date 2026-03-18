using UnityEngine;

namespace BigBang.Battle
{
    [DisallowMultipleComponent]
    public class Battle2Ball : MonoBehaviour
    {
        [HideInInspector] private TrailRenderer trailRendererBlue;
        [HideInInspector] private TrailRenderer trailRendererRed;
        [HideInInspector] public Transform ballRotTrans;
        [HideInInspector] public Transform ballTrans;
        private void Awake()
        {
            if (trailRendererBlue == null)
            {
                trailRendererBlue = transform.Find("trail_blue").GetComponent<TrailRenderer>();
            }
            if (trailRendererRed == null)
            {
                trailRendererRed = transform.Find("trail_red").GetComponent<TrailRenderer>();
            }
            if (ballRotTrans == null)
            {
                ballRotTrans = transform.Find("ballRot");
            }
            if (ballTrans == null)
            {
                ballTrans = ballRotTrans.Find("ball");
            }
        }
        public void ClearTrail()
        {
            if (trailRendererBlue != null)
            {
                trailRendererBlue.Clear();
            }
            if (trailRendererRed != null)
            {
                trailRendererRed.Clear();
            }
        }
        public void SetTrail(bool isRed)
        {
            trailRendererRed.gameObject.SetActive(isRed);
            trailRendererBlue.gameObject.SetActive(!isRed);
        }
    }
}