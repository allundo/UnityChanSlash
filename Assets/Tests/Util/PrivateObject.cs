using System;
using System.Reflection;

public class PrivateObject
{
    private readonly object obj;
    public PrivateObject(object obj)
    {
        this.obj = obj;
    }

    public object Invoke(string methodName, params object[] args)
    {
        var type = obj.GetType();
        var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance;
        try
        {
            return type.InvokeMember(methodName, bindingFlags, null, obj, args);
        }
        catch (Exception e)
        {
            throw e.InnerException;
        }
    }
}