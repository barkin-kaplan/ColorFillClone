using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ColorFill.helper
{
    public static class Util
    {
        /// <summary>
        /// Starts a thread and then returns it.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Thread ThreadStart(Action action)
        {
            Thread t = new Thread(() =>
            {
                action();
            });
            t.Start();
            return t;
        }
        
        public static bool DontDestroyOnLoad<T>(GameObject gameObject) where T : UnityEngine.Object
        {
            int gameStatusCount = Object.FindObjectsOfType<T>().Length;
            if (gameStatusCount > 1)
            {
                gameObject.SetActive(false);
                Object.Destroy(gameObject);
                return false;
            }
            else
            {
                Object.DontDestroyOnLoad(gameObject);
                return true;
            }
        }

        public static int ParseInt(string s)
        {
            return Int32.Parse(s, CultureInfo.InvariantCulture);
        }

        public static int ParseInt(char s)
        {
            return ParseInt(s.ToString());
        }

        public static float ParseFloat(string s)
        {
            return float.Parse(s,CultureInfo.InvariantCulture);
        }

        public static void EditorLog(string s)
        {
#if UNITY_EDITOR
            Debug.Log(s);
#endif
        }

        public static string ToString(int i)
        {
            return i.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToString(float value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static Vector3 ToVec3(Vector2 vec2)
        {
            return new Vector3(vec2.x, vec2.y, 0);
        }
        
        public static Vector2 ToVec2(Vector3 vec3)
        {
            return new Vector2(vec3.x,vec3.y);
        }

        public static bool CompareIntegerEqual(float x, float y)
        {
            return (int) x == (int) y;
        }

        public static float Tangent(float degree)
        {
            return Mathf.Sin(degree) / Mathf.Cos(degree);
        }

        public static bool HaveInstersection<T>(HashSet<T> set1, HashSet<T> set2) where T : IEquatable<T>
        {
            foreach (var item in set1)
            {
                if (set2.Contains(item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}