namespace Topshelf.Model
{
    using System;

    public static class Extensions
    {
        public static void IfNotNull(this IServiceController serviceController, Action<IServiceController> action)
        {
            if (serviceController != null)
                action(serviceController);
        }

        public static TT IfNotNull<TT>(this IServiceController serviceController, Func<IServiceController, TT> action, TT ifNull)
        {
            if (serviceController != null)
                return action(serviceController);

            return ifNull;
        }
    }
}