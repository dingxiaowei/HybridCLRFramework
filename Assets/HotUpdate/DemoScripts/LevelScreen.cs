using UnityEngine;
using UnityEngine.UI;

namespace libx
{
    public class LevelScreen : MonoBehaviour
    {
        public Button buttonBack;
        public Button buttonPatch;
        public Slider progressBar;
        public Text progressText;

        private void Start()
        {
            buttonBack.onClick.AddListener(Back);
            if (Assets.currentVersions != null)
            {
                var patches = Assets.currentVersions.patches;
                for (var i = 0; i < patches.Count; i++)
                {
                    var patch = patches[i];
                    var go = Instantiate(buttonPatch.gameObject, buttonPatch.transform.parent, false);
                    go.name = patch.name;
                    var text = go.GetComponentInChildren<Text>();
                    text.text = go.name;
                    go.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        Downloader handler;
                        if (Assets.DownloadAll(new[] {patch.name}, out handler))
                        {
                            var totalSize = handler.size;
                            var tips = string.Format("总计需要下载 {0} 内容", Downloader.GetDisplaySize(totalSize));
                            MessageBox.Show("更新提示", tips, download =>
                            {
                                if (download)
                                {
                                    handler.onUpdate += delegate(long progress, long size, float speed)
                                    {
                                        //刷新界面
                                        OnMessage(string.Format("下载中...{0}/{1}, 速度：{2}",
                                            Downloader.GetDisplaySize(progress),
                                            Downloader.GetDisplaySize(size),
                                            Downloader.GetDisplaySpeed(speed)));
                                        OnProgress(progress * 1f / size);
                                    };
                                    handler.onFinished += delegate
                                    {
                                        OnMessage("下载完成");
                                        LoadScene(patch);
                                    };
                                    handler.Start();
                                }
                                else
                                {
                                    MessageBox.Show("提示", "下载失败：用户取消", isOk => { }, "确定", "退出");
                                }
                            }, "下载");
                        }
                        else
                        {
                            LoadScene(patch);
                        }
                    });
                } 
            }
            buttonPatch.gameObject.SetActive(false); 
        }

        private static void LoadScene(Patch patch)
        {
            if (string.IsNullOrEmpty(patch.name))
            {
                return;
            }
            Assets.LoadSceneAsync(ResFormat.GetScene(patch.name));
        }

        private void OnProgress(float progress)
        {
            progressBar.value = progress;
        }

        private void OnMessage(string msg)
        {
            progressText.text = msg;
        }

        private void Back()
        {
            Assets.LoadSceneAsync(ResFormat.GetScene("Loading"));
        }
    }
}