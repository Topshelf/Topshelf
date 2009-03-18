namespace Topshelf
{
    using System;

    public static class AppDomainExtensions
    {
        public static T CreateInstanceAndUnwrap<T>(this AppDomain domain)
        {
            var type = typeof(T);
            return (T)domain.CreateInstanceAndUnwrap(type.Assembly.GetName().FullName, type.FullName);
        }
        public static T CreateInstanceAndUnwrap<T>(this AppDomain domain, params object[] args)
        {
            var type = typeof(T);
            return (T)domain.CreateInstanceAndUnwrap(type.Assembly.GetName().FullName, type.FullName, true, 0, null, args, null, null, null);
        }
    }
}