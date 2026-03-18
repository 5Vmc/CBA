using System;
using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;
using Protocol;
using Babu;

namespace BigBang.UI
{

    public class ArenaFirstInfoUIProperties : WindowProperties
    {

        public ArenaTeamData TeamData;
        public string ReqTeamId = "";
        public ArenaFirstInfoUIProperties(ArenaTeamData data, string reqTeamId=null)
        {
            TeamData = data;
            ReqTeamId = reqTeamId;
        }
    }
    public class ArenaFirstInfoUI :  AWindowController<ArenaFirstInfoUIProperties>
    {

        [SerializeField] private Button closeBtn;
        //[SerializeField] private TMP_Text titleText;
        //[SerializeField] private LeagueRewardsUIAnim Anim;


        [SerializeField] private TMP_Text nameTextDEF;
        [SerializeField] private TMP_Text nameTextATK;

        [SerializeField] private ArenaFirstInfoPlayerItem itemFw;
        [SerializeField] private ArenaFirstInfoPlayerItem itemKw;
        [SerializeField] private ArenaFirstInfoPlayerItem itemZf;

        [SerializeField] private ArenaFirstInfoPlayerItem itemXq;
        [SerializeField] private ArenaFirstInfoPlayerItem itemDq;

        [SerializeField] private ClubIconItem teamLogo;
        [SerializeField] private TMP_Text textTeamName;
        

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
        }
        protected override void OnPropertiesSet()
        {

            base.OnPropertiesSet();
            //Properties.TeamData;

            
            foreach(ArenaFirstInfoPlayerItem item in new List<ArenaFirstInfoPlayerItem>(){itemFw, itemKw, itemZf, itemXq, itemDq}){
                item.InitState();
            }

            if(Properties.TeamData!=null){
                this._updateUI(Properties.TeamData);
            }
            else if(Properties.ReqTeamId!=null){
                NetworkManager.Instance.arenaRankDetail(Properties.ReqTeamId, resp=>{
                    if(resp.Succeed){
                    
                        if(resp.Team.Type == FightTeamType.NPC){
                            //Configs.ArenaPlayer.GetDataDictionary //arena_player
                            Debug.Log("FightTeamType.NPC");
                        }
                        else if(resp.Team.Type == FightTeamType.PLAYER)
                        {
                            this._updateUI(resp.Team);
                        }
                        
                    }
                    else{
                        Tips.PopTips("返回详情错误");
                    }

                }); 
            }

            //Anim.PlayEnter();
        }

        private void _updateUI(ArenaTeamData teamData)
        {
            if(teamData.Type == FightTeamType.PLAYER){
                Debug.Log("---->FightTeamType.PLAYER");
                string tacticAtkName = "";
                string tacticDefName = "";
                List<int> tacticsIdList = ProtoUtils.UnPackRepeatedField2<int>(teamData.TacticsIdList);
                DataConvUtil.TacticsIdList2AtkDef(tacticsIdList, ref tacticAtkName, ref tacticDefName);
                this.nameTextATK.text = tacticAtkName;
                this.nameTextDEF.text = tacticDefName;
                this.textTeamName.text = teamData.Name;
                this.teamLogo.SetIcon(teamData.Icon);
                foreach(PlayerCardMiniInfo cardInfo in teamData.StarterPlayerList){
                    int pos = Configs.FormationBoard.GetDataDictionary()[cardInfo.BoardId].SeparatedPosition; //separated_position
                    ArenaFirstInfoPlayerItem playItem = ItemByPosition((PositionSeparatedType)pos);
                    //cardInfo.
                    playItem.SetData(cardInfo.CardId, cardInfo.CombatEffectiveness, cardInfo.Quality);
                }
            }
            else{
                //Debug.Log("---->FightTeamType.NPC");
                //Configs.ArenaPlayer    teamData.Id
                ArenaClubConfig club = Configs.ArenaClub.GetDataDictionary()[int.Parse(teamData.Id)];

                string tacticAtkName = "";
                string tacticDefName = "";
                DataConvUtil.Tactics2AtkDef(club.Tactics, ref tacticAtkName, ref tacticDefName);
                this.nameTextATK.text = tacticAtkName;
                this.nameTextDEF.text = tacticDefName;

                this.textTeamName.text = club.Name;
                this.teamLogo.SetIcon(club.Icon);

                List<ArenaPlayerConfig> clubPlayers = new List<ArenaPlayerConfig>();
                foreach(ArenaPlayerConfig player in Configs.ArenaPlayer.GetConfigList())
                {
                    if(player.ClubId == int.Parse(teamData.Id)){
                        clubPlayers.Add(player);
                    }
                }
               
                //找出5个首发
                Dictionary<int, ArenaPlayerConfig> fivePlayersDict = this.GenFirstFiveCard(club, clubPlayers);
                foreach(int position in fivePlayersDict.Keys){
                    ArenaPlayerConfig player = fivePlayersDict[position];
                    CardModelConfig cardModel = Configs.CardModel.GetDataDictionary() [player.CardId];
                    //Debug.Log("---->找cardModel.AdaptPosition[0]=" + cardModel.AdaptPosition[0]);
                    
                    ArenaFirstInfoPlayerItem playItem = ItemByPosition((PositionSeparatedType)position);
                    var positionCfg = Configs.SeparatedPosition.GetConfig(position);
                    int ability = DataConvUtil.GetCombatEffectiveness(cardModel.Ability, positionCfg.AbilityRatio);
                    playItem.SetData(player.CardId, ability, cardModel.Quality);
                }


            }
        }
        //取相同位置上能力最强的
        private Dictionary<int, ArenaPlayerConfig> GenFirstFiveCard(ArenaClubConfig club, List<ArenaPlayerConfig> playerCfgList)
        {
            var retDict = new Dictionary<int, ArenaPlayerConfig>();
            var sysFormationCfg = Configs.SysFormation.GetConfig(club.Formation);
            foreach (var boardId in sysFormationCfg.BoardIdList) //其实只有一条记录
            {
                var separatedPostion = Configs.FormationBoard.GetConfig(boardId).SeparatedPosition;
                var positionCfg = Configs.SeparatedPosition.GetConfig(separatedPostion);
                int posId = positionCfg.Id;
                int targetIndex = 0;
                int maxScore = -1;
                
                for (int i = 0; i < playerCfgList.Count; i++)
                {
                    //playerCfgList[i].CardId
                   // if(playerCfgList[i].Position != separatedPostion) //不是同一个位置的不选
                    //    continue;
                    if (retDict.ContainsKey(posId) || retDict.ContainsValue(playerCfgList[i])) continue;
                    var score = GetCombatEffectiveness(playerCfgList[i], positionCfg);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        targetIndex = i;
                    }
                }
                retDict.Add(posId, playerCfgList[targetIndex]);
            }
            return retDict;
        }

        private int GetCombatEffectiveness(ArenaPlayerConfig playerCfg, SeparatedPositionConfig positionCfg)
        {
           /* float sum = 0;
            for (int i = AbilityId.Shoot; i <= AbilityId.Will; i++)
            {
                sum += playerCfg.Ability[i] * positionCfg.AbilityRatio[i];
            }
            return Mathf.FloorToInt(sum / GameConst.ABILITY_NORMAL); */

            return DataConvUtil.GetCombatEffectiveness(playerCfg.Ability, positionCfg.AbilityRatio);
        }

        private ArenaFirstInfoPlayerItem ItemByPosition(PositionSeparatedType pos){
            switch(pos){
                case PositionSeparatedType.DaQianFeng:
                return this.itemDq;
                case PositionSeparatedType.DeFenHouWei:
                return this.itemFw;
                case PositionSeparatedType.KongQiuHouWei:
                return this.itemKw;
                case PositionSeparatedType.XiaoQianFeng:
                return this.itemXq;
                case PositionSeparatedType.ZhongFeng:
                return this.itemZf;
            }
            return null;
        }

        private void OnClose()
        {
           
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<ArenaFirstInfoUI>(); 
            /*Anim.PlayExit(() =>
            {
               UIController.Instance.CloseWindow<ArenaFirstInfoUI>(); 
            });*/
        }
    }
}