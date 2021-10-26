using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    [RequireComponent(typeof(ScrollRect), typeof(CanvasGroup))]
    public class ScrollRectAnimator : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        private CanvasGroup _canvasGroup;

        public float Duration = 1f;
        public float Displacement = 50f;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Play()
        {
            var contentPosition = _scrollRect.content.localPosition;

            if (_scrollRect.vertical)
            {
                contentPosition.y = -Displacement;
            }
            else
            {
                contentPosition.x = Displacement;
            }

            _scrollRect.content.localPosition = contentPosition;
            StartCoroutine(AnimateScrollRect(Duration));
        }

        private IEnumerator AnimateScrollRect(float duration)
        {
            _canvasGroup.alpha = 0f;

            var time = Time.time;

            while (Time.time - time < duration)
            {
                _canvasGroup.alpha = (Time.time - time) / duration;
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }
    }
}