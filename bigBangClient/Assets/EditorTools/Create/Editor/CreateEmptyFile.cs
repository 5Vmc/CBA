using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 创建一个空文件,可指定文件后缀
/// </summary>
public class CreateEmptyFile
{
    [MenuItem("Assets/创建空文件", false)]
    private static void Create()
    {
        string directory = AssetDatabase.GetAssetPath(Selection.objects[0]);
        var absolutePath = ToAbsolutePath(directory);
        if (File.Exists(absolutePath))
        {
            int len = new FileInfo(absolutePath).Name.Length;
            directory = directory.Remove(directory.Length - len, len);
        }
        var icon = EditorGUIUtility.IconContent("d_TextAsset Icon").image as Texture2D;
        ProjectWindowUtil.CreateAssetWithContent(directory + "/NewFile", string.Empty, icon);
    }

    private static string ToAbsolutePath(string path)
    {
        return $"{Application.dataPath}/{path.Substring("Assets/".Length)}";
    }
}
