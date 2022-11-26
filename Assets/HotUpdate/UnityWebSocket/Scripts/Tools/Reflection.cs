using System.Collections.Generic;
using System.Reflection;
using System;

public static class Reflection
{
    public static IEnumerable<Type> GetAllTypes(this Assembly asm)
    {
        return asm.GetTypes();
    }

    /// <summary>
    /// ��ȡ�������еĳ���
    /// </summary>
    /// <returns></returns>
    public static Assembly GetExecutingAssembly()
    {
        return typeof(Reflection).Assembly;
    }

    /// <summary>
    /// ��ȡ�������еķ���
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
    {
        return type.GetMethods();
    }

    /// <summary>
    /// ��չmethod��������һ��ί�з���
    /// </summary>
    /// <param name="method"></param>
    /// <param name="delegateType"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static Delegate CreateDelegate(this MethodInfo method, Type delegateType, object target = null)
    {
        if (method == null)
            return null;
        return target == null ? Delegate.CreateDelegate(delegateType, method) : Delegate.CreateDelegate(delegateType, target, method);
    }

    /// <summary>
    /// ��չί�еĴ�����
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public static MethodInfo GetMethodInfo(this Delegate func)
    {
        return func.Method;
    }
}
