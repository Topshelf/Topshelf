namespace Topshelf.Caching
{
    delegate TValue MissingValueProvider<TKey, TValue>(TKey key);
}