using UnityEngine;
using System.Collections.Generic;
namespace BicUtil.Tween
{
    [System.Serializable]
	public class BezierCurve{
        [SerializeField]
        public string name;
        [SerializeField]
		public List<Vector3> points;
        [SerializeField]
		public bool loop = false;
        [SerializeField]
		public GameObject targetObject;
        [SerializeField]
		public float animationTime = 1f;
        [SerializeField]
		public LeanTweenType easeType = LeanTweenType.linear;
        [SerializeField]
		public Transform transform; 

		public bool Loop {
			get {
				return loop;
			}
			set {
				loop = value;
				if (value == true) {
					SetControlPoint(0, points[0]);
				}
			}
		}

		public int ControlPointCount {
			get {
				return points.Count;
			}
		}

		public Vector3 GetControlPoint (int index) {
			return points[index];
		}

		public void SetControlPoint (int index, Vector3 point) {
			if (index % 3 == 0) {
				Vector3 delta = point - points[index];
				if (loop) {
					if (index == 0) {
						points[1] += delta;
						points[points.Count - 2] += delta;
						points[points.Count - 1] = point;
					}
					else if (index == points.Count - 1) {
						points[0] = point;
						points[1] += delta;
						points[index - 1] += delta;
					}
					else {
						points[index - 1] += delta;
						points[index + 1] += delta;
					}
				}
				else {
					if (index > 0) {
						points[index - 1] += delta;
					}
					if (index + 1 < points.Count) {
						points[index + 1] += delta;
					}
				}
			}
			points[index] = point;
		}


		public int CurveCount {
			get {
				return (points.Count - 1) / 3;
			}
		}

		public Vector3 GetPoint (float t) {
			int i;
			if (t >= 1f) {
				t = 1f;
				i = points.Count - 4;
			}
			else {
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}
			return transform.TransformPoint(Curve.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
		}

		public Vector3 GetVelocity (float t) {
			int i;
			if (t >= 1f) {
				t = 1f;
				i = points.Count - 4;
			}
			else {
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}
			return transform.TransformPoint(Curve.GetFirstD(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
		}

		public Vector3 GetDirection (float t) {
			return GetVelocity(t).normalized;
		}

		public void AddCurveLast () {
			Vector3 point = points[points.Count - 1];
			point.x += 50f;
			points.Add (point);
			point.x += 50f;
			points.Add (point);
			point.x += 50f;
			points.Add (point);
			if (loop) {
				points[points.Count - 1] = points[0];
			}
		}

		public void AddCurveNext(int _index){
			if(_index + 1 == points.Count){
				AddCurveLast();
			}else{
				var _currentPoint = points[_index];
				var _nextPoint = points[_index + 3];
				var _interval = (_nextPoint - _currentPoint) / 2f;
				points.Insert(_index + 2, _currentPoint + _interval/2f);
				points.Insert(_index + 3, _currentPoint + _interval);
				points.Insert(_index + 4, _currentPoint + _interval*1.5f);
			}

			if (loop) {
				points[points.Count - 1] = points[0];
			}

		}

		public void RemoveCurve(int _index){
			if(_index >= points.Count - 1){
				points.RemoveRange(points.Count - 3, 3);
			}else if(_index == 0 && points.Count > 3){
				points.RemoveRange(0, 3);
			}else{
				points.RemoveRange(_index - 1, 3);
			}
		}

		public void Reset () {
			points = new  List<Vector3> {
				new Vector3(0f, 0f, 0f),
				new Vector3(60f, 0f, 0f),
				new Vector3(30f, 0f, 0f),
				new Vector3(90f, 0f, 0f)
			};

		}

		public List<Vector3> GetPathForLeantween(Vector3 _offset){
			List<Vector3> _path = new List<Vector3>();
			for(int i = 0; i < points.Count; i++){
				
                _path.Add(points[i] + _offset);

                if(i != 0 && i < points.Count - 1 && i % 3 == 0){
                    _path.Add(points[i] + _offset);
                }
			}

			return _path;
		}

		public TweenModel Tween{
			get{
                var _array = GetPathForLeantween(Vector3.zero).ToArray();
                targetObject.transform.localPosition = _array[0];
				//easeType
				Debug.LogWarning("fix here");
				return BicUtil.Tween.BicTween.MoveByBezier(targetObject, _array, animationTime).SetEase(EaseFuncs.Linear);
			}
		}
	}
}