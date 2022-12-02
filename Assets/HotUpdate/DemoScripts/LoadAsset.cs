using System.Collections.Generic;
using UnityEngine;

namespace libx
{
    public class LoadAsset : MonoBehaviour
    {
        public string[] assetNames;

        List<AssetRequest> list = new List<AssetRequest>();
        // Use this for initialization
        void Start()
        {
            foreach (var assetName in assetNames)
            {
                var request = Assets.LoadAssetAsync(assetName, typeof(GameObject), (rq) =>
                {
                    var go = Instantiate(rq.asset) as GameObject;
                    if (go != null)
                    {
                        go.name = rq.asset.name;
                        var holder = go.GetComponent<ObjectHolder>();
                        if (holder.objects != null)
                        {
                            foreach (var o in holder.objects)
                            {
                                var go2 = Instantiate(o) as GameObject;
                                go2.name = o.name;
                            }
                        }
                    }
                });
                list.Add(request);
                //request.completed += delegate
                //{
                //    var go = Instantiate(request.asset) as GameObject;
                //    if (go != null)
                //    {
                //        go.name = request.asset.name;
                //        var holder = go.GetComponent<ObjectHolder>();
                //        if (holder.objects != null)
                //        {
                //            foreach (var o in holder.objects)
                //            {
                //                var go2 = Instantiate(o) as GameObject;
                //                go2.name = o.name;
                //            }
                //        } 
                //    }
                //}; 
            }
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    item.Release();
                }
                list.Clear();
                Assets.LoadSceneAsync(ResFormat.GetScene("Level"));
            }
        }
    }
}
