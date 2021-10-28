using UnityEngine;
using UnityEngine.UI;

namespace PooledScrollList.Controller
{
    public class PooledScrollRectController : PooledScrollRectBase
    {
        private LayoutElement _spaceElement;

        protected override void Awake()
        {
            base.Awake();

            var layoutGroup = ScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                LayoutSpacing = layoutGroup.spacing;
                Padding = layoutGroup.padding;
                ElementSize += LayoutSpacing;
            }
            else
            {
                Debug.LogWarning("Failed to get HorizontalOrVerticalLayoutGroup assigned to ScrollRect's content. PooledScrollRectController won't work as expected.");
            }

            _spaceElement = CreateSpaceElement(ScrollRect, 0f);
            _spaceElement.transform.SetParent(ScrollRect.content.transform, false);
        }

        protected override void UpdateContent()
        {
            AdjustContentSize(ElementSize * TotalElementsCount);

            var scrollAreaSize = ExternalViewPort != null ? GetScrollAreaSize(ExternalViewPort) : GetScrollAreaSize(ScrollRect.viewport);
            var elementsVisibleInScrollArea = Mathf.CeilToInt(scrollAreaSize / ElementSize);
            var elementsCulledAbove = Mathf.Clamp(Mathf.FloorToInt(GetScrollRectNormalizedPosition() * (TotalElementsCount - elementsVisibleInScrollArea)), 0,
                Mathf.Clamp(TotalElementsCount - (elementsVisibleInScrollArea + 1), 0, int.MaxValue));

            AdjustSpaceElement(elementsCulledAbove * ElementSize);

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

        protected override void AdjustSpaceElement(float size)
        {
            if (size <= 0)
            {
                _spaceElement.ignoreLayout = true;
            }
            else
            {
                _spaceElement.ignoreLayout = false;
                size -= LayoutSpacing;
            }

            if (ScrollRect.vertical)
            {
                _spaceElement.minHeight = size;
            }
            else
            {
                _spaceElement.minWidth = size;
            }

            _spaceElement.transform.SetSiblingIndex(0);
        }

        protected override void ReorientElement(ReorientMethod reorientMethod, int elementsCulledAbove)
        {
            if (ActiveElements.Count <= 1)
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_spaceElement.gameObject);
        }
    }
}