namespace BigBang
{
    public enum FormationTempType
    {
        System = 1,
        Custom,
    }

    public class FormationTempBase
    {
        public int TempId;
        public string Name;
        public FormationTempType Type;
        public long CreateTime;

        public FormationTempBase()
        {
        }

        public string GetNewName()
        {
            return $"{Name}a";
        }
    }
}