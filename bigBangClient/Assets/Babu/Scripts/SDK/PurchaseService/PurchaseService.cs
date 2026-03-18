using System;

namespace Babu.SDK
{

    public class PurchaseInfo
    {
        private string _goodsId;
        private string _goodsName;
        private double _amount;
        private string _serverId;
        private string _serverName;
        private string _roleId;
        private string _roleName;
        private string _extrasParam;

        private int _shopItemId;

        private int _gameRoleLevel;
        private int _gameRolePower;
        private int _createTime;

        public PurchaseInfo(string goodsId, string goodsName, double amount, string svrId, string svrName, string roleId, string roleName, string extrasParam, int shopItemId)
        {
            this._goodsId = goodsId;
            this._goodsName = goodsName;
            this._amount = amount;
            this._serverId = svrId;
            this._serverName = svrName;
            this._roleId = roleId;
            this._roleName = roleName;
            this._extrasParam = extrasParam;
            this._shopItemId = shopItemId;
        }
        public PurchaseInfo(string goodsId, string goodsName, double amount, string svrId, string svrName, string roleId, string roleName, string extrasParam, int shopItemId, int gameRoleLevel, int gameRolePower, int createTime)
        {
            this._goodsId = goodsId;
            this._goodsName = goodsName;
            this._amount = amount;
            this._serverId = svrId;
            this._serverName = svrName;
            this._roleId = roleId;
            this._roleName = roleName;
            this._extrasParam = extrasParam;
            this._shopItemId = shopItemId;
            this._gameRoleLevel = gameRoleLevel;
            this._gameRolePower = gameRolePower;
            this._createTime = createTime;
        }

        public string GoodsId
        {
            get { return this._goodsId; }
            private set { }
        }

        public string GoodsName
        {
            get { return this._goodsName; }
            private set { }
        }

        public double Amount
        {
            get { return this._amount; }
            private set { }
        }

        public string ServerId
        {
            get { return this._serverId; }
            private set { }
        }

        public string ServerName
        {
            get { return this._serverName; }
            private set { }
        }

        public string RoleId
        {
            get { return this._roleId; }
            private set { }
        }

        public string RoleName
        {
            get { return this._roleName; }
            private set { }
        }

        public String ExtraParams
        {
            get{ return this._extrasParam;}
            private set {}
        }

        public int ShopItemId
        {
            get { return this._shopItemId; }
            private set { }
        }

        public int GameRoleLevel
        {
            get { return this._gameRoleLevel; }
            private set { }
        }

        public int GameRolePower
        {
            get { return this._gameRolePower; }
            private set { }
        }

        public int CreateTime
        {
            get { return this._createTime; }
            private set { }
        }

    }
    public abstract class PurchaseService
    {
        public abstract void Init(string[] productIdList);
        public abstract void Purchase(PurchaseInfo info);
    }
}
