using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;

public class LangIDCreator
{
    // LangID文件路径
    private static readonly string filePath = Path.Combine(Application.dataPath, "LocalAsset", "Config", "cfg_lang.csv");
    // 模板文件路径
    private static readonly string templatePath = Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "LangIDTemplate.txt");
    // 脚本生成路径
    private static readonly string scriptPath = Path.Combine(Application.dataPath, "Scripts", "GameConst", "LangID.cs");

    private const char langIDStart = '1';
    // 根据需要添加
    // private const char otherIDStart = '2';
    private const char errorIDStart = '9';

    [MenuItem("Babu/GenLanID")]
    public static void CreateOrUpdateScript()
    {
        Debug.Log("文件更新:LangID.cs");
        string template = File.ReadAllText(templatePath);
        StreamReader reader = new StreamReader(filePath);
        StringBuilder langID = new StringBuilder();
        StringBuilder errorID = new StringBuilder();
        // 根据需要添加
        // StringBuilder otherID = new StringBuilder();
        // 去除标题栏
        reader.ReadLine();
        while (!reader.EndOfStream)
        {
            var data = reader.ReadLine().Split(',');
            if (data.Length < 3) break;
            // 枚举ID
            var id = data[0];
            // 变量名
            var name = data[1];
            // 注释
            var desc = data[2];
            var property = $"//{desc}\r\n        {name} = {id},\r\n        ";
            switch (id[0])
            {
                case langIDStart:
                    langID.Append(property);
                    break;
                case errorIDStart:
                    errorID.Append(property);
                    break;
                    // 根据需要添加
                    // case otherIDStart:
                    //     otherID.Append(property);
                    //     break;
            }
        }
        reader.Close();
        var output = template.Replace("#LANGID_PROPERTY#", langID.ToString().TrimEnd('\r', '\n', ' '));
        output = output.Replace("#ERRORID_PROPERTY#", errorID.ToString().TrimEnd('\r', '\n', ' '));
        // 根据需要添加（须在模板文件：LangIDTemplate.txt 中添加该类）
        // output = output.Replace("#OTHERID_PROPERTY", otherID.ToString());
        File.WriteAllText(scriptPath, output);
        AssetDatabase.Refresh();
    }
}