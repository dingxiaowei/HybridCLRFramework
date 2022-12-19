using libx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TestHybridCLR : MonoBehaviour
{
    string cubePath = "Assets/Demo/TestPrefabs/HotUpdatePrefab.prefab";
    List<AssetRequest> _requests = new List<AssetRequest>();
    List<GameObject> _objs = new List<GameObject>();
    void Start()
    {
        Debug.Log("TestHybridCLR---------测试热更新");
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(200, 0, 200, 50), "加载电视"))
        {
            var request1 = Assets.LoadAssetAsync("Assets/Prefabs/TV.prefab", typeof(GameObject), (rq) =>
            {
                var go = Instantiate(rq.asset) as GameObject;
                if (go != null)
                {
                    go.name = rq.asset.name;
                    //go.transform.localPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
                    _objs.Add(go);
                }
            });
            _requests.Add(request1);
        }
        if (GUI.Button(new Rect(200, 50, 200, 50), "加载Prefab"))
        {
            var request = Assets.LoadAssetAsync(cubePath, typeof(GameObject), (rq) =>
            {
                var go = Instantiate(rq.asset) as GameObject;
                if (go != null)
                {
                    go.name = rq.asset.name;
                    go.transform.localPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
                    _objs.Add(go);
                }
            });
            _requests.Add(request);
        }
        if (GUI.Button(new Rect(200, 150, 200, 50), "卸载Prefab"))
        {
            foreach (var obj in _objs)
            {
                DestroyImmediate(obj);
            }
            _objs.Clear();

            Release();
        }
        if (GUI.Button(new Rect(200, 250, 200, 50), "返回"))
        {
            Release();
            Assets.LoadSceneAsync(ResFormat.GetScene("Level"));
        }

        if (GUI.Button(new Rect(200, 350, 200, 50), "测试加载二进制数据表"))
        {
            Debug.Log("=====================测试查询表格数据");
            var request = Assets.LoadAssetAsync("Assets/Demo/Configs/wolfKill.bytes", typeof(TextAsset), rq =>
            {
                var table = TableManager.ReadTableFromBytes<wolfKillConfig>(rq.asset as TextAsset);
                var datas = table.QueryById(2);
                if (datas != null)
                {
                    var data = datas.FirstOrDefault();
                    Debug.Log($"查询表格数据是:{data.Id} {data.Hunter}");
                }
            });
            _requests.Add(request);
        }
    }

    private void Release()
    {
        foreach (var request in _requests)
        {
            request.Release();
        }

        _requests.Clear();
    }
}
