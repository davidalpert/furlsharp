using System;
using System.Reflection;

namespace FurlStrong.AOP
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly)]
    public class LogMethodEntryAndExitAttribute : Attribute
    {
        private MethodBase _method;

        public void Init(object instance, MethodBase method, object[] args)
        {
            _method = method;
            Console.WriteLine("instance is null? {0}", instance == null);
            if (instance != null)
            {
                Console.WriteLine("instance is of type: {0}", instance.GetType());
            }
        }

        public void OnEntry()
        {
            Console.WriteLine("OnEntry: {0}", _method.DeclaringType.FullName + "." + _method.Name);
        }

        public void OnExit()
        {
            Console.WriteLine("OnExit: {0}", _method.DeclaringType.FullName + "." + _method.Name);
        }

        public void OnException(Exception exception)
        {
            Console.WriteLine("OnException: {0} - {1}: {2}", _method.DeclaringType.FullName + "." + _method.Name,
                              exception.GetType(), exception.Message);
        }
    }
}
