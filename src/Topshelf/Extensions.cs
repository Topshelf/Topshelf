namespace Topshelf
{
    using System;
    using Internal;

    public static class Extensions
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

        public static string FormatWith(this string format, params string[] args)
        {
            return string.Format(format, args);
        }

        public static void IfNotNull(this IServiceController serviceController, Action<IServiceController> action)
        {
            if (serviceController != null)
                action(serviceController);
        }
    }
}