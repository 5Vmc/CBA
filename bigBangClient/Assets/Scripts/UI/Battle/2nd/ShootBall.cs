using UnityEngine;

namespace BigBang.Battle
{
    [DisallowMultipleComponent]
    public class ShootBall : MonoBehaviour
    {
        [HideInInspector] private TrailRenderer trailRendererBlue;
        [HideInInspector] private TrailRenderer trailRendererRed;
        [HideInInspector] public Transform ballRotTrans;
        [HideInInspector] public Transform ballTrans2;
        [HideInInspector] public Transform ballTrans3;
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
            if (ballTrans2 == null)
            {
                ballTrans2 = ballRotTrans.Find("ball2");
            }
            if (ballTrans3 == null)
            {
                ballTrans3 = ballRotTrans.Find("ball3");
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
        public void ShowTrail()
        {
            bool is2 = shootBallScoreType == ShootBallScoreType.Two;
            trailRendererRed.gameObject.SetActive(!is2);
            trailRendererBlue.gameObject.SetActive(is2);
        }
        public void HideTrail()
        {
            trailRendererRed.gameObject.SetActive(false);
            trailRendererBlue.gameObject.SetActive(false);
        }

        private ShootBallScoreType shootBallScoreType = ShootBallScoreType.Two;
        public void SetShootBallScoreType(ShootBallScoreType shootBallScoreType)
        {
            this.shootBallScoreType = shootBallScoreType;
            bool is2 = shootBallScoreType == ShootBallScoreType.Two;
            ballTrans2.gameObject.SetActive(is2);
            ballTrans3.gameObject.SetActive(!is2);
        }



    }
}