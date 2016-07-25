using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCache.Services
{
    public interface IService
    {
        string PutLocal(string key, string value, string partitionId = "");
        string GetLocal(string key, string partitionId = "");
    }
}
