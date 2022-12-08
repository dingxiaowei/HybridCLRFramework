using System;
using System.Collections.Generic;

public interface IResetable
{
    void Reset();
}

/// <summary>
/// 泛型对象本身继承自IResetable方法,双重Reset
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPoolWithTReset<T> where T : class, IResetable, new()
{
    private Stack<T> m_objectStack;

    private Action<T> m_resetAction;
    private Action<T> m_onetimeInitAction;

    public int Count { get { return m_objectStack == null ? 0 : m_objectStack.Count; } }

    public ObjectPoolWithTReset(int initialBufferSize, Action<T> ResetAction = null, Action<T> OnetimeInitAction = null)
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
            //自行重置
            t.Reset();

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

    public void Store(T obj)
    {
        m_objectStack.Push(obj);
    }

    public void Clear()
    {
        m_objectStack.Clear();
    }
}