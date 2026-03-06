using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectAegis.Mobile
{
    /// <summary>
    /// Mobile-optimized input manager supporting touch gestures
    /// </summary>
    public class MobileInputManager : MonoBehaviour
    {
        public static MobileInputManager Instance { get; private set; }
        
        [Header("Touch Settings")]
        [SerializeField] private float tapThreshold = 0.2f;
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float doubleTapThreshold = 0.3f;
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 3f;
        
        // Events
        public System.Action<Vector2> OnTap;
        public System.Action<Vector2> OnDoubleTap;
        public System.Action<Vector2, Vector2> OnSwipe;
        public System.Action<float> OnPinchZoom;
        public System.Action<Vector2> OnDrag;
        
        // State
        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isDragging;
        private float lastTapTime;
        private Vector2 lastTapPosition;
        
        // Pinch tracking
        private float lastPinchDistance;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        void Update()
        {
            HandleTouchInput();
        }
        
        private void HandleTouchInput()
        {
            // Single touch
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStartPosition = touch.position;
                        touchStartTime = Time.time;
                        isDragging = false;
                        break;
                        
                    case TouchPhase.Moved:
                        float moveDistance = Vector2.Distance(touch.position, touchStartPosition);
                        if (moveDistance > swipeThreshold)
                        {
                            isDragging = true;
                            OnDrag?.Invoke(touch.deltaPosition);
                        }
                        break;
                        
                    case TouchPhase.Ended:
                        HandleTouchEnd(touch);
                        break;
                }
            }
            // Pinch zoom (two fingers)
            else if (Input.touchCount == 2)
            {
                HandlePinchZoom();
            }
        }
        
        private void HandleTouchEnd(Touch touch)
        {
            float touchDuration = Time.time - touchStartTime;
            float moveDistance = Vector2.Distance(touch.position, touchStartPosition);
            
            // Check for tap
            if (touchDuration < tapThreshold && moveDistance < swipeThreshold)
            {
                // Check for double tap
                if (Time.time - lastTapTime < doubleTapThreshold && 
                    Vector2.Distance(touch.position, lastTapPosition) < swipeThreshold)
                {
                    OnDoubleTap?.Invoke(touch.position);
                    lastTapTime = 0; // Reset
                }
                else
                {
                    OnTap?.Invoke(touch.position);
                    lastTapTime = Time.time;
                    lastTapPosition = touch.position;
                }
            }
            // Check for swipe
            else if (moveDistance > swipeThreshold)
            {
                Vector2 swipeDirection = (touch.position - touchStartPosition).normalized;
                OnSwipe?.Invoke(touchStartPosition, swipeDirection);
            }
        }
        
        private void HandlePinchZoom()
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            // Calculate current distance
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);
            
            // If just started, record initial distance
            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                lastPinchDistance = currentDistance;
                return;
            }
            
            // Calculate zoom delta
            float deltaDistance = currentDistance - lastPinchDistance;
            float zoomDelta = deltaDistance * zoomSpeed * 0.01f;
            
            OnPinchZoom?.Invoke(zoomDelta);
            
            lastPinchDistance = currentDistance;
        }
        
        /// <summary>
        /// Check if touch is over UI element
        /// </summary>
        public static bool IsTouchOverUI(Vector2 screenPosition)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPosition;
            
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            return results.Count > 0;
        }
        
        /// <summary>
        /// Get world position from touch
        /// </summary>
        public static Vector3 GetWorldPosition(Vector2 screenPosition, Camera camera = null)
        {
            if (camera == null) camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(screenPosition);
            
            // Assuming ground plane at y=0
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            
            return Vector3.zero;
        }
    }
}
