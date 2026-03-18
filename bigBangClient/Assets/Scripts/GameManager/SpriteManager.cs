using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

namespace BigBang
{
    public static class SpriteManager
    {
        private const string suffix = ".spriteatlas";
        private static Dictionary<string, SpriteAtlas> atlas = new Dictionary<string, SpriteAtlas>();

        // 返回值:加载耗时(单位：秒)
        public static async Task<float> LoadAsync()
        {
            var startTime = DateTime.Now;
            var handles = new List<AssetOperationHandle>();
            var tasks = new List<Task>();
            // 加载所有图集
            var type = typeof(AtlasNames);
            // 添加加载异步任务
            foreach (var item in type.GetFields())
            {
                Debug.Log("开始加载图集：" + item.Name);
                var h = YooAssets.LoadAssetAsync<SpriteAtlas>(ResourcePath.SpriteAtlasPath + item.Name + suffix);
                handles.Add(h);
                tasks.Add(h.Task);
            }
            // 等待所有异步任务完成
            await Task.WhenAll(tasks);
            Debug.Log("图集加载完成");
            // 保存结果
            foreach (var item in type.GetFields().Zip(handles, (field, h) => (field.Name, h.AssetObject as SpriteAtlas)))
            {
                if (item.Item2 == null)
                {
                    Debug.LogWarning($"LoadAsync , item.Result == null,图集丢失:AtlasName={item.Name}");
                    continue;
                }
                atlas.Add(item.Name, item.Item2);
            }
            return (float)(DateTime.Now - startTime).TotalSeconds;
        }

        // 返回值:加载耗时(单位：秒)
        public static float Load()
        {
            var startTime = DateTime.Now;
            // 加载所有图集
            var type = typeof(AtlasNames);
            foreach (var item in type.GetFields())
            {
                Debug.Log("加载图集：" + item.Name);
                var handle = YooAssets.LoadAssetSync<SpriteAtlas>(ResourcePath.SpriteAtlasPath + item.Name + suffix);
                var spriteAtlas = handle.AssetObject as SpriteAtlas;
                if (spriteAtlas == null)
                {
                    Debug.LogError($"spriteAtlas == null , 图集{item.Name}加载失败");
                }
                if (atlas.ContainsKey(item.Name))
                {
                    Debug.LogError($"atlas.ContainsKey(item.Name) , 图集{item.Name}重复加载");
                }
                else
                {
                    Debug.Log($"图集{item.Name}加载完成");
                    if (spriteAtlas == null)
                    {
                        Debug.LogWarning($"Load , spriteAtlas == null,图集丢失:AtlasName={item.Name}");
                        continue;
                    }
                    atlas.Add(item.Name, spriteAtlas);
                }
                // Addressables.Release(handle);//此处释放会导致暂时没有引用到的图集被释放掉
            }
            DebugPrintAtlas();
            return (float)(DateTime.Now - startTime).TotalSeconds;
        }

        public static void UnloadAll()
        {
            atlas.Clear();
        }


#if !UNITY_WEBGL
        static Task<SpriteAtlas> LoadSpriteAtlas(string atlasName)
        {
            if (!atlas.ContainsKey(atlasName))
            {
                Debug.Log("加载图集：" + atlasName);
                var handle = YooAssets.LoadAssetSync<SpriteAtlas>(ResourcePath.SpriteAtlasPath + atlasName + suffix);
                var spriteAtlasNow = handle.AssetObject as SpriteAtlas;
                handle.Release();
                if (spriteAtlasNow == null)
                {
                    Debug.LogWarning($"spriteAtlasNow == null,图集丢失:AtlasName={atlasName}");
                    return Task.FromResult<SpriteAtlas>(null);
                }
                atlas.Add(atlasName, spriteAtlasNow);
                return Task.FromResult(spriteAtlasNow);
            }
            else
            {
                return Task.FromResult(atlas[atlasName]);
            }
        }
#else
        static async Task<SpriteAtlas> LoadSpriteAtlas(string atlasName)
        {
            if (!atlas.ContainsKey(atlasName))
            {
                Debug.Log("加载图集：" + atlasName);
                var handle = YooAssets.LoadAssetAsync<SpriteAtlas>(ResourcePath.SpriteAtlasPath + atlasName + suffix);
                await handle.Task;
                var spriteAtlasNow = handle.AssetObject as SpriteAtlas;
                handle.Release();
                if (spriteAtlasNow == null)
                {
                    Debug.LogWarning($"spriteAtlasNow == null,图集丢失:AtlasName={atlasName}");
                    return null;
                }
                //atlas.Add(atlasName, spriteAtlasNow);
                atlas[atlasName] = spriteAtlasNow;
                return spriteAtlasNow;
            }
            else
            {
                return atlas[atlasName];
            }
        }
#endif
#if UNITY_WEBGL
        private static Dictionary<string, string> dict = new Dictionary<string, string>()
        {
            //默认路径
            ["Achievement"] = "Icons",
            //["Activity"] = "",
            //["ActivityRecruit"] = "",
            ["Arena"] = "tier",
            ["Bounty"] = "badge",
            //["Card"] = "",
            //["CardUp"] = "",
            //["CardYellowBg"] = "",
            //["Challenge"] = "",
            //["ClubIcon"] = "",
            //["CountryFlag"] = "",
            //["Email"] = "",
            //["FightPoint"] = "",
            //["Formation"] = "",
            //["Guide"] = "",
            ["HeroChapter"] = "HeroIcon",
            //["Home"] = "",
            //["Inventory"] = "",
            ["Invitation"] = "ICON",
            //["MonthCard"] = "",
            //["Npc"] = "",
            //["Player"] = "",
            //["Portrait"] = "",
            //["PortraitYellow"] = "",
            //["PropIcon"] = "",
            ["Scout"] = "Recruit",
            //["Shoot"] = "",
            ["Shop"] = "ItemIcon",
            ["Skill"] = "SkillIcon",
            //["Strengthen"] = "",
            //["Task"] = "",
            ["TrainUI"] = "UpgradeSpeed",
            //["Unlock"] = "",
            //["UnlockTrain"] = "",
            //["WorldMap"] = "",
            //--------------------------------------
            //前缀路径
            //Activity
            ["train"] = "OtherPlayerTeam",
            //Arena
            ["badge"] = "ArenaEndReward",
            //Battle
            ["BattleCardFire"] = "",
            ["StageFire"] = "StageFire",
            //Card
            ["Back"] = "Back",
            ["Background"] = "Background",
            ["BackStar"] = "BackStar",
            ["Border"] = "Border",
            ["DebrisBackground"] = "DebrisBackground",
            ["DebrisBorder"] = "DebrisBorder",
            ["DebrisEdge"] = "DebrisEdge",
            ["DebrisLight"] = "DebrisLight",
            ["FormationBench"] = "FormationBench",
            ["FormationMain"] = "FormationMain",
            ["icon"] = "Icon",
            ["SmallQualityCard"] = "SmallQualityCard",
            ["SmallQualityL2H"] = "SmallQualityCard",
            ["Square"] = "Square",
            ["Stat"] = "Stat",
            //CardUp
            ["pos"] = "CardUpCommon",
            //["banner"] = "CardUpStar",
            ["CardLevel"] = "球员框LV数字",
            ["条"]= "CardUpStar",
            ["CardStage"] = "阶段数字",
            //Email
            ["email"] = "Sequence",
            ["Light"] = "",
            //Inventory
            ["Quality"] = "Quality",
            //League
            ["GameResult"] = "GameResult",
            ["Goal"] = "Goal",
            ["Number"] = "Number",
            //Player
            ["state"] = "State",
            //Shop
            ["train"] = "",
            //Skill
            ["quality"] = "SkillQuality",
            ["State"] = "StateIcon",
            //Task
            ["bg"] = "MainTask",
            ["tab"] = "MainTask",
            //直接索引
            //MonthCard
            ["off_1"] = "normal",
            ["reward_bg_1"] = "normal",
            ["reward_bg_2"] = "super",
            ["off_2"] = "super",
            //Player
            ["img_168_1"] = "",
            ["img_168_2"] = "",
            ["img_700"] = "",
            ["img_701"] = "",
            ["img_702"] = "",
            //Skill
            ["img_229"] = "StateIcon",
            //State
            ["close"] = "State",
            ["obtain"] = "State",
            ["open"] = "State",
            ["btn_9_5"] = "",
            //CardUp
            ["CardStage阶"]= "阶段数字",
            ["LV"] = "球员框LV数字",
            ["背光"] = "阶段数字",
            //Shoot
            ["ShootTime："]="",
            //Card
            ["Light1"] = "Light",
            ["Light2"] = "Light",
            ["Light3"] = "Light",
            ["Light4"] = "Light",
            ["Light5"] = "Light",
        };
#endif
        public static async Task<Sprite> GetSprite(string atlasName, string spriteName)
        {

#if UNITY_WEBGL
            try
            {
                StringBuilder path = new StringBuilder("Sprites/Sprite/");
                if (dict.ContainsKey(spriteName))
                {
                    //存在前缀或直接索引
                    path.Append(atlasName + (string.IsNullOrEmpty(dict[spriteName]) ? "/" : ("/" + dict[spriteName] + "/")));
                }
                else
                {
                    //判断是否是正常字母与最后数字组合
                    Regex reg = new Regex(".*?(?=[0-9%+._])");
                    string match = reg.Match(spriteName).Value;
                    //不是字母数字拼接的名字。直接名字查找,是字母数字拼接的去除最后一个字符查找路径
                    string key = string.IsNullOrEmpty(match) ? spriteName : match;
                    if (dict.ContainsKey(key))
                    {
                        //存在前缀或直接索引
                        path.Append(atlasName + (string.IsNullOrEmpty(dict[key]) ? "/" : ("/" + dict[key] + "/")));
                    }
                    else
                    {
                        //不存在前缀或直接索引路径，走默认路线
                        if (!dict.ContainsKey(atlasName))
                        {
                            //不存在默认路径则直接一级文件夹找寻
                            path.Append(atlasName + "/");
                        }
                        else
                        {
                            //存在默认路径
                            path.Append(atlasName + (string.IsNullOrEmpty(dict[atlasName]) ? "/" : ("/" + dict[atlasName] + "/")));
                        }
                    }
                }
                //Debug.LogError($"图集{atlasName}-图片{spriteName}-路径{path}");
                var handle = YooAssets.LoadAssetAsync<Sprite>(path + $"{spriteName}.png");
                await handle.Task;
                Sprite sprite = handle.AssetObject as Sprite;
                return sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"图片丢失:AtlasName={atlasName},SpriteName={spriteName}," + ex);
                return await GetSprite(AtlasNames.Public,"error");
            }
#else
            try
            {
                var atlas = await LoadSpriteAtlas(atlasName);
                if (atlas == null)
                {
                    return await GetErrorSprite();
                }

                Sprite sprite = atlas.GetSprite(spriteName);
                if (sprite == null)
                {
                    Debug.LogWarning($"sprite == null,图片丢失:AtlasName={atlasName},SpriteName={spriteName}");
                    return await GetErrorSprite();
                }
                return sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"图集或图片丢失:AtlasName={atlasName},SpriteName={spriteName}," + ex);
                return await GetErrorSprite();
            }
#endif
        }

        public static async void GetSprite(string atlasName, string spriteName, Action<Sprite> callback)
        {
            var sprite = await GetSprite(atlasName, spriteName);
            callback(sprite);
        }

        private static void DebugPrintAtlas()
        {
#if USER_DEBUG
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("加载到的图集 , atlas:\n");
            foreach (var item in atlas)
            {
                stringBuilder.Append("item.Key=" + item.Key + " , item.Value=" + item.Value + "\n");
            }
            Debug.Log(stringBuilder.ToString());
#endif
        }

        public static async Task<Sprite> GetSprite(string atlasName, string spriteName, Sprite defaultSprite)
        {
#if UNITY_WEBGL

            try
            {
                Sprite sprite = await GetSprite(atlasName, spriteName);
                if (sprite == null)
                {
                    return defaultSprite;
                }
                return sprite;
            }
            catch(Exception ex)
            {
                return defaultSprite;
            }
#else
            try
            {
                var atlas = await LoadSpriteAtlas(atlasName);
                if (atlas == null)
                {
                    return defaultSprite;
                }

                Sprite sprite = atlas.GetSprite(spriteName);
                if (sprite == null)
                {
                    return defaultSprite;
                }
                return sprite;
            }
            catch (Exception _)
            {
                return defaultSprite;
            }
#endif
        }

        public static async Task<Sprite> GetErrorSprite()
        {
            await LoadSpriteAtlas(AtlasNames.Public);
            return atlas[AtlasNames.Public].GetSprite(SpriteNames.Public.Error);
        }

    }
}