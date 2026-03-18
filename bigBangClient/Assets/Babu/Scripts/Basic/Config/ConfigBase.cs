using System.IO;

namespace Babu.Config
{
    public interface IConfig
    {
        int Id { get; set; }
        abstract void LoadFromBinary(BinaryReader binaryReader);
    }

    public abstract class ConfigBase : IConfig
    {
        public int Id { get; set; }

        public abstract void LoadFromBinary(BinaryReader binaryReader);
    }
}