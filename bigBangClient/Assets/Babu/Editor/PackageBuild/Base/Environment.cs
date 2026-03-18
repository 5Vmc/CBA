using LightJson;
using System.IO;

namespace Babu.Editor.Build
{
    public abstract class Environment
    {
        protected JsonValue environmentJson;
        private bool _loaded = false;

        protected void Load()
        {
            FileUtils.CreateDirectory("Assets/Resources");
            if (FileUtils.Exists("Assets/Resources/Environment.json") == false)
            {
                FileUtils.CreateFile("Assets/Resources/Environment.json");
                FileUtils.WriteFile("Assets/Resources/Environment.json", "{}");
            }

            string content = FileUtils.ReadFile("Assets/Resources/Environment.json");
            environmentJson = JsonValue.Parse(content);
            _loaded = true;
        }

        protected void Write()
        {
            FileUtils.WriteFile("Assets/Resources/Environment.json", environmentJson.ToString(false));
        }

        protected void SaveEnvironment(JsonValue jsonValue, string prefix)
        {
            if (_loaded == false)
            {
                Load();
            }

            foreach (var iter in jsonValue.AsJsonObject)
            {
                environmentJson[prefix + "_" + iter.Key] = iter.Value;
            }

            Write();
        }
    }
}
