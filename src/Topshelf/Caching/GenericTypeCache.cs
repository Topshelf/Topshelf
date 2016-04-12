namespace Topshelf.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    class GenericTypeCache<TInterface> :
        Cache<Type, TInterface>
    {
        readonly Cache<Type, TInterface> _cache;
        readonly Type _genericType;

        GenericTypeCache(Type genericType, Cache<Type, TInterface> cache)
        {
            if (!genericType.IsGenericType)
                throw new ArgumentException("The type specified must be a generic type", "genericType");
            if (genericType.GetGenericArguments().Length != 1)
                throw new ArgumentException("The generic type must have a single generic argument");
            _genericType = genericType;
            _cache = cache;
        }

        /// <summary>
        /// Constructs a cache for the specified generic type
        /// </summary>
        /// <param name="genericType">The generic type to close</param>
        public GenericTypeCache(Type genericType)
            : this(genericType, new ConcurrentCache<Type, TInterface>(DefaultMissingValueProvider(genericType)))
        {
        }

        /// <summary>
        /// Constructs a cache for the specified generic type.
        /// </summary>
        /// <param name="genericType">The generic type to close</param>
        /// <param name="missingValueProvider">The implementation provider, which must close the generic type with the passed type</param>
        public GenericTypeCache(Type genericType, MissingValueProvider<Type, TInterface> missingValueProvider)
            : this(genericType, new ConcurrentCache<Type, TInterface>(missingValueProvider))
        {
        }

        public Type GenericType
        {
            get { return _genericType; }
        }

        public IEnumerator<TInterface> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        public int Count
        {
            get { return _cache.Count; }
        }

        public bool Has(Type key)
        {
            return _cache.Has(key);
        }

        public bool HasValue(TInterface value)
        {
            return _cache.HasValue(value);
        }

        public void Each(Action<TInterface> callback)
        {
            _cache.Each(callback);
        }

        public void Each(Action<Type, TInterface> callback)
        {
            _cache.Each(callback);
        }

        public bool Exists(Predicate<TInterface> predicate)
        {
            return _cache.Exists(predicate);
        }

        public bool Find(Predicate<TInterface> predicate, out TInterface result)
        {
            return _cache.Find(predicate, out result);
        }

        public Type[] GetAllKeys()
        {
            return _cache.GetAllKeys();
        }

        public TInterface[] GetAll()
        {
            return _cache.GetAll();
        }

        public MissingValueProvider<Type, TInterface> MissingValueProvider
        {
            set { _cache.MissingValueProvider = value; }
        }

        public CacheItemCallback<Type, TInterface> ValueAddedCallback
        {
            set { _cache.ValueAddedCallback = value; }
        }

        public CacheItemCallback<Type, TInterface> DuplicateValueAdded
        {
            set { _cache.DuplicateValueAdded = value; }
        }

        public CacheItemCallback<Type, TInterface> ValueRemovedCallback
        {
            set { _cache.ValueRemovedCallback = value; }
        }

        public KeySelector<Type, TInterface> KeySelector
        {
            set { _cache.KeySelector = value; }
        }

        public TInterface Get(Type key)
        {
            return _cache.Get(key);
        }

        public TInterface Get(Type key, MissingValueProvider<Type, TInterface> missingValueProvider)
        {
            return _cache.Get(key, missingValueProvider);
        }

        public TInterface GetValue(Type key, TInterface defaultValue)
        {
            return _cache.GetValue(key, defaultValue);
        }

        public TInterface GetValue(Type key, Func<TInterface> defaultValueProvider)
        {
            return _cache.GetValue(key, defaultValueProvider);
        }

        public bool TryGetValue(Type key, out TInterface value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public TInterface this[Type key]
        {
            get { return _cache[key]; }
            set { _cache[key] = value; }
        }

        public void Add(Type key, TInterface value)
        {
            _cache.Add(key, value);
        }

        public void AddValue(TInterface value)
        {
            _cache.AddValue(value);
        }

        public void Remove(Type key)
        {
            _cache.Remove(key);
        }

        public void RemoveValue(TInterface value)
        {
            _cache.RemoveValue(value);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Fill(IEnumerable<TInterface> values)
        {
            _cache.Fill(values);
        }

        public bool WithValue(Type key, Action<TInterface> callback)
        {
            return _cache.WithValue(key, callback);
        }

        public TResult WithValue<TResult>(Type key,
            Func<TInterface, TResult> callback,
            TResult defaultValue)
        {
            return _cache.WithValue(key, callback, defaultValue);
        }

        public TResult WithValue<TResult>(Type key, Func<TInterface, TResult> callback, Func<Type, TResult> defaultValue)
        {
            return _cache.WithValue(key, callback, defaultValue);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        static MissingValueProvider<Type, TInterface> DefaultMissingValueProvider(Type genericType)
        {
            return type =>
                {
                    Type buildType = genericType.MakeGenericType(type);

                    return (TInterface)Activator.CreateInstance(buildType);
                };
        }
    }
}