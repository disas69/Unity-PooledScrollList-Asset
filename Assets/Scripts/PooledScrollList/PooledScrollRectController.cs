using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.PooledScrollList
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class PooledScrollRectController<TData, TElement> : MonoBehaviour where TElement : PooledElement<TData>
    {
        protected enum ReorientMethod
        {
            TopToBottom,
            BottomToTop
        }

        private Pool<TElement> _elementsPool;

        protected readonly List<TElement> ActiveElements = new List<TElement>();

        protected ScrollRect ScrollRect;
        protected float LayoutSpacing;
        protected RectOffset Padding;
        protected float ElementSize;
        protected int LastElementsCulledAbove = -1;

        public TElement Template;
        public int ElementsPoolCapacity;
        public RectTransform ExternalViewPort;
        public List<TData> Data = new List<TData>();

        protected int TotalElementsCount
        {
            get { return Data.Count; }
        }

        public virtual void Initialize(List<TData> data)
        {
            Data = data;
            Initialize();
        }

        public void Initialize()
        {
            LastElementsCulledAbove = -1;
            ResetPosition();
            UpdateContent();
            UpdateActiveElements();
        }

        public void Add(TData item)
        {
            Data.Add(item);
            UpdateContent();
            UpdateActiveElements();
        }

        public void Insert(int index, TData item)
        {
            Data.Insert(index, item);
            UpdateContent();
            UpdateActiveElements();
        }

        public void Remove(TData item)
        {
            Data.Remove(item);
            UpdateContent();
            UpdateActiveElements();
        }

        public void RemoveAt(int index)
        {
            Data.RemoveAt(index);
            UpdateContent();
            UpdateActiveElements();
        }

        public void Clear()
        {
            Data.Clear();
            UpdateContent();
            UpdateActiveElements();
        }

        protected virtual void Awake()
        {
            _elementsPool = new Pool<TElement>(Template, transform, ElementsPoolCapacity);

            ScrollRect = GetComponent<ScrollRect>();
            ScrollRect.onValueChanged.AddListener(ScrollMoved);

            ElementSize = ScrollRect.vertical ? Template.RectTransform.rect.height : Template.RectTransform.rect.width;
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected abstract void UpdateContent();
        protected abstract void AdjustSpaceElement(float size);
        protected abstract void ReorientElement(ReorientMethod reorientMethod, int elementsCulledAbove);

        protected virtual void UpdateActiveElements()
        {
            for (var i = 0; i < ActiveElements.Count; i++)
            {
                var activeElement = ActiveElements[i];
                var activeData = Data[LastElementsCulledAbove + i];

                if (!activeElement.Data.Equals(activeData))
                {
                    activeElement.Data = activeData;
                }
            }
        }

        protected void InitializeElements(int requiredElementsInList, int numElementsCulledAbove)
        {
            for (var i = 0; i < ActiveElements.Count; i++)
            {
                _elementsPool.Return(ActiveElements[i]);
            }

            ActiveElements.Clear();

            for (var i = 0; i < requiredElementsInList && i + numElementsCulledAbove < TotalElementsCount; i++)
            {
                ActiveElements.Add(CreateElement(i + numElementsCulledAbove));
            }
        }

        protected virtual TElement CreateElement(int index)
        {
            var newElement = _elementsPool.GetNext();
            newElement.transform.SetParent(ScrollRect.content, false);
            newElement.transform.SetSiblingIndex(index);
            newElement.Data = Data[index];

            return newElement;
        }

        protected void AdjustContentSize(float size)
        {
            var currentSize = ScrollRect.content.sizeDelta;
            size -= LayoutSpacing;

            if (ScrollRect.vertical)
            {
                if (Padding != null)
                {
                    size += Padding.top + Padding.bottom;
                }

                currentSize.y = size;
            }
            else
            {
                if (Padding != null)
                {
                    size += Padding.left + Padding.right;
                }

                currentSize.x = size;
            }

            ScrollRect.content.sizeDelta = currentSize;
        }

        protected float GetScrollAreaSize(RectTransform viewPort)
        {
            return ScrollRect.vertical ? viewPort.rect.height : viewPort.rect.width;
        }

        protected virtual void ResetPosition()
        {
            if (ScrollRect.vertical)
            {
                ScrollRect.verticalNormalizedPosition = 1f;
            }
            else
            {
                ScrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        protected float GetScrollRectNormalizedPosition()
        {
            return Mathf.Clamp01(ScrollRect.vertical ? 1 - ScrollRect.verticalNormalizedPosition : ScrollRect.horizontalNormalizedPosition);
        }

        protected static LayoutElement CreateSpaceElement(ScrollRect scrollRect, float elementSize)
        {
            var spaceElement = new GameObject("SpaceElement").AddComponent<LayoutElement>();

            if (scrollRect.vertical)
            {
                spaceElement.minHeight = elementSize;
            }
            else
            {
                spaceElement.minWidth = elementSize;
            }

            return spaceElement;
        }

        private void ScrollMoved(Vector2 delta)
        {
            UpdateContent();
        }

        protected virtual void OnDestroy()
        {
            ScrollRect.onValueChanged.RemoveListener(ScrollMoved);

            for (var i = 0; i < ActiveElements.Count; i++)
            {
                _elementsPool.Return(ActiveElements[i]);
            }

            _elementsPool.Dispose();
            ActiveElements.Clear();
        }
    }
}