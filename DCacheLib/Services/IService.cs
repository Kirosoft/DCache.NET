using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCacheLib.Services
{
    public interface IService
    {
        string put(string key, string value, string partitionId = "");
        string get(string key, string partitionId = "");
    }
}
