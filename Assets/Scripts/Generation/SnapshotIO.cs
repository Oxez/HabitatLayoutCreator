// Author Oxe
// Created at 04.10.2025 19:05

using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class SnapshotIO
{
    public static string DirPath =>
        Path.Combine(Application.persistentDataPath, "HabitatVariants");

    public static void EnsureDir() { if (!Directory.Exists(DirPath)) Directory.CreateDirectory(DirPath); }

    public static string PathFor(string fileNameNoExt)
        => Path.Combine(DirPath, fileNameNoExt + ".json");

    public static void Save(LayoutSnapshot snap, string fileNameNoExt)
    {
        EnsureDir();
        var json = JsonUtility.ToJson(snap, true);
        File.WriteAllText(PathFor(fileNameNoExt), json);
#if UNITY_EDITOR
        Debug.Log($"[SnapshotIO] Saved: {PathFor(fileNameNoExt)}");
#endif
    }

    public static LayoutSnapshot Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<LayoutSnapshot>(json);
    }

    public static List<string> ListFiles()
    {
        EnsureDir();
        var list = new List<string>(Directory.GetFiles(DirPath, "*.json"));
        list.Sort();
        return list;
    }
}
