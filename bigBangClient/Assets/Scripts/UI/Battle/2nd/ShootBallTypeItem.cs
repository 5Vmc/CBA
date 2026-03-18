using UnityEngine;

namespace BigBang.Battle
{

    public enum ShootBallEnterType
    {
        Enter,//亮色
        NotEnter,//灰色
        NotUse,//暗色
    }
    public enum ShootBallScoreType
    {
        Two = 2,//2分
        Three = 3,//3分
    }

    [DisallowMultipleComponent]
    public class ShootBallTypeItem : MonoBehaviour
    {
        [SerializeField] public RectTransform rect;
        [SerializeField] private GameObject ball2EnterImage;
        [SerializeField] private GameObject ball2NotEnterImage;
        [SerializeField] private GameObject ball2NotUseImage;
        [SerializeField] private GameObject ball3EnterImage;
        [SerializeField] private GameObject ball3NotEnterImage;
        [SerializeField] private GameObject ball3NotUseImage;

        private ShootBallEnterType shootBallEnterType = ShootBallEnterType.NotUse;
        public void SetShootBallEnterType(ShootBallEnterType shootBallEnterType)
        {
            this.shootBallEnterType = shootBallEnterType;
        }

        private ShootBallScoreType shootBallScoreType = ShootBallScoreType.Two;
        public void SetShootBallScoreType(ShootBallScoreType shootBallScoreType)
        {
            this.shootBallScoreType = shootBallScoreType;
        }

        public void RefreshShow()
        {
            ball2EnterImage.SetActive(false);
            ball2NotEnterImage.SetActive(false);
            ball2NotUseImage.SetActive(false);
            ball3EnterImage.SetActive(false);
            ball3NotEnterImage.SetActive(false);
            ball3NotUseImage.SetActive(false);

            switch (shootBallScoreType)
            {
                case ShootBallScoreType.Two:
                    {
                        switch (shootBallEnterType)
                        {
                            case ShootBallEnterType.Enter: ball2EnterImage.SetActive(true); break;
                            case ShootBallEnterType.NotEnter: ball2NotEnterImage.SetActive(true); break;
                            case ShootBallEnterType.NotUse: ball2NotUseImage.SetActive(true); break;
                        }
                    }
                    break;
                case ShootBallScoreType.Three:
                    {
                        switch (shootBallEnterType)
                        {
                            case ShootBallEnterType.Enter: ball3EnterImage.SetActive(true); break;
                            case ShootBallEnterType.NotEnter: ball3NotEnterImage.SetActive(true); break;
                            case ShootBallEnterType.NotUse: ball3NotUseImage.SetActive(true); break;
                        }
                    }
                    break;
            }
        }


    }



}