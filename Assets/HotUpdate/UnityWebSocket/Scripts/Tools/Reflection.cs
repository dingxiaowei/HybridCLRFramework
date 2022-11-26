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
    /// 获取正在运行的程序集
    /// </summary>
    /// <returns></returns>
    public static Assembly GetExecutingAssembly()
    {
        return typeof(Reflection).Assembly;
    }

    /// <summary>
    /// 获取正在运行的方法
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
    {
        return type.GetMethods();
    }

    /// <summary>
    /// 扩展method方法创建一个委托方法
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
    /// 扩展委托的代理方法
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public static MethodInfo GetMethodInfo(this Delegate func)
    {
        return func.Method;
    }
}
