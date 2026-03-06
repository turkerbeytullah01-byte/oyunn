using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectAegis.UI
{
    /// <summary>
    /// Static utility class for UI animations
    /// Provides smooth transitions and effects for UI elements
    /// </summary>
    public static class UIAnimator
    {
        #region Fade Animations
        
        /// <summary>
        /// Fades in a CanvasGroup
        /// </summary>
        public static void FadeIn(CanvasGroup group, float duration, Action onComplete = null)
        {
            if (group == null) return;
            
            var runner = group.GetComponent<AnimationRunner>();
            if (runner == null) runner = group.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FadeInCoroutine(group, duration, onComplete));
        }
        
        /// <summary>
        /// Fades out a CanvasGroup
        /// </summary>
        public static void FadeOut(CanvasGroup group, float duration, Action onComplete = null)
        {
            if (group == null) return;
            
            var runner = group.GetComponent<AnimationRunner>();
            if (runner == null) runner = group.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FadeOutCoroutine(group, duration, onComplete));
        }
        
        /// <summary>
        /// Fades a CanvasGroup to a specific alpha
        /// </summary>
        public static void FadeTo(CanvasGroup group, float targetAlpha, float duration, Action onComplete = null)
        {
            if (group == null) return;
            
            var runner = group.GetComponent<AnimationRunner>();
            if (runner == null) runner = group.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FadeToCoroutine(group, targetAlpha, duration, onComplete));
        }
        
        /// <summary>
        /// Fades in a Graphic component
        /// </summary>
        public static void FadeIn(Graphic graphic, float duration, Action onComplete = null)
        {
            if (graphic == null) return;
            
            var runner = graphic.GetComponent<AnimationRunner>();
            if (runner == null) runner = graphic.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FadeGraphicInCoroutine(graphic, duration, onComplete));
        }
        
        /// <summary>
        /// Fades out a Graphic component
        /// </summary>
        public static void FadeOut(Graphic graphic, float duration, Action onComplete = null)
        {
            if (graphic == null) return;
            
            var runner = graphic.GetComponent<AnimationRunner>();
            if (runner == null) runner = graphic.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FadeGraphicOutCoroutine(graphic, duration, onComplete));
        }
        
        #endregion
        
        #region Slide Animations
        
        /// <summary>
        /// Slides a RectTransform in from a direction
        /// </summary>
        public static void SlideIn(RectTransform rect, Vector2 fromOffset, float duration, Action onComplete = null)
        {
            if (rect == null) return;
            
            var runner = rect.GetComponent<AnimationRunner>();
            if (runner == null) runner = rect.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(SlideInCoroutine(rect, fromOffset, duration, onComplete));
        }
        
        /// <summary>
        /// Slides a RectTransform out to a direction
        /// </summary>
        public static void SlideOut(RectTransform rect, Vector2 toOffset, float duration, Action onComplete = null)
        {
            if (rect == null) return;
            
            var runner = rect.GetComponent<AnimationRunner>();
            if (runner == null) runner = rect.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(SlideOutCoroutine(rect, toOffset, duration, onComplete));
        }
        
        /// <summary>
        /// Slides a RectTransform to a target position
        /// </summary>
        public static void SlideTo(RectTransform rect, Vector2 targetPosition, float duration, Action onComplete = null)
        {
            if (rect == null) return;
            
            var runner = rect.GetComponent<AnimationRunner>();
            if (runner == null) runner = rect.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(SlideToCoroutine(rect, targetPosition, duration, onComplete));
        }
        
        #endregion
        
        #region Scale Animations
        
        /// <summary>
        /// Scales a Transform in from zero
        /// </summary>
        public static void ScaleIn(Transform transform, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            var runner = transform.GetComponent<AnimationRunner>();
            if (runner == null) runner = transform.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(ScaleInCoroutine(transform, duration, onComplete));
        }
        
        /// <summary>
        /// Scales a Transform out to zero
        /// </summary>
        public static void ScaleOut(Transform transform, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            var runner = transform.GetComponent<AnimationRunner>();
            if (runner == null) runner = transform.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(ScaleOutCoroutine(transform, duration, onComplete));
        }
        
        /// <summary>
        /// Scales a Transform to a target scale
        /// </summary>
        public static void ScaleTo(Transform transform, Vector3 targetScale, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            var runner = transform.GetComponent<AnimationRunner>();
            if (runner == null) runner = transform.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(ScaleToCoroutine(transform, targetScale, duration, onComplete));
        }
        
        /// <summary>
        /// Pops a Transform (scale up then back)
        /// </summary>
        public static void Pop(Transform transform, float duration, Action onComplete = null)
        {
            if (transform == null) return;
            
            var runner = transform.GetComponent<AnimationRunner>();
            if (runner == null) runner = transform.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(PopCoroutine(transform, duration, onComplete));
        }
        
        #endregion
        
        #region Effect Animations
        
        /// <summary>
        /// Pulses a Transform (continuous scaling)
        /// </summary>
        public static void Pulse(Transform transform, float duration, float scale = 1.1f)
        {
            if (transform == null) return;
            
            var runner = transform.GetComponent<AnimationRunner>();
            if (runner == null) runner = transform.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(PulseCoroutine(transform, duration, scale));
        }
        
        /// <summary>
        /// Shakes a Transform
        /// </summary>
        public static void Shake(Transform transform, float duration, float intensity = 10f)
        {
            if (transform == null) return;
            
            var runner = transform.GetComponent<AnimationRunner>();
            if (runner == null) runner = transform.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(ShakeCoroutine(transform, duration, intensity));
        }
        
        /// <summary>
        /// Bounces a Transform
        /// </summary>
        public static void Bounce(Transform transform, float duration, float height = 30f)
        {
            if (transform == null) return;
            
            var runner = transform.GetComponent<AnimationRunner>();
            if (runner == null) runner = transform.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(BounceCoroutine(transform, duration, height));
        }
        
        /// <summary>
        /// Flashes a Graphic (white flash)
        /// </summary>
        public static void Flash(Graphic graphic, float duration, Action onComplete = null)
        {
            if (graphic == null) return;
            
            var runner = graphic.GetComponent<AnimationRunner>();
            if (runner == null) runner = graphic.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FlashCoroutine(graphic, duration, onComplete));
        }
        
        #endregion
        
        #region Fill Animations
        
        /// <summary>
        /// Animates a Slider to a target value
        /// </summary>
        public static void FillSlider(Slider slider, float targetValue, float duration, Action onComplete = null)
        {
            if (slider == null) return;
            
            var runner = slider.GetComponent<AnimationRunner>();
            if (runner == null) runner = slider.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FillSliderCoroutine(slider, targetValue, duration, onComplete));
        }
        
        /// <summary>
        /// Animates an Image fill amount
        /// </summary>
        public static void FillImage(Image image, float targetFill, float duration, Action onComplete = null)
        {
            if (image == null) return;
            
            var runner = image.GetComponent<AnimationRunner>();
            if (runner == null) runner = image.gameObject.AddComponent<AnimationRunner>();
            
            runner.StartCoroutine(FillImageCoroutine(image, targetFill, duration, onComplete));
        }
        
        #endregion
        
        #region Coroutines
        
        private static IEnumerator FadeInCoroutine(CanvasGroup group, float duration, Action onComplete)
        {
            float elapsed = 0f;
            float startAlpha = group.alpha;
            
            group.interactable = true;
            group.blocksRaycasts = true;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                group.alpha = Mathf.Lerp(startAlpha, 1f, t);
                yield return null;
            }
            
            group.alpha = 1f;
            onComplete?.Invoke();
        }
        
        private static IEnumerator FadeOutCoroutine(CanvasGroup group, float duration, Action onComplete)
        {
            float elapsed = 0f;
            float startAlpha = group.alpha;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                group.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            onComplete?.Invoke();
        }
        
        private static IEnumerator FadeToCoroutine(CanvasGroup group, float targetAlpha, float duration, Action onComplete)
        {
            float elapsed = 0f;
            float startAlpha = group.alpha;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            group.alpha = targetAlpha;
            onComplete?.Invoke();
        }
        
        private static IEnumerator FadeGraphicInCoroutine(Graphic graphic, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Color startColor = graphic.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                graphic.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            graphic.color = targetColor;
            onComplete?.Invoke();
        }
        
        private static IEnumerator FadeGraphicOutCoroutine(Graphic graphic, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Color startColor = graphic.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                graphic.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            graphic.color = targetColor;
            onComplete?.Invoke();
        }
        
        private static IEnumerator SlideInCoroutine(RectTransform rect, Vector2 fromOffset, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector2 targetPosition = rect.anchoredPosition;
            Vector2 startPosition = targetPosition + fromOffset;
            
            rect.anchoredPosition = startPosition;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            rect.anchoredPosition = targetPosition;
            onComplete?.Invoke();
        }
        
        private static IEnumerator SlideOutCoroutine(RectTransform rect, Vector2 toOffset, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector2 startPosition = rect.anchoredPosition;
            Vector2 targetPosition = startPosition + toOffset;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            rect.anchoredPosition = targetPosition;
            onComplete?.Invoke();
        }
        
        private static IEnumerator SlideToCoroutine(RectTransform rect, Vector2 targetPosition, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector2 startPosition = rect.anchoredPosition;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            
            rect.anchoredPosition = targetPosition;
            onComplete?.Invoke();
        }
        
        private static IEnumerator ScaleInCoroutine(Transform transform, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector3 targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
            onComplete?.Invoke();
        }
        
        private static IEnumerator ScaleOutCoroutine(Transform transform, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
            
            transform.localScale = Vector3.zero;
            onComplete?.Invoke();
        }
        
        private static IEnumerator ScaleToCoroutine(Transform transform, Vector3 targetScale, float duration, Action onComplete)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            transform.localScale = targetScale;
            onComplete?.Invoke();
        }
        
        private static IEnumerator PopCoroutine(Transform transform, float duration, Action onComplete)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 popScale = originalScale * 1.2f;
            float halfDuration = duration / 2f;
            
            // Scale up
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale, popScale, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
            
            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(popScale, originalScale, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }
            
            transform.localScale = originalScale;
            onComplete?.Invoke();
        }
        
        private static IEnumerator PulseCoroutine(Transform transform, float duration, float scale)
        {
            Vector3 originalScale = transform.localScale;
            
            while (true)
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Sin(elapsed / duration * Mathf.PI * 2f) * 0.5f + 0.5f;
                    transform.localScale = Vector3.Lerp(originalScale, originalScale * scale, t);
                    yield return null;
                }
            }
        }
        
        private static IEnumerator ShakeCoroutine(Transform transform, float duration, float intensity)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float remaining = 1f - (elapsed / duration);
                
                float x = UnityEngine.Random.Range(-1f, 1f) * intensity * remaining;
                float y = UnityEngine.Random.Range(-1f, 1f) * intensity * remaining;
                
                transform.localPosition = originalPosition + new Vector3(x, y, 0);
                yield return null;
            }
            
            transform.localPosition = originalPosition;
        }
        
        private static IEnumerator BounceCoroutine(Transform transform, float duration, float height)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)) * height * (1f - t);
                
                transform.localPosition = originalPosition + new Vector3(0, bounce, 0);
                yield return null;
            }
            
            transform.localPosition = originalPosition;
        }
        
        private static IEnumerator FlashCoroutine(Graphic graphic, float duration, Action onComplete)
        {
            Color originalColor = graphic.color;
            graphic.color = Color.white;
            
            yield return new WaitForSeconds(duration * 0.5f);
            
            float elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.5f);
                graphic.color = Color.Lerp(Color.white, originalColor, t);
                yield return null;
            }
            
            graphic.color = originalColor;
            onComplete?.Invoke();
        }
        
        private static IEnumerator FillSliderCoroutine(Slider slider, float targetValue, float duration, Action onComplete)
        {
            float elapsed = 0f;
            float startValue = slider.value;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                slider.value = Mathf.Lerp(startValue, targetValue, t);
                yield return null;
            }
            
            slider.value = targetValue;
            onComplete?.Invoke();
        }
        
        private static IEnumerator FillImageCoroutine(Image image, float targetFill, float duration, Action onComplete)
        {
            float elapsed = 0f;
            float startFill = image.fillAmount;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                image.fillAmount = Mathf.Lerp(startFill, targetFill, t);
                yield return null;
            }
            
            image.fillAmount = targetFill;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Helper Component
        
        /// <summary>
        /// Helper component to run coroutines on UI elements
        /// </summary>
        private class AnimationRunner : MonoBehaviour
        {
            private void Awake()
            {
                hideFlags = HideFlags.HideInInspector;
            }
        }
        
        #endregion
    }
}
