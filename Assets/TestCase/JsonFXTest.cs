using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JsonFx.Json;
using UnityEngine.UI;

public class person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public override string ToString()
    {
        return $"{Name} {Age}";
    }
}

public class JsonFXTest : MonoBehaviour
{
    public Button ParseJsonBtn;
    public Text Result;
    JsonReader reader;
    JsonWriter writer;
    void Start()
    {
        reader = new JsonReader();
        writer = new JsonWriter();
        ParseJsonBtn.onClick.AddListener(() =>
        {
            var p = new person() { Name = "dxw", Age = 33 };
            Debug.LogError(writer.Write(p));
            var newPerson = reader.Read<person>(writer.Write(p));
            Debug.LogError(newPerson);
            Result.text = newPerson.ToString();
        });
    }
}
