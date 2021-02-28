using System;
using ColorFill.helper.object_manager;
using UnityEngine;

namespace ColorFill.game.elements.mover.vertical_mover
{
    public class VerticalMover : AMover
    {
        
        private float bottomY;
        private float topY;

        private Vector3 speed = Vector3.down * 1.8f;
        public void SetMoveAmount(float amount)
        {
            topY = transform.position.y;
            bottomY = topY - amount;
        }

        void Update()
        {
            transform.position += speed * Time.deltaTime;
            if (speed.y < 0)
            {
                if (transform.position.y < bottomY)
                {
                    speed *= -1;
                }
            }
            else
            {
                if (transform.position.y > topY)
                {
                    speed *= -1;
                }
            }
        }

        
    }
}
