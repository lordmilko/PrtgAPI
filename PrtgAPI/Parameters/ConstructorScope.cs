using System;

namespace PrtgAPI.Parameters
{
    class ConstructorScope : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
