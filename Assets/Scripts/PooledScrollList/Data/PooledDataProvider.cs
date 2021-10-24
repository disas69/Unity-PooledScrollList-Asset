using System.Collections.Generic;
using UnityEngine;

namespace PooledScrollList.Data
{
    public abstract class PooledDataProvider : MonoBehaviour
    {
        public abstract List<PooledData> GetData();
    }
}