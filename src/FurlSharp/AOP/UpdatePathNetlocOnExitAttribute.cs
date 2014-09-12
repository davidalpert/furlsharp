using System;
using System.Reflection;

namespace FurlSharp.AOP
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly)]
    public class UpdatePathNetlocOnExitAttribute : Attribute
    {
        private Furl _furl;

        public void Init(object instance, MethodBase method, object[] args)
        {
            _furl = instance as Furl;
            if (_furl == null)
            {
                throw new InvalidOperationException(
                    GetType().Name 
                    + " must be placed on an attribute of the " 
                    + typeof (Furl).Name + " class.");
            }
        }

        public void OnEntry()
        {
        }

        public void OnExit()
        {
            _furl.Path.UseNetloc(_furl.NetLoc);
        }

        public void OnException(Exception exception)
        {
        }
    }
}
