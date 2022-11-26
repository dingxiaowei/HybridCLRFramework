using System;
using LitJson;
using UnityEngine;


public class LitJsonTest : MonoBehaviour
{
    public class Pet
    {
        public string name;
        public int age;
        public string color;

        public Pet() { }
        public Pet(string _name,int _age,string _color)
        {
            name = _name;
            age = _age;
            color = _color;
        }
        
        public override string ToString()
        {
            return $"Name:{name} Age:{age} color:{color}";
        }
    }


    private void Start()
    {
        LitJsonTestUnit();
    }

    private void LitJsonTestUnit()
    {
        string jsonStr = JsonMapper.ToJson(new Pet("Cat",1,"Yellow"));
        Debug.Log($"对象序列化Json字符串:{jsonStr}");

        Pet cat =  JsonMapper.ToObject(typeof(Pet),jsonStr) as Pet;
        Debug.Log($"字符串反序列化对象:{cat.ToString()}");
    }
}