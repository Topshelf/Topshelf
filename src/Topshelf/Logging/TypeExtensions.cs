namespace Topshelf.Logging
{
    using System;

    static class TypeExtensions
    {
        static readonly TypeNameFormatter _typeNameFormatter = new TypeNameFormatter();


        public static string GetTypeName(this Type type)
        {
            return _typeNameFormatter.GetTypeName(type);
        }
    }
}