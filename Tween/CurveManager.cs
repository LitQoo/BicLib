using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace BicUtil.Tween
{
    public class CurveManager : MonoBehaviour {
		[Serializable]
		public class PreviewEvent : UnityEvent { }
		[SerializeField]
		public PreviewEvent PreviewAction = new PreviewEvent();
		[SerializeField]
		public List<BezierCurve> Paths;

		public LTDescr GetTween(string _name){
			for(int i = 0; i < Paths.Count; i++){
				if(Paths[i].name == _name){
					return Paths[i].Tween;
				}
			}
			
			return null; 
		}
	}

	public static class Curve {

		public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
			t = Mathf.Clamp01(t);
			float negt = 1f - t;
			return negt * negt * p0 + 2f * negt * t * p1 + t * t * p2;
		}
		public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float negt = 1f - t;
			return negt * negt * negt * p0 + 3f * negt * negt * t * p1 + 3f * negt * t * t * p2 + t * t * t * p3;
		}

		public static Vector3 GetFirstD (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
			return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
		}
			
		public static Vector3 GetFirstD(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float negt = 1f - t;
			return 3f * negt * negt * (p1 - p0) + 6f * negt * t * (p2 - p1) + 3f * t * t * (p3 - p2);
		}
	}	
}