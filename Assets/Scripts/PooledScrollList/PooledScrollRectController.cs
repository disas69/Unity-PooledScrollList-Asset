using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.PooledScrollList
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class PooledScrollRectController<TData, TElement> : MonoBehaviour where TElement : PooledElement<TData>
    {
        private enum ReorientMethod
        {
            TopToBottom,
            BottomToTop
        }

        private Pool<TElement> _pool;
        private float _elementSize;
        private float _layoutSpacing;
        private RectOffset _padding;
        private ScrollRect _scrollRect;
        private LayoutElement _spaceElement;

        protected readonly List<TElement> ActiveElements = new List<TElement>();
        protected int LastElementsCulledAbove = -1;

        public TElement Template;
        public int PoolCapacity;
        public RectTransform ExternalViewPort;
        public List<TData> Data = new List<TData>();

        protected int TotalElementsCount
        {
            get { return Data.Count; }
        }

        public void Initialize(List<TData> data)
        {
            Data = data;
            Initialize();
        }

        public void Initialize()
        {
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
            _pool = new Pool<TElement>(Template, transform, PoolCapacity);

            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(ScrollMoved);

            _spaceElement = CreateSpaceElement(_scrollRect, 0f);
            _elementSize = _scrollRect.vertical ? Template.RectTransform.rect.height : Template.RectTransform.rect.width;

            var layoutGroup = _scrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                _layoutSpacing = layoutGroup.spacing;
                _padding = layoutGroup.padding;
                _elementSize += _layoutSpacing;
            }
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected void UpdateContent()
        {
            AdjustContentSize(_elementSize * TotalElementsCount);

            var scrollAreaSize = ExternalViewPort != null ? GetScrollAreaSize(ExternalViewPort) : GetScrollAreaSize(_scrollRect.viewport);
            var elementsVisibleInScrollArea = Mathf.CeilToInt(scrollAreaSize / _elementSize);
            var elementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (TotalElementsCount - elementsVisibleInScrollArea)), 0,
                Mathf.Clamp(TotalElementsCount - (elementsVisibleInScrollArea + 1), 0, int.MaxValue));

            AdjustSpaceElement(elementsCulledAbove * _elementSize);

            var requiredElementsInList = Mathf.Min(elementsVisibleInScrollArea + 1, TotalElementsCount);

            if (ActiveElements.Count != requiredElementsInList)
            {
                InitializeElements(requiredElementsInList, elementsCulledAbove);
            }
            else if (LastElementsCulledAbove != elementsCulledAbove)
            {
                ReorientElement(elementsCulledAbove > LastElementsCulledAbove ? ReorientMethod.TopToBottom : ReorientMethod.BottomToTop, elementsCulledAbove);
            }

            LastElementsCulledAbove = elementsCulledAbove;
        }

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

        protected virtual void ResetPosition()
        {
            if (_scrollRect.vertical)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
            else
            {
                _scrollRect.horizontalNormalizedPosition = 0f;
            }
        }

        protected virtual TElement CreateElement(int index)
        {
            var newElement = _pool.GetNext();
            newElement.transform.SetParent(_scrollRect.content, false);
            newElement.transform.SetSiblingIndex(index);
            newElement.Data = Data[index];

            return newElement;
        }

        protected virtual void OnDestroy()
        {
            for (var i = 0; i < ActiveElements.Count; i++)
            {
                _pool.Return(ActiveElements[i]);
            }

            ActiveElements.Clear();

            _pool.Dispose();
            _scrollRect.onValueChanged.RemoveListener(ScrollMoved);

            Destroy(_spaceElement.gameObject);
        }

        private float GetScrollAreaSize(RectTransform viewPort)
        {
            return _scrollRect.vertical ? viewPort.rect.height : viewPort.rect.width;
        }

        private void ScrollMoved(Vector2 delta)
        {
            UpdateContent();
        }

        private void AdjustContentSize(float size)
        {
            var currentSize = _scrollRect.content.sizeDelta;
            size -= _layoutSpacing;

            if (_scrollRect.vertical)
            {
                if (_padding != null)
                {
                    size += _padding.top + _padding.bottom;
                }

                currentSize.y = size;
            }
            else
            {
                if (_padding != null)
                {
                    size += _padding.left + _padding.right;
                }

                currentSize.x = size;
            }

            _scrollRect.content.sizeDelta = currentSize;
        }

        private void AdjustSpaceElement(float size)
        {
            if (size <= 0)
            {
                _spaceElement.ignoreLayout = true;
            }
            else
            {
                _spaceElement.ignoreLayout = false;
                size -= _layoutSpacing;
            }

            if (_scrollRect.vertical)
            {
                _spaceElement.minHeight = size;
            }
            else
            {
                _spaceElement.minWidth = size;
            }

            _spaceElement.transform.SetSiblingIndex(0);
        }

        private float GetScrollRectNormalizedPosition()
        {
            return Mathf.Clamp01(_scrollRect.vertical ? 1 - _scrollRect.verticalNormalizedPosition : _scrollRect.horizontalNormalizedPosition);
        }

        private void InitializeElements(int requiredElementsInList, int numElementsCulledAbove)
        {
            for (var i = 0; i < ActiveElements.Count; i++)
            {
                _pool.Return(ActiveElements[i]);
            }

            ActiveElements.Clear();

            for (var i = 0; i < requiredElementsInList && i + numElementsCulledAbove < TotalElementsCount; i++)
            {
                ActiveElements.Add(CreateElement(i + numElementsCulledAbove));
            }
        }

        private void ReorientElement(ReorientMethod reorientMethod, int elementsCulledAbove)
        {
            if (ActiveElements.Count == 0)
            {
                return;
            }

            if (reorientMethod == ReorientMethod.TopToBottom)
            {
                var top = ActiveElements[0];
                ActiveElements.RemoveAt(0);
                ActiveElements.Add(top);

                top.transform.SetSiblingIndex(ActiveElements[ActiveElements.Count - 2].transform.GetSiblingIndex() + 1);
                top.Data = Data[elementsCulledAbove + ActiveElements.Count - 1];
            }
            else
            {
                var bottom = ActiveElements[ActiveElements.Count - 1];
                ActiveElements.RemoveAt(ActiveElements.Count - 1);
                ActiveElements.Insert(0, bottom);

                bottom.transform.SetSiblingIndex(ActiveElements[1].transform.GetSiblingIndex());
                bottom.Data = Data[elementsCulledAbove];
            }
        }

        private static LayoutElement CreateSpaceElement(ScrollRect scrollRect, float elementSize)
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

            spaceElement.transform.SetParent(scrollRect.content.transform, false);
            return spaceElement;
        }
    }
}