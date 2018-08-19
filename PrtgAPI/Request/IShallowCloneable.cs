namespace PrtgAPI.Request
{
    interface IShallowCloneable
    {
        object ShallowClone();
    }

    interface IShallowCloneable<T> : IShallowCloneable
    {
        new T ShallowClone();
    }
}
