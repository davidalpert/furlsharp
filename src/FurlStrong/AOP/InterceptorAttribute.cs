using System;
using System.Diagnostics;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public class LogMethodEntryAndExitAttribute : MethodDecoratorAttribute
{
    public override void Init(object instance, MethodBase method, object[] args)
    {
        Console.WriteLine("instance is null? {0}", instance == null);
        if (instance != null)
        {
            Console.WriteLine("instance is of type: {0}", instance.GetType());
        }
    }

    public override void OnEntry(MethodBase method)
    {
        Console.WriteLine("OnEntry: {0}", method.DeclaringType.FullName + "." + method.Name);
    }

    public override void OnExit(MethodBase method)
    {
        Console.WriteLine("OnExit: {0}", method.DeclaringType.FullName + "." + method.Name);
    }

    public override void OnException(MethodBase method, Exception exception)
    {
        Console.WriteLine("OnException: {0} - {1}: {2}", method.DeclaringType.FullName + "." + method.Name,
                          exception.GetType(), exception.Message);
    }
}
