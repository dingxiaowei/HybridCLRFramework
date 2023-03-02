using System.IO;
using UnityEditor;
using UnityEngine;

public class Tools : Editor
{
    [MenuItem("Tools/StartServer")]
    public static void StartServer()
    {
        var unityProjPath = $"{Application.dataPath}";
        var dir = Directory.CreateDirectory(unityProjPath);
        var parentDir = dir.Parent;
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = $"{parentDir}/Server/src/Samples/ConsoleApp/bin/Debug/Fleck.Samples.ConsoleApp.exe";
        process.StartInfo = startInfo;
        process.Start();
    }
}
