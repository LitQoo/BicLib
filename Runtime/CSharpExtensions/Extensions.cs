using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine{
    public class DebugForEditor{
        public static void Log(string _format){
            #if UNITY_EDITOR
            UnityEngine.Debug.Log(_format);
            #endif
        }
    }
}

namespace BicUtil.CSharpExtensions
{
    public static class BicUtilExtensions
    {
        private static System.Random rng = new System.Random();  

        public static IList<T> Shuffle<T>(this IList<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  

            return list;
        }

        public static List<T> Shuffle<T>(this List<T> list)  
        {  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  

            return list;
        }

        public static Vector2Int ConvertToVector2Int(this Vector3 _vector){
            return new Vector2Int((int)_vector.x, (int)_vector.y);
        }

        public static Vector2Int ConvertToVector2Int(this Vector2 _vector){
            return new Vector2Int((int)_vector.x, (int)_vector.y);
        }

        public static Vector2 ConvertToVector2(this Vector2Int _vector){
            return new Vector2(_vector.x, _vector.y);
        }

        public static Vector2Int ToVector2Int(this Vector3 _vector){
            return ConvertToVector2Int(_vector);
        }

        public static Vector2Int ToVector2Int(this Vector2 _vector){
            return ConvertToVector2Int(_vector);
        }

        public static Vector2 ToVector2(this Vector2Int _vector){
            return ConvertToVector2(_vector);
        }

        public static Vector2 CalculateSize(this RectTransform _rectTransform){
            var _corners = new Vector3[4];
            _rectTransform.GetWorldCorners(_corners);

            return new Vector2(_corners[3].x - _corners[0].x, _corners[1].y - _corners[0].y);
        }
    }
}