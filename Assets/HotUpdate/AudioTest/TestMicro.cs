using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class TestMicro : MonoBehaviour
{
    private bool micConnected = false;//��˷��Ƿ�����
    private int minFreq, maxFreq;//��С�����Ƶ��

    public Button StartBtn;
    public Button EndBtn;
    public Button SaveBtn;
    public Button PlayBtn;
    public AudioClip RecordedClip;//¼��
    public AudioSource audioSource;//���ŵ���Ƶ
    public Text Infotxt;//��ʾ��Ϣ
    public Text Adress;//��Ƶ�����ַ
    private string fileName;//������ļ���
    private byte[] data;
    private void Start()
    {
        if (Microphone.devices.Length <= 0)
        {
            Infotxt.text = "ȱ����˷��豸��";
        }
        else
        {
            Infotxt.text = "�豸����Ϊ��" + Microphone.devices[0].ToString() + "����Start��ʼ¼����";
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
    /// ��ʼ¼��
    /// </summary>
    public void Begin()
    {
        if (micConnected)
        {
            if (!Microphone.IsRecording(null))
            {
                RecordedClip = Microphone.Start(null, false, 60, maxFreq);
                Infotxt.text = "��ʼ¼����";
            }
            else
            {
                Infotxt.text = "����¼���У������ظ����Start��";
            }
        }
        else
        {
            Infotxt.text = "��ȷ����˷��豸�Ƿ������ӣ�";
        }
    }
    /// <summary>
    /// ֹͣ¼��
    /// </summary>
    public void Stop()
    {
        data = GetRealAudio(ref RecordedClip);
        Microphone.End(null);
        Infotxt.text = "¼��������";
    }
    /// <summary>
    /// ����¼��
    /// </summary>
    public void Player()
    {
        if (!Microphone.IsRecording(null))
        {

            audioSource.clip = RecordedClip;
            audioSource.Play();
            Infotxt.text = "���ڲ���¼����";
        }
        else
        {
            Infotxt.text = "����¼���У�����ֹͣ¼����";
        }
    }
    /// <summary>
    /// ����¼��
    /// </summary>
    public void Save()
    {
        if (!Microphone.IsRecording(null))
        {
            fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            if (!fileName.ToLower().EndsWith(".wav"))
            {//������ǡ�.wav����ʽ�ģ����Ϻ�׺
                fileName += ".wav";
            }
            string path = Path.Combine(Application.persistentDataPath, fileName);//¼������·��
            print(path);//���·��
            Adress.text = path;
            using (FileStream fs = CreateEmpty(path))
            {
                fs.Write(data, 0, data.Length);
                WriteHeader(fs, RecordedClip); //wav�ļ�ͷ
            }
        }
        else
        {
            Infotxt.text = "����¼���У�����ֹͣ¼����";
        }
    }
    /// <summary>
    /// ��ȡ������С��¼��
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
    /// д�ļ�ͷ
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
    /// ����wav��ʽ�ļ�ͷ
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private FileStream CreateEmpty(string filepath)
    {
        FileStream fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 44; i++) //Ϊwav�ļ�ͷ�����ռ�
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }
}