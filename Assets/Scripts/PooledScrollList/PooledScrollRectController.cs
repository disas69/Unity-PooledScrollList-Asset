using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.PooledScrollList
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class PooledScrollRectController<T, TElement> : MonoBehaviour where TElement : PooledElement<T>
    {
        private enum ReorientMethod
        {
            TopToBottom,
            BottomToTop
        }

        private float _elementSize;
        private float _layoutSpacing;
        private ScrollRect _scrollRect;
        private LayoutElement _spaceElement;

        protected readonly List<TElement> ActiveElements = new List<TElement>();
        protected int LastElementsCulledAbove = -1;

        public TElement Template;
        public List<T> Data = new List<T>();

        protected Transform Content
        {
            get { return _scrollRect.content; }
        }

        protected int TotalNumElements
        {
            get { return Data.Count; }
        }

        public void Initialize(List<T> data)
        {
            Data = data;
            UpdateContent();

            Initialize();
        }

        public void Initialize()
        {
            ResetPosition();
            UpdateContent();
            UpdateActiveElements();
        }

        public void Add(T item)
        {
            Data.Add(item);
            UpdateContent();
            UpdateActiveElements();
        }

        public void Insert(int index, T item)
        {
            Data.Insert(index, item);
            UpdateContent();
            UpdateActiveElements();
        }

        public void Remove(T item)
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

        protected virtual void OnEnable()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            _scrollRect.onValueChanged.AddListener(ScrollMoved);

            if (_spaceElement == null)
            {
                _spaceElement = CreateSpaceElement(_scrollRect, 0f);
            }

            _elementSize = _scrollRect.vertical ? Template.RectTransform.rect.height : Template.RectTransform.rect.width;

            var layoutGroup = _scrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                _layoutSpacing = layoutGroup.spacing;
                _elementSize += _layoutSpacing;
            }

            Initialize();
        }

        protected void UpdateContent()
        {
            AdjustContentSize(_elementSize * TotalNumElements);

            var scrollAreaSize = _scrollRect.vertical ? ((RectTransform) _scrollRect.transform).rect.height : ((RectTransform) _scrollRect.transform).rect.width;
            var elementsVisibleInScrollArea = Mathf.CeilToInt(scrollAreaSize / _elementSize);
            var elementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (TotalNumElements - elementsVisibleInScrollArea)), 0,
                Mathf.Clamp(TotalNumElements - (elementsVisibleInScrollArea + 1), 0, int.MaxValue));

            AdjustSpaceElement(elementsCulledAbove * _elementSize);

            var requiredElementsInList = Mathf.Min(elementsVisibleInScrollArea + 1, TotalNumElements);

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
                if (!activeElement.Data.Equals(Data[LastElementsCulledAbove + i]))
                {
                    activeElement.Setup(LastElementsCulledAbove + i, Data);
                }
            }
        }

        protected virtual void ResetPosition()
        {
            _scrollRect.normalizedPosition = Vector2.zero;
        }

        protected virtual TElement CreateElement(int index)
        {
            var newElement = Instantiate(Template);
            newElement.transform.SetParent(_scrollRect.content, false);
            newElement.transform.SetSiblingIndex(index);
            newElement.Setup(index, Data);

            return newElement;
        }

        protected virtual void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(ScrollMoved);
        }

        protected virtual void OnDestroy()
        {
            Destroy(_spaceElement.gameObject);

            for (var i = 0; i < ActiveElements.Count; i++)
            {
                Destroy(ActiveElements[i].gameObject);
            }

            ActiveElements.Clear();
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
                currentSize.y = size;
            }
            else
            {
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
                Destroy(ActiveElements[i].gameObject);
            }

            ActiveElements.Clear();

            for (var i = 0; i < requiredElementsInList && i + numElementsCulledAbove < TotalNumElements; i++)
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
                top.Setup(elementsCulledAbove + ActiveElements.Count - 1, Data);
            }
            else
            {
                var bottom = ActiveElements[ActiveElements.Count - 1];
                ActiveElements.RemoveAt(ActiveElements.Count - 1);
                ActiveElements.Insert(0, bottom);

                bottom.transform.SetSiblingIndex(ActiveElements[1].transform.GetSiblingIndex());
                bottom.Setup(elementsCulledAbove, Data);
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