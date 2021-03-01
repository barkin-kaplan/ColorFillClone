using UnityEngine;

namespace ColorFill.game.elements.mover.horizontal_mover
{
    public class HorizontalMover : AMover
    {
        protected Vector3 speed = Vector3.right * 1.8f;
        private float leftX;
        private float rightX;

        
        public void SetMoveAmount(float amount)
        {
            leftX = transform.position.x;
            rightX = leftX + amount;
        }

        void Update()
        {
            transform.position += speed * Time.deltaTime;
            if (speed.x < 0)
            {
                if (transform.position.x < leftX)
                {
                    speed *= -1;
                }
            }
            else
            {
                if (transform.position.x > rightX)
                {
                    speed *= -1;
                }
            }
        }
    }
}