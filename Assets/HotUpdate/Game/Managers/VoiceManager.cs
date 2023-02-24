using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActDemo
{
    public class VoiceManager : ManagerBase<VoiceManager>
    {
        public AudioSource AudioSourceComp;
        private bool micConnected = false;//麦克风是否连接
        private AudioClip recordedClip;//录音
        private int minFreq, maxFreq;//最小和最大频率
        private byte[] data;
        private bool isInRecord;
        private bool isPlayRecord;
        private float recordStartTime;

        public Google.Protobuf.ByteString AudioClipByteString
        {
            get
            {
                if (recordedClip != null)
                    return Google.Protobuf.ByteString.CopyFrom(WavUtility.FromAudioClip(recordedClip));
                return Google.Protobuf.ByteString.Empty;
            }
        }

        public override void Start()
        {
            base.Start();
            InitDevice();
        }

        void InitDevice()
        {
            AudioSourceComp = Camera.main.GetComponent<AudioSource>();
            if (AudioSourceComp == null)
            {
                Debug.LogError("主相机下没有挂AudioSouce组件");
            }
            if (Microphone.devices.Length <= 0)
            {
                Debug.LogError("缺少麦克风设备");
            }
            else
            {
                Debug.Log("设备名称为：" + Microphone.devices[0].ToString());
                micConnected = true;
                Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);
                if (minFreq == 0 && maxFreq == 0)
                {
                    maxFreq = 44100;
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        public void BeginRecord()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //设置不息屏
            Application.runInBackground = true;
            if (micConnected)
            {
                if (!Microphone.IsRecording(null))
                {
                    isInRecord = true;
                    recordStartTime = Time.realtimeSinceStartup;
                    recordedClip = Microphone.Start(null, false, 60, maxFreq);
                }
                else
                {
                    Debug.LogError("正在录音中，请勿重复调用录音！");
                }
            }
            else
            {
                Debug.LogError("请确认麦克风设备是否已连接！");
            }
        }
        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopRecord()
        {
            isInRecord = false;
            data = ToolUtility.GetRealAudio(ref recordedClip);
            Microphone.End(null);
        }

        public void PlayRecord(byte[] voice)
        {
            if (AudioSourceComp == null)
            {
                Debug.LogError("VoiceManager没有初始化AudioSource组件");
                return;
            }

            if (voice != null && voice.Length > 0)
            {
                //if (!Microphone.IsRecording(null))
                //{
                AudioSourceComp.clip = WavUtility.ToAudioClip(voice);
                AudioSourceComp.Play();
                //}
            }
        }

        public void PlayRecord(AudioClip audioClip)
        {
            if (audioClip == null)
            {
                Debug.LogError("请传入audioClip");
                return;
            }
            byte[] rclip = WavUtility.FromAudioClip(audioClip);
            PlayRecord(rclip);
        }

        public void PlayRecord(Google.Protobuf.ByteString audio)
        {
            if (audio != null && !Google.Protobuf.ByteString.Empty.Equals(audio))
            {
                var audioClip = WavUtility.ToAudioClip(audio.ToByteArray());
                PlayRecord(audioClip);
            }
        }

        public void PlayRecord()
        {
            if (recordedClip != null)
            {
                AudioSourceComp.clip = recordedClip;
                AudioSourceComp.Play();
            }
            else
            {
                Debug.LogError("请先调用BeginRecord,然后调用StopRecord");
            }
        }
    }
}
