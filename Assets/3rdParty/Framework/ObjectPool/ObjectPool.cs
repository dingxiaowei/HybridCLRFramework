using System.Collections.Generic;

public abstract class BasePool<T>
{
    protected Stack<T> m_objectstack = new Stack<T>();
    public virtual int PoolCount()
    {
        return m_objectstack.Count;
    }

    public virtual T Get()
    {
        if (m_objectstack.Count > 0)
            return m_objectstack.Pop();
        else
            return default(T);
    }

    public virtual void Store(T t)
    {
        m_objectstack.Push(t);
    }
}

/// <summary>
/// ��Ҫnew����ͨ�����
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> : BasePool<T> where T : class, new()
{
    public override T Get()
    {
        return (m_objectstack.Count == 0) ? new T() : m_objectstack.Pop();
    }
}

/// <summary>
/// ����ҪNew�Ķ���أ�����int��float��GameObject��
/// </summary>
/// <typeparam name="T"></typeparam>
public class NotNewObjectPool<T> : BasePool<T>
{

}
