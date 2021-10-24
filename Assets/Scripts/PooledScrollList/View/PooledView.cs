using PooledScrollList.Data;
using UnityEngine;

namespace PooledScrollList.View
{
    public abstract class PooledView : MonoBehaviour
    {
        public PooledData Data { get; private set; }

        public virtual void SetData(PooledData data)
        {
            Data = data;
        }
    }
}