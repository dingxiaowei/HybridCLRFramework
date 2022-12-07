using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace libx
{
    public class UpdateScreen : MonoBehaviour
    {
        Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();
        List<string> AOTMetaAssemblyNames { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };

        public Button buttonClear;
        public Button buttonStart;
        public Slider progressBar;
        public Text progressText;
        public Text version;
        private void Start()
        {
            NetworkMonitor.Instance.onReachabilityChanged += OnReachablityChanged;
            buttonStart.gameObject.SetActive(true);
            buttonStart.onClick.AddListener(StartUpdate);
            buttonClear.onClick.AddListener(Clear);
            version.text = Assets.currentVersions.ver;
        }

        private void OnReachablityChanged(NetworkReachability reachability)
        {
            if (reachability == NetworkReachability.NotReachable)
            {
                OnMessage("网络错误");
            }
        }

        private void OnProgress(float progress)
        {
            progressBar.value = progress;
        }

        private void OnMessage(string msg)
        {
            progressText.text = msg;
        }

        public void Clear()
        {
            MessageBox.Show("提示", "清除数据后所有数据需要重新下载，请确认！,", cleanup =>
            {
                if (cleanup)
                {
                    File.Delete(Assets.updatePath + "/" + Assets.Versions);
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    OnMessage("数据清除完毕, TOUCH TO START");
                    OnProgress(0);
                    buttonStart.gameObject.SetActive(true);
                }
            }, "清除");
        }

        public void StartUpdate()
        {
#if UNITY_EDITOR
            if (Assets.development)
            {
                LoadDll();
                return;
            }
#endif
            OnMessage("正在获取版本信息...");
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                MessageBox.Show("提示", "请检查网络连接状态", retry =>
                {
                    if (retry)
                    {
                        StartUpdate();
                    }
                    else
                    {
                        Quit();
                    }
                }, "重试", "退出");
            }
            else
            {
                Assets.DownloadVersions(error =>
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show("提示", string.Format("获取服务器版本失败：{0}", error), retry =>
                        {
                            if (retry)
                            {
                                StartUpdate();
                            }
                            else
                            {
                                Quit();
                            }
                        });
                    }
                    else
                    {
                        Downloader handler;
                        // 按分包下载版本更新，返回true的时候表示需要下载，false的时候，表示不需要下载
                        if (Assets.DownloadAll(Assets.patches4Init, out handler))
                        {
                            var totalSize = handler.size;
                            var tips = string.Format("发现内容更新，总计需要下载 {0} 内容", Downloader.GetDisplaySize(totalSize));
                            MessageBox.Show("提示", tips, download =>
                            {
                                if (download)
                                {
                                    handler.onUpdate += delegate (long progress, long size, float speed)
                                    {
                                        //刷新界面
                                        OnMessage(string.Format("下载中...{0}/{1}, 速度：{2}",
                                            Downloader.GetDisplaySize(progress),
                                            Downloader.GetDisplaySize(size),
                                            Downloader.GetDisplaySpeed(speed)));
                                        OnProgress(progress * 1f / size);
                                    };
                                    handler.onFinished += OnComplete;
                                    handler.Start();
                                }
                                else
                                {
                                    Quit();
                                }
                            }, "下载", "退出");
                        }
                        else
                        {
                            OnComplete();
                        }
                    }
                });
            }
        }

        private void OnComplete()
        {
            OnProgress(1);
            version.text = Assets.currentVersions.ver;
            OnMessage("更新完成");
            LoadDll();
        }

        void LoadDll()
        {
            if (!AppConfig.LoadedDll)
            {
                AppConfig.LoadedDll = true;
                StartCoroutine(DownLoadDllAssets(this.StartEnterLevel));
            }
            else
            {
                Debug.LogWarning("dll已经加载过了");
                StartEnterLevel();
            }
        }

        void StartEnterLevel()
        {
            StartCoroutine(EnterLevel());
        }

        void LoadAssemblies()
        {
            LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
        var gameAss = System.Reflection.Assembly.Load(GetAssetData("HotUpdateLogic.dll"));
#else
            var gameAss = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "Assembly-CSharp");
#endif
        }

        string GetWebRequestPath(string asset)
        {
            //如果是Develop模式则从工程加载，如果是AB模式则走PersistentDataPath路径
            string path = "";
            if (Assets.development)
            {
                path = $"{Application.dataPath}/DllCache/{asset}";
            }
            else
            {
                path = $"{Application.persistentDataPath}/Bundles/DllCache/{asset}";
                if (!path.Contains("://"))
                {
                    path = "file://" + path;
                }
            }
            if (path.EndsWith(".dll"))
            {
                path += ".bytes";
            }
            return path;
        }

        IEnumerator DownLoadDllAssets(Action onDownloadComplete)
        {
            Debug.Log("开始加载程序集,程序集不能加载两次哦");
            var assets = new List<string>
            {
                "HotUpdateLogic.dll",
            }.Concat(AOTMetaAssemblyNames);

            foreach (var asset in assets)
            {
                string dllPath = GetWebRequestPath(asset);
                Debug.Log($"start download asset:{dllPath}");
                UnityWebRequest www = UnityWebRequest.Get(dllPath);
                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
#else
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
#endif
                else
                {
                    // Or retrieve results as binary data
                    byte[] assetData = www.downloadHandler.data;
                    Debug.Log($"dll:{asset}  size:{assetData.Length}");
                    s_assetDatas[asset] = assetData;
                }
            }
            LoadAssemblies();
            onDownloadComplete();
        }

        byte[] GetAssetData(string dllName)
        {
            return s_assetDatas[dllName];
        }

        void LoadMetadataForAOTAssemblies()
        {
            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessors里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in AOTMetaAssemblyNames)
            {
                byte[] dllBytes = GetAssetData(aotDllName);
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }

        private IEnumerator EnterLevel()
        {
            yield return null;
            OnProgress(0);
            OnMessage("加载游戏场景");
            var scene = Assets.LoadSceneAsync(ResFormat.GetScene("Level"));
            while (!scene.isDone)
            {
                OnProgress(scene.progress);
                yield return null;
            }
        }

        private void OnDestroy()
        {
            MessageBox.Dispose();
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}