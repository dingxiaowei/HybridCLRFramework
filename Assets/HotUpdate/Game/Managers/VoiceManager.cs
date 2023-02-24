using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActDemo
{
    public class VoiceManager : ManagerBase<VoiceManager>
    {
        public AudioSource AudioSourceComp;
        private bool micConnected = false;//��˷��Ƿ�����
        private AudioClip recordedClip;//¼��
        private int minFreq, maxFreq;//��С�����Ƶ��
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
                Debug.LogError("�������û�й�AudioSouce���");
            }
            if (Microphone.devices.Length <= 0)
            {
                Debug.LogError("ȱ����˷��豸");
            }
            else
            {
                Debug.Log("�豸����Ϊ��" + Microphone.devices[0].ToString());
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
        /// ��ʼ¼��
        /// </summary>
        public void BeginRecord()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //���ò�Ϣ��
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
                    Debug.LogError("����¼���У������ظ�����¼����");
                }
            }
            else
            {
                Debug.LogError("��ȷ����˷��豸�Ƿ������ӣ�");
            }
        }
        /// <summary>
        /// ֹͣ¼��
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
                Debug.LogError("VoiceManagerû�г�ʼ��AudioSource���");
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
                Debug.LogError("�봫��audioClip");
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
                Debug.LogError("���ȵ���BeginRecord,Ȼ�����StopRecord");
            }
        }
    }
}
