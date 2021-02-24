using UnityEngine;

namespace ColorFill.helper.geometry
{
    public static class VectorExtension
    {
        public const float TwoPI = Mathf.PI * 2;
        
        public static float FindAngleBetweenDegree(this Vector2 vector2, Vector2 other)
        {
            return 360 * FindAngleBetweenRadian(vector2, other) / TwoPI;
        }

        public static float FindAngleBetweenRadian(this Vector2 vector2, Vector2 other)
        {
            float angle = Mathf.Atan2(other.y, other.x) - Mathf.Atan2(vector2.y, vector2.x);

            if(angle < 0) {
                angle += TwoPI;
            }

            return angle;
        }

        public static bool Approximately(this Vector2 vector2, Vector2 other)
        {
            return Mathf.Approximately(vector2.x, other.x) && Mathf.Approximately(vector2.y, other.y);
        }
    }
}