using libx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActDemoLoader : MonoBehaviour
{
    public static ActDemoLoader Instance;
    public JoyStickMove JoyStickController;
    public XRInputManager XRInputController;
    public CameraFollow CameraFollow;

    private const string ActDemoRootPrefab = "Assets/Demo/ActDemo/Prefabs/ActDemoLoader.prefab";
    AssetRequest _assetRequest;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitActDemo();
        JoyStickController.onMoveStart += OnMoveStart;
        JoyStickController.onMoving += OnMoving;
        JoyStickController.onMoveEnd += OnMoveEnd;
    }

    void InitActDemo()
    {
        _assetRequest = Assets.LoadAssetAsync(ActDemoRootPrefab, typeof(GameObject), (rq) =>
        {
            var go = Instantiate(rq.asset) as GameObject;
            if (go != null)
            {
                go.name = rq.asset.name;
            }
        });
    }

    void OnMoveStart()
    {
        //Debug.Log("OnMoveStart");
    }

    void OnMoving(Vector2 vector2Move)
    {
        //Debug.Log($"OnMoving  x:{vector2Move.x} y:{vector2Move.y}");
    }

    void OnMoveEnd()
    {
        //Debug.Log("OnMoveEnd");
    }

    public void OnDestroy()
    {
        JoyStickController.onMoveStart -= OnMoveStart;
        JoyStickController.onMoving -= OnMoving;
        JoyStickController.onMoveEnd -= OnMoveEnd;

        _assetRequest?.Release();
    }
}
