using UnityEngine;

namespace ColorFill.helper.input_helper
{
    public class SwipeEventArgs
    {
        public Vector2 Direction;
        public int fingerId;

        public SwipeEventArgs(Vector2 direction,int fingerId)
        {
            this.Direction = direction;
            this.fingerId = fingerId;
        }
    }
}