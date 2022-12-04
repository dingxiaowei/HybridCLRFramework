using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using libx;

public class LoadPrefab : MonoBehaviour
{
    public string prefab = "Assets/Prefabs/TestHybridCLR.prefab";
    private AssetRequest request = null;
    void Start()
    {
        request = Assets.LoadAsset(prefab, typeof(GameObject));
        if (request.asset != null)
        {
            Instantiate<GameObject>(request.asset as GameObject);
        }
    }

    private void OnDestroy()
    {
        request?.Release();
    }
}
