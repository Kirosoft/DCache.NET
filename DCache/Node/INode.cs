using System;

namespace DCache
{
    public interface INode: IComparable
    {
        bool SendAsync(string data);
    }
}
