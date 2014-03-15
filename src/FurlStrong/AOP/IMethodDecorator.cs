using System;
using System.Reflection;

// ReSharper disable CheckNamespace

/// <summary>
/// Derive from this to implement Method interception with Fody.
/// https://github.com/Fody/MethodDecorator
/// </summary>
/// <remarks>
/// This interface must be present in the assembly without any namespaces
/// so that MethodDecorator.Fody can find it.
/// </remarks>
public interface IMethodDecorator
{
    void Init(object instance, MethodBase method, object[] args);
    void OnEntry();
    void OnExit();
    void OnException(Exception exception);
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public abstract class MethodDecoratorAttribute : Attribute{
    public abstract void Init(object instance, MethodBase method, object[] args);
    public abstract void OnEntry(MethodBase method);
    public abstract void OnExit(MethodBase method);
    public abstract void OnException(MethodBase method, Exception exception);
}
// ReSharper restore CheckNamespace