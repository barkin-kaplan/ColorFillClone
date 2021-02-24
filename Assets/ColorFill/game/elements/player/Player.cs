using ColorFill.helper.geometry;
using ColorFill.helper.input_helper;
using UnityEngine;

namespace ColorFill.game.elements
{
    public class Player : MonoBehaviour
    {
        private Swipeable _swipeable;
        private Vector2 _currentMoveDirection = Vector2.zero;
        private Rigidbody _rigidbody;

        void Awake()
        {
            _swipeable = GetComponent<Swipeable>();
            _rigidbody = GetComponent<Rigidbody>();
            SetSwipeEvent();
        }

        void SetSwipeEvent()
        {
            _swipeable.SetOnSwipe(args =>
            {
                Turn(args.Direction);
            });
        }

        void Turn(Vector2 direction)
        {
            //can't go opposite way
            if ((_currentMoveDirection + direction).Approximately(Vector2.zero))
            {
                return;
            }

            _currentMoveDirection = direction;
            _rigidbody.velocity = _currentMoveDirection * 5f;
        }

    }
}