using System.Collections.Generic;
using UnityEngine;

namespace AiReserialize
{
    [CreateAssetMenu(
        fileName = "AiReserializeSettings",
        menuName = "Tools/AI Reserialize/Settings",
        order = 0)]
    public sealed class AiReserializeSettings : ScriptableObject
    {
        [Header("Auto Run")]
        public bool runOnEditorLoad = true;

        [Tooltip("If true, auto-run occurs only once per editor session.")]
        public bool runOncePerSession = true;

        [Header("Target Folders (Project Relative)")]
        [Tooltip("Example: Assets/Prefabs, Assets/Data")]
        public List<string> targetFolderPaths = new List<string>
        {
            "Assets/Prefabs",
            "Assets/Data",
            "Assets/Resources",
            "Assets/ETC"
        };

        [Header("Asset Type Filters")]
        public bool includePrefabs = true;
        public bool includeScenes = true;
        public bool includeScriptableObjects = true;
        public bool includeMaterials = false;

        [Tooltip("Include other .asset files that are NOT ScriptableObject, and other YAML-like assets (.anim, .controller, etc.).")]
        public bool includeOtherAssets = false;

        [Header("Execution")]
        [Min(1)]
        public int batchSize = 200;

        public bool showProgressBar = true;
        public bool logToConsole = true;
    }
}
