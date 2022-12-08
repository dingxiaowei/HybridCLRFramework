using System;
using System.Collections.Generic;

/// <summary>
/// 基本的实现初始化和重置的池(A basic pool with initialization and reset)
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPoolWithReset<T> where T : class, new()
{
    private Stack<T> m_objectStack;
    private Action<T> m_resetAction;
    private Action<T> m_onetimeInitAction;

    /// <summary>
    /// ResetObjectPool
    /// </summary>
    /// <param name="initialBufferSize"></param>
    /// <param name="ResetAction">获取之后初始化</param>
    /// <param name="OnetimeInitAction">新生成之后的初始化</param>
    public ObjectPoolWithReset(int initialBufferSize, Action<T> ResetAction = null, Action<T> OnetimeInitAction = null)
    {
        m_objectStack = new Stack<T>(initialBufferSize);
        m_resetAction = ResetAction;
        m_onetimeInitAction = OnetimeInitAction;
    }

    public T Get()
    {
        if (m_objectStack.Count > 0)
        {
            T t = m_objectStack.Pop();

            if (m_resetAction != null)
                m_resetAction(t);

            return t;
        }
        else
        {
            T t = new T();

            if (m_onetimeInitAction != null)
                m_onetimeInitAction(t);

            return t;
        }
    }

    public int PoolCount()
    {
        return m_objectStack.Count;
    }
}