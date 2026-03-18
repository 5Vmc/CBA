
namespace Babu
{
    public static class RemoteLoadPath
    {
        public static void SetLoadPath(string newLoadPath)
        {
            loadPath = newLoadPath;
        }
        private static string loadPath = "Unknow";
        public static string LoadPath
        {
            get
            {
                return loadPath;
            }
        }
    }
}