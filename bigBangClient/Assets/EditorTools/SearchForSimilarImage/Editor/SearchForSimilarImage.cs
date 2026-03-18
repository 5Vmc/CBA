using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CoenM.ImageHash.HashAlgorithms;
using System.Text;
using CoenM.ImageHash;
using System.Security.Cryptography;

public class SearchForSimilarImage
{
    // 哈希文件保存路径
    private static string hashFilePath = "Assets/EditorTools/SearchForSimilarImage/ImageHash.csv";
    // Key:guid Value:ImageFileInfo
    private static Dictionary<string, ImageFileInfo> hashs = new Dictionary<string, ImageFileInfo>();
    // 图片1的guid,图片2的guid,它们之间的相似度
    public static List<SimilarityInfo> Result = new List<SimilarityInfo>();

    public class ImageFileInfo
    {
        private Texture asset;

        public string Guid;
        public string FileHash;
        public ulong ImgHash;

        public Texture Asset
        {
            get
            {
                asset ??= AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(Guid));
                return asset;
            }
        }

        public ImageFileInfo(string guid, string fileHash, ulong imgHash)
        {
            Guid = guid;
            FileHash = fileHash;
            ImgHash = imgHash;
        }
    }

    public class SimilarityInfo
    {
        public ImageFileInfo Img1;
        public ImageFileInfo Img2;
        // 相似度（取值范围：0-1）
        public float Similarity;

        public SimilarityInfo(ImageFileInfo img1, ImageFileInfo img2, float similarity)
        {
            Img1 = img1;
            Img2 = img2;
            Similarity = similarity;
        }
    }

    // 过滤条件
    private static bool FilterCondition(string path)
    {
        return path.StartsWith("Assets/") && (path.EndsWith(".png") || path.EndsWith(".jpg"));
    }

    // 查找相似图片
    public static void Search()
    {
        // 加载
        LoadHashFile();
        // 计算
        CalculateHash();
        // 保存
        SaveHashFile();

        Result.Clear();
        var list = hashs.ToList();
        int similarCount = 0;
        int sameCount = 0;
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                var similarity = CompareHash.Similarity(list[i].Value.ImgHash, list[j].Value.ImgHash);

                if (similarity < 95) continue;

                if (Mathf.Abs(list[i].Value.Asset.width * list[i].Value.Asset.height - list[j].Value.Asset.width * list[j].Value.Asset.height) > 5) continue;

                if (list[i].Value.FileHash != list[j].Value.FileHash)
                {
                    similarity *= 0.99f;
                }
                else
                {
                    sameCount++;
                }
                Result.Add(new SimilarityInfo(list[i].Value, list[j].Value, (float)similarity / 100f));
                similarCount++;
            }
        }
        // 按相似度排序
        Result = Result.OrderByDescending(item => item.Similarity).ToList();
    }

    // 计算图片哈希
    private static void CalculateHash()
    {
        var relativePaths = AssetDatabase.GetAllAssetPaths().Where(FilterCondition).ToList();
        var absolutePaths = relativePaths.Select(item => Path.Combine(Application.dataPath, "..", item)).ToList();
        var guids = relativePaths.Select(item => AssetDatabase.AssetPathToGUID(item)).ToList();
        var hasher = new PerceptualHash();

        for (int i = 0; i < absolutePaths.Count; i++)
        {
            if (hashs.ContainsKey(guids[i])) continue;

            EditorUtility.DisplayProgressBar("正在计算图片哈希值", "", (float)i / absolutePaths.Count);
            using var img = Image.Load<Rgba32>(absolutePaths[i]);
            var fileHash = GetFileHash(absolutePaths[i]);
            var imgHash = hasher.Hash(img);
            var texture = AssetDatabase.LoadAssetAtPath<Texture>(relativePaths[i]);
            hashs[guids[i]] = new ImageFileInfo(guids[i], fileHash, imgHash);
        }
        EditorUtility.ClearProgressBar();
        SaveHashFile();
    }

    // 保存哈希文件
    private static void SaveHashFile()
    {
        var builder = new StringBuilder();
        foreach (var item in hashs)
        {
            builder.AppendLine(item.Key + "," + item.Value.FileHash + "," + item.Value.ImgHash);
        }
        File.WriteAllText(hashFilePath, builder.ToString().Trim());
        AssetDatabase.Refresh();
    }

    // 读取哈希文件
    private static void LoadHashFile()
    {
        if (!File.Exists(hashFilePath)) return;

        hashs.Clear();
        foreach (var line in File.ReadAllLines(hashFilePath))
        {
            var split = line.Split(',');
            var guid = split[0];
            var fileHash = split[1];
            var imgHash = ulong.Parse(split[2]);
            var relativePath = AssetDatabase.GUIDToAssetPath(guid);

            // 排除不存在的资源
            if (AssetDatabase.LoadAssetAtPath<Object>(relativePath) == null) continue;

            var absolutePath = Path.Combine(Application.dataPath, "..", relativePath);

            // 排除被修改的资源
            if (fileHash != GetFileHash(absolutePath)) continue;

            hashs[guid] = new ImageFileInfo(guid, fileHash, imgHash);
        }
    }

    // 获得文件哈希
    private static string GetFileHash(string path)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(path);
        StringBuilder builder = new StringBuilder();
        foreach (var item in md5.ComputeHash(stream))
        {
            builder.Append(item.ToString("x2"));
        }
        return builder.ToString();
    }

    public static void Clear()
    {
        Result.Clear();
        hashs.Clear();
    }
}