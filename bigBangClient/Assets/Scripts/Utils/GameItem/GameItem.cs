using BigBang;
using BigBang.UI;
using GameConfig;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.GameItem
{
    public abstract class GameItem
    {
        public GameItemType Type { get; set; }
        public int Id { get; set; }
        public int Count { get; set; }
        public string Desc { get; set; }

        public abstract string CountString();

        public GameItem(GameItemType type, int id, int count)
        {
            Type = type;
            Id = id;
            Count = count;
        }

        public abstract Task<Sprite> GetIcon();

        //获得道具名称
        public abstract string GetName();

        //获得当前数量
        public abstract int GetPlayerCount();

        //获得物品描述
        public abstract string GetDescription();

        public abstract int GetQuality();

        //获得途径
        public int[] GetWay()
        {
            var cfg = Configs.WayOfGain.GetConfig((int)Type * 1000000 + Id);
            if (cfg is null) return null;
            return cfg.Way;
        }

        public abstract void ShowTip();
    }

    public class GoodsGameItem : GameItem
    {

        public GoodsGameItem(int id, int count) : base(GameItemType.Goods, id, count) { }

        public override string CountString()
        {
            return Count.ToString();
        }

        public override string GetDescription()
        {
            var cfg = Configs.Goods.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return cfg.Desc;
        }

        public override Task<Sprite> GetIcon()
        {
            var cfg = Configs.Goods.GetConfig(Id);
            if (cfg == null) return SpriteProxy.Error;
            return SpriteProxy.GetPropIcon(cfg.Icon);
        }

        public override string GetName()
        {
            var cfg = Configs.Goods.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return cfg.Name;
        }

        public override int GetPlayerCount()
        {
            return Player.PackageManager.GetGoodsNumber(Id);
        }

        public override int GetQuality()
        {
            var cfg = Configs.Goods.GetConfig(Id);
            if (cfg == null) return QualityType.Green;
            return cfg.Quality;
        }

        public override void ShowTip()
        {
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(this));
        }
    }

    public class CardGameItem : GameItem
    {
        public CardGameItem(int id, int count) : base(GameItemType.Card, id, count) { }

        public override string CountString()
        {
            return Count.ToString();
        }

        public override string GetDescription()
        {
            var cfg = Configs.CardModel.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return cfg.Desc;
        }

        public override Task<Sprite> GetIcon()
        {
            return SpriteProxy.GetPropCard(Id);
        }

        public override string GetName()
        {
            var cfg = Configs.CardModel.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return PlayerCard.GetFullName(cfg);
        }

        public override int GetPlayerCount()
        {
            var card = Player.CardManager.GetCard(Id);
            if (card == null) return 0;
            return 1;
        }

        public override int GetQuality()
        {
            var cfg = Configs.CardModel.GetConfig(Id);
            if (cfg == null) return QualityType.Green;
            return cfg.Quality;
        }

        public override void ShowTip()
        {
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(this));
        }
    }

    public class ResourceGameItem : GameItem
    {
        public ResourceGameItem(int id, int count) : base(GameItemType.Resource, id, count) { }

        public override string CountString()
        {
            if (Id == ResourceId.TrainExpMin)
            {
                return Count.ToString() + "'";
            }
            return Count.ToString();
        }

        public override string GetDescription()
        {
            var cfg = Configs.Goods.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return cfg.Desc;
        }

        public override Task<Sprite> GetIcon()
        {
            if (Id == ResourceId.Diamond)
            {
                return SpriteManager.GetSprite(AtlasNames.PropIcon, SpriteNames.PropIcon.DiamondImg);
            }
            if (Id == ResourceId.Money)
            {
                return SpriteManager.GetSprite(AtlasNames.PropIcon, SpriteNames.PropIcon.EuroImg);
            }
            if (Id == ResourceId.TrainExpMin)
            {
                return SpriteManager.GetSprite(AtlasNames.PropIcon, SpriteNames.PropIcon.ExpImg);
            }
            if (Id == ResourceId.PlayerExp)
            {
                return SpriteManager.GetSprite(AtlasNames.PropIcon, SpriteNames.PropIcon.PlayerExp);
            }
            if (Id == ResourceId.HeroExp)
            {
                return SpriteManager.GetSprite(AtlasNames.PropIcon, SpriteNames.PropIcon.CardExp);
            }
            if (Id == ResourceId.Energy)
            {
                return SpriteManager.GetSprite(AtlasNames.PropIcon, SpriteNames.PropIcon.PlayerEnergy);
            }
            return SpriteProxy.Error;
        }

        public override string GetName()
        {
            var cfg = Configs.Goods.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return cfg.Name;
        }

        public override int GetPlayerCount()
        {
            return Player.PackageManager.GetResourceCount(Id);
        }

        public override int GetQuality()
        {
            var cfg = Configs.Goods.GetConfig(Id);
            if (cfg == null) return QualityType.Green;
            return cfg.Quality;
        }

        public override void ShowTip()
        {
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(this));
        }
    }

    public class HonourGameItem : GameItem
    {
        public HonourGameItem(int id, int count) : base(GameItemType.Honour, id, count) { }

        public override string CountString()
        {
            return Count.ToString();
        }

        public override string GetDescription()
        {
            var cfg = Configs.Achievement.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return cfg.Desc;
        }

        public override Task<Sprite> GetIcon()
        {
            var cfg = Configs.Achievement.GetConfig(Id);
            if (cfg == null) return SpriteProxy.Error;
            return SpriteProxy.GetHonourCup(cfg.Icon);
        }

        public override string GetName()
        {
            var cfg = Configs.Achievement.GetConfig(Id);
            if (cfg == null) return string.Empty;
            return cfg.Name;
        }

        public override int GetPlayerCount()
        {
            AchievementData achievementData = Player.AchievementManager.GetAchievementData(Id);
            return achievementData == null ? 0 : achievementData.HonourCurrentShow;
        }

        public override int GetQuality()
        {
            return QualityType.Red;
        }

        public override void ShowTip()
        {
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(this));
        }
    }
}