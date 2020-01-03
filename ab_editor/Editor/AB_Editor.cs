using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AB_Editor : Editor 
{
    [CustomEditor(typeof(AB_Editor.AB_Config))]
    public class AB_Config_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Save"))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    [CreateAssetMenu]
    public class AB_Config:ScriptableObject
    {
        public BuildTarget Target;
        public string ABVariant = "ab";
        public string LocalBuildOutPath = "Assets/StreamingAssets/";
        public string ResParent = "Assets/AB_Res/";
    }

    public static class Config
    {
        public const string MENU_ROOT = "Assets/ab_editor/";
        public const string AB_CONFIG_LOCAL_PATH="Assets/ab_editor/AB_Config.asset";
    }

	[MenuItem(Config.MENU_ROOT+"Set Name")]
	public static void Set_AB_Name()
    {
        var ab_config = getConfig();
        var sels = Selection.assetGUIDs;
        if(sels!=null && sels.Length>0)
        {
            for (int i = 0; i < sels.Length; i++)
            {
                string localPath = AssetDatabase.GUIDToAssetPath(sels[i]);
                var asset = AssetImporter.GetAtPath(localPath);
                asset.assetBundleName = localPath.Replace(ab_config.ResParent,"");
                asset.assetBundleVariant = ab_config.ABVariant;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

	[MenuItem(Config.MENU_ROOT+"Clear")]
	public static void Clear_AB_Name()
    {
        var sels = Selection.assetGUIDs;
        if(sels!=null && sels.Length>0)
        {
            for (int i = 0; i < sels.Length; i++)
            {
                string localPath = AssetDatabase.GUIDToAssetPath(sels[i]);
                var asset = AssetImporter.GetAtPath(localPath);
                asset.assetBundleName = string.Empty;
                // asset.assetBundleVariant = string.Empty;
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static AB_Config getConfig()
    {
        var ab_config = AssetDatabase.LoadAssetAtPath<AB_Config>(Config.AB_CONFIG_LOCAL_PATH);
        if(ab_config==null)
        {
            Debug.Log("[AB_Editor.getConfig]无配置文件 自动创建;");
            ab_config = AB_Config.CreateInstance<AB_Config>();
            AssetDatabase.CreateAsset(ab_config,Config.AB_CONFIG_LOCAL_PATH);
            AssetDatabase.Refresh();
        }
        return ab_config;
    }

    [MenuItem("AB_Editor/Open Config")]
    public static void OpenConfig()
    {
        var ab_config = getConfig();
        AssetDatabase.OpenAsset(ab_config);
    }

    [MenuItem("AB_Editor/Build")]
    public static void Build()
    {
        var ab_config = getConfig();
        Debug.LogFormat("[AB_Editor.Build]{0} outpath:{1}",ab_config.Target,ab_config.LocalBuildOutPath);
        string outDir = Application.dataPath.Replace("Assets",ab_config.LocalBuildOutPath)+ab_config.Target;
        
        if(!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        BuildPipeline.BuildAssetBundles(ab_config.LocalBuildOutPath+ab_config.Target,BuildAssetBundleOptions.DisableWriteTypeTree|BuildAssetBundleOptions.ChunkBasedCompression,ab_config.Target);
        AssetDatabase.Refresh();
    }
}

