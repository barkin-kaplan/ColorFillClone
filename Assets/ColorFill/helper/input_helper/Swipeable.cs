using System;
using ColorFill.helper.DI;
using UnityEngine;

namespace ColorFill.helper.input_helper
{
    public class Swipeable : MonoBehaviour
    {
        private TouchHelper touchHelper;

        private TouchHelper _touchHelper
        {
            get
            {
                if (touchHelper == null)
                {
                    touchHelper = DIContainer.GetSingle<TouchHelper>();
                }

                return touchHelper;
            }
        }
        public void SetOnSwipe(Action<SwipeEventArgs> callback)
        {
            _touchHelper.SubscribeSwipe(callback,GetInstanceID());   
        }

        private void OnDestroy()
        {
            _touchHelper.UnsubscribeSwipe(GetInstanceID());
        }
    }
}