using GameConfig;
using GameConfig.Config;
using Protocol;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang
{
    public class GoodsData
    {
        public int Id { get; set; }
        public GoodsConfig Config { get; set; }

        public bool IsNew { get; set; }
        public int Count
        {
            get => _count.Count;
        }
        private Resource _count { get; set; }

        public GoodsData(int id, int count, bool isNew = false)
        {
            Id = id;
            _count = count;
            IsNew = isNew;
            Config = Configs.Goods.GetConfig(id);
        }

        public void UnPack(Goods goods)
        {
            _count = goods.Count;
            IsNew = goods.IsNew;
        }

        public void AddCount(int num)
        {
            _count.AddCount(num);
        }

        public void DelCount(int num)
        {
            _count.DelCount(num);
        }

        public bool IsEnough(int num)
        {
            return _count.IsEnough(num);
        }

        public override string ToString()
        {
            return $"{Config.Name} * {Count}";
        }

        public GameItem ToGameItem()
        {
            return GameItemUtils.CreateGameItem(GameItemType.Goods, Id, Count);
        }
    }
}