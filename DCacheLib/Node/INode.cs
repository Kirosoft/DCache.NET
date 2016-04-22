using System;

namespace DCacheLib
{
    public interface INode: IComparable
    {
        bool Send(string data);
    }
}
