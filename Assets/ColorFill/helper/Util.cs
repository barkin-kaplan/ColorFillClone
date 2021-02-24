using System;
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
        
        public static void DontDestroyOnLoad<T>(GameObject gameObject) where T : UnityEngine.Object
        {
            int gameStatusCount = Object.FindObjectsOfType<T>().Length;
            if (gameStatusCount > 1)
            {
                gameObject.SetActive(false);
                Object.Destroy(gameObject);
            }
            else
            {
                Object.DontDestroyOnLoad(gameObject);
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
    }
}