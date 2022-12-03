using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace libx
{
    public static class MenuItems
    {
        [MenuItem("ABTool/Generate/Dll")]
        private static void GenerateDll()
        {
            //必须要的程序集拷贝
            CompileDllCommand.CompileDllActiveBuildTarget();
            CopyAOTAssembliesToDllCache();
            CopyHotUpdateAssembliesToDllCache();
        }

        [MenuItem("ABTool/Clear/Dll")]
        private static void ClearDll()
        {
            var path = DllCachePath;
            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                File.Delete(file.FullName);
            }
            AssetDatabase.Refresh();
        }

        static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };

        static string DllCachePath
        {
            get
            {
                return $"{Application.dataPath}/DllCache";
            }
        }

        static void CopyAOTAssembliesToDllCache()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            string aotAssembliesDstDir = DllCachePath; //拷贝到DllCache

            foreach (var dll in AOTMetaAssemblyNames)
            {
                string srcDllPath = $"{aotAssembliesSrcDir}/{dll}";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }
                string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.bytes";
                File.Copy(srcDllPath, dllBytesPath, true);
                Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
            }
            AssetDatabase.Refresh();
        }

        static void CopyHotUpdateAssembliesToDllCache()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string hotfixAssembliesDstDir = DllCachePath;
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFiles)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                string dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
            AssetDatabase.Refresh();
        }

        //[MenuItem("ABTool/Generate/LinkXML")]
        //private static void GenerateLinkXML()
        //{

        //}

        [MenuItem("ABTool/Copy Bundles")]
        private static void CopyBundles()
        {
            BuildScript.CopyAssets();
        }

        [MenuItem("ABTool/Build/Bundles")]
        private static void BuildBundles()
        {
            var watch = new Stopwatch();
            watch.Start();
            BuildScript.BuildRules();
            BuildScript.BuildAssetBundles();
            watch.Stop();
            Debug.Log("BuildBundles " + watch.ElapsedMilliseconds + " ms.");

            ClearDll();
        }

        [MenuItem("ABTool/Build/Player")]
        private static void BuildPlayer()
        {
            var watch = new Stopwatch();
            watch.Start();
            BuildScript.BuildPlayer();
            watch.Stop();
            Debug.Log("BuildPlayer " + watch.ElapsedMilliseconds + " ms.");
        }

        [MenuItem("ABTool/Build/Rules")]
        private static void BuildRules()
        {
            var watch = new Stopwatch();
            watch.Start();
            BuildScript.BuildRules();
            watch.Stop();
            Debug.Log("BuildRules " + watch.ElapsedMilliseconds + " ms.");
        }

        [MenuItem("ABTool/View/Versions")]
        private static void ViewVersions()
        {
            var path = EditorUtility.OpenFilePanel("OpenFile", Environment.CurrentDirectory, "");
            if (string.IsNullOrEmpty(path)) return;
            BuildScript.ViewVersions(path);
        }

        //[MenuItem("ABTool/View/Bundles")]
        //private static void ViewBundles()
        //{
        //    EditorUtility.OpenWithDefaultApp(Assets.Bundles);
        //}

        [MenuItem("ABTool/View/Download")]
        private static void ViewDownload()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }

        [MenuItem("ABTool/View/Temp")]
        private static void ViewTemp()
        {
            EditorUtility.OpenWithDefaultApp(Application.temporaryCachePath);
        }

        //[MenuItem("ABTool/View/CRC")]
        //private static void GetCRC()
        //{
        //    var path = EditorUtility.OpenFilePanel("OpenFile", Environment.CurrentDirectory, "");
        //    if (string.IsNullOrEmpty(path)) return;

        //    using (var fs = File.OpenRead(path))
        //    {
        //        var crc = Utility.GetCRC32Hash(fs);
        //        Debug.Log(crc);
        //    }
        //}

        //[MenuItem("ABTool/View/MD5")]
        //private static void GetMD5()
        //{
        //    var path = EditorUtility.OpenFilePanel("OpenFile", Environment.CurrentDirectory, "");
        //    if (string.IsNullOrEmpty(path)) return;

        //    using (var fs = File.OpenRead(path))
        //    {
        //        var crc = Utility.GetMD5Hash(fs);
        //        Debug.Log(crc);
        //    }
        //}

        //[MenuItem("ABTool/Dump Assets")]
        //private static void DumpAssets()
        //{
        //    var path = EditorUtility.SaveFilePanel("DumpAssets", null, "dump", "txt");
        //    if (string.IsNullOrEmpty(path)) return;
        //    var s = Assets.DumpAssets();
        //    File.WriteAllText(path, s);
        //    EditorUtility.OpenWithDefaultApp(path);
        //}

        [MenuItem("ABTool/Take Screenshot")]
        private static void Screenshot()
        {
            var path = EditorUtility.SaveFilePanel("截屏", null, "screenshot_", "png");
            if (string.IsNullOrEmpty(path)) return;

            ScreenCapture.CaptureScreenshot(path);
        }

        [MenuItem("Assets/ToJson")]
        private static void ToJson()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var json = JsonUtility.ToJson(Selection.activeObject);
            File.WriteAllText(path.Replace(".asset", ".json"), json);
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Copy Path")]
        private static void CopyPath()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            EditorGUIUtility.systemCopyBuffer = path;
        }

        [MenuItem("Assets/GroupBy/None")]
        private static void GroupByNone()
        {
            GroupAssets(GroupBy.None);
        }

        [MenuItem("Assets/GroupBy/Filename")]
        private static void GroupByFilename()
        {
            GroupAssets(GroupBy.Filename);
        }

        [MenuItem("Assets/GroupBy/Directory")]
        private static void GroupByDirectory()
        {
            GroupAssets(GroupBy.Directory);
        }

        [MenuItem("Assets/GroupBy/Explicit/shaders")]
        private static void GroupByExplicitShaders()
        {
            GroupAssets(GroupBy.Explicit, "shaders");
        }

        [MenuItem("Assets/PatchBy/CurrentScene")]
        private static void PatchAssets()
        {
            var selection = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            var rules = BuildScript.GetBuildRules();
            foreach (var o in selection)
            {
                var path = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(path) || Directory.Exists(path)) continue;
                rules.PatchAsset(path);
            }

            EditorUtility.SetDirty(rules);
            AssetDatabase.SaveAssets();
        }

        private static void GroupAssets(GroupBy nameBy, string bundle = null)
        {
            var selection = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            var rules = BuildScript.GetBuildRules();
            foreach (var o in selection)
            {
                var path = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(path) || Directory.Exists(path)) continue;
                rules.GroupAsset(path, nameBy, bundle);
            }

            EditorUtility.SetDirty(rules);
            AssetDatabase.SaveAssets();
        }
    }
}