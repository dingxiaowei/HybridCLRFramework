using UnityEngine;

public class TableManager
{
    /// <summary>
    /// 加载Resources/Config下面的二进制Table
    /// </summary>
    /// <param name="name">表格名</param>
    /// <returns></returns>
    static TextAsset LoadBinaryConfigTable(string name)
    {
        return Resources.Load<TextAsset>($"Config/{name}");
    }

    /// <summary>
    /// 加载Resources/Config下面的二进制Table
    /// </summary>
    /// <typeparam name="T">对应的表格xxxConfig类</typeparam>
    /// <param name="tableName">表格名</param>
    /// <returns></returns>
    public static T ReadTable<T>(string tableName) where T : IBinarySerializable, new()
    {
        var bytes = LoadBinaryConfigTable(tableName);
        if (bytes != null)
        {
            IBinarySerializable data = new T();
            var readOK = FileManager.ReadBinaryDataFromBytes(bytes.bytes, ref data);
            if (readOK)
            {
                return (T)data;
            }
            else
            {
                Debug.LogError($"{tableName}解析出错  类型{typeof(T)}");
            }
        }
        return default(T);
    }
}
