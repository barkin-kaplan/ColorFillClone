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
        
        public static bool Approximately(this Vector3 vector3, Vector3 other)
        {
            return Mathf.Approximately(vector3.x, other.x) && Mathf.Approximately(vector3.y, other.y) && Mathf.Approximately(vector3.z,other.z);
        }

        public static bool CompareIntegerEqual(this Vector3 vector3, Vector3 other)
        {
            return Util.CompareIntegerEqual(vector3.x, other.x) &&
                   Util.CompareIntegerEqual(vector3.y, other.y) &&
                   Util.CompareIntegerEqual(vector3.z, other.z);
        }

        public static float DotUnit(this Vector3 vector3, Vector3 other)
        {
            return Vector3.Dot(vector3 / vector3.magnitude, other / other.magnitude);
        }
    }
}