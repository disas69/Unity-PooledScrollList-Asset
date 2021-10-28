using System.Collections.Generic;
using PooledScrollList.Data;
using PooledScrollList.View;
using UnityEngine;
using UnityEngine.UI;

namespace PooledScrollList.Controller
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class PooledScrollRectBase : MonoBehaviour
    {
        protected enum ReorientMethod
        {
            TopToBottom,
            BottomToTop
        }

        private Pool<PooledElement> _elementsPool;

        protected readonly List<PooledElement> ActiveElements = new List<PooledElement>();
        protected readonly List<PooledData> Data = new List<PooledData>();

        protected ScrollRect ScrollRect;
        protected float LayoutSpacing;
        protected RectOffset Padding;
        protected float ElementSize;
        protected int LastElementsCulledAbove = -1;

        public PooledElement Template;
        public int PoolCapacity = 5;
        public RectTransform ExternalViewPort;
        public PooledDataProvider DataProvider;

        protected int TotalElementsCount
        {
            get { return Data.Count; }
        }

        public virtual void Initialize(List<PooledData> data)
        {
            Data.Clear();
            Data.AddRange(data);
            Initialize();
        }

        public void Initialize()
        {
            LastElementsCulledAbove = -1;
            ResetPosition();
            UpdateContent();
            UpdateActiveElements();
        }

        public void Add(PooledData item)
        {
            Data.Add(item);
            UpdateContent();
            UpdateActiveElements();
        }

        public void Insert(int index, PooledData item)
        {
            Data.Insert(index, item);
            UpdateContent();
            UpdateActiveElements();
        }

        public void Remove(PooledData item)
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
            _elementsPool = new Pool<PooledElement>(Template, transform, PoolCapacity);

            ScrollRect = GetComponent<ScrollRect>();
            ScrollRect.onValueChanged.AddListener(ScrollMoved);

            ElementSize = ScrollRect.vertical ? Template.RectTransform.rect.height : Template.RectTransform.rect.width;
        }

        protected virtual void Start()
        {
            if (DataProvider != null)
            {
                Initialize(DataProvider.GetData());
            }
            else
            {
                Initialize();
            }
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

        protected virtual PooledElement CreateElement(int index)
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
            UpdateActiveElements();
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