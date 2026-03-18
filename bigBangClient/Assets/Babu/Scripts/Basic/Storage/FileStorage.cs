namespace Babu
{
    abstract class FileStorage : Storage
    {
        protected abstract string GetStoragePath();

        public override bool Exists(string file)
        {
            string path = GetStoragePath() + "/" + file;
            return FileUtils.Exists(path);
        }

        public override void CreateDirectory(string dir)
        {
            string path = GetStoragePath() + "/" + dir;
            FileUtils.CreateDirectory(path);
        }

        public override string Load(string file)
        {
            string path = GetStoragePath() + "/" + file;
            if(FileUtils.Exists(path))
                return FileUtils.ReadFile(path);
            else
                return null;
        }

        public override byte[] LoadBytes(string file)
        {
            string path = GetStoragePath() + "/" + file;
            return FileUtils.ReadFileBytes(path);
        }

        public override void Store(string file, string data)
        {
            string path = GetStoragePath() + "/" + file;
            FileUtils.WriteFile(path, data);
        }

        public override void StoreBytes(string file, byte[] data)
        {
            string path = GetStoragePath() + "/" + file;
            FileUtils.WriteFileBytes(path, data);
        }
    }
}
