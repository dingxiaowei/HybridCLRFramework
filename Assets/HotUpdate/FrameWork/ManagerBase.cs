using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerBase<T> : IMgr where T : IMgr, new()
{
    static protected T mInstance;

    static public T Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new T();
            }
            return mInstance;
        }
    }

    protected ManagerBase()
    {
    }

    virtual public void Init()
    {

    }

    virtual public void Start()
    {

    }

    virtual public void Update()
    {

    }

    virtual public void LateUpdate()
    {

    }

    virtual public void FixedUpdate()
    {

    }

    virtual public void OnDestroy()
    {

    }

    virtual public void InitOnLogin()
    {

    }

    virtual public void ClearOnLogout()
    {

    }

    virtual public void InitOnLoadScene()
    {

    }
}
