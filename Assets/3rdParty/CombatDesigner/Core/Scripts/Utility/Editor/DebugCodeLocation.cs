using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CombatDesigner
{
    public class DebugCodeLocation
    {
#if UNITY_EDITOR
        // ����asset�򿪵�callback����
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        static bool OnOpenAsset(int instance, int line)
        {
            // �Զ��庯����������ȡlog�е�stacktrace�������ں��档
            string stack_trace = GetStackTrace();
            // ͨ��stacktrace����λ�Ƿ��������Զ����log���ҵ�log������������[FoxLog]���ܺ�ʶ��
            if (!string.IsNullOrEmpty(stack_trace)) // �����Զ����ǩ ���������;ԭ�д�����Ҳ����޸�,��Ҫ�Լ���λ;
            {
                string strLower = stack_trace.ToLower();
                if (strLower.Contains("[combatdebugger]"))
                {
                    Match matches = Regex.Match(stack_trace, @"\(at(.+)\)", RegexOptions.IgnoreCase);
                    string pathline = "";
                    if (matches.Success)
                    {
                        pathline = matches.Groups[1].Value;
                        matches = matches.NextMatch();  // ���������һ�� ������;
                        if (matches.Success)
                        {
                            pathline = matches.Groups[1].Value;
                            pathline = pathline.Replace(" ", "");

                            int split_index = pathline.LastIndexOf(":");
                            string path = pathline.Substring(0, split_index);
                            line = Convert.ToInt32(pathline.Substring(split_index + 1));
                            string fullpath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                            fullpath = fullpath + path;
                            string strPath = fullpath.Replace('/', '\\');
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(strPath, line, 0);
                        }
                        else
                        {
                            Debug.LogError("DebugCodeLocation OnOpenAsset, Error StackTrace");
                        }

                        matches = matches.NextMatch();
                    }
                    return true;
                }
            }
            return false;
        }

        static string GetStackTrace()
        {
            // �ҵ�UnityEditor.EditorWindow��assembly
            var assembly_unity_editor = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
            if (assembly_unity_editor == null) return null;

            // �ҵ���UnityEditor.ConsoleWindow
            var type_console_window = assembly_unity_editor.GetType("UnityEditor.ConsoleWindow");
            if (type_console_window == null) return null;
            // �ҵ�UnityEditor.ConsoleWindow�еĳ�Աms_ConsoleWindow
            var field_console_window = type_console_window.GetField("ms_ConsoleWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (field_console_window == null) return null;
            // ��ȡms_ConsoleWindow��ֵ
            var instance_console_window = field_console_window.GetValue(null);
            if (instance_console_window == null) return null;

            // ���console����ʱ���㴰�ڵĻ�����ȡstacktrace
            if ((object)UnityEditor.EditorWindow.focusedWindow == instance_console_window)
            {
                // ͨ��assembly��ȡ��ListViewState
                var type_list_view_state = assembly_unity_editor.GetType("UnityEditor.ListViewState");
                if (type_list_view_state == null) return null;

                // �ҵ���UnityEditor.ConsoleWindow�еĳ�Աm_ListView
                var field_list_view = type_console_window.GetField("m_ListView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field_list_view == null) return null;

                // ��ȡm_ListView��ֵ
                var value_list_view = field_list_view.GetValue(instance_console_window);
                if (value_list_view == null) return null;

                // �ҵ���UnityEditor.ConsoleWindow�еĳ�Աm_ActiveText
                var field_active_text = type_console_window.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field_active_text == null) return null;

                // ���m_ActiveText��ֵ������������Ҫ��stacktrace
                string value_active_text = field_active_text.GetValue(instance_console_window).ToString();
                return value_active_text;
            }
            return null;
        }
    }
#endif
}