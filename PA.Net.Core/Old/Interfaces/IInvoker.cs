using System;

namespace PA.Net.Interfaces
{
    public interface IInvoker
    {
        void Invoke(Action code);
    }
}
