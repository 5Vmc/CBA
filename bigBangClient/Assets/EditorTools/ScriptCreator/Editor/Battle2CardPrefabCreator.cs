using System;
using BigBang.Battle;
using TMPro;
using UnityEditor;
using UnityEngine;
using Utils;

public class Battle2CardPrefabCreator
{

    private static string battle2CardPrefabPath = "Assets/LocalAsset/Lanqiu/_Resources/Prefabs/Zhandou_ka.prefab";

    private static string prefabChineseName = "战斗2卡片prefab";
    [MenuItem("工具/3D/战斗2/处理Battle2卡片.prefab")]
    public static void ProcessBattle2CardPrefab()
    {
        try
        {

            Debug.Log("开始处理" + prefabChineseName);

            //加载Prefab
            GameObject battle2CardGameObject = AssetDatabase.LoadAssetAtPath(battle2CardPrefabPath, typeof(GameObject)) as GameObject;
            if (battle2CardGameObject == null)
            {
                Debug.LogError("找不到" + prefabChineseName);
                Debug.LogError("battle2CardPrefabPath = " + battle2CardPrefabPath);
                return;
            }

            //设置层级
            battle2CardGameObject.SetLayerInThisAndAllChild(Layers.Battle2);

            //添加脚本
            BattleCardItem battleCardItem = battle2CardGameObject.GetComponent<BattleCardItem>();
            if (battleCardItem == null)
            {
                battleCardItem = battle2CardGameObject.AddComponent<BattleCardItem>();
            }

            //设置脚本引用
            Transform battle2CardTrans = battle2CardGameObject.transform;
            battleCardItem.MoveParent = battle2CardTrans.Find("MoveParent");
            battleCardItem.BattleCardBg = battleCardItem.MoveParent.Find("BattleCardBg").GetComponent<SpriteRenderer>();
            battleCardItem.PlayerImg = battleCardItem.MoveParent.Find("PlayerImg").GetComponent<SpriteRenderer>();
            battleCardItem.CombatEffectivenessText = battleCardItem.MoveParent.Find("CombatEffectivenessText").GetComponent<TMP_Text>();
            battleCardItem.PositionText = battleCardItem.MoveParent.Find("PositionText").GetComponent<TMP_Text>();
            battleCardItem.NameText = battleCardItem.MoveParent.Find("NameText").GetComponent<TMP_Text>();
            battleCardItem.BattleCardBall = battleCardItem.MoveParent.Find("BattleCardBall").GetComponent<SpriteRenderer>();
            battleCardItem.CardHighLightImage = battleCardItem.MoveParent.Find("CardHighLightImage").GetComponent<MeshRenderer>();
            battleCardItem.starImageList = new();
            for (int i = 1; i <= 5; i++)
            {
                battleCardItem.starImageList.Add(battleCardItem.MoveParent.Find("starImage" + i).GetComponent<SpriteRenderer>());
            }

            //保存修改到硬盘
            AssetDatabase.SaveAssets();

            Debug.Log(prefabChineseName + "处理完成");
        }
        catch (Exception ex)
        {
            Debug.Log(prefabChineseName + "处理出错");
            Debug.LogError(ex);
        }
    }
}