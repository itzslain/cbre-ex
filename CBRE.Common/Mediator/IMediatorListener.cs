using System;

namespace CBRE.Common.Mediator
{
    public interface IMediatorListener
    {
        void Notify(string message, object data);
    }
}