using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace libx
{
    public static class EditorInitializer
    {
        
        [RuntimeInitializeOnLoadMethod]
        private static void OnInitialize()
        { 
            var sceneAssets = new List<string>();
            var rules = BuildScript.GetBuildRules();

            foreach (var guid in AssetDatabase.FindAssets("t:Scene", rules.scenesFolders))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid); 
                sceneAssets.Add(assetPath);  
            } 
            
            var patches = new List<Patch>();
            var assets = new List<AssetRef>();
            var searchPaths = new List<string>();  
            var dirs = new Dictionary<string, int>();
            foreach (var asset in rules.assets)
            { 
                if (! File.Exists(asset.name))
                {
                    continue;
                }
                var dir = Path.GetDirectoryName(asset.name);
                if (! string.IsNullOrEmpty(dir))
                {
                    if (! searchPaths.Contains(dir))
                    {
                        dirs[dir] = searchPaths.Count;
                        searchPaths.Add(dir);
                    }   
                } 
                var ar = new AssetRef {name = Path.GetFileName(asset.name), bundle = -1, dir = dirs[dir] };  
                assets.Add(ar);
            }  
            
            var scenes = new EditorBuildSettingsScene[sceneAssets.Count];
            for (var index = 0; index < sceneAssets.Count; index++)
            {
                var asset = sceneAssets[index]; 
                scenes[index] = new EditorBuildSettingsScene(asset, true);
                var dir = Path.GetDirectoryName(asset);
                if (! searchPaths.Contains(dir))
                {
                    searchPaths.Add(dir);
                }
            }

            for (var i = 0; i < rules.patches.Count; i++)
            {
                var item = rules.patches[i];
                var patch = new Patch();
                patch.name = item.name;
                patches.Add(patch);
            }
            
            var developVersions = new Versions();
            developVersions.dirs = searchPaths.ToArray();
            developVersions.assets = assets;
            developVersions.patches = patches;
            Assets.platform = BuildScript.GetPlatformName();
            Assets.basePath = Environment.CurrentDirectory.Replace("\\", "/") + "/" + BuildScript.outputPath + "/";
            Assets.assetLoader = AssetDatabase.LoadAssetAtPath; 
            Assets.versionsLoader += () => developVersions;
            Assets.onAssetLoaded += rules.OnLoadAsset;
            Assets.onAssetUnloaded += rules.OnUnloadAsset;   
            rules.BeginSample();
            EditorBuildSettings.scenes = scenes; 
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged; 
        }

        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                Assets.onAssetLoaded = null;
                Assets.onAssetUnloaded = null; 
                var rules = BuildScript.GetBuildRules(); 
                rules.EndSample();
                EditorUtility.SetDirty(rules);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        [InitializeOnLoadMethod]
        private static void OnEditorInitialize()
        {
            EditorUtility.ClearProgressBar(); 
        }
    }
}