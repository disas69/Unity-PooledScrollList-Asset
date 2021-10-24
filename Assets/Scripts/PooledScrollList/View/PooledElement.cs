using PooledScrollList.Data;
using UnityEngine;

namespace PooledScrollList.View
{
    [RequireComponent(typeof(PooledView))]
    [RequireComponent(typeof(RectTransform))]
    public class PooledElement : MonoBehaviour
    {
        private PooledView _pooledView;
        private RectTransform _rectTransform;

        public PooledView PooledView
        {
            get
            {
                if (_pooledView == null)
                {
                    _pooledView = GetComponent<PooledView>();
                }

                return _pooledView;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        public PooledData Data
        {
            get => PooledView.Data;
            set => PooledView.SetData(value);
        }
    }
}