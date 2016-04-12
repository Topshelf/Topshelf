namespace Topshelf.Caching
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A cache implementation that extends the capability of most dictionary style classes to
    /// have a more complete set of methods commonly used in a dictionary scenario.
    /// </summary>
    /// <typeparam name="TKey">The key type of the cache</typeparam>
    /// <typeparam name="TValue">The value type of the cache</typeparam>
    interface Cache<TKey, TValue> :
        ReadCache<TKey, TValue>
    {
        /// <summary>
        /// Sets the missing value provider used by the cache to create requested values that do not exist in the cache
        /// </summary>
        MissingValueProvider<TKey, TValue> MissingValueProvider { set; }

        /// <summary>
        /// Sets the callback that is called when a new value is added to the cache
        /// </summary>
        CacheItemCallback<TKey, TValue> ValueAddedCallback { set; }

        /// <summary>
        /// Sets the callback that is called when a value is removed or replaced from the cache
        /// </summary>
        CacheItemCallback<TKey, TValue> ValueRemovedCallback { set; }

        /// <summary>
        /// Sets the callback that is called when a duplicate value is added to the cache
        /// </summary>
        CacheItemCallback<TKey, TValue> DuplicateValueAdded { set; }

        /// <summary>
        /// Specifies a selector that returns the key from a value which is used when a value is added to the cache
        /// </summary>
        KeySelector<TKey, TValue> KeySelector { set; }

        /// <summary>
        /// References a value in the cache, returning a newly created or existing value for the specified key, and
        /// adding a new or replacing an existing value in the cache
        /// </summary>
        /// <param name="key">The key references the value</param>
        /// <returns>The value from the cache</returns>
        TValue this[TKey key] { get; set; }

        /// <summary>
        /// Get the value for the specified key
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <returns>The matching value if the key exists in the cache, otherwise an exception is thrown</returns>
        TValue Get(TKey key);

        /// <summary>
        /// Get the value for the specified key, overriding the default missing value provider
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="missingValueProvider">An overloaded missing value provider to create the value if it is not found in the cache</param>
        /// <returns>The matching value if the key exists in the cache, otherwise an exception is thrown</returns>
        TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider);

        /// <summary>
        /// Get a value for the specified key, if not found returns the specified default value
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="defaultValue">The default value to return if the key is not found in the cache</param>
        /// <returns>The matching value if it exists in the cache, otherwise the default value</returns>
        TValue GetValue(TKey key, TValue defaultValue);

        /// <summary>
        /// Get a value for the specified key, if not found returns the specified default value
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="defaultValueProvider">The default value to return if the key is not found in the cache</param>
        /// <returns>The matching value if it exists in the cache, otherwise the default value</returns>
        TValue GetValue(TKey key, Func<TValue> defaultValueProvider);

        /// <summary>
        /// Gets a value for the specified key if it exists
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="value">The value if it exists in the cache, otherwise the default value</param>
        /// <returns>True if the item was in the cache, otherwise false</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Adds a value to the cache using the specified key. If the key already exists in the cache, an exception is thrown.
        /// </summary>
        /// <param name="key">The key referencing the value</param>
        /// <param name="value">The value</param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Adds a value to the cache using the KeySelector to extract the key from the value. If the key already exists
        /// in the cache, an exception is thrown.
        /// </summary>
        /// <param name="value">The value</param>
        void AddValue(TValue value);

        /// <summary>
        /// Remove an existing value from the cache
        /// </summary>
        /// <param name="key">The key referencing the value</param>
        void Remove(TKey key);

        /// <summary>
        /// Remove an existing value from the cache, using the KeySelector to extract the key to find the value
        /// </summary>
        /// <param name="value">The value to remove</param>
        void RemoveValue(TValue value);

        /// <summary>
        /// Removes all items from the cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Fills the cache from a list of values, using the KeySelector to extract the key for each value.
        /// </summary>
        /// <param name="values"></param>
        void Fill(IEnumerable<TValue> values);

        /// <summary>
        /// Calls the callback with the value matching the specified key
        /// </summary>
        /// <param name="key">The key referencing the value</param>
        /// <param name="callback">The callback to call</param>
        /// <returns>True if the value exists and the callback was called</returns>
        bool WithValue(TKey key, Action<TValue> callback);

        /// <summary>
        /// Calls the function with the value matching the specified key, returning the result of that function
        /// </summary>
        /// <typeparam name="TResult">The result type of the function</typeparam>
        /// <param name="key">The key references the value</param>
        /// <param name="callback">The function to call</param>
        /// <param name="defaultValue">The default return value if the item does not exist in the cache</param>
        /// <returns>The return value of the function, or the defaultValue specified if the item does not exist in the cache</returns>
        TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, TResult defaultValue);

        /// <summary>
        /// Calls the function with the value matching the specified key, returning the result of that function
        /// </summary>
        /// <typeparam name="TResult">The result type of the function</typeparam>
        /// <param name="key">The key references the value</param>
        /// <param name="callback">The function to call</param>
        /// <param name="defaultValue">The default return value if the item does not exist in the cache</param>
        /// <returns>The return value of the function, or the defaultValue specified if the item does not exist in the cache</returns>
        TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue);
    }
}