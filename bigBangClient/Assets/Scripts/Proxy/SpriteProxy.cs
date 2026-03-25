using CBA;
using GameConfig.Config;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BigBang
{
    public class SpriteProxy
    {

        // 大叉叉图片
        public static Task<Sprite> Error
        {
            get => SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.Error);
        }

        // 未知图片
        public static Task<Sprite> Unknown
        {
            get => SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.Unknown);
        }

        // 全透明图片
        public static Task<Sprite> None
        {
            get => SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.None);
        }

        public static Task<Sprite> RedCard
        {
            get => SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.RedCard);
        }

        public static Task<Sprite> YellowCard
        {
            get => SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.YellowCard);
        }

        public static Task<Sprite> SubstitutionUp
        {
            get => SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.SubstitutionUp);
        }

        public static Task<Sprite> SubstitutionDown
        {
            get => SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.SubstitutionDown);
        }

        public static Task<Sprite> Assist
        {
            get => SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.Assist);
        }

        public static Task<Sprite> PenaltyGoal
        {
            get => SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.PenaltyGoal);
        }

        public static Task<Sprite> PenaltyFail
        {
            get => SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.PenaltyFail);
        }

        public static Task<Sprite> YellowBtnEnable
        {
            get => SpriteManager.GetSprite(AtlasNames.Public, "btn1_small");
        }

        public static Task<Sprite> YellowBtnDisable
        {
            get => SpriteManager.GetSprite(AtlasNames.Public, "btn_9_3");
        }

        public static Task<Sprite> YellowSmallBtnEnable
        {
            get => SpriteManager.GetSprite(AtlasNames.Task, "btn_9_5");
        }

        public static Task<Sprite> YellowSmallBtnDisable
        {
            get => SpriteManager.GetSprite(AtlasNames.Public, "btn_9_4");
        }

        public static Task<Sprite> UnknownClubIcon
        {
            get => SpriteManager.GetSprite(AtlasNames.ClubIcon, "0");
        }

        public static Task<Sprite> DefaultHonourCup
        {
            get => SpriteManager.GetSprite(AtlasNames.HonourCup, "default");
        }

        // 获得俱乐部图标
        public static async Task<Sprite> GetClubIcon(string icon)
        {
            try
            {
                if (icon == "") return await UnknownClubIcon;
                var sprite = await SpriteManager.GetSprite(AtlasNames.ClubIcon, icon, await UnknownClubIcon);
                return sprite;
            }
            catch (Exception)
            {
                return await UnknownClubIcon;
            }
        }

        // 获得训练解锁图片（图集：UnlockTrain）
        public static Task<Sprite> GetUnlockTrain(int id)
        {
            return SpriteManager.GetSprite(AtlasNames.UnlockTrain, SpriteNames.UnlockTrain.BG.Replace("{id}", id.ToString()));
        }

        // 获得球员头像（图集：Portrait)
        public static Task<Sprite> GetPlayerPortrait(int id)
        {
            return SpriteManager.GetSprite(AtlasNames.Portrait, id.ToString());
        }

        // 获得球员头像
        public static Task<Sprite> GetPortrait(int id)
        {
            if (id > 999999)
            {
                return SpriteManager.GetSprite(AtlasNames.Npc, id.ToString());//图集：Npc
            }
            else
            {
                return SpriteManager.GetSprite(AtlasNames.Portrait, id.ToString());//图集：Portrait
            }
        }

        public static Task<Sprite> GetPlayerCardYellowBg(int id)//黄色卡片的卡牌背景
        {
            return SpriteManager.GetSprite(AtlasNames.CardYellowBg, id.ToString());
        }
        public static Task<Sprite> GetPlayerPortraitYellow(int id)//黄色卡牌的特殊头像
        {
            return SpriteManager.GetSprite(AtlasNames.PortraitYellow, id.ToString());
        }
        public static Task<Sprite> GetPlayerYellowBigImage(int id)//黄色卡牌的全屏海报大图
        {
            return null;// SpriteManager.GetSprite(AtlasNames.YellowBigImage, id.ToString());
        }

        /// <summary>
        /// 获取活动图片
        /// </summary>
        /// <param name="type"></param>
        /// <param name="imgName"></param>
        /// <returns></returns>
        public static Task<Sprite> GetFestivalImg(int type, string imgName)
        {
            return SpriteManager.GetSprite(AtlasNames.Festival, "t" + type.ToString() + "_" + imgName);
        }


        // 球员卡片道具图片(图集:PropCard)
        public static Task<Sprite> GetPropCard(int id)
        {
            //return SpriteManager.GetSprite(AtlasNames.PropCard, id.ToString());//头像的小图
            return GetPlayerPortrait(id);//使用头像的大图
        }

        // 获得随机头像（图集：Portrait)
        public static Task<Sprite> RandomPortrait()
        {
            var portraitList = GameConfig.Configs.CardModel.GetConfigList().Select(item => item.Portrait).ToList();
            string portrait = portraitList[UnityEngine.Random.Range(0, portraitList.Count)].ToString();
            return SpriteManager.GetSprite(AtlasNames.Portrait, portrait);
        }

        // 获得球员卡片相关图片（图集：Card）
        public static Task<Sprite> GetCardQualitySprite(string qualityGroup, int quality)
        {
            string spriteName = qualityGroup.Replace("{quality}", quality.ToString());
            return SpriteManager.GetSprite(AtlasNames.Card, spriteName);
        }
        // 布阵界面首发球员箭头
        public static Task<Sprite> GetFormationMainArrpwSprite(int quality)
        {
            return SpriteManager.GetSprite(AtlasNames.Formation, "FormationMainArrow" + quality);
        }

        /**
        获得卡片上表示升阶过来的图片
        */
        public static Task<Sprite> GetQualityAdvanceTagInTag(string qualityGroup, int quality)
        {
            string spriteName = qualityGroup.Replace("{quality}", quality.ToString()) + "_2";
            return SpriteManager.GetSprite(AtlasNames.Card, spriteName);
        }

        /**
        获得统计UI卡片的底色
        */
        public static Task<Sprite> GetStatQualityBg(string qualityGroup, int quality)
        {
            string spriteName = qualityGroup.Replace("{quality}", quality.ToString());
            return SpriteManager.GetSprite(AtlasNames.Card, spriteName);
        }



        // 获得道具图片（图集：PropIcon）
        public static Task<Sprite> GetPropIcon(int id)
        {
            return SpriteManager.GetSprite(AtlasNames.PropIcon, id.ToString());
        }
        public static Task<Sprite> GetPropIcon(string idstr)
        {
            return SpriteManager.GetSprite(AtlasNames.PropIcon, idstr);
        }

        public static Task<Sprite> GetResourcesIcon(string name)
        {
            return SpriteManager.GetSprite(AtlasNames.PropIcon, name.ToString());
        }

        // 获得技能图标
        public static Task<Sprite> GetSkillIcon(int id)
        {
            return SpriteManager.GetSprite(AtlasNames.Skill, id.ToString());
        }

        // 获得技能图标
        public static Task<Sprite> GetSkillIcon(string id)
        {
            return SpriteManager.GetSprite(AtlasNames.Skill, id);
        }

        // 获得技能状态图标
        public static Task<Sprite> GetSkillStateImage(int state)
        {
            return SpriteManager.GetSprite(AtlasNames.Skill, "State_" + state);
        }
        //获得最佳位置圆片图
        public static Task<Sprite> GetBestPositionImage(int pos)
        {
            return SpriteManager.GetSprite(AtlasNames.Player, "img_" + pos);
        }
        // 获得仓库物品质量图片
        public static Task<Sprite> GetInvetoryQuality(int quality)
        {
            return SpriteManager.GetSprite(AtlasNames.Inventory, SpriteNames.Inventory.Quality + quality);
        }

        // 图集：League；获得比赛结果图片
        public static Task<Sprite> GetGameResult(int type)
        {
            return SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.GameResult + type);
        }

        // 获得进球数图片
        public static Task<Sprite> GetGoal(int number)
        {
            return SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.Goal + number);
        }

        // 获得排名图片
        public static Task<Sprite> GetRank(int rank)
        {
            return SpriteManager.GetSprite(AtlasNames.League, SpriteNames.League.RankImg + rank);
        }

        // 获得商城物品图标
        public static Task<Sprite> GetShopItem(int shopItemID)
        {
            return SpriteManager.GetSprite(AtlasNames.Shop, shopItemID.ToString());
        }

        //获取强化图标
        public static Task<Sprite> GetStrengthenIcon(string strengthenType)
        {
            return SpriteManager.GetSprite(AtlasNames.Strengthen, strengthenType);
        }

        public static Task<Sprite> GetLastGameScoreBoardBG(bool hasScore)
        {
            if (hasScore) return SpriteManager.GetSprite(AtlasNames.Home, "img_733");
            else return SpriteManager.GetSprite(AtlasNames.Home, "img_732_2");
        }

        public static Task<Sprite> GetHomeIcon(string iconid)
        {
            return SpriteManager.GetSprite(AtlasNames.Home, iconid);
        }

        public static Task<Sprite> GetSoccerFieldAreaSprite(string areaName, bool isHighLight)
        {
            if (isHighLight)
            {
                return SpriteManager.GetSprite(AtlasNames.Formation, areaName + "_2");
            }
            else
            {
                return SpriteManager.GetSprite(AtlasNames.Formation, areaName);
            }
        }

        // 获得邀请赛队徽
        public static async Task<Sprite> GetInviteOrganizerSprite(string icon)
        {

            var sprite = await SpriteManager.GetSprite(AtlasNames.Invitation, icon, (Sprite)null);
            if (sprite == null)
            {
                return await GetClubIcon(icon);
            }
            return sprite;
        }

        // 从1开始
        public static Task<Sprite> GetMainTaskTab(int index)
        {
            return SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.MainTaskTab + index);
        }

        // 从1开始
        public static Task<Sprite> GetMainTaskBG(int index)
        {
            return SpriteManager.GetSprite(AtlasNames.Task, SpriteNames.Task.MainTaskBG + index);
        }

        public static Task<Sprite> GetAchievementIcon(int icon)
        {
            return SpriteManager.GetSprite(AtlasNames.Achievement, icon.ToString());
        }


        public static Task<Sprite> GetArenaTierIcon(string icon)
        {
            return SpriteManager.GetSprite(AtlasNames.Arena, icon);
        }

        public static Task<Sprite> GetArenaRankIcon(int rank)
        {
            return SpriteManager.GetSprite(AtlasNames.Arena, "rank_" + rank);
        }

        public static Task<Sprite> GetArenaTitleIcon(int rank)
        {
            return SpriteManager.GetSprite(AtlasNames.Arena, "title_" + rank);
        }


        // 获得NPC球员头像（图集：Npc)
        public static Task<Sprite> GetNpcPortrait(int id)
        {
            return SpriteManager.GetSprite(AtlasNames.Npc, id.ToString());
        }

        public static Task<Sprite> GetColorfulStar()
        {
            return SpriteManager.GetSprite(AtlasNames.Player, "img_168_2");
        }
        public static Task<Sprite> GetYellowStar()
        {
            return SpriteManager.GetSprite(AtlasNames.Player, "img_168_1");
        }

        public static Task<Sprite> GetGiftSkillImg(GiftSkillConfig cfg)
        {
            return SpriteManager.GetSprite(AtlasNames.PropIcon, cfg.Icon);

            //if (cfg.When == FActionTimeType.OnBattleWith)
            //{
            //    return SpriteManager.GetSprite(AtlasNames.PropIcon, "906");
            //}
            //else if (cfg.Action == FActionType.ForceOff)
            //{
            //    return SpriteManager.GetSprite(AtlasNames.PropIcon, "903");
            //}
            //else if (cfg.When == FActionTimeType.OnOff)
            //{
            //    return SpriteManager.GetSprite(AtlasNames.PropIcon, "910");
            //}
            //else if (cfg.Action == FActionType.AddBuff || cfg.Action == FActionType.AddProp)
            //{
            //    if (cfg.Side == (int)FActionObjectSide.Our)
            //    {
            //        return SpriteManager.GetSprite(AtlasNames.PropIcon, "911");
            //    }
            //    else
            //    {
            //        return SpriteManager.GetSprite(AtlasNames.PropIcon, "912");
            //    }
            //}
            //return null;
        }

        #region 月卡样式
        public static Task<Sprite> GetMonthCardImageStyle(string name)
        {
            return SpriteManager.GetSprite(AtlasNames.MonthCard, name);
        }

        #endregion

        #region 活动底图
        public static Task<Sprite> GetActivityImage(string activityBgName)
        {
            return SpriteManager.GetSprite(AtlasNames.Activity, activityBgName);
        }
        #endregion

        #region 竞技场徽章
        public static Task<Sprite> GetBadge(int stage)
        {
            return SpriteManager.GetSprite(AtlasNames.Arena, "badge" + stage);
        }
        #endregion

        #region 常规赛（新版推图）
        public static Task<Sprite> GetCountryFlag(string flagIconName)
        {
            return SpriteManager.GetSprite(AtlasNames.CountryFlag, flagIconName);
        }
        public static Task<Sprite> GetMapFlag(string flagIconName)
        {
            return SpriteManager.GetSprite(AtlasNames.WorldMap, flagIconName);
        }
        #endregion

        #region 剧情推图
        public static Task<Sprite> GetHeroIcon(string heroIconName)
        {
            return SpriteManager.GetSprite(AtlasNames.HeroIcon, heroIconName);
        }
        #endregion


        #region 悬赏任务
        public static Task<Sprite> GetBountyTaskBadge(string heroIconName)
        {
            return SpriteManager.GetSprite(AtlasNames.Bounty, heroIconName);
        }
        #endregion

        #region 战斗中球员爆发时的火焰
        public static Task<Sprite> GetBattle2CardFire(int quality, int index)
        {
            return SpriteManager.GetSprite(AtlasNames.Battle, "BattleCardFire" + quality + index);
        }
        #endregion

        #region 限时抽卡活动

        public async static Task<Sprite> GetActivityRecruitSprite(string iconName, bool useDefault = false)
        {
            var sprite = await SpriteManager.GetSprite(AtlasNames.ActivityRecruit, iconName, (Sprite)null);
            if (useDefault)
            {
                if (sprite == null)
                {
                    return await SpriteManager.GetSprite(AtlasNames.ActivityRecruit, "default");
                }
            }
            return sprite;
        }

        public async static Task<Sprite> GetActivityRecruitHomeSprite(string iconName)
        {
            var sprite = await SpriteManager.GetSprite(AtlasNames.ActivityRecruit, iconName + "_home", (Sprite)null);
            if (sprite == null)
            {
                Debug.LogWarning("SpriteProxy , GetActivityRecruitHomeSprite , sprite == null , iconName = " + iconName);
                return await None;
            }
            return sprite;
        }

        #endregion

        #region 巅峰球员
        public static async Task<Sprite> GetPeakImage(string peakLogoName)
        {
            return await SpriteManager.GetSprite(AtlasNames.Peak, peakLogoName, await None);
        }
        #endregion

        #region

        public static Task<Sprite> GetNFTTitleSprite(int icon)
        {
            return SpriteManager.GetSprite(AtlasNames.NFTTitle, icon.ToString());
        }

        #endregion

        #region 荣誉室

        public static async Task<Sprite> GetHonourCup(int icon)
        {
            return await SpriteManager.GetSprite(AtlasNames.HonourCup, icon.ToString(), await DefaultHonourCup);
        }

        #endregion

        #region 2024季后赛总决赛竞猜

        public static Task<Sprite> GetPlayoffFinalsGuessMVPPlayerSprite(int icon)
        {
            return SpriteManager.GetSprite(AtlasNames.PlayoffFinalsGuessMVPPlayer, icon.ToString());
        }
        public static Task<Sprite> GetPlayoffFinalsGuessMVPTeamSprite(int icon)
        {
            return SpriteManager.GetSprite(AtlasNames.PlayoffFinalsGuessMVPTeam, icon.ToString());
        }
        public static Task<Sprite> GetPlayoffFinalsGuessEndTeamLogoSprite(int icon)
        {
            return SpriteManager.GetSprite(AtlasNames.PlayoffFinalsGuessEndTeamLogo, icon.ToString());
        }
        public static Task<Sprite> GetPlayoffFinalsGuessEndTeamPlayerSprite(int icon)
        {
            return SpriteManager.GetSprite(AtlasNames.PlayoffFinalsGuessEndTeamPlayer, icon.ToString());
        }

        #endregion

    }
}
