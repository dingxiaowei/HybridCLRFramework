using Google.Protobuf;
using Protoc;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProtoTest : MonoBehaviour
{
    void Start()
    {
        #region protobuf3 测试
        Debug.Log("Protobuf3 TestCase");

        Person person = new Person();
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

        var desPerson1 = Person.Parser.ParseFrom(person.ToByteArray());
        DebugPerson(desPerson1);

        //序列化
        using (MemoryStream personMs = new MemoryStream())
        {
            person.WriteTo(personMs);
            Debug.Log("bytes length:" + personMs.Length.ToString());
            //获取ByteString
            //var byteStr = ByteString.CopyFrom(personMs.ToArray());

            //反序列化
            personMs.Position = 0;
            Person desPerson = Person.Parser.ParseFrom(personMs);
            DebugPerson(desPerson);
            Debug.Log("测试OK");
        }
        #endregion
    }

    void DebugPerson(Person desPerson)
    {
        Debug.Log(string.Format("ID:{0} Name:{1} Email:{2} Address:{3}", desPerson.Id, desPerson.Name, desPerson.Email, System.Text.Encoding.UTF8.GetString(desPerson.Address.ToByteArray())));
        for (int i = 0; i < desPerson.Phone.Count; i++)
        {
            Debug.Log(string.Format("PhoneNum:{0} Type:{1}", desPerson.Phone[i].Number, desPerson.Phone[i].Type));
        }
    }
}
