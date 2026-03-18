using GameConfig;
using System;
using Protocol;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using BigBang.UI;

namespace BigBang
{
    public class PlayerChallengeManager
    {
        public int ChallengeId { get; private set; }
        public int ChallengeTimes { get; private set; }
        public int MapId { get; private set; }
        public bool IsAllFinish { get; private set; }

        public void UnPack(ChallengeDataNotify data)
        {
            //ChallengeId = data.ChallengeId;
            //ChallengeTimes = data.ChallengeTimes;

            var cfg = Configs.ChallengeClub.GetConfig(ChallengeId);
            MapId = cfg.Country;
        }

        public void Save(int mapId)
        {
            PlayerPrefs.SetInt(LocalSaveID.CHALLENGE_MAP, mapId);
            PlayerPrefs.Save();
        }

        public Google.Protobuf.Collections.MapField<int, PointInfo> pointInfoDic = new();
        public void GetChallengeId(Action<bool, int> callback)
        {
            NetworkManager.Instance.GetChallengeId(response =>
            {
                //Debug.Log("Get ChallengeId Success: " + response.ChallengeId);
                //if (response.ChallengeId == GameConst.ChallengeEnd)
                //{
                //    ChallengeId = Configs.ChallengeClub.GetConfig(GameConst.ChallengeEndLast).Id;
                //    IsAllFinish = true;
                //}
                //else
                //{
                //    ChallengeId = response.ChallengeId;
                //}
                //ChallengeTimes = response.ChallengeTimes;
                //var cfg = Configs.ChallengeClub.GetConfig(ChallengeId);
                //MapId = cfg.Country;
                //pointInfoDic = response.ChallengeResultMap;
                //callback(IsFirstIn(cfg.Country, cfg.Country), cfg.Country);
            });
        }

        private bool IsFirstIn(int mapId, int index)
        {
            if (index == 1)
            {
                int save = PlayerPrefs.GetInt(LocalSaveID.CHALLENGE_MAP, 0);
                if (save < mapId)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsNewMap()
        {
            return false;
            var cfg = Configs.ChallengeClub.GetConfig(ChallengeId);
            return IsFirstIn(cfg.Country, cfg.Country);
        }

        public bool IsComplete(int challengeId)
        {
            return ChallengeId > challengeId;
        }

        /// <summary>
        /// 旧版本战斗
        /// </summary>
        /// <param name="callback"></param>
        public void ChallengeStart(Action<ChallengeStartResponse> callback)
        {
            //NetworkManager.Instance.ChallengeStart(response =>
            //{

            //    //Debug.Log("ChallengeStart Success  " + response.ChallengeId);
            //    if (response.Succeed == true)
            //    {
            //        Debug.Log("ChallengeStart Success ChallengeId=" + Player.ChallengeManager.ChallengeId);
            //        var cfg = Configs.ChallengeClub.GetConfig(Player.ChallengeManager.ChallengeId);
            //        if (cfg != null)
            //        {
            //            ChallengeId = cfg.Country;
            //        }
            //        callback(response);
            //        //Tips.PopTips("比赛胜利");
            //    }
            //    else
            //    {
            //        Debug.Log("ChallengeStart Fail");
            //        callback(response);
            //        //Tips.PopTips("比赛失败");
            //    }
            //});
        }

        public void OpenChallengeUI()
        {
            //TouchManager.Instance.DisableTouch();
            //Player.ChallengeManager.GetChallengeId((bool isNewMap, int mapId) =>
            //{
            //    if (isNewMap)
            //    {
            //        TouchManager.Instance.EnableTouch();
            //        UIController.Instance.ShowPanel<WorldMapUI>(new WorldMapUIProperties(mapId, true));
            //    }
            //    else
            //    {
            //        SceneManagerFor3D.LoadAddressableSceneAdditive(() =>
            //        {
            //            TouchManager.Instance.EnableTouch();
            //            UIController.Instance.ShowPanel<ChallengeUI>(new ChallengeUIProperties(mapId, true));
            //        });
            //    }
            //});
        }

    }
}
