namespace Topshelf.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    class ConcurrentCache<TKey, TValue> :
        Cache<TKey, TValue>
    {
        readonly ConcurrentDictionary<TKey, TValue> _values;
        CacheItemCallback<TKey, TValue> _duplicateValueAdded = ThrowOnDuplicateValue;

        KeySelector<TKey, TValue> _keySelector = DefaultKeyAccessor;
        MissingValueProvider<TKey, TValue> _missingValueProvider = ThrowOnMissingValue;
        CacheItemCallback<TKey, TValue> _valueAddedCallback = DefaultCacheItemCallback;
        CacheItemCallback<TKey, TValue> _valueRemovedCallback = DefaultCacheItemCallback;

        public ConcurrentCache()
        {
            _values = new ConcurrentDictionary<TKey, TValue>();
        }

        public ConcurrentCache(MissingValueProvider<TKey, TValue> missingValueProvider)
            : this()
        {
            _missingValueProvider = missingValueProvider;
        }

        public ConcurrentCache(IEqualityComparer<TKey> equalityComparer)
        {
            _values = new ConcurrentDictionary<TKey, TValue>(equalityComparer);
        }

        public ConcurrentCache(KeySelector<TKey, TValue> keySelector)
        {
            _values = new ConcurrentDictionary<TKey, TValue>();
            _keySelector = keySelector;
        }

        public ConcurrentCache(KeySelector<TKey, TValue> keySelector, IEnumerable<TValue> values)
            : this(keySelector)
        {
            Fill(values);
        }

        public ConcurrentCache(IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public ConcurrentCache(IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            _values = new ConcurrentDictionary<TKey, TValue>(values);
        }

        public ConcurrentCache(IDictionary<TKey, TValue> values,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values)
        {
            _missingValueProvider = missingValueProvider;
        }

        public ConcurrentCache(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> equalityComparer)
        {
            _values = new ConcurrentDictionary<TKey, TValue>(values, equalityComparer);
        }

        public ConcurrentCache(IDictionary<TKey, TValue> values,
            IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values, equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set { _missingValueProvider = value ?? ThrowOnMissingValue; }
        }

        public CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set { _valueAddedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set { _valueRemovedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set { _duplicateValueAdded = value ?? ThrowOnDuplicateValue; }
        }

        public KeySelector<TKey, TValue> KeySelector
        {
            set { _keySelector = value ?? DefaultKeyAccessor; }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public TValue this[TKey key]
        {
            get { return Get(key); }
            set
            {
                TValue existingValue;
                if (_values.TryGetValue(key, out existingValue))
                {
                    _valueRemovedCallback(key, existingValue);
                    _values[key] = value;
                    _valueAddedCallback(key, value);
                }
                else
                    Add(key, value);
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        public TValue Get(TKey key)
        {
            return Get(key, _missingValueProvider);
        }

        public TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            bool added = false;

            TValue value = _values.GetOrAdd(key, x =>
                {
                    added = true;
                    return missingValueProvider(x);
                });

            if (added)
                _valueAddedCallback(key, value);

            return value;
        }

        public TValue GetValue(TKey key, TValue defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            return defaultValue;
        }

        public TValue GetValue(TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            return defaultValueProvider();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _values.TryGetValue(key, out value);
        }

        public bool Has(TKey key)
        {
            return _values.ContainsKey(key);
        }

        public bool HasValue(TValue value)
        {
            TKey key = _keySelector(value);

            return Has(key);
        }

        public void Add(TKey key, TValue value)
        {
            bool added = _values.TryAdd(key, value);
            if (added)
                _valueAddedCallback(key, value);
            else
                _duplicateValueAdded(key, value);
        }

        public void AddValue(TValue value)
        {
            TKey key = _keySelector(value);
            Add(key, value);
        }

        public void Fill(IEnumerable<TValue> values)
        {
            foreach (TValue value in values)
            {
                TKey key = _keySelector(value);
                Add(key, value);
            }
        }

        public void Each(Action<TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Value);
        }

        public void Each(Action<TKey, TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Key, value.Value);
        }

        public bool Exists(Predicate<TValue> predicate)
        {
            return _values.Any(value => predicate(value.Value));
        }

        public bool Find(Predicate<TValue> predicate, out TValue result)
        {
            foreach (var value in _values.Where(value => predicate(value.Value)))
            {
                result = value.Value;
                return true;
            }

            result = default(TValue);
            return false;
        }

        public TKey[] GetAllKeys()
        {
            return _values.Keys.ToArray();
        }

        public TValue[] GetAll()
        {
            return _values.Values.ToArray();
        }

        public void Remove(TKey key)
        {
            TValue existingValue;
            if (_values.TryRemove(key, out existingValue))
                _valueRemovedCallback(key, existingValue);
        }

        public void RemoveValue(TValue value)
        {
            TKey key = _keySelector(value);

            Remove(key);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool WithValue(TKey key, Action<TValue> callback)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
            {
                callback(value);
                return true;
            }

            return false;
        }

        public TResult WithValue<TResult>(TKey key,Func<TValue, TResult> callback,TResult defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return callback(value);

            return defaultValue;
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return callback(value);

            return defaultValue(key);
        }

        static TValue ThrowOnMissingValue(TKey key)
        {
            throw new KeyNotFoundException("The specified element was not found: " + key);
        }

        static void ThrowOnDuplicateValue(TKey key, TValue value)
        {
            throw new ArgumentException(
                string.Format("An item with the same key already exists in the cache: {0}", key), "key");
        }

        static void DefaultCacheItemCallback(TKey key, TValue value)
        {
        }

        static TKey DefaultKeyAccessor(TValue value)
        {
            throw new InvalidOperationException("No default key accessor has been specified");
        }
    }
}