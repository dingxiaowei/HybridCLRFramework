using UnityEngine;
using System.Collections;
using UnityEditor;//ע��Ҫ����

public class MyWindow : EditorWindow
{
    [MenuItem("Window/MyWindow")]//��unity�˵�Window����MyWindowѡ��
    static void Init()
    {
        MyWindow myWindow = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow), false, "MyWindow", true);//��������
        myWindow.Show();//չʾ
    }
}