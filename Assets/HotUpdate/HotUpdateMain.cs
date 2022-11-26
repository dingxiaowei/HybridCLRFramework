using Protoc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HotUpdateMain : MonoBehaviour
{
    private string serverUrl = "ws://116.205.247.142:8081";
    //private string serverUrl = "ws://127.0.0.1:8081";
    private WSSocketSession socketSession;
    public string text;

    public Button StartBtn;
    public Button StopBtn;
    public Button PlayBtn;
    private bool micConnected = false;//麦克风是否连接
    AudioClip RecordedClip;//录音
    public AudioSource audioSource;//播放的音频
    public Text Infotxt;//提示信息
    public Text Adress;//音频保存地址
    private int minFreq, maxFreq;//最小和最大频率
    private byte[] data;
    private bool isInRecord;
    private bool isPlayRecord;
    private float recordStartTime;

    public static System.Action<byte[]> OnReceiveVoiceMsg;
    /// <summary>
    /// 开始录音
    /// </summary>
    public void Begin()
    {
        Debug.Log("点击开始录音------------");
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //设置不息屏
        Application.runInBackground = true;
        if (micConnected)
        {
            if (!Microphone.IsRecording(null))
            {
                isInRecord = true;
                recordStartTime = Time.realtimeSinceStartup;
                RecordedClip = Microphone.Start(null, false, 60, maxFreq);
                Infotxt.text = $"开始录音:{(Time.realtimeSinceStartup - recordStartTime):0.00}s";
            }
            else
            {
                Infotxt.text = "正在录音中，请勿重复点击Start！";
            }
        }
        else
        {
            Infotxt.text = "请确认麦克风设备是否已连接！";
        }
    }
    /// <summary>
    /// 停止录音
    /// </summary>
    public void Stop()
    {
        isInRecord = false;
        data = TestMicro.GetRealAudio(ref RecordedClip);
        Microphone.End(null);
        Infotxt.text = "录音结束！";
    }

    /// <summary>
    /// 播放录音
    /// </summary>
    public void Player()
    {
        if (!Microphone.IsRecording(null))
        {
            isPlayRecord = true;
            audioSource.clip = RecordedClip;
            audioSource.Play();
            Infotxt.text = $"正在播放录音:{audioSource.time:0.00}s";
        }
        else
        {
            Infotxt.text = "正在录音中，请先停止录音！";
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 40), "连接Socket"))
        {
            socketSession?.ConnectAsync();
        }
        if (GUI.Button(new Rect(10, 60, 100, 40), "连发100消息"))
        {
            for (int i = 0; i < 100; i++)
            {
                S2C_EnterMap msg = new S2C_EnterMap();
                msg.Message = "服务器向客户端发送的进入地图消息";
                msg.RpcId = 1;
                msg.UnitId = i;
                msg.Error = 0;
                List<Protoc.UnitInfo> unitInfos = new List<UnitInfo>();
                unitInfos.Add(new UnitInfo() { UnitId = 222, X = 1, Y = 1, Z = 1 });
                msg.Units.AddRange(unitInfos);
                socketSession.SendAsync((int)OuterOpcode.S2C_EnterMapResponse, msg);
                Debug.Log("-----------客户端向服务器发送消息");
                msg.Debug();
            }
        }
        if (GUI.Button(new Rect(10, 110, 100, 40), "发送消息"))
        {
            S2C_EnterMap msg = new S2C_EnterMap();
            msg.Message = "服务器向客户端发送的进入地图消息";
            msg.RpcId = 1;
            msg.UnitId = 0;
            msg.Error = 0;
            List<Protoc.UnitInfo> unitInfos = new List<UnitInfo>();
            unitInfos.Add(new UnitInfo() { UnitId = 222, X = 1, Y = 1, Z = 1 });
            msg.Units.AddRange(unitInfos);
            if (RecordedClip != null)
            {
                byte[] rclip = WavUtility.FromAudioClip(RecordedClip);
                msg.Voice = Google.Protobuf.ByteString.CopyFrom(rclip);
                Debug.Log($"发送语音消息的长度:{rclip.Length}");
            }
            else
            {
                msg.Voice = Google.Protobuf.ByteString.Empty;
            }
            socketSession.SendAsync((int)OuterOpcode.S2C_EnterMapResponse, msg);
        }

        if (isInRecord)
        {
            Infotxt.text = $"开始录音:{(Time.realtimeSinceStartup - recordStartTime):0.00}s";
        }

        if (isPlayRecord)
        {
            if (audioSource.isPlaying)
            {
                Infotxt.text = $"正在播放录音:{audioSource.time:0.00}s";
            }
            else
            {
                isPlayRecord = false;
            }
        }
    }

    private void Update()
    {
        if (socketSession != null && socketSession.IsConnected)
        {
            socketSession.Update();
        }
    }

    private void OnDestroy()
    {
        socketSession?.Disconnect();
        MessageDispatcher.sInstance.Dispose();
    }

    string BytesToString(byte[] bytes)
    {
        return System.Text.Encoding.Default.GetString(bytes);
    }

    byte[] StringToBytes(string str)
    {
        return System.Text.Encoding.Default.GetBytes(str);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("这个热更新脚本挂载在prefab上，打包成ab。通过从ab中实例化prefab成功还原");
        Debug.LogFormat("hello, {0}.", text);

        gameObject.AddComponent<CreateByCode>();

        Debug.Log("=======看到此条日志代表你成功运行了示例项目的热更新代码=======");

        gameObject.AddComponent<HotUpdateLogic>();
        Debug.Log("===============添加一个热更测试组件==============");

        Debug.Log("==============测试一个自定义的程序集=============");
        var testLogic = gameObject.AddComponent<TestLogic>();
        testLogic.TestDebug();

        //LitJson测试
        gameObject.AddComponent<LitJsonTest>();

        //添加Proto测试
        //gameObject.AddComponent<ProtobufTest>();

        Debug.Log("=====================测试查询表格数据");
        //测试表格
        var table = TableManager.ReadTable<wolfKillConfig>("wolfKill");
        var datas = table.QueryById(2);
        if (datas != null)
        {
            var data = datas.FirstOrDefault();
            Debug.Log($"查询表格数据是:{data.Id} {data.Hunter}");
        }
        //测试加载prefab
        AssetBundle prefabAb = AssetBundle.LoadFromFile($"{Application.streamingAssetsPath}/prefabs1");
        GameObject testPrefab1 = Instantiate(prefabAb.LoadAsset<GameObject>("TestPrefab.prefab"));
        prefabAb.Unload(false);
        Debug.Log("测试加载的prefab");

        var headers = new Dictionary<string, string>();
        headers.Add("User", "dxw");
        socketSession = new WSSocketSession(serverUrl, "1001", headers, (res) =>
        {
            var connectState = res ? "连接成功" : "连接失败";
            Debug.Log($"websocket {connectState}");
        });

        MessageDispatcher.sInstance.AutoRegistHandlers();

        //语音测试
        if (Microphone.devices.Length <= 0)
        {
            Infotxt.text = "缺少麦克风设备！";
        }
        else
        {
            Infotxt.text = "设备名称为：" + Microphone.devices[0].ToString() + "请点击Start开始录音！";
            micConnected = true;
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
            if (minFreq == 0 && maxFreq == 0)
            {
                maxFreq = 44100;
            }
        }
        StartBtn.onClick.AddListener(Begin);
        StopBtn.onClick.AddListener(Stop);
        PlayBtn.onClick.AddListener(Player);

        OnReceiveVoiceMsg = (voice) =>
        {
            if (voice != null && voice.Length > 0)
            {
                if (!Microphone.IsRecording(null))
                {
                    audioSource.clip = WavUtility.ToAudioClip(voice);
                    audioSource.Play();
                    Infotxt.text = "正在播放录音！";
                }
                else
                {
                    Infotxt.text = "正在录音中，请先停止录音！";
                }
            }
        };
    }
}
