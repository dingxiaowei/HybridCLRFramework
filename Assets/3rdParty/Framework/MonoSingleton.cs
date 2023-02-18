using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private const string allSingletonName = "All_Singletons";
    protected static GameObject allSingleton;
    protected static T _instance = null;
    protected static Transform _transform = null;
    protected MonoSingleton() { }
    public static bool IsValid()
    {
        return _instance != null;
    }

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                if (allSingleton == null)
                {
                    allSingleton = GameObject.Find(allSingletonName);
                    if (allSingleton == null)
                    {
                        allSingleton = new GameObject(allSingletonName);
                    }
                }

                var go = new GameObject("_" + typeof(T).Name);
                _instance = go.AddComponent<T>();
                _transform = go.transform;
                _transform.parent = allSingleton.transform;
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (this == _instance)
            _instance = null;
    }

    public virtual void OnDestroy()
    {
        if (this == _instance)
        {
            _instance = null;
        }
    }
}
