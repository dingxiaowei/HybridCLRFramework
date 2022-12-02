using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace libx
{
    public class LoadAssetTest : MonoBehaviour
    {
        public string[] assetNames;
        public string cubePath = "Assets/Demo/TestPrefabs/Cube1.prefab";
        public Button LoadCube1;
        public Button UnLoadCube1;
        List<AssetRequest> _requests = new List<AssetRequest>();
        List<GameObject> _objs = new List<GameObject>();
        void Start()
        {
            //foreach (var assetName in assetNames)
            //{
            //    var request = Assets.LoadAssetAsync(assetName, typeof(GameObject), (rq) =>
            //    {
            //        var go = Instantiate(rq.asset) as GameObject;
            //        if (go != null)
            //        {Random.Range(0, 4)
            //            go.name = rq.asset.name;
            //            go.transform.localPosition = Vector3.zero;
            //        }
            //    });
            //    list.Add(request);
            //}

            LoadCube1?.onClick.AddListener(() =>
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
            });

            UnLoadCube1?.onClick.AddListener(StartUnload);

        }

        private void StartUnload()
        {
            foreach (var obj in _objs)
            {
                DestroyImmediate(obj);
            }
            _objs.Clear();

            Release();
        }

        private void Release()
        {
            foreach (var request in _requests)
            {
                request.Release();
            }

            _requests.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Release();
                Assets.LoadSceneAsync(ResFormat.GetScene("Level"));
            }
        }
    }
}
