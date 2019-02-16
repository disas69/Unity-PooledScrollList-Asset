using UnityEngine;

namespace Assets.Scripts.PooledScrollList
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class PooledElement<T> : MonoBehaviour
    {
        private RectTransform _rectTransform;

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

        public abstract T Data { get; set; }
    }
}