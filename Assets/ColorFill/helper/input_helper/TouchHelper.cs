using System;
using System.Collections.Generic;
using ColorFill.helper.context;
using ColorFill.helper.geometry;
using UnityEngine;

namespace ColorFill.helper.input_helper
{
    public class TouchHelper : IManualUpdate
    {
        private const float HalfPI = Mathf.PI / 2;

        public TouchHelper()
        {
            SwipeThreshold = Screen.currentResolution.width / (float)25;
            Debug.Log(SwipeThreshold);
        }
        public Touch GetMouseTouch(out bool isClicked)
        {
            isClicked = true;
            TouchPhase touchPhase = TouchPhase.Began;
            Vector2 position = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                touchPhase = TouchPhase.Began;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                touchPhase = TouchPhase.Ended;
            }
            else if (Input.GetMouseButton(0))
            {
                touchPhase = TouchPhase.Moved;
            }
            else
            {
                isClicked = false;
            }
            

            return new Touch()
            {
                position = Input.mousePosition,
                fingerId = 0,
                phase = touchPhase
            };
        }
    
        public void ManualUpdate()
        {
            List<Touch> touches = new List<Touch>();
            TouchPhase touchPhase;
            Vector2 position;
            Touch touch;
            
#if UNITY_EDITOR
            touch = GetMouseTouch(out bool isClicked);
            
            if (!isClicked)
            {
                return;
            }
            
            touches.Add(touch);
#else
        if (Input.touchCount < 1)
        {
            return;
        }
        for (int touchNumber = 0; touchNumber < Input.touchCount; touchNumber++)
        {
            touch = Input.GetTouch(touchNumber);
            touches.Add(touch);
        }
#endif
        
            
            foreach (var t in touches)
            {
                HandleTouch(t);
            }
        }
    
        void HandleTouch(Touch touch)
        {
            var touchPhase = touch.phase;
            switch (touchPhase)
            {
                case TouchPhase.Began:
                    OnSwipeDown(touch);
                    break;
                case TouchPhase.Canceled:
                    break;
                case TouchPhase.Ended:
                    // OnSwipeEnd(touch);
                    break;
                case TouchPhase.Moved:
                    OnSwipeMove(touch);
                    break;
            }
        }

        private class SwipeData
        {
            public Vector2 origPosition;
            public bool firstEventFired;
            public int lastDirection;
        }

        private Dictionary<int, SwipeData> _activeSwipes = new Dictionary<int, SwipeData>();
        private float SwipeThreshold;


        void OnSwipeDown(Touch touch)
        {
            _activeSwipes[touch.fingerId] = new SwipeData()
            {
                origPosition = touch.position,
            };
        }

        void OnSwipeMove(Touch touch)
        {
            var swipeData = _activeSwipes[touch.fingerId];
            var diffVector = touch.position - swipeData.origPosition;
            if (diffVector.magnitude > SwipeThreshold)
            {
                var angle = Vector2.right.FindAngleBetweenRadian(diffVector);
                var direction = (int)(angle / HalfPI + 0.5f); // 0 -> right, 1 -> top, 2 -> left, 3 -> down, 4 -> right...
                direction %= 4;
                Vector2 directionVec = Vector2.right;
                switch (direction)
                {
                    case 1:
                        directionVec = Vector2.up;
                        break;
                    case 2:
                        directionVec = Vector2.left;
                        break;
                    case 3:
                        directionVec = Vector2.down;
                        break;
                }

                if (swipeData.firstEventFired && swipeData.lastDirection == direction)
                {
                    return;
                }
                OnSwipeEvent(new SwipeEventArgs(directionVec,touch.fingerId));
                swipeData.origPosition = touch.position;
                swipeData.lastDirection = direction;
                swipeData.firstEventFired = true;
            }
        }


        private struct SwipeSubscriber
        {
            public int instanceId;
            public Action<SwipeEventArgs> callback;
        }

        private List<SwipeSubscriber> _swipeSubscribers = new List<SwipeSubscriber>();

        public void SubscribeSwipe(Action<SwipeEventArgs> callback, int instanceId)
        {
            _swipeSubscribers.Add(new SwipeSubscriber()
            {
                callback = callback,
                instanceId = instanceId
            });
        }

        public void UnsubscribeSwipe(int instanceId)
        {
            //linear search is okay with such a small list
            _swipeSubscribers.RemoveAll(o => o.instanceId == instanceId);
        }

        void OnSwipeEvent(SwipeEventArgs args)
        {
            Debug.Log($"event fired {args.Direction}");
            for (int i = _swipeSubscribers.Count - 1; i > -1; i--)
            {
                _swipeSubscribers[i].callback(args);
            }
        }
    }
}
