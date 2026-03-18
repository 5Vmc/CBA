using System;
using Babu;
using Babu.BigNumber;
using BigBang.UI;
using Protocol;
using Utils;

namespace BigBang
{
    public class BigBangController
    {
        public BigBangController()
        {
        }

        public BigBangController(PlayerTrainManager trainManager)
        {
            _trainManager = trainManager;
        }

        private PlayerTrainManager _trainManager;
        //超训次数
        public int BigBangTimes { get; set; } = 0;

        //当天清除cd的次数
        private int _clearCDTimes { get; set; }

        //超训上一次cd 的时间
        private long _lastCDTime { get; set; } = 0;

        public void UnPack(BigBangControllerInfo data)
        {
            BigBangTimes = data.BigBangTimes;
            _clearCDTimes = data.ClearCdTimes;
            _lastCDTime = data.LastCdTime;
        }

        /**
         * 获取觉醒需要的总经验
         */
        public BigNumber BigBangNeedTotalExp()
        {
            if (_trainManager.Force > 0)
            {
                BigNumber needExp = 4 * BigNumberMath.Pow(_trainManager.Force / 150, 2) * BigNumberMath.Pow(10, 15);
                return needExp;
            }
            else
            {
                return 1.2f * BigNumberMath.Pow(10, 14);
            }
        }

        public void CheckRedDot() {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/BigBang");
            node.AddValue(CanBigBang() ? 1 : -1);
        }

        public bool CanBigBang()
        {
            return IsBigBangCdOver() && IsBigBangExpReady();
        }

        //超训剩余cd 单位s
        public long BigBangCDSecond()
        {
            return (GameConst.BigBangCdTime - (Utils.DataConvUtil.ServerTimeEx - _lastCDTime)) / 1000;
        }
        public bool IsBigBangCdOver()
        {
            return BigBangCDSecond() <= 0;
        }

        public bool IsBigBangExpReady()
        {
            return _trainManager.TotalExp >= BigBangNeedTotalExp();
        }

        public int GetClearBigBangCDDiamond()
        {
            int cdMin = (int)Math.Ceiling(BigBangCDSecond() / (TimeUtils.Min * 1.0f));
            if (cdMin <= 0) return 0;
            //超训清cd公式调整
            return (int)Math.Ceiling(2 * cdMin * Math.Pow(0.5 * _clearCDTimes, 2));
        }

        public bool CanClearBigBangCD()
        {
            int cost = GetClearBigBangCDDiamond();
            if (!Player.PackageManager.IsResourceEnough(ResourceId.Diamond, cost))
            {
                return false;
            }
            return true;
        }

        public void DoClearBigBangCd()
        {
            if (!CanClearBigBangCD()) return;

            NetworkManager.Instance.ClearBigBangCD(OnClearBigBangCD);
        }

        private void OnClearBigBangCD(ClearBigbangCDResponse response)
        {
            _lastCDTime = response.LastCdTime;
            _clearCDTimes = response.ClearCdTimes;

            EventManager.Instance.Dispatch(EventID.OnBigBangRefresh);
        }

        public BigNumber GetGiveForce(bool isBuffAdd = false)
        {
            var ret = BigNumberMath.Sqrt(_trainManager.TotalExp / BigNumberMath.Pow(10, 15)) * 150 - _trainManager.Force;
            if (isBuffAdd) ret *= 1.1f;
            return ret;
        }
        public BigNumber GetIncomeForceAddAfterBigBang()
        {
            var force = _trainManager.Force + GetGiveForce();
            if (force == 0) return 1;
            return 1 + force * (0.02 + _trainManager.ForceAdd);
        }

        private void SuperBigBangPlayVideo(Action callback)
        {

        }

        //超级训练
        public void DoBigBang(bool isBuffAdd = false)
        {
            var result = (Player.TrainManager.BigBangController.GetIncomeForceAddAfterBigBang() / Player.TrainManager.GetIncomeForceAdd());

            if (!CanBigBang()) return;


            if(isBuffAdd == true && ChannelManager.Instance.EnableAds){ //超能，并且广告
                SuperBigBangPlayVideo(()=>{DoBigBangImpl(isBuffAdd, result);});
                return;
            }
            
            DoBigBangImpl(isBuffAdd, result);
        }

        private void DoBigBangImpl(bool isBuffAdd, BigNumber result)
        {
             _trainManager.CheckAllIncome();

            var addForce = GetGiveForce(isBuffAdd);
            _trainManager.AddForce(addForce);

            _lastCDTime = Utils.DataConvUtil.ServerTimeEx;

            BigBangTimes++;

            int arg2 = 0;
            if(isBuffAdd==false)
                arg2 = (int)OfflineExpConfirmType.Noraml;
            else{
                if(ChannelManager.Instance.EnableAds){
                    arg2 = (int)OfflineExpConfirmType.Video;
                }else{
                    arg2 = (int)OfflineExpConfirmType.Diamond;
                }
            }
            _trainManager.AddTrainEvent(TrainEventIds.BigBang, isBuffAdd ? 1 : 0, arg2);

            EventManager.Instance.Dispatch(EventID.OnBigBangRefresh);

            UIController.Instance.OpenWindow<BigBangResultUI>(new BigBangResultProperties(addForce, result));
        }

    }
}