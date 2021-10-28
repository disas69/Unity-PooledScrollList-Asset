using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PooledScrollList.Controller
{
    public class PooledScrollRectGridController : PooledScrollRectBase
    {
        private readonly List<LayoutElement> _activeSpaceElements = new List<LayoutElement>();

        private Pool<LayoutElement> _spaceElenemtsPool;
        private int _constraintCount;

        public int SpaceElementsPoolCapacity = 10;

        protected override void Awake()
        {
            base.Awake();

            _spaceElenemtsPool = new Pool<LayoutElement>(CreateSpaceElement(ScrollRect, ElementSize), transform, SpaceElementsPoolCapacity);

            var gridLayoutGroup = ScrollRect.content.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup != null)
            {
                _constraintCount = gridLayoutGroup.constraintCount;
                LayoutSpacing = ScrollRect.vertical ? gridLayoutGroup.spacing.y : gridLayoutGroup.spacing.x;
                Padding = gridLayoutGroup.padding;
                ElementSize += LayoutSpacing;
            }
            else
            {
                Debug.LogWarning("Failed to get GridLayoutGroup assigned to ScrollRect's content. PooledScrollRectGridController won't work as expected.");
            }
        }

        protected override void UpdateContent()
        {
            var linesCount = TotalElementsCount % _constraintCount > 0 ? TotalElementsCount / _constraintCount + 1 : TotalElementsCount / _constraintCount;
            AdjustContentSize(ElementSize * linesCount);

            var scrollAreaSize = ExternalViewPort != null ? GetScrollAreaSize(ExternalViewPort) : GetScrollAreaSize(ScrollRect.viewport);
            var elementsVisibleInScrollArea = Mathf.CeilToInt(scrollAreaSize / ElementSize) * _constraintCount;
            var elementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (TotalElementsCount - elementsVisibleInScrollArea)), 0,
                Mathf.Clamp(TotalElementsCount - (elementsVisibleInScrollArea + _constraintCount), 0, int.MaxValue));

            if (elementsCulledAbove != TotalElementsCount - (elementsVisibleInScrollArea + _constraintCount))
            {
                elementsCulledAbove -= elementsCulledAbove % _constraintCount;
            }

            AdjustSpaceElement(elementsCulledAbove);

            var requiredElementsInList = Mathf.Min(elementsVisibleInScrollArea + _constraintCount, TotalElementsCount);

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

        protected override void AdjustSpaceElement(float size)
        {
            var requiredSpaceElements = (int) size;
            if (_activeSpaceElements.Count == requiredSpaceElements)
            {
                return;
            }

            while (_activeSpaceElements.Count < requiredSpaceElements)
            {
                var spaceElement = _spaceElenemtsPool.GetNext();
                spaceElement.transform.SetParent(ScrollRect.content.transform, false);
                spaceElement.transform.SetSiblingIndex(0);
                _activeSpaceElements.Add(spaceElement);
            }

            while (_activeSpaceElements.Count > requiredSpaceElements)
            {
                _spaceElenemtsPool.Return(_activeSpaceElements[_activeSpaceElements.Count - 1]);
                _activeSpaceElements.RemoveAt(_activeSpaceElements.Count - 1);
            }
        }

        protected override void ReorientElement(ReorientMethod reorientMethod, int elementsCulledAbove)
        {
            if (ActiveElements.Count <= 1)
            {
                return;
            }

            var count = Mathf.Abs(elementsCulledAbove - LastElementsCulledAbove);

            if (reorientMethod == ReorientMethod.TopToBottom)
            {
                for (var i = 0; i < count; i++)
                {
                    var top = ActiveElements[0];
                    ActiveElements.RemoveAt(0);
                    ActiveElements.Add(top);

                    top.transform.SetSiblingIndex(ActiveElements[ActiveElements.Count - 2].transform.GetSiblingIndex() + 1);
                    top.Data = Data[elementsCulledAbove + (i + 1 - count) + ActiveElements.Count - 1];
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    var bottom = ActiveElements[ActiveElements.Count - 1];
                    ActiveElements.RemoveAt(ActiveElements.Count - 1);
                    ActiveElements.Insert(0, bottom);

                    bottom.transform.SetSiblingIndex(ActiveElements[1].transform.GetSiblingIndex());
                    bottom.Data = Data[elementsCulledAbove - (i + 1 - count)];
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            for (var i = 0; i < _activeSpaceElements.Count; i++)
            {
                _spaceElenemtsPool.Return(_activeSpaceElements[i]);
            }

            _spaceElenemtsPool.Dispose();
            _activeSpaceElements.Clear();
        }
    }
}