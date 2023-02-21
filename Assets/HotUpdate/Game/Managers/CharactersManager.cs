using libx;
using UnityEngine;

namespace ActDemo
{
    public class CharactersManager : ManagerBase<CharactersManager>
    {
        private const string PenguinPrefab = "Assets/Demo/ActDemo/Prefabs/Penguin.prefab";
        AssetRequest _mainPlayerRequest;
        Vector3 mainPlayerBornVector = new Vector3(3.12f, 4.17f, 17.71f);

        public override void Start()
        {
            LoadMainPlayer();
        }

        void LoadMainPlayer()
        {
            _mainPlayerRequest = Assets.LoadAssetAsync(PenguinPrefab, typeof(GameObject), (rq) =>
            {
                var go = GameObject.Instantiate(rq.asset) as GameObject;
                if (go != null)
                {
                    go.name = rq.asset.name;
                    go.transform.localPosition = mainPlayerBornVector;
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;
                }
                ActDemoLoader.Instance.CameraFollow.target = go.transform;
            });
        }
    }
}
