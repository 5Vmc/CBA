using System.Collections.Generic;
using System.Linq;
using BigBang;
using GameConfig;
using TMPro;

namespace BigBang.UI
{
    public class DropdownMaker
    {
        public static async void SetData(TMP_Dropdown dropdown, List<string> descList)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            int index = 0;
            foreach (var item in descList)
            {
                var img = await SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.Dropdown + (index % 2));
                var optionData = new TMP_Dropdown.OptionData(item, img);
                options.Add(optionData);
                index++;
            }
            dropdown.AddOptions(options);
        }

        public static void SetData(TMP_Dropdown dropdown, int type, string specialText = "")
        {
            var descList = GetDescList(type);
            if (specialText != "") 
                descList.Add(specialText);
            SetData(dropdown, descList);
        }

        private static List<string> GetDescList(int type)
        {
            return Configs.Options.GetConfigList()
                .Where(item => item.Type == type)
                .Select(item => item.Content).ToList<string>();
        }

        public static int GetOptionValueByType(int type, int index)
        {
            var table = Configs.Options.GetConfigList();
            var option = table.Where(item => item.Type == type).ElementAtOrDefault(index);
            return option == null ? -1 : option.Value;
        }
    }
}