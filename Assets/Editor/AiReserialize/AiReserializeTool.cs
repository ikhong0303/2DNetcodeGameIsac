using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AiReserialize;

public static class AiReserializeTool
{
    private const string SessionKeyDidRun = "AiReserializeTool_DidRunThisSession";

    [InitializeOnLoadMethod]
    private static void RunOnEditorLoad()
    {
        EditorApplication.delayCall += () =>
        {
            AiReserializeSettings settings = LoadSettingsAsset();
            if (settings == null)
                return;

            if (!settings.runOnEditorLoad)
                return;

            if (settings.runOncePerSession && SessionState.GetBool(SessionKeyDidRun, false))
                return;

            try
            {
                ReserializeFoldersFromSettings(settings);
            }
            finally
            {
                if (settings.runOncePerSession)
                    SessionState.SetBool(SessionKeyDidRun, true);
            }
        };
    }

    // ---------------------------
    // Menu (Tools)
    // ---------------------------

    [MenuItem("Tools/AI Reserialize/Run Now (Settings Folders)")]
    private static void RunNowFromSettingsMenu()
    {
        AiReserializeSettings settings = LoadSettingsAsset();
        if (settings == null)
        {
            Debug.LogWarning("[AI Reserialize] Settings asset not found. Create one via 'Tools/AI Reserialize/Create Settings Asset'.");
            return;
        }

        ReserializeFoldersFromSettings(settings);
    }

    [MenuItem("Tools/AI Reserialize/Open Settings Asset")]
    private static void OpenSettingsAssetMenu()
    {
        AiReserializeSettings settings = LoadSettingsAsset();
        if (settings == null)
        {
            Debug.LogWarning("[AI Reserialize] Settings asset not found. Create one via 'Tools/AI Reserialize/Create Settings Asset'.");
            return;
        }

        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
    }

    [MenuItem("Tools/AI Reserialize/Create Settings Asset")]
    private static void CreateSettingsAssetMenu()
    {
        const string defaultFolder = "Assets/Settings";
        EnsureFolderExists(defaultFolder);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{defaultFolder}/AiReserializeSettings.asset");
        var settings = ScriptableObject.CreateInstance<AiReserializeSettings>();

        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);

        Debug.Log($"[AI Reserialize] Created settings asset: {assetPath}");
    }

    [MenuItem("Tools/AI Reserialize/Enable Force Text Serialization")]
    private static void EnableForceTextSerializationMenu()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;

        // This controls meta visibility for many VCS workflows.
        // Values commonly include: "Visible Meta Files", "Hidden Meta Files", "Perforce"
        EditorSettings.externalVersionControl = "Visible Meta Files";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AI Reserialize] Enabled: Force Text Serialization + Visible Meta Files");
    }

    [MenuItem("Tools/AI Reserialize/Reserialize Selected (Project Window)")]
    private static void ReserializeSelectedMenu()
    {
        AiReserializeSettings settings = LoadSettingsAsset();
        ReserializeSelection(settings);
    }

    [MenuItem("Tools/AI Reserialize/Reserialize Folder... (Pick in OS)")]
    private static void ReserializeFolderPickMenu()
    {
        string folderOsPath = EditorUtility.OpenFolderPanel("Pick a folder under this Unity project", Application.dataPath, "");
        if (string.IsNullOrEmpty(folderOsPath))
            return;

        string folderAssetPath = ConvertToAssetPath(folderOsPath);
        if (string.IsNullOrEmpty(folderAssetPath) || !AssetDatabase.IsValidFolder(folderAssetPath))
        {
            Debug.LogWarning("[AI Reserialize] Selected folder is not inside this project's Assets folder.");
            return;
        }

        AiReserializeSettings settings = LoadSettingsAsset();
        ReserializeFolders(new List<string> { folderAssetPath }, settings);
    }

    // ---------------------------
    // Context Menu (Assets)
    // ---------------------------

    [MenuItem("Assets/AI Reserialize/Reserialize Selected", false, 2000)]
    private static void ReserializeSelectedContextMenu()
    {
        AiReserializeSettings settings = LoadSettingsAsset();
        ReserializeSelection(settings);
    }

    [MenuItem("Assets/AI Reserialize/Reserialize Selected", true)]
    private static bool ValidateReserializeSelectedContextMenu()
    {
        return Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0;
    }

    // ---------------------------
    // Core
    // ---------------------------

    private static void ReserializeFoldersFromSettings(AiReserializeSettings settings)
    {
        List<string> folderPaths = settings != null
            ? settings.targetFolderPaths.Where(AssetDatabase.IsValidFolder).Distinct().ToList()
            : new List<string>();

        if (folderPaths.Count == 0)
        {
            Debug.LogWarning("[AI Reserialize] No valid folders to reserialize. Check settings.targetFolderPaths.");
            return;
        }

        ReserializeFolders(folderPaths, settings);
    }

    private static void ReserializeSelection(AiReserializeSettings settings)
    {
        string[] selectedGuids = Selection.assetGUIDs;
        if (selectedGuids == null || selectedGuids.Length == 0)
        {
            Debug.LogWarning("[AI Reserialize] Nothing selected.");
            return;
        }

        var selectedPaths = new List<string>(selectedGuids.Length);
        foreach (string guid in selectedGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
                selectedPaths.Add(path);
        }

        // Split into folders and files
        List<string> folderPaths = selectedPaths.Where(AssetDatabase.IsValidFolder).Distinct().ToList();
        List<string> filePaths = selectedPaths.Where(p => !AssetDatabase.IsValidFolder(p)).Distinct().ToList();

        var collectedPaths = new List<string>();
        collectedPaths.AddRange(filePaths);

        if (folderPaths.Count > 0)
        {
            List<string> folderAssetPaths = CollectAssetPathsFromFolders(folderPaths);
            collectedPaths.AddRange(folderAssetPaths);
        }

        ReserializeAssets(collectedPaths, settings, "Selection");
    }

    private static void ReserializeFolders(List<string> folderPaths, AiReserializeSettings settings)
    {
        List<string> assetPaths = CollectAssetPathsFromFolders(folderPaths);
        ReserializeAssets(assetPaths, settings, "Folders");
    }

    private static List<string> CollectAssetPathsFromFolders(List<string> folderPaths)
    {
        string[] guids = AssetDatabase.FindAssets("", folderPaths.ToArray());

        var assetPaths = new List<string>(guids.Length);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                continue;

            if (AssetDatabase.IsValidFolder(path))
                continue;

            assetPaths.Add(path);
        }

        return assetPaths.Distinct().ToList();
    }

    private static void ReserializeAssets(List<string> assetPaths, AiReserializeSettings settings, string scopeLabel)
    {
        if (assetPaths == null || assetPaths.Count == 0)
        {
            Debug.LogWarning($"[AI Reserialize] No assets found to reserialize ({scopeLabel}).");
            return;
        }

        AiReserializeSettings safeSettings = settings != null ? settings : CreateDefaultSettings();

        List<string> filteredPaths = FilterAssetPaths(assetPaths, safeSettings);
        if (filteredPaths.Count == 0)
        {
            Debug.LogWarning($"[AI Reserialize] Assets found but none matched filters ({scopeLabel}).");
            return;
        }

        int batchSize = Mathf.Max(1, safeSettings.batchSize);
        int totalCount = filteredPaths.Count;
        int processedCount = 0;

        DateTime startTime = DateTime.Now;

        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < totalCount; i += batchSize)
            {
                int count = Mathf.Min(batchSize, totalCount - i);
                List<string> batchPaths = filteredPaths.GetRange(i, count);

                float progress = totalCount <= 0 ? 1f : (float)processedCount / totalCount;

                if (safeSettings.showProgressBar)
                {
                    bool canceled = EditorUtility.DisplayCancelableProgressBar(
                        "AI Reserialize",
                        $"Reserializing {scopeLabel}... ({processedCount}/{totalCount})",
                        progress);

                    if (canceled)
                    {
                        Debug.LogWarning("[AI Reserialize] Canceled by user.");
                        break;
                    }
                }

                ForceReserialize(batchPaths);
                processedCount += count;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            if (safeSettings.showProgressBar)
                EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (safeSettings.logToConsole)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            Debug.Log($"[AI Reserialize] Done. Scope={scopeLabel}, Reserialized={processedCount}/{totalCount}, Time={elapsed.TotalSeconds:0.00}s");
        }
    }

    private static void ForceReserialize(List<string> assetPaths)
    {
#if UNITY_2021_2_OR_NEWER
        AssetDatabase.ForceReserializeAssets(assetPaths, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
#else
        AssetDatabase.ForceReserializeAssets(assetPaths);
#endif
    }

    private static List<string> FilterAssetPaths(List<string> assetPaths, AiReserializeSettings settings)
    {
        var filtered = new List<string>(assetPaths.Count);

        foreach (string path in assetPaths)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            // Skip scripts and project settings files (usually not desired for AI YAML workflows)
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".cs" || ext == ".asmdef" || ext == ".dll" || ext == ".json" || ext == ".txt")
                continue;

            if (!ShouldIncludePath(path, settings))
                continue;

            filtered.Add(path);
        }

        return filtered.Distinct().ToList();
    }

    private static bool ShouldIncludePath(string assetPath, AiReserializeSettings settings)
    {
        string ext = Path.GetExtension(assetPath).ToLowerInvariant();

        if (ext == ".prefab")
            return settings.includePrefabs;

        if (ext == ".unity")
            return settings.includeScenes;

        if (ext == ".mat")
            return settings.includeMaterials;

        if (ext == ".asset")
        {
            if (!settings.includeScriptableObjects && !settings.includeOtherAssets)
                return false;

            Type mainType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            bool isScriptableObject = mainType != null && typeof(ScriptableObject).IsAssignableFrom(mainType);

            if (isScriptableObject)
                return settings.includeScriptableObjects;

            return settings.includeOtherAssets;
        }

        if (!settings.includeOtherAssets)
            return false;

        // Common YAML-ish Unity assets (not exhaustive)
        return ext == ".anim"
            || ext == ".controller"
            || ext == ".overridecontroller"
            || ext == ".playable"
            || ext == ".mask"
            || ext == ".physicmaterial"
            || ext == ".guiskin";
    }

    private static AiReserializeSettings LoadSettingsAsset()
    {
        string[] guids = AssetDatabase.FindAssets("t:AiReserializeSettings");
        if (guids == null || guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<AiReserializeSettings>(path);
    }

    private static AiReserializeSettings CreateDefaultSettings()
    {
        var settings = ScriptableObject.CreateInstance<AiReserializeSettings>();
        settings.runOnEditorLoad = false;
        settings.runOncePerSession = false;
        return settings;
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = "Assets";
        string[] parts = folderPath.Replace("\\", "/").Split('/');

        for (int i = 1; i < parts.Length; i++)
        {
            string current = $"{parent}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(current))
                AssetDatabase.CreateFolder(parent, parts[i]);
            parent = current;
        }
    }

    private static string ConvertToAssetPath(string osPath)
    {
        if (string.IsNullOrEmpty(osPath))
            return null;

        string dataPath = Application.dataPath.Replace("\\", "/");
        string normalized = osPath.Replace("\\", "/");

        if (!normalized.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
            return null;

        string relative = "Assets" + normalized.Substring(dataPath.Length);
        return relative;
    }
}
