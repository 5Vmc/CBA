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

    public class LeagueFirstInfoUIProperties : WindowProperties
    {

        public CourseTeamData data;
        public LeagueFirstInfoUIProperties(CourseTeamData data)
        {
            this.data = data;
        }
    }
    public class LeagueFirstInfoUI : AWindowController<LeagueFirstInfoUIProperties>
    {

        [SerializeField] private Button closeBtn;

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

            foreach (ArenaFirstInfoPlayerItem item in new List<ArenaFirstInfoPlayerItem>() { itemFw, itemKw, itemZf, itemXq, itemDq })
            {
                item.InitState();
            }

            this._updateUI(Properties.data);
        }

        private void _updateUI(CourseTeamData data)
        {
            string tacticAtkName = "";
            string tacticDefName = "";
            List<int> tacticsIdList = ProtoUtils.UnPackRepeatedField2<int>(data.TacticsIdList);
            DataConvUtil.TacticsIdList2AtkDef(tacticsIdList, ref tacticAtkName, ref tacticDefName);
            this.nameTextATK.text = tacticAtkName;
            this.nameTextDEF.text = tacticDefName;
            this.textTeamName.text = data.Team.TeamName;
            this.teamLogo.SetIcon(data.Team.TeamIcon);
            foreach ( var item in data.BoardCardMap)
            {
                int boardId = item.Key;
                PlayerCardMiniInfo cardInfo = item.Value;
                int pos = Configs.FormationBoard.GetDataDictionary()[boardId].SeparatedPosition;
                ArenaFirstInfoPlayerItem playItem = ItemByPosition((PositionSeparatedType)pos);
                playItem.SetData(cardInfo.CardId, cardInfo.CombatEffectiveness, cardInfo.Quality);
            }
        }

        private ArenaFirstInfoPlayerItem ItemByPosition(PositionSeparatedType pos)
        {
            switch (pos)
            {
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
            UIController.Instance.CloseWindow<LeagueFirstInfoUI>();
        }
    }
}