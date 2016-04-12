namespace Topshelf.Caching
{
    delegate TKey KeySelector<out TKey, in TValue>(TValue value);
}