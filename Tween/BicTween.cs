using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BicUtil.Tween
{

    public static class BicTween {
		private static int id = 0;
		public static int GetNewId(){
			id++;
			return id;
		}

		public static void PlayMecanimAnimation(Animator _animator, string _stateHashName){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				_animator.Play(_stateHashName);
			}else{
				_animator.Play(_stateHashName, -1, 0f);
				LeanTweenOnEditor.AddUpdateCallback((_id, _dt)=>{
					if(_animator.isActiveAndEnabled == true){
						_animator.Update(_dt);
					}

					if(_animator.isActiveAndEnabled == false || _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1){
						LeanTweenOnEditor.RemoveUpdateCallback(_id);
					}
			 	});
			}
			#else
			_animator.Play(_stateHashName);
			#endif
		}

		static public TweenModel MoveSlider(UnityEngine.UI.Slider _slider, float _to, float _time){
			return MoveSlider(_slider, _slider.value, _to, _time);
		}

		static public TweenModel MoveSlider(UnityEngine.UI.Slider _slider, float _from, float _to, float _time){
			return BicTween.Value(_from, _to, _time).SubscribeUpdate(_value=>{
				_slider.value = _value.x;
			});
		}

		static public TweenModel Levelup(UnityEngine.UI.Slider _slider, float _toValue, float _time, float _nextMax, float _levelUpDelay){
			if(_toValue >= _slider.maxValue && _nextMax > 0){
				//levelup
				
				float _delay = _time;
				BicTween.MoveSlider(_slider, _slider.maxValue, _delay);

				_delay += 1f/60f + _levelUpDelay;
				BicTween.Delay(_delay).SubscribeComplete(()=>{
					_slider.minValue = _slider.maxValue;
					_slider.maxValue = _nextMax;
					BicTween.MoveSlider(_slider, _toValue, _time);
				});

				_delay += 1f/60f + _time;
				return BicTween.Delay(_delay);

			}else{
				BicTween.MoveSlider(_slider, _toValue, _time);
				return BicTween.Delay(_time);
			}
		}



		////////////////////////////////////////////////

		private static float previousRealTime;
		public static float realDeltaTime = 0;
		private static TweenPool defaultPool = null;
		public static TweenPool DefaultPool{
			get{
				if(defaultPool == null){
					defaultPool = createPool();
				}

				return defaultPool;
			}
		}
		
		#region Logic
		private static void init(){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				var _manager = DefaultPool;
			}
			#else
				var _manager = BicTweenManager.Instance;
			#endif
		}

		private static TweenPool createPool(){
			var _container = new GameObject();  
			_container.name = "TweenPool";  
			var _pool = _container.AddComponent(typeof(TweenPool)) as TweenPool;  
			_pool.DontDestroy();
			return _pool;
		}

		public static void UpdateDeltaTime(){
			#if UNITY_EDITOR
			realDeltaTime = Time.realtimeSinceStartup - previousRealTime;
			previousRealTime = Time.realtimeSinceStartup;
			#endif
			
		}
		#endregion

		#region Animation

		public static TweenModel CreateModel(TweenPool _pool){
			TweenModel _tween = null;
			if(_pool != null){
				_tween = _pool.CreateModel();
			}else{
				_tween = DefaultPool.CreateModel();
			}

			return _tween;
		}

		public static TweenModel MoveLocal(GameObject _object, Vector3 _to, float _time, TweenPool _pool = null){
			return MoveLocal(_object, _object.transform.localPosition, _to, _time, _pool);
		}

		public static TweenModel MoveLocal(GameObject _object, Vector3 _from, Vector3 _to, float _time, TweenPool _pool = null){

			TweenModel _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = _from;
			_tween.DiffValue = _to - _from;
			_tween.Time = _time;
			_tween.Type = TweenType.Move;
			_tween.UpdateFunc = UpdateFuncs.Move;

			return _tween;
		}

		// public static TweenModel MoveLocal(GameObject _object, Vector3[] _to, float _time, TweenPool _pool = null){
		// 	TweenModel _tween = CreateModel(_pool);
		// 	_tween.TargetObject = _object;
		// 	_tween.OriginValue = new Vector4(0, 0, 0, 0);
		// 	_tween.DiffValue = new Vector4(1f, 0, 0, 0);
		// 	_tween.Time = _time;
		// 	_tween.Data = new BezierPath(_to);
		// 	_tween.Type = TweenType.Curve;
		// 	_tween.UpdateFunc = UpdateFuncs.Curve;

		// 	return _tween;
		// }


		public static TweenModel MoveByBezier(GameObject _object, Vector3[] _to, float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.Type = TweenType.Bezier;
			_tween.OriginValue = new Vector4(0, 0, 0, 0);
			_tween.DiffValue = new Vector4(1f, 0, 0, 0);
			_tween.Time = _time;
			_tween.UpdateFunc = null;
			_tween.UpdateFunc = UpdateFuncs.Rotate;
			_tween.EaseFunc = null;
			_tween.IsPlaying = false;
			_tween.childDataList = BezierToChildData(_to);
			return _tween;
		}

		public static TweenModel Scale(GameObject _object, Vector3 _to, float _time){
			return Scale(_object, _object.transform.localScale, _to, _time);
		}


		public static TweenModel Scale(GameObject _object, Vector3 _from, Vector3 _to, float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = _from;
			_tween.DiffValue = _to - _from;
			_tween.Time = _time;
			_tween.Type = TweenType.Scale;
			_tween.UpdateFunc = UpdateFuncs.Scale;

			return _tween;
		}

		public static TweenModel Rotate(GameObject _object, Vector3 _to, float _time, TweenPool _pool = null){
			return Rotate(_object, _object.transform.eulerAngles, _to, _time);
		}

		public static TweenModel Rotate(GameObject _object, Vector3 _from, Vector3 _to, float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = _from;
			_tween.DiffValue = _to - _from;
			_tween.Time = _time;
			_tween.Type = TweenType.Rotate;
			_tween.UpdateFunc = UpdateFuncs.Rotate;
			return _tween;
		}

		public static TweenModel RotateZ(GameObject _object, float _toZ, float _time, TweenPool _pool = null){
			return RotateZ(_object, _object.transform.eulerAngles.z, _toZ, _time);
		}

		public static TweenModel RotateZ(GameObject _object, float _fromZ, float _toZ, float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			float _originRotateX = _object.transform.eulerAngles.x;
			float _originRotateY = _object.transform.eulerAngles.y;
			Vector3 _from = new Vector3(_originRotateX, _originRotateY, _fromZ);
			Vector3 _to = new Vector3(_originRotateX, _originRotateY, _toZ);

			_tween.TargetObject = _object;
			_tween.OriginValue = _from;
			_tween.DiffValue = _to - _from;
			_tween.Time = _time;
			_tween.Type = TweenType.Rotate;
			_tween.UpdateFunc = UpdateFuncs.Rotate;
			return _tween;
		}

		public static TweenModel Delay(float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = null;
			_tween.Time = _time;
			_tween.Type = TweenType.Delay;
			_tween.UpdateFunc = null;
			return _tween;
		}

		public static TweenModel Value(float _from, float _to, float _time, TweenPool _pool = null){
			return Value(new Vector4(_from, 0, 0, 0), new Vector4(_to, 0, 0, 0), _time);
		}

		public static TweenModel Value(Vector4 _from, Vector4 _to, float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = null;
			_tween.OriginValue = _from;
			_tween.DiffValue = _to - _from;
			_tween.Time = _time;
			_tween.Type = TweenType.Value;
			_tween.UpdateFunc = null;
			return _tween;
		}

		public static TweenModel Sequance(TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.Type = TweenType.Sequance;
			_tween.OriginValue = Vector2.zero;
			_tween.DiffValue = Vector2.zero;
			_tween.Time = 0;
			_tween.UpdateFunc = null;
			_tween.EaseFunc = null;
			_tween.IsPlaying = false;
			_tween.childDataList = new List<int>();
			return _tween;
		}

		public static TweenModel Spawn(TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.Type = TweenType.Spawn;
			_tween.OriginValue = Vector2.zero;
			_tween.DiffValue = Vector2.zero;
			_tween.Time = 0;
			_tween.UpdateFunc = null;
			_tween.EaseFunc = null;
			_tween.IsPlaying = false;
			_tween.childDataList = new List<int>();
			return _tween;
		}
		#endregion


		public static List<Vector3> ChildDataToBezier(List<int> chidDataList){
			List<Vector3> _list = new List<Vector3>();
			for(int i = 0; i < chidDataList.Count; i+=3){
				_list.Add(new Vector3(chidDataList[i] / 1000f, chidDataList[i+1] / 1000f, chidDataList[i+2] / 1000f));
			}

			return _list;
		}

		public static List<int> BezierToChildData(Vector3[] _vezier){
			var _result = new List<int>();
			for(int i = 0; i < _vezier.Length; i++){
				_result.Add((int)(_vezier[i].x*1000));
				_result.Add((int)(_vezier[i].y*1000));
				_result.Add((int)(_vezier[i].z*1000));
			}

			return _result;
		}

		// public static List<Vector3> GetPathFortween(List<int> chidDataList){

		// 	List<Vector3> _path = new List<Vector3>();
		// 	for(int i = 0; i < _points.Count; i++){

		// 		_path.Add(_points[i]);

		// 		if(i != 0 && i < _points.Count - 1 && i % 3 == 0){
		// 			_path.Add(_points[i]);
		// 		}
		// 	}

		// 	return _path;
		// }
	}

	[Serializable]
	public enum TweenType
	{
		None,
		Sequance,
		Spawn,
		Move,
		Active,
		Scale,
		Rotate,
		Delay,
		Value,
		Bezier,
		Virtual
	}

	public interface IEaseData{
		Vector4 OriginValue {get;}
		Vector4 DiffValue {get;}
		Vector4 CurrentValue {get;set;}
		float Rate {get;}
	}

	public interface IUpdateData{
		Vector4 CurrentValue{get;}
		GameObject TargetObject{get;}
		object Data{get;set;}
		float Rate {get;}
		List<int> ChildDataList{get;}
		Vector4 DiffValue{get;}
	}

	public class TimeType{
		public static float ScaledTime(){
			return UnityEngine.Time.deltaTime;
		}

		public static float UnscaledTime(){
			return UnityEngine.Time.unscaledDeltaTime;
		}

		public static float FixedTime(){
			return UnityEngine.Time.fixedDeltaTime;
		}

		public static float RealTime(){
			return BicTween.realDeltaTime;
		}
	}


}
