using UnityEngine;
using System.Collections;
using UnityEditor;//注意要引用

public class MyWindow : EditorWindow
{
    [MenuItem("Window/MyWindow")]//在unity菜单Window下有MyWindow选项
    static void Init()
    {
        MyWindow myWindow = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow), false, "MyWindow", true);//创建窗口
        myWindow.Show();//展示
    }
}