using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestCase
{
    public class Student
    {
        public string Name { get; set; }
    }

    public class Person : IResetable
    {
        public string Name { get; set; }
        public void Reset()
        {
            Debug.Log("Person�����Reset");
        }
    }

    public class TestObjectPool : MonoBehaviour
    {
        ObjectPool<Student> student1Pool = new ObjectPool<Student>();
        ObjectPoolWithReset<Student> student2Pool = new ObjectPoolWithReset<Student>(20);
        ObjectPoolWithTReset<Person> personPool;
        void Start()
        {
            Student s = new Student() { Name = "aladdin" };
            student1Pool.Store(s);
            var newS = student1Pool.Get();
            Debug.Log(newS.Name);

            personPool = new ObjectPoolWithTReset<Person>(10, (person) =>
            {
                person.Name = "p1";
                Debug.Log("�ڴ���е�person����,name:" + person.Name);
            }, (person) =>
            {
                person.Name = "defaultPersonName";
                Debug.Log("��һ�γ�ʼ��������,name:" + person.Name);
            });
            var cachePerson = personPool.Get();
            Debug.Log("��һ�λ�������ɵ�person,name:" + cachePerson.Name);
            personPool.Store(cachePerson);
            var cachePerson1 = personPool.Get();
            Debug.Log("�ڶ��λ�ȡ������е�person,name:" + cachePerson1.Name);
        }
    }
}
