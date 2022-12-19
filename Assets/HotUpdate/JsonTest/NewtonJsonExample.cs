using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Student
{
    private string name;
    public string Name
    {
        get { return name; }
        set { name = value; }
    }
    private int age;
    public int Age
    {
        get { return age; }
        set { age = value; }
    }
    public override string ToString()
    {
        return $"Name:{name} Age:{age}";
    }
}
public class NewtonJsonExample : MonoBehaviour
{
    void Start()
    {
        Student student = new Student();
        student.Name = "GoldenEasy";
        student.Age = 25;
        string strSerializeJSON = JsonConvert.SerializeObject(student);
        Debug.Log(strSerializeJSON);

        var newStudent = JsonConvert.DeserializeObject<Student>(strSerializeJSON);
        newStudent.Name = "newName";
        Debug.Log(newStudent);
    }
}
