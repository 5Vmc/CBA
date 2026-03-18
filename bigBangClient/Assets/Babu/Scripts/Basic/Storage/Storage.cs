namespace Babu
{
    abstract class Storage
    {
        public abstract bool Exists(string file);
        public abstract void CreateDirectory(string dir);

        public abstract void Store(string file, string data);
        public abstract void StoreBytes(string file, byte[] data);

        public abstract string Load(string file);
        public abstract byte[] LoadBytes(string file);
    }
}
