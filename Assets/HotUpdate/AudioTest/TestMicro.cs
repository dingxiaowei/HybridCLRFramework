using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class TestMicro : MonoBehaviour
{
    private bool micConnected = false;//麦克风是否连接
    private int minFreq, maxFreq;//最小和最大频率

    public Button StartBtn;
    public Button EndBtn;
    public Button SaveBtn;
    public Button PlayBtn;
    public AudioClip RecordedClip;//录音
    public AudioSource audioSource;//播放的音频
    public Text Infotxt;//提示信息
    public Text Adress;//音频保存地址
    private string fileName;//保存的文件名
    private byte[] data;
    private void Start()
    {
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
        EndBtn.onClick.AddListener(Stop);
        PlayBtn.onClick.AddListener(Player);
        SaveBtn.onClick.AddListener(Save);
    }
    /// <summary>
    /// 开始录音
    /// </summary>
    public void Begin()
    {
        if (micConnected)
        {
            if (!Microphone.IsRecording(null))
            {
                RecordedClip = Microphone.Start(null, false, 60, maxFreq);
                Infotxt.text = "开始录音！";
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
        data = GetRealAudio(ref RecordedClip);
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

            audioSource.clip = RecordedClip;
            audioSource.Play();
            Infotxt.text = "正在播放录音！";
        }
        else
        {
            Infotxt.text = "正在录音中，请先停止录音！";
        }
    }
    /// <summary>
    /// 保存录音
    /// </summary>
    public void Save()
    {
        if (!Microphone.IsRecording(null))
        {
            fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            if (!fileName.ToLower().EndsWith(".wav"))
            {//如果不是“.wav”格式的，加上后缀
                fileName += ".wav";
            }
            string path = Path.Combine(Application.persistentDataPath, fileName);//录音保存路径
            print(path);//输出路径
            Adress.text = path;
            using (FileStream fs = CreateEmpty(path))
            {
                fs.Write(data, 0, data.Length);
                WriteHeader(fs, RecordedClip); //wav文件头
            }
        }
        else
        {
            Infotxt.text = "正在录音中，请先停止录音！";
        }
    }
    /// <summary>
    /// 获取真正大小的录音
    /// </summary>
    /// <param name="recordedClip"></param>
    /// <returns></returns>
    public static byte[] GetRealAudio(ref AudioClip recordedClip)
    {
        int position = Microphone.GetPosition(null);
        if (position <= 0 || position > recordedClip.samples)
        {
            position = recordedClip.samples;
        }
        float[] soundata = new float[position * recordedClip.channels];
        recordedClip.GetData(soundata, 0);
        recordedClip = AudioClip.Create(recordedClip.name, position,
        recordedClip.channels, recordedClip.frequency, false);
        recordedClip.SetData(soundata, 0);
        int rescaleFactor = 32767;
        byte[] outData = new byte[soundata.Length * 2];
        for (int i = 0; i < soundata.Length; i++)
        {
            short temshort = (short)(soundata[i] * rescaleFactor);
            byte[] temdata = BitConverter.GetBytes(temshort);
            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }
        Debug.Log("position=" + position + "  outData.leng=" + outData.Length);
        return outData;
    }
    /// <summary>
    /// 写文件头
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="clip"></param>
    public static void WriteHeader(FileStream stream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        stream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        stream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(stream.Length - 8);
        stream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        stream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        stream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        stream.Write(subChunk1, 0, 4);

        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        stream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        stream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        stream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        stream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        stream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        stream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        stream.Write(subChunk2, 0, 4);
    }
    /// <summary>
    /// 创建wav格式文件头
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private FileStream CreateEmpty(string filepath)
    {
        FileStream fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 44; i++) //为wav文件头留出空间
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }
}