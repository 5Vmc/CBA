using System.Net.Sockets;
using Babu;
using Babu.SDK;
using Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using Environment = Babu.Environment;
using Utils;
using LightJson;
using UnityEditor;
using BigBang.UI;
using GameConfig.Config;
using GameConfig;
using Google.Protobuf;
using static BigBang.LoginManager;

namespace BigBang
{
    public class NetworkManager : Singleton<NetworkManager>
    {
        public NetworkManager()
        {
            ServerNotificationCenter.Instance.Register(this);
        }

        public void Call<T>(string methodName, IMessage message, Action<T> callback) where T : IMessage
        {
            CbaLogManager.Instance.NetworkCallIn(callback.Method.Name, callback.Target.ToString());
            SocketService.Instance.Call<T>(methodName, message, (T t) =>
            {
                callback.Invoke(t);
                CbaLogManager.Instance.NetworkCallOut(callback.Method.Name, callback.Target.ToString());
            });
        }

        public void Login(LoginUserInfo userInfo, Action<LoginResponse> callBack)
        {
            Debug.Log("Connect To Server Succ!");
            LoginRequest loginRequest = new LoginRequest();
            loginRequest.AccountId = AccountServiceManager.Instance.GetAccountId(AccountServiceManager.AccountServiceType.Local);
            loginRequest.Token = "";
            loginRequest.Channel = DisChannel.ChannelName;
            loginRequest.Os = SystemInfo.operatingSystem;
            loginRequest.Model = SystemInfo.deviceModel;
            loginRequest.GameVersion = Babu.Environment.GetValue<string>("minor_version", "");
            loginRequest.ResVersion = Babu.Environment.bundleCreatTime;
            loginRequest.Platform = Babu.Environment.GetValue<string>("operation_platform", "");

            if (userInfo != null)
            {
                loginRequest.Channel = userInfo.channel;//MiGuPlay
                loginRequest.Token = "";
                loginRequest.Uid = userInfo.userId;
            }

            Call<LoginResponse>(ProcID.Login, loginRequest, (msg) =>
            {
                LoginResponse loginResponse = msg as LoginResponse;
                callBack(loginResponse);
            });
        }

        public void FetchAllPlayer(Action<FetchAllPlayersResponse> callBack)
        {
            FetchAllPlayersRequest request = new FetchAllPlayersRequest();
            request.AccountId = AccountServiceManager.Instance.GetAccountId(AccountServiceManager.AccountServiceType.Local);
            request.Session = Environment.GetValue("session", "");
            Call<FetchAllPlayersResponse>(ProcID.FetchAllPlayers, request, msg =>
            {
                FetchAllPlayersResponse response = msg;

                Debug.Log("FetchAllPlayer Result: " + response.Players.Count);

                callBack(response);
            });
        }

        public void EnterGame(string gbid, Action<EnterGameResponse> enterGameCallBack)
        {
            EnterGameRequest request = new EnterGameRequest();
            request.AccountId = AccountServiceManager.Instance.GetAccountId(AccountServiceManager.AccountServiceType.Local);
            request.Session = Environment.GetValue("session", "");
            request.Gbid = gbid;
            Debug.Log("enterGame request: " + gbid);
            Call<EnterGameResponse>(ProcID.EnterGame, request, (msg) =>
            {
                EnterGameResponse response = msg as EnterGameResponse;
                enterGameCallBack(response);
            });
        }

        public void CreatePlayer(CreatePlayerRequest request, Action<CreatePlayerResponse> onCreatePlayer)
        {
            request.AccountId = AccountServiceManager.Instance.GetAccountId(AccountServiceManager.AccountServiceType.Local);
            request.Session = Environment.GetValue("session", "");
            Call<CreatePlayerResponse>(ProcID.CreatePlayer, request,
                (msg) => { onCreatePlayer(msg as CreatePlayerResponse); });
        }

        public void SyncTrainEvents(TrainEvent[] events)
        {
            SyncTrainEventsRequest request = new SyncTrainEventsRequest();
            foreach (var trainEvent in events)
            {
                request.TrainEvents.Add(trainEvent);
            }

            Call<SyncTrainEventsResponse>(ProcID.SyncTrainEvents, request, (msg) =>
            {
                SyncTrainEventsResponse response = msg as SyncTrainEventsResponse;
                Debug.Log("enterGame Result: " + response.Result);
            });
        }

        public void ClearBigBangCD(Action<ClearBigbangCDResponse> onClearBigBangCd)
        {
            ClearBigbangCDRequest request = new ClearBigbangCDRequest();
            Call(ProcID.ClearBigbangCD, request, onClearBigBangCd);
        }

        public void FetchInviteMatchInfo(Action<FetchInviteMatchInfoResponse> onFetchInviteMatchInfo)
        {
            FetchInviteMatchInfoRequest request = new FetchInviteMatchInfoRequest();
            Call<FetchInviteMatchInfoResponse>(ProcID.FetchInviteMatchInfo, request, (msg) =>
            {
                FetchInviteMatchInfoResponse response = msg as FetchInviteMatchInfoResponse;
                Debug.Log("cs_fetchInviteMatchInfo result ");

                onFetchInviteMatchInfo(response);
            });
        }

        public void DoInviteMatch(int id, Action<DoInviteMatchResponse> onDoInviteMatch)
        {
            DoInviteMatchRequest request = new DoInviteMatchRequest();
            request.Id = id;
            Call<DoInviteMatchResponse>(ProcID.DoInviteMatch, request, (msg) =>
            {
                DoInviteMatchResponse response = msg as DoInviteMatchResponse;
                Debug.Log("cs_doInviteMatch result  ");

                onDoInviteMatch(response);
            });
        }

        public void DoInviteMatchReward(int id, OfflineExpConfirmType type,
            Action<DoInviteMatchRewardResponse> onDoInviteMatchReward)
        {
            DoInviteMatchRewardRequest request = new DoInviteMatchRewardRequest();
            request.Id = id;
            request.VideoBuff = (int)type;
            Call<DoInviteMatchRewardResponse>(ProcID.DoInviteMatchReward, request, (msg) =>
            {
                DoInviteMatchRewardResponse response = msg as DoInviteMatchRewardResponse;

                onDoInviteMatchReward(response);
            });
        }

        public void DoOfflineReward(OfflineExpConfirmType type, Action<DoOfflineRewardResponse> onDoOfflineReward)
        {
            DoOfflineRewardRequest request = new DoOfflineRewardRequest();
            request.VideoBuff = (int)type;

            Call<DoOfflineRewardResponse>(ProcID.DoOfflineReward, request, (msg) =>
            {
                DoOfflineRewardResponse response = msg as DoOfflineRewardResponse;

                onDoOfflineReward(response);
            });
        }

        public void SendCommand(DevelopCommand command, string param1, string param2, string param3, Action<DevelopResponse> onCallBack)
        {
            var request = new DevelopRequest();
            request.Command = command;
            request.Param1 = param1;
            request.Param2 = param2;
            request.Param3 = param3;
            Call<DevelopResponse>(ProcID.Develop, request, onCallBack);
        }


        #region card

        // public void TestTenCard(Action<TestTenCardResponse> onTestTenCard)
        // {
        //     var request = new TestTenCardRequest();
        //     Call<TestTenCardResponse>(ProcID.TestTenCard, request, onTestTenCard);
        // }

        public void CardUpgradeStar(int cardId, Action<CardUpgradeStarResponse> onCardUpgradeStar)
        {
            var request = new CardUpgradeStarRequest();
            request.CardId = cardId;
            Call<CardUpgradeStarResponse>(ProcID.CardUpgradeStar, request, onCardUpgradeStar);
        }

        public void CardUpgradeQualityRequest(int cardId, Action<CardUpgradeQualityResponse> onCardUpgradeQuality)
        {
            var req = new CardUpgradeQualityRequest();
            req.CardId = cardId;
            Call<CardUpgradeQualityResponse>(ProcID.CardUpgradeQuality, req, onCardUpgradeQuality);
        }

        public void RefreshCardData(int cardid, Action<SynchCardInfoResponse> onRefreshCardData)
        {
            var request = new SynchCardInfoRequest();
            request.CardId = cardid;
            Call<SynchCardInfoResponse>(ProcID.SynchCardInfo, request, onRefreshCardData);
        }

        public void CardFire(List<int> cardIds, Action<CardFireResponse> onCardFire)
        {
            var request = new CardFireRequest();
            foreach (var id in cardIds)
            {
                request.CardIdList.Add(id);
            }

            Call<CardFireResponse>(ProcID.CardFire, request, onCardFire);
        }

        public void MergeCard(int piecesId, Action<MergeCardResponse> onMergeCard)
        {
            var request = new MergeCardRequest();
            request.PiecesId = piecesId;
            Call<MergeCardResponse>(ProcID.MergeCard, request, onMergeCard);
        }

        //招募
        public void Recruit(int poolId, RecruitCountType recruitCountType, RecruitCostType costType, Action<RecruitResponse> onRecruit)
        {
            var request = new RecruitRequest();
            request.PoolId = poolId;
            request.RecruitCountType = (int)recruitCountType;
            request.CostType = (int)costType;
            Call(ProcID.Recruit, request, onRecruit);
        }

        /// <summary>
        /// 领取招募奖励
        /// </summary>
        /// <param name="rewardid"></param>
        /// <param name="onRecruit"></param>
        public void GetRecruitRewards(int rewardid, Action<CollectRecruitRewardResponse> onRecruit)
        {
            var request = new CollectRecruitRewardRequest();
            request.RewardId = rewardid;
            Call(ProcID.CollectRecruitReward, request, onRecruit);
        }

        public void ChangeAppointCard(int poolId, int index, int cardId,
            Action<ChangeAppointCardResponse> onChangeAppointCard)
        {
            var request = new ChangeAppointCardRequest();
            request.PoolId = poolId;
            request.Index = index;
            request.CardId = cardId;
            Call(ProcID.ChangeAppointCard, request, onChangeAppointCard);
        }

        public void UnlockSkill(int skillId, Action<UnlockSkillResponse> onUnlockSkill)
        {
            var request = new UnlockSkillRequest();
            request.SkillId = skillId;
            Call(ProcID.UnlockSkill, request, onUnlockSkill);
        }

        public void UpgradeSkill(int cardId, int skillId, Action<CardUpgradeSkillResponse> action)
        {
            var req = new CardUpgradeSkillRequest();
            req.CardId = cardId;
            req.SkillId = skillId;
            Call(ProcID.CardUpgradeSkill, req, action);
        }

        // 解锁训练坑位
        public void UnlockSkillTrainRoom(int roomId, Action<UnlockSkillTrainRoomResponse> onUnlockSkillTrainRoom)
        {
            var request = new UnlockSkillTrainRoomRequest();
            request.RoomId = roomId;
            Call(ProcID.UnlockSkillTrainRoom, request, onUnlockSkillTrainRoom);
        }

        // 训练室开始训练
        public void BeginTrainSkill(int roomId, int cardId, int skillId, Action<BeginTrainSKillResponse> onBeginTrainSkill)
        {
            var request = new BeginTrainSkillRequest();
            request.RoomId = roomId;
            request.CardId = cardId;
            request.SkillId = skillId;
            Call(ProcID.BeginTrainSkill, request, onBeginTrainSkill);
        }

        // 清除训练室cd
        public void ClearTrainRoomCD(int roomId, Action<ClearTrainRoomCDResponse> onClearTrainRoomCD)
        {
            var request = new ClearTrainRoomCDRequest();
            request.RoomId = roomId;
            //Debug.Log("44   "+roomId);
            Call(ProcID.ClearTrainRoomCD, request, onClearTrainRoomCD);
        }

        /// <summary>
        /// 装备升级
        /// </summary>
        /// <param name="partIndex"></param>
        /// <param name="cardId"></param>
        /// <param name="onEquipPartLevelUp"></param>
        public void EquipPartLevelUp(int partIndex, int cardId, Action<CardUpgradeJerseyResponse> onEquipPartLevelUp)
        {
            var request = new CardUpgradeJerseyRequest();
            request.Part = partIndex;
            request.CardId = cardId;
            //Debug.Log("44   "+roomId);
            Call(ProcID.CardUpgradeJersey, request, onEquipPartLevelUp);
        }

        /// <summary>
        /// 装备突破
        /// </summary>
        /// <param name="partIndex"></param>
        /// <param name="cardId"></param>
        /// <param name="onEquipPartLevelUp"></param>
        public void EquipPartUpGrade(int cardId, Action<CardBreakJerseyResponse> onEquipPartUpGrade)
        {
            var request = new CardBreakJerseyRequest();
            request.CardId = cardId;
            //Debug.Log("44   "+roomId);
            Call(ProcID.CardBreakJersey, request, onEquipPartUpGrade);
        }


        #endregion

        #region package

        public void DelGoodsList(List<Goods> list, Action<DelGoodsResponse> onCallBack)
        {
            DelGoodsRequest request = new DelGoodsRequest();
            foreach (var goods in list)
            {
                request.DelList.Add(goods);
            }

            Call<DelGoodsResponse>(ProcID.DelGoods, request, onCallBack);
        }

        public void OpenBox(int goodsId, int number, Action<OpenBoxResponse> onCallback)
        {
            OpenBoxRequest request = new OpenBoxRequest();
            request.GoodsId = goodsId;
            request.Num = number;

            Call<OpenBoxResponse>(ProcID.OpenBox, request, onCallback);
        }

        public void MergeSplinter(int splinterId, Action<MergeSplinterResponse> callback)
        {
            MergeSplinterRequest request = new MergeSplinterRequest();
            request.GoodsId = splinterId;
            Call<MergeSplinterResponse>(ProcID.MergeSplinter, request, callback);
        }

        #endregion


        #region formation

        public void SaveFormationTemp(string name, List<int> boardIdList, Action<SaveFormationTempResponse> callBack)//保存上场位置
        {
            var request = new SaveFormationTempRequest();
            request.Name = name;
            foreach (var id in boardIdList)
            {
                request.BoardIdList.Add(id);
            }

            Call(ProcID.SaveFormationTemp, request, callBack);
        }

        //public void SaveTacticsTemp(List<int> tacticsIdList, Action<SaveTacticsTempResponse> callBack)//保存战术
        //{
        //    var request = new SaveTacticsTempRequest();
        //    request.Name = "";
        //    foreach (var id in tacticsIdList)
        //    {
        //        request.TacticsIdList.Add(id);
        //    }

        //    Call(ProcID.SaveTacticsTemp, request, callBack);
        //}
        public void UpgradeTactics(int tacticsId, Action<UpgradeTacticsResponse> callBack)//升级战术
        {
            var request = new UpgradeTacticsRequest();
            request.TacticsId = tacticsId;

            Debug.Log("UpgradeTactics:" + request.ToString());

            Call(ProcID.UpgradeTactics, request, callBack);
        }

        public void ChangeFormationTempName(int tempId, string name, Action<ChangeFormationTempNameResponse> callBack)
        {
            var request = new ChangeFormationTempNameRequest();
            request.TempId = tempId;
            request.Name = name;

            Call(ProcID.ChangeFormationTempName, request, callBack);
        }

        public void DelFormationTemp(int tempId, Action<DelFormationTempResponse> callBack)
        {
            var request = new DelFormationTempRequest();
            request.TempId = tempId;

            Call(ProcID.DelFormationTemp, request, callBack);
        }

        public void SaveFormation(int formationId, Formation formation, Action<SaveFormationResponse> callBack)
        {
            var request = new SaveFormationRequest();
            request.FormationId = formationId;
            request.Formation = formation.Pack();

            Debug.Log("SaveFormation:" + request.ToString());

            Call(ProcID.SaveFormation, request, callBack);
        }

        #endregion

        #region fight

        private readonly string noReportTipStr = "战斗录像版本过低，无法播放";
        public void GetFightReport(string fightId, Action<FightInfo> callBack, bool noTip = false, Action<FightInfo> nullCallBack = null)
        {
            Debug.Log("获取战报数据返回");
            //调试
            //fightId = "50101663813320271_2";
            string url = $"{ServerConst.PVP_BATTLE_URL}?fightId=" + fightId;
            UnityHttpServiceFix.Instance.AsyncGet(url, delegate (bool result, string response)
            {
                if (result == false)
                {
                    if (!noTip) Tips.PopTips(noReportTipStr);
                    nullCallBack?.Invoke(null);
                    return;
                }

                Debug.Log("response=" + response);

                JsonValue json = JsonValue.Parse(response);
                JsonObject jObj = json.AsJsonObject;
                if (jObj["result"] != 0)
                {
                    if (!noTip) Tips.PopTips(noReportTipStr);
                    nullCallBack?.Invoke(null);
                    return;
                }

                byte[] data = Convert.FromBase64String(jObj["data"].AsString);

                FightInfo fightInfo = FightInfo.Parser.ParseFrom(data);
                //resp.Fight = FightReportData.Parser.ParseFrom(response);
                callBack?.Invoke(fightInfo);
                nullCallBack?.Invoke(fightInfo);
            }, 10);
        }
        public void GetChallengeId(Action<GetChallengeDataResponse> callback)
        {
            var request = new GetChallengeDataRequest();
            Call(ProcID.GetChallengeData, request, callback);
        }


        public void ChallengeStart(int clubId, Action<ChallengeStartResponse> callback)
        {
            var request = new ChallengeStartRequest();
            request.ChallengeId = clubId;
            Call(ProcID.ChallengeStart, request, callback);
        }
        public void DevelopChallengeStart(Action<ChallengeStartResponse> callback)
        {
            var request = new DevChallengeStartRequest();
            Call<DevChallengeStartResponse>(ProcID.DevChallengeStart, request, (response) =>
            {
                ChallengeStartResponse challengeStartResponse = new();
                challengeStartResponse.Succeed = response.Succeed;
                //challengeStartResponse.ChallengeTimes = response.ChallengeTimes;
                //challengeStartResponse.ChallengeId = response.ChallengeId;
                challengeStartResponse.Fight = response.Fight;
                callback.Invoke(challengeStartResponse);
            });
        }

        #endregion

        #region compition

        // 获得首页比赛数据
        public void GetMainUIMatch(Action<GetMainUIMatchResponse> callback)
        {
            var request = new GetMainUIMatchRequest();
            Call(ProcID.GetMainUIMatch, request, callback);
        }

        // 获得积分榜
        public void GetLeagueScorebar(int compitionID, int leagueID, Action<GetLeagueScorebarResponse> callback)
        {
            var request = new GetLeagueScorebarRequest();
            request.CompitionId = compitionID;
            request.LeagueId = leagueID;

            Call(ProcID.GetLeagueScorebar, request, callback);
        }

        // 获取赛程
        public void GetLeagueCourse(int compitionID, int leagueID, int type, Action<GetLeagueCourseResponse> callback)
        {
            var request = new GetLeagueCourseRequest();
            request.Type = type;
            request.CompitionId = compitionID;
            request.LeagueId = leagueID;

            Call(ProcID.GetLeagueCourse, request, callback);
        }

        // 获得球员榜
        public void GetLeagueCardRank(int compitionID, int LeagueID, Action<GetLeagueCardRankResponse> callback)
        {
            var request = new GetLeagueCardRankRequest();
            request.CompitionId = compitionID;
            request.LeagueId = LeagueID;

            Call(ProcID.GetLeagueCardRank, request, callback);
        }

        // 修改时间
        public void ChangeCourseTime(int courseID, long time, Action<ChangeCourseTimeResponse> callback)
        {
            var request = new ChangeCourseTimeRequest();
            request.CourseId = courseID;
            request.Time = time;
            Call(ProcID.ChangeCourseTime, request, callback);
        }

        //获得比赛预览数据
        public void GetGamePreviewData(int compitionID, Action<GetGamePreviewDataResponse> callback)
        {
            var request = new GetGamePreviewDataRequest();
            request.CompitionId = compitionID;
            Call(ProcID.GetGamePreviewData, request, callback);
        }

        // 获得比赛信息
        public void GetCompitionData(Action<GetCompitionDataResponse> callBack)
        {
            var request = new GetCompitionDataRequest();

            Call(ProcID.GetCompitionData, request, callBack);
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        /// <param name="matchType"></param>
        /// <param name="callback"></param>
        public void GetPVPReward(int matchType, Action<ReceiveCompitionRewardResponse> callback)
        {
            var request = new ReceiveCompitionRewardRequest();
            request.CompitionId = matchType;
            Call(ProcID.ReceiveCompitionReward, request, callback);
        }

        // 设置New标签
        public void SetGoodsAsOldRequest(IList<int> goodsID, Action<SetGoodsAsOldResponse> callback)
        {
            var request = new SetGoodsAsOldRequest();
            request.GoodsId.AddRange(goodsID);
            Call(ProcID.SetGoodsAsOld, request, callback);
        }
        #endregion

        #region shop
        public void TrainShop(int shopItemID, Action<TrainShopResponse> callback)
        {
            var request = new TrainShopRequest();
            request.ShopItemId = shopItemID;
            Call(ProcID.TrainShop, request, callback);
        }

        /// <summary>
        /// 兑换接口，以物换物的购买。
        /// </summary>
        /// <param name="shopItemID"></param>
        /// <param name="callback"></param>
        public void ExChangeShopItem(int shopItemID, int count, Action<GameItemShopResponse> callback)
        {
            var request = new GameItemShopRequest();
            request.ShopItemId = shopItemID;
            request.Count = count;
            Call(ProcID.GameItemShop, request, callback);
        }

        #endregion

        // 获取到默认阵容
        public void GetDefaultFormation(int formationId, Action<GetDefaultFormationResponse> callBack)
        {
            var request = new GetDefaultFormationRequest();
            request.FormationId = formationId;
            Call(ProcID.GetDefaultFormation, request, callBack);
        }

        public void WatchFight(string fightID)
        {
            var request = new WatchFightRequest();
            request.FightId = fightID;
            Call<WatchFightResponse>(ProcID.WatchFight, request, response => { });
        }

        public void FetchFightFrames(string fightID, int index)
        {
            var request = new FetchFightFramesRequest();
            request.FightId = fightID;
            request.BeginFrame = index;
            Call<FetchFightFramesResponse>(ProcID.FetchFightFrames, request, response => { });
        }
        public void GetWatchBeginFrame(string fightID, Action<GetWatchBeginFrameResponse> callBack)
        {
            var request = new GetWatchBeginFrameRequest();
            request.FightId = fightID;
            Call(ProcID.GetWatchBeginFrame, request, callBack);
        }

        public void GetShootoutPrepareLeftTime(string fightID, Action<GetShootoutPrepareLeftTimeResponse> callBack)
        {
            var request = new GetShootoutPrepareLeftTimeRequest();
            request.FightId = fightID;
            Call(ProcID.GetShootoutPrepareLeftTime, request, callBack);
        }

        // 点球换人
        public void ShootOutExchangeCard(string fightId, int a_index, int b_index, Action<ShootoutExchangeCardResponse> callBack)
        {
            var request = new ShootoutExchangeCardRequest();
            request.FightId = fightId;
            request.AIndex = a_index;
            request.BIndex = b_index;
            Call(ProcID.ShootoutExchangeCard, request, callBack);
        }

        public void FightExchangeFormation(string fightId, FightFormation formation, Action<FightExchangeFormationResponse> callBack)
        {
            var request = new FightExchangeFormationRequest();
            request.FightId = fightId;
            request.Formation = formation.Pack();
            Call(ProcID.FightExchangeFormation, request, callBack);
        }

        //加一个换人的重载版本
        public void FightExchangeFormation(string fightId, FormationInfo info, Action<FightExchangeFormationResponse> callBack)
        {
            var request = new FightExchangeFormationRequest();
            request.FightId = fightId;
            request.Formation = info;
            Call(ProcID.FightExchangeFormation, request, callBack);
        }

        public void TestMessage()
        {
            TestMessageRequest request = new TestMessageRequest();

            Call<TestMessageResponse>("test_module.cs_sayHello", request, response =>
            {
                Debug.Log(response.Msg);
            });

        }

        #region Task
        public void CollectTaskReward(int taskID, Action<CollectTaskRewardResponse> callback)
        {
            var request = new CollectTaskRewardRequest();
            request.TaskId = taskID;
            Call(ProcID.CollectTaskReward, request, callback);
        }

        public void CollectTaskBoxReward(int boxId, Action<CollectTaskBoxRewardResponse> callback)
        {
            var request = new CollectTaskBoxRewardRequest();
            request.BoxId = boxId;
            Call(ProcID.CollectTaskBoxReward, request, callback);
        }
        #endregion

        #region Email
        public void SendTestSysEmail()
        {
            var request = new EmailSysTestRequest();
            Call<EmailSysTestResponse>(ProcID.EmailSysTest, request, response => { });
        }

        public void FetchServerTime(Action<FetchServerTimeResponse> callback)
        {
            FetchServerTimeRequest request = new FetchServerTimeRequest();
            Call<FetchServerTimeResponse>(ProcID.FetchServerTime, request, (response) =>
            {
                Debug.Log("Fetch Server Time: " + response.ServerTime);
                callback(response);
            });
        }

        public void ReadEmail(string emailId, Action<EmailReadResponse> callback)
        {
            var request = new EmailReadRequest();
            request.EmailId = emailId;
            Call(ProcID.EmailRead, request, callback);
        }

        public void DeleteEmail(string emailId, Action<EmailDeleteResponse> callback)
        {
            var request = new EmailDeleteRequest();
            request.EmailId = emailId;
            Call(ProcID.EmailDelete, request, callback);
        }

        public void ReceiveEmail(string emailId, Action<EmailReceiveResponse> callback)
        {
            var request = new EmailReceiveRequest();
            request.EmailId = emailId;
            Call(ProcID.EmailReceive, request, callback);
        }

        public void ReceiveAllEmails(List<string> emailList, Action<EmailReceiveAllResponse> callback)
        {
            var request = new EmailReceiveAllRequest();
            foreach (var emailId in emailList)
            {
                request.EmailList.Add(emailId);
            }
            Call(ProcID.EmailReceiveAll, request, callback);
        }

        // 引导邮件
        public void GuideEmail(int emailID, string name)
        {
            var request = new GuideEmailRequest();
            request.EmailId = emailID;
            request.Name = name;
            Action<GuideEmailResponse> callback = response => { };
            Call(ProcID.GuideEmail, request, callback);
        }

        // 完成引导
        public void FinishGuide(params GuideID[] guideIDs)
        {
            //foreach (var item in guideIDs)
            //{
            //    Debug.LogFormat("FinishGuide , guideID = {0}", item);
            //}

            var request = new FinishGuideRequest();
            foreach (var item in guideIDs)
            {
                request.GuideId.Add((int)item);
            }
            Action<FinishGuideResponse> callback = response => { };
            Call(ProcID.FinishGuide, request, callback);
        }

        public void DeleteAllEmails(List<string> emailList, Action<EmailDeleteAllResponse> callback)
        {
            var request = new EmailReceiveAllRequest();
            foreach (var emailId in emailList)
            {
                request.EmailList.Add(emailId);
            }
            Call(ProcID.EmailDeleteAll, request, callback);
        }
        #endregion

        #region Activity
        public void ReceiveSevenDayReward(bool re, Action<ReceiveResponse> callback)
        {
            var request = new ReceiveRequest();
            request.IsReceive = true;
            Call(ProcID.Receive, request, callback);
        }
        public void ReceiveMonthSignReward(int Id, int Type, Action<MonthSignResponse> callback)
        {
            var request = new MonthSignRequest();
            request.Id = Id;
            request.RewardType = Type;
            Call(ProcID.MonthSign, request, callback);
        }

        #endregion

        public void ChangePlayerCardNumber(int cardId, int number, Action<ChangePlayerCardNumberResponse> callBack)
        {
            var request = new ChangePlayerCardNumberRequest();
            request.CardId = cardId;
            request.Number = number;
            Call(ProcID.ChangePlayerCardNumber, request, callBack);

        }

        public void ExchangePlayerCardNumber(int cardId1, int cardId2, Action<ExchangePlayerCardNumberResponse> callBack)
        {
            var request = new ExchangePlayerCardNumberRequest();
            request.CardId1 = cardId1;
            request.CardId2 = cardId2;
            Call(ProcID.ExchangePlayerCardNumber, request, callBack);
        }

        //改名
        public void ReviseName(string newName, Action<ReviseNameResponse> callback)
        {
            var request = new ReviseNameRequest();
            request.NewName = newName;
            Call(ProcID.ReviseName, request, callback);
        }

        public void GetStrength(Action<GetStrengthResponse> callback)
        {
            GetStrengthRequest request = new GetStrengthRequest();
            Call(ProcID.GetStrength, request, callback);
        }

        public void CheckPlayerAchievement()
        {
            var request = new CheckPlayerAchievementRequest();
            Call<CheckPlayerAchievementResponse>(ProcID.CheckPlayerAchievement, request, response => { });
        }

        public void ClearAllAchievement()
        {
            var request = new ClearAllAchievementRequest();
            Call<ClearAllAchievementResponse>(ProcID.ClearAllAchievement, request, response => { });

        }


        #region 竞技场
        public void GetArenaInfo(Action<ArenaInfoResponse> callBack)
        {
            ArenaInfoRequest request = new ArenaInfoRequest();

            Call(ProcID.ArenaInfo, request, callBack);
        }

        public void ChangeOpponent(Action<ChangeOpponentResponse> callBack)
        {
            ChangeOpponentRequest req = new ChangeOpponentRequest();
            Call(ProcID.ChangeOpponent, req, callBack);
        }

        public void BeatOpponent(string teamId, Action<BattleResponse> callBack)
        {
            BattleRequest req = new BattleRequest();
            req.Gbid = teamId;
            Call(ProcID.Battle, req, callBack);
        }

        public void AddBattleTimes(Action<BuyEntriesResponse> callBack)
        {
            BuyEntriesRequest req = new BuyEntriesRequest();
            Call(ProcID.BuyEntries, req, callBack);
        }

        public void fetchMoreArenRank(Action<ArenaRankResponse> callback)
        {
            ArenaRankRequest req = new ArenaRankRequest();
            Call(ProcID.ArenaRank, req, callback);
        }

        public void arenaRankDetail(string teamId, Action<ArenaRankDetailResponse> callback)
        {
            ArenaRankDetailRequest req = new ArenaRankDetailRequest();
            req.Gbid = teamId;
            Call(ProcID.ArenaRankDetail, req, callback);
        }

        public void arenaBuy(int sid, Action<BuyGoodsFromArenaResponse> callback)
        {
            BuyGoodsFromArenaRequest req = new BuyGoodsFromArenaRequest();
            req.Sid = sid;
            Call(ProcID.BuyGoodsFromArena, req, callback);
        }

        public void getBattleLog(Action<GetBattleLogResponse> callback)
        {
            GetBattleLogRequest req = new GetBattleLogRequest();
            Call(ProcID.GetBattleLog, req, callback);
        }

        public void CollectArenaDailyAward(Action<CollectDailyAwardResponse> callback)
        {
            CollectDailyAwardRequest req = new CollectDailyAwardRequest();
            Call(ProcID.CollectDailyAward, req, callback);
        }

        #endregion

        #region 球员恢复
        public void RecoverPlayer(int cardId, int gid, Action<RecoverResponse> callback)
        {
            RecoverRequest req = new RecoverRequest();
            req.CardId = cardId;
            req.Gid = gid;
            Call(ProcID.Recover, req, callback);
        }
        #endregion

        #region 月卡 首充等
        public void GetFirstChargeReward(Action<GetFirstChargeRewardResponse> callback)
        {
            GetFirstChargeRewardRequest req = new GetFirstChargeRewardRequest();
            Call(ProcID.GetFirstChargeReward, req, callback);
        }

        public void GetMonthCardReward(int itemId, Action<GetMonthCardRewardResponse> callback)
        {
            GetMonthCardRewardRequest req = new GetMonthCardRewardRequest();
            req.ShopItemId = itemId;
            Call(ProcID.GetMonthCardReward, req, callback);
        }
        #endregion

        #region 新手目标

        public void GetNoviceTaskReward(int tid, Action<GetRewardResponse> callback)
        {
            GetRewardRequest req = new GetRewardRequest();
            req.Id = tid;
            Call(ProcID.GetReward, req, callback);
        }
        #endregion


        #region 
        public void GuideChallenge(Action<GuideChallengeResponse> callback)
        {
            GuideChallengeRequest req = new GuideChallengeRequest();
            Call(ProcID.GuideChallenge, req, callback);
        }
        #endregion

        //测试
        public void DiamondShop(int shopItemID, Action<PurchaseDiamondSuccessResponse> callback)
        {
            var request = new PurchaseDiamondSuccessRequest();
            request.ShopItemId = shopItemID;
            request.OrderId = Guid.NewGuid().ToString();
            Call(ProcID.PurchaseDiamondSuccess, request, callback);
        }

        public void GiftShop(int shopItemID, Action<PurchaseGiftSuccessResponse> callback)
        {
            var request = new PurchaseGiftSuccessRequest();
            request.ShopItemId = shopItemID;
            request.OrderId = Guid.NewGuid().ToString();
            Call(ProcID.PurchaseGiftSuccess, request, callback);
        }

        public void MonthCardBuy(int shopItemID, Action<PurchaseMonthCardSuccessResponse> callback)
        {
            var request = new PurchaseMonthCardSuccessRequest();
            request.ShopItemId = shopItemID;
            request.OrderId = Guid.NewGuid().ToString();
            Call(ProcID.PurchaseMonthCardSuccess, request, callback);
        }

        /// <summary>
        /// 定时领体力
        /// </summary>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        public void GetEnergyReward(int index, Action<GetEnergyTimeLimitResponse> callback)
        {
            var request = new GetEnergyTimeLimitRequest();
            request.ReceiveId = index;
            Call(ProcID.GetEnergyTimeLimit, request, callback);
        }

        /// <summary>
        /// 领取成就奖励
        /// </summary>
        /// <param name="achievementId"></param>
        /// <param name="callback"></param>
        public void GetAchievementRewards(int achievementId, Action<ReceiveAchievementResponse> callback)
        {
            var request = new ReceiveAchievementRequest();
            request.Id = achievementId;
            Call(ProcID.ReceiveAchievement, request, callback);
        }

        //心跳检测
        public void SyncHandshakeHeartbeat(Action<HeartResponse> callback)
        {
            var request = new HeartRequest();
            Call(ProcID.Heart, request, callback);
        }

        #region NFT
        public void GetNFTs(Action<GetNFTGoodsResponse> callback)
        {
            var request = new GetNFTGoodsRequest();
            Call(ProcID.GetNFTGoods, request, callback);
        }
        #endregion

        #region 投篮训练

        public void GetShootGameReward(int finish, int point, Action<ReceiveShootGameRewardResponse> callback)
        {
            ReceiveShootGameRewardRequest req = new ReceiveShootGameRewardRequest();
            req.Finish = finish;
            req.Point = point;
            Call(ProcID.ReceiveShootGameReward, req, callback);
        }

        #endregion

        #region 常规赛（新版本推图）

        /// <summary> 获取大地图界面数据 </summary>
        public void GetChallengeMapData(int level, Action<GetChallengeMapDataResponse> callback)
        {
            GetChallengeMapDataRequest req = new()
            {
                Level = level
            };
            Call(ProcID.GetChallengeMapData, req, callback);
        }

        /// <summary> 挑战界面的快速挑战 </summary>
        public void ChallengeStartFast(int challengeId, int fastTimes, Action<ChallengeStartFastResponse> callback)
        {
            ChallengeStartFastRequest req = new()
            {
                ChallengeId = challengeId,
                FastTimes = fastTimes
            };
            Call(ProcID.ChallengeStartFast, req, callback);
        }

        /// <summary> 挑战界面数据 </summary>
        public void GetChallengeData(int chapter, Action<GetChallengeDataResponse> callback)
        {
            GetChallengeDataRequest req = new()
            {
                Chapter = chapter
            };
            Call(ProcID.GetChallengeData, req, callback);
        }
        /// <summary> 挑战界面领取星星宝箱 </summary>
        public void CollectChapterBoxReward(int chapter, int box, Action<CollectChapterBoxRewardResponse> callback)
        {
            CollectChapterBoxRewardRequest req = new()
            {
                Chapter = chapter,
                Box = box
            };
            Call(ProcID.CollectChapterBoxReward, req, callback);
        }
        #endregion

        #region 剧情赛

        /// <summary> 剧情章节界面数据 </summary>
        public void GetChallengeHeroChapterData(Action<GetChallengeHeroChapterDataResponse> callback)
        {
            GetChallengeHeroChapterDataRequest req = new();
            Call(ProcID.GetChallengeHeroChapterData, req, callback);
        }

        /// <summary> 开始剧情战斗 </summary>
        public void ChallengeStartHero(int ChallengeId, FormationInfo formationInfo, Action<ChallengeStartHeroResponse> callback)
        {
            ChallengeStartHeroRequest req = new()
            {
                ChallengeId = ChallengeId,
                Formation = formationInfo
            };
            Call(ProcID.ChallengeStartHero, req, callback);
        }

        #endregion

        #region 悬赏任务

        /// <summary> 开始悬赏任务 </summary>
        public void StartBountyTask(int taskId, FormationInfo formation, Action<StartBountyTaskResponse> callback)
        {
            StartBountyTaskRequest req = new()
            {
                TaskId = taskId,
                Formation = formation
            };
            Call(ProcID.StartBountyTask, req, callback);
        }

        /// <summary> 领取悬赏任务奖励 </summary>
        public void CollectBountyTaskReward(int taskId, Action<CollectBountyTaskRewardResponse> callback)
        {
            CollectBountyTaskRewardRequest req = new()
            {
                TaskId = taskId,
            };
            Call(ProcID.CollectBountyTaskReward, req, callback);
        }

        /// <summary> 领取悬赏任务宝箱奖励 </summary>
        public void CollectBountyTaskBoxReward(int boxId, Action<CollectBountyTaskBoxRewardResponse> callback)
        {
            CollectBountyTaskBoxRewardRequest req = new()
            {
                BoxId = boxId,
            };
            Call(ProcID.CollectBountyTaskBoxReward, req, callback);
        }

        #endregion

        #region 爬塔玩法

        /// <summary>
        /// 领取爬塔奖励
        /// </summary>
        /// <param name="rewardId"></param>
        /// <param name="callback"></param>
        public void FBTowerGetRewards(int rewardId, Action<CollectTowerStarRewardResponse> callback)
        {
            var req = new CollectTowerStarRewardRequest();
            req.RewardId = rewardId;
            Call(ProcID.CollectTowerStarReward, req, callback);
        }

        /// <summary>
        /// 重置爬塔
        /// </summary>
        /// <param name="callback"></param>
        public void FBTowerReset(Action<ResetTowerResponse> callback)
        {
            var req = new ResetTowerRequest();
            Call(ProcID.ResetTower, req, callback);
        }

        /// <summary>
        /// 选择buff
        /// </summary>
        /// <param name="buffId"></param>
        /// <param name="callback"></param>
        public void FBTowerSelectBuff(int buffId, Action<SelectTowerBuffResponse> callback)
        {
            var req = new SelectTowerBuffRequest();
            req.Buffer = buffId;
            Call(ProcID.SelectTowerBuff, req, callback);
        }
        /// <summary>
        /// 扫荡接口
        /// </summary>
        /// <param name="callback"></param>
        public void FBTowerBatchBattle(Action<RaidTowerResponse> callback)
        {
            var req = new RaidTowerRequest();
            Call(ProcID.RaidTower, req, callback);
        }

        public void FBTowerBattle(Action<StartTowerChallengeResponse> callback)
        {
            var req = new StartTowerChallengeRequest();
            Call(ProcID.StartTowerChallenge, req, callback);
        }

        #endregion

        #region 球员升级

        public void CardUpgradeLevel(int cardId, Dictionary<int, int> useGoodsDic, Action<CardUpgradeLevelResponse> callback)
        {
            CardUpgradeLevelRequest req = new();
            req.CardId = cardId;
            foreach (var item in useGoodsDic)
            {
                Goods goods = new();
                goods.Id = item.Key;
                goods.Count = item.Value;
                req.Goods.Add(goods);
            }
            Call(ProcID.CardUpgradeLevel, req, callback);
        }

        /// <summary>
        /// 购买体力
        /// </summary>
        /// <param name="callback"></param>
        public void GetEnergyRequest(Action<GetEnergyResponse> callback)
        {
            GetEnergyRequest req = new();
            Call(ProcID.GetEnergy, req, callback);
        }

        /// <summary>
        /// 礼包码
        /// </summary>
        /// <param name="callback"></param>
        public void GetCodeGift(string _code, Action<ReceiveGiftCodeResponse> callback)
        {
            ReceiveGiftCodeRequest req = new();
            req.Code = _code;
            Call(ProcID.ReceiveGiftCode, req, callback);
        }

        #endregion

        #region 活动

        /// <summary> 领取积分奖励 </summary>
        public void ReceivePointReward(int activityId, List<int> receiveIdList, Action<ReceivePointRewardResponse> callback)
        {
            ReceivePointRewardRequest receivePointRewardRequest = new();
            receivePointRewardRequest.ActivityId = activityId;
            receivePointRewardRequest.ReceiveIds.AddRange(receiveIdList);
            Call(ProcID.ReceivePointReward, receivePointRewardRequest, (ReceivePointRewardResponse resp) =>
            {
                callback?.Invoke(resp);
                EventManager.Instance.Dispatch(EventID.RefreshWindow, activityId);
            });
        }
        /// <summary> 领取小额充值 </summary>
        public void ReceivePayMicroReward(int activityId, int giftId, Action<ReceivePayMicroResponse> callback)
        {
            ReceivePayMicroRequest receivePayMicroRequest = new()
            {
                GiftId = giftId
            };
            Call(ProcID.ReceivePayMicro, receivePayMicroRequest, (ReceivePayMicroResponse resp) =>
            {
                callback?.Invoke(resp);
                EventManager.Instance.Dispatch(EventID.RefreshWindow, activityId);
            });

        }
        /// <summary> 领取每日福利 </summary>
        public void ReceiveDailyGift(int activityId, Action<ReceiveDailyGiftResponse> callback)
        {
            ReceiveDailyGiftRequest receiveDailyGiftRequest = new()
            {
                ActivityId = activityId
            };
            Call(ProcID.ReceiveDailyGift, receiveDailyGiftRequest, (ReceiveDailyGiftResponse resp) =>
            {
                callback?.Invoke(resp);
                EventManager.Instance.Dispatch(EventID.RefreshWindow, activityId);
            });

        }
        /// <summary> 获得排行列表 </summary>
        public void GetActivityRankList(int activityId, int rankType, Action<GetRankListResponse> callback)
        {
            GetRankListRequest getRankListRequest = new();
            getRankListRequest.Type = rankType;
            Call(ProcID.GetRankList, getRankListRequest, (GetRankListResponse resp) =>
            {
                callback?.Invoke(resp);
                EventManager.Instance.Dispatch(EventID.RefreshWindow, activityId);
            });
        }

        /// <summary>
        /// 领取自选宝箱
        /// </summary>
        /// <param name="itemid"></param>
        /// <param name="chooseItemid"></param>
        /// <param name="num"></param>
        /// <param name="callback"></param>
        public void GetOptionalRewards(int itemid, int chooseItemid, int num, Action<OpenOptionalBoxResponse> callback)
        {
            var request = new OpenOptionalBoxRequest();
            request.GoodsId = itemid;
            request.OptionId = chooseItemid;
            request.Num = num;
            Call(ProcID.OpenOptionalBox, request, (OpenOptionalBoxResponse resp) =>
            {
                callback?.Invoke(resp);
            });
        }

        #endregion

        #region 排行榜详情

        /// <summary> 获得排行球员详情 </summary>
        public void GetRankCardDetail(int rankType, string gbid, int cardId, Action<PlayerCardInfo> callback)
        {
            GetRankCardDetailRequest getRankCardDetailRequest = new();
            getRankCardDetailRequest.RankType = rankType;
            getRankCardDetailRequest.Gbid = gbid;
            getRankCardDetailRequest.CardId = cardId;
            Call(ProcID.GetRankCardDetail, getRankCardDetailRequest, (GetRankCardDetailResponse resp) =>
            {
                callback?.Invoke(resp.Player);
            });
        }

        /// <summary> 获得排行球队详情 </summary>
        public void GetRankTeamDetail(int rankType, string gbid, Action<RankTeamInfo> callback)
        {
            GetRankTeamDetailRequest getRankTeamDetailRequest = new();
            getRankTeamDetailRequest.RankType = rankType;
            getRankTeamDetailRequest.Gbid = gbid;
            Call(ProcID.GetRankTeamDetail, getRankTeamDetailRequest, (GetRankTeamDetailResponse resp) =>
            {
                callback?.Invoke(resp.Team);
            });
        }

        #endregion

        #region 检查非法字符

        /// <summary> 
        /// 检查非法字符
        /// 返回true代表有敏感词
        ///  </summary>
        public void CheckStringContainIllegalCharacter(string str, Action<bool> callback)
        {
            CheckNameRequest checkNameRequet = new();
            checkNameRequet.Name = str;
            Call(ProcID.CheckName, checkNameRequet, (CheckNameResponse resp) =>
            {
                callback?.Invoke(resp.Code == 0 ? false : true);
            });
        }

        #endregion

        #region 消耗支付记录

        /// <summary> 
        /// 消耗支付记录
        ///  </summary>
        public void ConsumeOrderNo(List<string> orderNoList)
        {
            ConsumeOrderNoRequest consumeOrderNoRequest = new();
            consumeOrderNoRequest.OrderNo.AddRange(orderNoList);
            Call(ProcID.ConsumeOrderNo, consumeOrderNoRequest, (ConsumeOrderNoResponse resp) =>
            {
                Debug.Log("ConsumeOrderNo , resp.Succeed = " + resp.Succeed);
            });
        }

        #endregion

        #region 注销游戏账号

        /// <summary> 
        /// 注销游戏账号
        ///  </summary>
        public void DeletePlayer(Action deleteSuccessCallBack)
        {
            DeletePlayerRequest deletePlayerRequest = new();
            Call(ProcID.DeletePlayer, deletePlayerRequest, (DeletePlayerResponse resp) =>
            {
                Debug.Log("DeletePlayer success");
                deleteSuccessCallBack?.Invoke();
            });
        }

        #endregion

        #region 百分大战

        /// <summary>
        /// 百分大战报名
        /// </summary>
        /// <param name="ZoneId">分区id 1-8</param>
        /// <param name="callback">回调SignUpHundredReSponse</param>
        public void SignUpHundred(int ZoneId, Action<SignUpHundredResponse> callback)
        {
            var request = new SignUpHundredRequest();
            request.ZoneId = ZoneId;
            Call(ProcID.SignUpHundred, request, callback);
        }

        /// <summary>
        /// 百分大战赛程信息
        /// </summary>
        /// <param name="ZoneId">0 代表自己所在分区， 分区id 1-8来查看他人分区</param>
        /// <param name="callback">回调GetHundredCourseResponse</param>
        public void GetHundredCourse(int ZoneId, Action<GetHundredCourseResponse> callback)
        {
            var request = new GetHundredCourseRequest();
            request.ZoneId = ZoneId;
            Call(ProcID.GetHundredCourse, request, callback);
        }

        /// <summary>
        /// 百分大战单个玩家信息
        /// </summary>
        /// <param name="CompitionId">赛季 id</param>
        /// <param name="TeamId">队伍 gbid</param>
        /// <param name="callback">回调GetCourseTeamDataResponse</param>
        public void GetCourseTeamData(int CompitionId, string TeamId, Action<GetCourseTeamDataResponse> callback)
        {
            var request = new GetCourseTeamDataRequest();
            request.CompitionId = CompitionId;
            request.TeamId = TeamId;
            Call(ProcID.GetCourseTeamData, request, callback);
        }

        /// <summary>
        /// 获取百分大战往届赛季战绩
        /// </summary>
        /// <param name="SeasonTitle">历届赛季标题 格式:年,届</param>
        public void GetHundredHof(string SeasonTitle, Action<GetHundredHofResponse> callback)
        {
            var request = new GetHundredHofRequest();
            request.SeasonTitle = SeasonTitle;
            Call(ProcID.GetHundredHof, request, callback);
        }

        #endregion

        #region 圣诞树

        /// <summary>
        /// 开启节日包厢
        /// 消耗的道具走festival_box节日宝箱表
        /// </summary>
        /// <param name="ActivityId">活动 ID</param>
        /// <param name="OpenTimes">开启几个宝箱</param>
        /// <param name="callback">回调OpenFestivalBoxResponse</param>
        public void OpenFestivalBox(int ActivityId, int OpenTimes, Action<OpenFestivalBoxResponse> callback)
        {
            var request = new OpenFestivalBoxRequest();
            request.ActivityId = ActivityId;
            request.OpenTimes = OpenTimes;
            Call(ProcID.OpenFestivalBox, request, callback);
        }

        /// <summary>
        /// 领取节日任务
        /// </summary>
        /// <param name="FestivalTaskId">节日活动任务 ID，对应festival_task表</param>
        /// <param name="callback">回调GetFestivalTaskRewardResponse</param>
        public void GetFestivalTaskReward(int FestivalTaskId, Action<GetFestivalTaskRewardResponse> callback)
        {
            var request = new GetFestivalTaskRewardRequest();
            request.Id = FestivalTaskId;
            Call(ProcID.GetFestivalTaskReward, request, callback);
        }

        #endregion

        #region 元旦签到（祈愿）

        /// <summary>
        /// 设置奖励到许愿签上
        /// </summary>
        /// <param name="ActivityId">活动 ID</param>
        /// <param name="WishSign">新增一个许愿, 1开始</param>
        /// <param name="callback">回调SetWishSignResponse</param>
        public void SetWishSign(int ActivityId, int WishSign, Action<SetWishSignResponse> callback)
        {
            var request = new SetWishSignRequest();
            request.ActivityId = ActivityId;
            request.WishSign = WishSign;
            Call(ProcID.SetWishSign, request, callback);
        }

        /// <summary>
        /// 获取许愿签到奖励
        /// </summary>
        /// <param name="ActivityId">活动 ID</param>
        /// <param name="RewardIndex">第几个奖励, 1开始</param>
        /// <param name="callback">回调GetWishSignRewardResponse</param>
        public void GetWishSignReward(int ActivityId, int RewardIndex, Action<GetWishSignRewardResponse> callback)
        {
            var request = new GetWishSignRewardRequest();
            request.ActivityId = ActivityId;
            request.RewardIndex = RewardIndex;
            Call(ProcID.GetWishSignReward, request, callback);
        }

        #endregion

        #region 元旦（任务）

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Cycle">排行周期 1每日 2每周</param>
        /// <param name="Type">排行类型 1投篮游戏</param>
        /// <param name="callback">回调GetAllRankListResponse</param>
        public void GetAllRankList(int Cycle, int Type, Action<GetAllRankListResponse> callback)
        {
            var request = new GetAllRankListRequest();
            request.Cycle = Cycle;
            request.Type = Type;
            Call(ProcID.GetAllRankList, request, callback);
        }

        #endregion

        #region 新联赛

        /// <summary>
        /// 获取联赛信息
        /// </summary>
        /// <param name="callback">GetLeagueDataResponse</param>
        public void GetLeagueData(int LastLeagueId, Action<GetLeagueDataResponse> callback)
        {
            var request = new GetLeagueDataRequest();
            request.LastLeagueId = LastLeagueId;
            Call(ProcID.GetLeagueData, request, callback);
        }

        /// <summary>
        /// 报名联赛
        /// </summary>
        /// <param name="callback">回调GetLeagueSignUpResponse</param>
        public void GetLeagueSignUp(Action<GetLeagueSignUpResponse> callback)
        {
            var request = new GetLeagueSignUpRequest();
            Call(ProcID.GetLeagueSignUp, request, callback);
        }

        /// <summary>
        /// 获取联赛历届战绩
        /// </summary>
        /// <param name="callback">回调GetLeagueHistoryResponse</param>
        public void GetLeagueHistory(Action<GetLeagueHistoryResponse> callback)
        {
            var request = new GetLeagueHistoryRequest();
            Call(ProcID.GetLeagueHistory, request, callback);
        }

        /// <summary>
        /// 请求联赛巅峰榜单
        /// </summary>
        /// <param name="callback">回调GetLeagueChampionRankResponse</param>
        public void GetLeagueChampionRank(Action<GetLeagueChampionRankResponse> callback)
        {
            var request = new GetLeagueChampionRankRequest();
            Call(ProcID.GetLeagueChampionRank, request, callback);
        }

        /// <summary>
        /// 领取联赛结算奖励
        /// </summary>
        /// <param name="callback">回调ReceiveLeagueSettleRewardResponse</param>
        public void ReceiveLeagueSettleReward(Action<ReceiveLeagueSettleRewardResponse> callback)
        {
            var request = new ReceiveLeagueSettleRewardRequest();
            Call(ProcID.ReceiveLeagueSettleReward, request, callback);
        }

        #endregion

        #region 龙年红包

        /// <summary> 发红包 </summary>
        public void SendRedPacket(int ActivityId, int Count, Action<SendRedPacketResponse> callback)
        {
            var request = new SendRedPacketRequest();
            request.ActivityId = ActivityId;
            request.Count = Count;
            Call(ProcID.SendRedPacket, request, callback);
        }

        /// <summary> 抢红包 </summary>
        public void SnatchRedPacket(int ActivityId, Action<SnatchRedPacketResponse> callback)
        {
            var request = new SnatchRedPacketRequest();
            request.ActivityId = ActivityId;
            Call(ProcID.SnatchRedPacket, request, callback);
        }

        /// <summary> 点赞 </summary>
        public void LikeRedPacket(int ActivityId, string Gbid, Action<LikeRedPacketResponse> callback)
        {
            var request = new LikeRedPacketRequest();
            request.ActivityId = ActivityId;
            request.Gbid = Gbid;
            Call(ProcID.LikeRedPacket, request, callback);
        }

        /// <summary> 获得红包信息 </summary>
        public void GetRedPacketInfo(int ActivityId, Action<GetRedPacketInfoResponse> callback)
        {
            var request = new GetRedPacketInfoRequest();
            request.ActivityId = ActivityId;
            Call(ProcID.GetRedPacketInfo, request, callback);
        }

        /// <summary> 获得红包滚动公告 </summary>
        public void GetRedPacketMarquees(int ActivityId, Action<GetRedPacketMarqueesResponse> callback)
        {
            var request = new GetRedPacketMarqueesRequest();
            request.ActivityId = ActivityId;
            Call(ProcID.GetRedPacketMarquees, request, callback);
        }

        /// <summary> 获得红包领取记录 </summary>
        public void GetRedPacketLogs(int ActivityId, Action<GetRedPacketLogsResponse> callback)
        {
            var request = new GetRedPacketLogsRequest();
            request.ActivityId = ActivityId;
            Call(ProcID.GetRedPacketLogs, request, callback);
        }

        #endregion

        #region 2024全明星

        /// <summary>
        /// 获得全明星信息
        /// </summary>
        public void GetAllStarInfo(Action<GetAllStarInfoResponse> callback)
        {
            var request = new GetAllStarInfoRequest();
            Call(ProcID.GetAllStarInfo, request, callback);
        }

        /// <summary>
        /// 选择全明星阵营
        /// </summary>
        /// <param name="Area">1:北区 2:南区</param>
        public void PickAllStarArea(int Area, Action<PickAllStarAreaResponse> callback)
        {
            var request = new PickAllStarAreaRequest();
            request.Area = Area;
            Call(ProcID.PickAllStarArea, request, callback);
        }

        /// <summary>
        /// 同步全明星数据
        /// </summary>
        /// <param name="CardIdList">全明星卡牌ID</param>
        public void SyncAllStarData(List<int> CardIdList, List<int> CardPosList, Action<SyncAllStarResponse> callback)
        {
            var request = new SyncAllStarRequest();
            request.CardIdList.AddRange(CardIdList);
            request.CardPosList.AddRange(CardPosList);
            Call(ProcID.SyncAllStar, request, callback);
        }

        /// <summary>
        /// 获得全明星排行
        /// </summary>
        /// <param name="Area">1:北区 2:南区，传0会默认选择自己选择的分区</param>
        public void GetAllStarRank(int Area, Action<GetAllStarRankResponse> callback)
        {
            var request = new GetAllStarRankRequest();
            request.Area = Area;
            Call(ProcID.GetAllStarRank, request, callback);
        }

        /// <summary>
        /// 领取全明星战力奖励
        /// </summary>
        /// <param name="Option">奖励的战力，AllStarReward表的Option字段</param>
        public void GetAllStarStrengthReward(int Option, Action<GetAllStarStrengthRewardResponse> callback)
        {
            var request = new GetAllStarStrengthRewardRequest();
            request.Option = Option;
            Call(ProcID.GetAllStarStrengthReward, request, callback);
        }

        #endregion

        #region 百分大战竞猜（应援）

        /// <summary>
        /// 获取百分大战应援数据
        /// </summary>
        public void GetHundredSupport(Action<GetHundredSupportResponse> callback)
        {
            var request = new GetHundredSupportRequest();
            Call(ProcID.GetHundredSupport, request, callback);
        }

        /// <summary>
        /// 应援百分大战
        /// </summary>
        /// <param name="ZoneId">应援的分区 1-8淘汰赛分区 0是冠军赛</param>
        /// <param name="Round">第几轮比赛</param>
        /// <param name="TeamId">球队ID</param>
        public void SupportHundred(int zoneId, int courseId, string teamId, Action<SupportHundredResponse> callback)
        {
            var request = new SupportHundredRequest();
            request.ZoneId = zoneId;
            request.CourseId = courseId;
            request.TeamId = teamId;
            Call(ProcID.SupportHundred, request, callback);
        }

        /// <summary>
        /// 获得百分大战历届比赛数据
        /// </summary>
        /// <param name="Season">赛季 1-N</param>
        /// <param name="Stage">比赛阶段 1入围赛 2淘汰赛 3冠军赛</param>
        /// <param name="ZoneId">分区 1-8</param>
        public void GetHundredHistoryCourse(int Season, int Stage, int ZoneId, Action<GetHundredHistoryCourseResponse> callback)
        {
            var request = new GetHundredHistoryCourseRequest();
            request.Season = Season;
            request.Stage = Stage;
            request.ZoneId = ZoneId;
            Call(ProcID.GetHundredHistoryCourse, request, callback);
        }

        #endregion

        #region 2024五一活动

        /// <summary>
        /// 掷色子玩旅行棋盘
        /// </summary>
        /// <param name="ActivityId">活动</param>
        /// <param name="Count">掷色子次数</param>
        /// <param name="callback">回调掷色子结果</param>
        public void ThrowTravelDice(int ActivityId, int Count, Action<ThrowTravelDiceResponse> callback)
        {
            var request = new ThrowTravelDiceRequest();
            request.ActivityId = ActivityId;
            request.Count = Count;
            Call(ProcID.ThrowTravelDice, request, callback);
        }

        #endregion

        #region 2024全明星总决赛竞猜

        /// <summary>
        /// 2024全明星总决赛竞猜
        /// 获取比分与竞猜结果
        /// </summary>
        /// <param name="ActivityId">活动ID</param>
        /// <param name="callback">回调比分与竞猜结果</param>
        public void GetFinalsGuessInfo(int ActivityId, Action<GetFinalsGuessInfoResponse> callback)
        {
            var request = new GetFinalsGuessInfoRequest();
            request.ActivityId = ActivityId;
            Call(ProcID.GetFinalsGuessInfo, request, callback);
        }

        /// <summary>
        /// 2024全明星总决赛竞猜
        /// 竞猜
        /// </summary>
        /// <param name="RewardId">奖励类别ID</param>
        /// <param name="CourseId">赛程ID</param>
        /// <param name="Guess">竞猜的内容，MVP就是球员ID，某一场比赛就是比赛ID</param>
        /// <param name="callback">回调竞猜是否成功</param>
        public void FinalsGuess(int ActivityId, int RewardId, int CourseId, int Guess, Action<FinalsGuessResponse> callback)
        {
            var request = new FinalsGuessRequest();
            request.ActivityId = ActivityId;
            request.RewardId = RewardId;
            request.CourseId = CourseId;
            request.Guess = Guess;
            Call(ProcID.FinalsGuess, request, callback);
        }

        /// <summary>
        /// 2024全明星总决赛竞猜
        /// 领取竞猜奖励
        /// </summary>
        /// <param name="RewardId">奖励类别ID</param>
        /// <param name="CourseId">赛程ID</param>
        /// <param name="callback">领取竞猜奖励是否成功</param>
        public void GetFinalsGuessReward(List<MyFinalsGuess> myFinalsGuesseList, Action<GetFinalsGuessRewardResponse> callback)
        {
            var request = new GetFinalsGuessRewardRequest();
            request.FinalsGuessList.AddRange(myFinalsGuesseList);
            Call(ProcID.GetFinalsGuessReward, request, callback);
        }

        #endregion

        #region 2024端午节龙舟赛

        /// <summary>
        /// 获得赛龙舟信息
        /// </summary>
        /// <param name="ActivityId">活动ID</param>
        public void GetDragonBoatInfo(int ActivityId, Action<GetDragonBoatInfoResponse> callback)
        {
            var request = new GetDragonBoatInfoRequest();
            request.ActivityId = ActivityId;
            Call(ProcID.GetDragonBoatInfo, request, callback);
        }

        /// <summary>
        /// 选择支持赛龙舟队伍
        /// </summary>
        /// <param name="ActivityId">活动ID</param>
        /// <param name="Side">队伍1和2</param>
        public void PickDragonBoat(int ActivityId, int Side, Action<PickDragonBoatResponse> callback)
        {
            var request = new PickDragonBoatRequest();
            request.ActivityId = ActivityId;
            request.Side = Side;
            Call(ProcID.PickDragonBoat, request, callback);
        }

        /// <summary>
        /// 增加赛龙舟米数
        /// </summary>
        /// <param name="ActivityId">活动ID</param>
        /// <param name="CostCount">使用数量</param>
        public void AddDragonBoatMeters(int ActivityId, int CostCount, Action<AddDragonBoatMetersResponse> callback)
        {
            var request = new AddDragonBoatMetersRequest();
            request.ActivityId = ActivityId;
            request.CostCount = CostCount;
            Call(ProcID.AddDragonBoatMeters, request, callback);
        }

        /// <summary>
        /// 领取赛龙舟里程奖励
        /// </summary>
        /// <param name="ActivityId">活动ID</param>
        /// <param name="Option">里程奖励的选项</param>
        public void GetDragonBoatMetersReward(int ActivityId, int Option, Action<GetDragonBoatMetersRewardResponse> callback)
        {
            var request = new GetDragonBoatMetersRewardRequest();
            request.ActivityId = ActivityId;
            request.Option = Option;
            Call(ProcID.GetDragonBoatMetersReward, request, callback);
        }

        #endregion

        #region 数字藏品

        /// <summary>
        /// 查询数字藏品的球员道具
        /// </summary>
        public void GetPropCards(Action<GetPropCardsResponse> callback)
        {
            var request = new GetPropCardsRequest();
            Call(ProcID.GetPropCards, request, callback);
        }
        /// <summary>
        /// 从背包中上架出售球员道具
        /// </summary>
        /// <param name="CardId">配置表ID</param>
        /// <param name="callback">是否成功，刷新后的卡牌信息</param>
        public void SaleCard(int CardId, Action<SaleCardResponse> callback)
        {
            var request = new SaleCardRequest();
            request.CardId = CardId;
            Call(ProcID.SaleCard, request, callback);
        }
        /// <summary>
        /// 从数字藏品使用球员道具
        /// </summary>
        /// <param name="PropId">售卖道具ID</param>
        /// <param name="callback">是否成功，刷新后的卡牌信息</param>
        public void UsePropCard(string PropId, Action<UsePropCardResponse> callback)
        {
            var request = new UsePropCardRequest();
            request.PropId = PropId;
            Call(ProcID.UsePropCard, request, callback);
        }

        #endregion

    }

}
