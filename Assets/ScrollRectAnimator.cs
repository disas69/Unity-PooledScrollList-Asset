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

        public float Influence;
        public float Speed;

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
                contentPosition.y = -Influence;
            }
            else
            {
                contentPosition.x = Influence;
            }

            _scrollRect.content.localPosition = contentPosition;
            StartCoroutine(AnimateScrollRect(Speed));
        }

        private IEnumerator AnimateScrollRect(float speed)
        {
            _canvasGroup.alpha = 0f;

            while (_canvasGroup.alpha < 1f)
            {
                _canvasGroup.alpha += Time.deltaTime * speed;
                yield return null;
            }
        }
    }
}