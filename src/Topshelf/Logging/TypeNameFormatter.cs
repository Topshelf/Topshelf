namespace Topshelf.Logging
{
    using System;
    using System.Reflection;
    using System.Text;
    using Caching;

    class TypeNameFormatter
    {
        readonly Cache<Type, string> _cache;
        readonly string _genericArgumentSeparator;
        readonly string _genericOpen;
        readonly string _genericClose;
        readonly string _namespaceSeparator;
        readonly string _nestedTypeSeparator;

        public TypeNameFormatter()
            : this(",", "<", ">", ".", "+")
        {
        }

        public TypeNameFormatter(string genericArgumentSeparator, string genericOpen, string genericClose,
            string namespaceSeparator, string nestedTypeSeparator)
        {
            _genericArgumentSeparator = genericArgumentSeparator;
            _genericOpen = genericOpen;
            _genericClose = genericClose;
            _namespaceSeparator = namespaceSeparator;
            _nestedTypeSeparator = nestedTypeSeparator;

            _cache = new ConcurrentCache<Type, string>(FormatTypeName);
        }

        public string GetTypeName(Type type)
        {
            return _cache[type];
        }

        string FormatTypeName(Type type)
        {
            if (type.GetTypeInfo().IsGenericTypeDefinition)            
                throw new ArgumentException("An open generic type cannot be used as a message name");

            var sb = new StringBuilder("");

            return FormatTypeName(sb, type, null);
        }

        string FormatTypeName(StringBuilder sb, Type type, string scope)
        {
            if (type.GetTypeInfo().IsGenericParameter)
                return "";

            if (type.Namespace != null)
            {
                string ns = type.Namespace;
                if (!ns.Equals(scope))
                {
                    sb.Append(ns);
                    sb.Append(_namespaceSeparator);
                }
            }

            if (type.IsNested)
            {
                FormatTypeName(sb, type.DeclaringType, type.Namespace);
                sb.Append(_nestedTypeSeparator);
            }
            if (type.GetTypeInfo().IsGenericType)
            {
                string name = type.GetGenericTypeDefinition().Name;

                //remove `1
                int index = name.IndexOf('`');
                if (index > 0)
                    name = name.Remove(index);

                sb.Append(name);
                sb.Append(_genericOpen);
                Type[] arguments = type.GetTypeInfo().GenericTypeArguments;
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(_genericArgumentSeparator);
                    }

                    FormatTypeName(sb, arguments[i], type.Namespace);
                }

                sb.Append(_genericClose);
            }
            else
                sb.Append(type.Name);

            return sb.ToString();
        }
    }
}