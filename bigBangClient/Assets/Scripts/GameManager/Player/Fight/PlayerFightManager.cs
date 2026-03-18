using Protocol;

namespace BigBang
{
    public class PlayerFightManager : BaseManager
    {
        public FormationController FormationController { get; set; }
        public FightDataController FightDataController { get; set; }

        public PlayerFightManager()
        {
            FormationController = new FormationController(this);
            FightDataController = new FightDataController(this);
        }

        public void Init()
        {
            FormationController.Init();
        }

        public void UnPack(ModuleFightInfo data)
        {
            FormationController.UnPack(data.FormationController);
        }

        public void LoginSuccess()
        {
            FormationController.LoginSuccess();
        }
    }
}