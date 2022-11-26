using Google.Protobuf;
using Protoc;
using System.Collections.Generic;
using UnityEngine;

public enum MsgType : int
{
    EPersonMsg = 1,
    EPersonMsg2 = 2
}

public class WebSocketDemo : MonoBehaviour
{
    private string serverUrl = "ws://116.205.247.142:8081";
    private WSSocketSession socketSession;
    private Person person;//测试消息

    private void Awake()
    {
        person = new Person();
        person.Id = 1001;
        person.Name = "dxw";
        person.Address = ByteString.CopyFromUtf8("中国,江苏");
        person.Email = "dingxiaowei2@huawei.com";

        List<Person.Types.PhoneNumber> phoneNumList = new List<Person.Types.PhoneNumber>();
        Person.Types.PhoneNumber phoneNumber1 = new Person.Types.PhoneNumber();
        phoneNumber1.Number = "13262983383";
        phoneNumber1.Type = Person.Types.PhoneType.Home;
        phoneNumList.Add(phoneNumber1);
        Person.Types.PhoneNumber phoneNumber2 = new Person.Types.PhoneNumber();
        phoneNumber2.Number = "13262983384";
        phoneNumber2.Type = Person.Types.PhoneType.Mobile;
        phoneNumList.Add(phoneNumber2);
        person.Phone.AddRange(phoneNumList);
    }
    private void Start()
    {
        var headers = new Dictionary<string, string>();
        headers.Add("User", "dxw");
        socketSession = new WSSocketSession(serverUrl, "1001", headers, (res) =>
        {
            var connectState = res ? "连接成功" : "连接失败";
            Debug.Log($"websocket {connectState}");
        });

        //注册监听方式1：自动注册，无需手动写Register方法，前提：需要在监听的方法写上特性，特性支持多个ID号，并且方式需要是public属性的
        MessageDispatcher.sInstance.ResponseAutoRegister();
        //注册监听方式2：泛型监听，前提：需要手写在MessageIdList脚本里手动绑定Id跟类型的对应关系
        //MessageDispatcher.sInstance.RegisterOnMessageReceived<Person>(OnReceivedPersonMsg);
        //注册监听方式3：手动写上Id与监听方法
        //MessageDispatcher.sInstance.RegisterHandler((int)MsgType.EPersonMsg2, OnReceivedPersonMsg);
    }

    //必须是public的  否则无法被搜集
    [Response((int)MsgType.EPersonMsg, (int)MsgType.EPersonMsg2)]
    public void OnReceivedPersonMsg(object msg)
    {
        if (msg == null)
        {
            Debug.LogError("接受的person消息有误");
            return;
        }
        var person = Person.Parser.ParseFrom(msg as ByteString);
#if DEBUG_NETWORK
        Debug.Log("----打印消息分发的角色------");
        DebugPerson(person);
#endif
    }

    //测试用
    void DebugPerson(Person desPerson)
    {
        Debug.Log("打印person信息");
        Debug.Log(string.Format("ID:{0} Name:{1} Email:{2} Address:{3}", desPerson.Id, desPerson.Name, desPerson.Email, System.Text.Encoding.UTF8.GetString(desPerson.Address.ToByteArray())));
        for (int i = 0; i < desPerson.Phone.Count; i++)
        {
            Debug.Log(string.Format("PhoneNum:{0} Type:{1}", desPerson.Phone[i].Number, desPerson.Phone[i].Type));
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 40), "连接Socket"))
        {
            socketSession?.ConnectAsync();
        }
        if (GUI.Button(new Rect(110, 10, 100, 40), "断开Socket"))
        {
            socketSession?.Disconnect();
        }
        if (GUI.Button(new Rect(210, 10, 100, 40), "重连Socket"))
        {
            socketSession?.ReConnect();
        }
        if (GUI.Button(new Rect(10, 60, 100, 40), "连发100条消息"))
        {
            for (int i = 0; i < 100; i++)
            {
                person.Id = i;
                socketSession.SendAsync((int)MsgType.EPersonMsg, person);
            }
        }
        if (GUI.Button(new Rect(10, 110, 100, 40), "发送消息"))
        {
            socketSession.SendAsync<Person>(person);
        }
        if (GUI.Button(new Rect(10, 160, 100, 40), "收发多个消息"))
        {
            for (int i = 0; i < 10; i++)
            {
                person.Id = i;
                socketSession.SendAsync((int)MsgType.EPersonMsg, person);
                person.Id = 10 + i;
                socketSession.SendAsync((int)MsgType.EPersonMsg2, person);
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
        MessageDispatcher.sInstance.UnregisterOnMessageReceived<Person>(OnReceivedPersonMsg);
        socketSession?.Disconnect();
        MessageDispatcher.sInstance.Dispose();
    }
}
