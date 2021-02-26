using UnityEngine;

namespace ColorFill.game.elements.wall
{
    public class Wall : MonoBehaviour
    {
        public void SetCorners(Vector3 bottomLeft, Vector3 rightTop)
        {
            var center = (bottomLeft + rightTop) / 2;
            transform.position = center;
            var scale = rightTop - bottomLeft;
            scale.z = 1;
            transform.localScale = scale;
        }
    }
}
