namespace Topshelf.Caching
{
    delegate void CacheItemCallback<in TKey, in TValue>(TKey key, TValue value);
}