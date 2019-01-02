using System;
using System.Collections;
using System.Collections.Generic;
#if BICUTIL_SPINE
using Spine.Unity;
#endif
using UnityEngine;
namespace BicUtil.Tween
{

    public static class BicTween {
		private static int id = 0;
		public static int GetNewId(){
			id++;
			return id;
		}
		

		#if BICUTIL_UNITY_ANIMATOR
		public static void PlayMecanimAnimation(Animator _animator, string _stateHashName){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				_animator.Play(_stateHashName);
			}else{
				_animator.Play(_stateHashName, -1, 0f);
				throw new SystemException("write PlayMecanimAnimation");
				// LeanTweenOnEditor.AddUpdateCallback((_id, _dt)=>{
				// 	if(_animator.isActiveAndEnabled == true){
				// 		_animator.Update(_dt);
				// 	}

				// 	if(_animator.isActiveAndEnabled == false || _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1){
				// 		LeanTweenOnEditor.RemoveUpdateCallback(_id);
				// 	}
			 	// });
			}
			#else
			_animator.Play(_stateHashName);
			#endif
		}
		#endif

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
				
				var _seq = BicTween.Sequance();
				var _slider1 = BicTween.MoveSlider(_slider, _slider.maxValue, _time).SubscribeComplete(()=>{
					_slider.minValue = _slider.maxValue;
					_slider.maxValue = _nextMax;
				});

				var _slider2 = BicTween.MoveSlider(_slider, _toValue, _time);
				_seq.AddChild(_slider1);
				_seq.AddChild(_slider2);

				return _seq.Play();

			}else{
				return BicTween.MoveSlider(_slider, _toValue, _time);
			}
		}

		static public TweenModel TypeWriter(UnityEngine.UI.Text _text, string _string, float _time, TweenPool _pool = null){
			TweenModel _tween = CreateModel(_pool);
			_tween.TargetObject = _text.gameObject;
			_tween.StringData = _string;
			_tween.Time = _time;
			_tween.Type = TweenType.TypeWriting;
			_tween.RepeatCount = _tween.StringData.Length;

			return _tween;
		}

		static public TweenModel Interval(float _intervalTime, int _count, TweenPool _pool = null){
			TweenModel _tween = CreateModel(_pool);
			_tween.Time = _intervalTime;
			_tween.RepeatCount = _count;
			_tween.Type = TweenType.None;
			return _tween;
		}

		static public TweenModel Counting(TextMesh _text, int _from, int _to, float _time, float _intervalTime = 0.05f, TweenPool _pool = null){
			int _repeatCount = (int)(_time / _intervalTime);
			float _dt = (_to - _from)/(float)_repeatCount;

			_text.text = _from.ToString();
			var _result = Interval(_intervalTime, _repeatCount, _pool).SubscribeRepeat((_tween, _count)=>{
				_text.text = ((int)(_from + _dt * _count)).ToString();
			}).SubscribeComplete(()=>{
				_text.text = _to.ToString();
			});


			return _result;

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
		// private static void init(){
		// 	#if UNITY_EDITOR
		// 	if(Application.isPlaying){
		// 		var _manager = DefaultPool;
		// 	}
		// 	#else
		// 		var _manager = BicTweenManager.Instance;
		// 	#endif
		// }

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
			_tween.Type = TweenType.MoveLocal;
			_tween.UpdateFunc = UpdateFuncs.MoveLocal;

			return _tween;
		}

		public static TweenModel MoveWorld(GameObject _object, Vector3 _to, float _time, TweenPool _pool = null){
			return MoveWorld(_object, _object.transform.position, _to, _time, _pool);
		}

		public static TweenModel MoveWorld(GameObject _object, Vector3 _from, Vector3 _to, float _time, TweenPool _pool = null){

			TweenModel _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = _from;
			_tween.DiffValue = _to - _from;
			_tween.Time = _time;
			_tween.Type = TweenType.MoveWorld;
			_tween.UpdateFunc = UpdateFuncs.MoveWorld;

			return _tween;
		}

		public static TweenModel Follow(GameObject _object, GameObject _targetObject, float _time, TweenPool _pool = null){
			TweenModel _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = _object.transform.position;
			_tween.DiffValue = _targetObject.transform.position - (Vector3)_tween.OriginValue;
			_tween.Data = _targetObject.transform;
			_tween.Time = _time;
			_tween.Type = TweenType.Follow;
			_tween.UpdateFunc = UpdateFuncs.Follow;

			return _tween;
		}

		public static TweenModel FollowWithSpeed(GameObject _object, GameObject _targetObject, float _distancePerSecond, TweenPool _pool = null){
			var _time = Vector3.Distance(_object.transform.position, _targetObject.transform.position) / _distancePerSecond;
			return Follow(_object, _targetObject, _time, _pool);
		}

		public static TweenModel MoveLocalWithSpeed(GameObject _object, Vector3 _from, Vector3 _to, float _distancePerSecond, TweenPool _pool = null){
			var _time = Vector3.Distance(_from, _to) / _distancePerSecond;
			return MoveLocal(_object, _from, _to, _time, _pool);
		}

		public static TweenModel MoveLocalWithSpeed(GameObject _object, Vector3 _to, float _distancePerSecond, TweenPool _pool = null){
			return MoveLocalWithSpeed(_object, _object.transform.localPosition, _to, _distancePerSecond, _pool);
		}


		public static TweenModel MoveWorldWithSpeed(GameObject _object, Vector3 _from, Vector3 _to, float _distancePerSecond, TweenPool _pool = null){
			var _time = Vector3.Distance(_from, _to) / _distancePerSecond;
			return MoveWorld(_object, _from, _to, _time, _pool);
		}

		public static TweenModel MoveWorldWithSpeed(GameObject _object, Vector3 _to, float _distancePerSecond, TweenPool _pool = null){
			return MoveWorldWithSpeed(_object, _object.transform.position, _to, _distancePerSecond, _pool);
		}

		public static TweenModel MoveLocalX(GameObject _object, float _to, float _time, TweenPool _pool = null){
			return MoveLocalX(_object, _object.transform.localPosition.x, _to, _time, _pool);
		}

		public static TweenModel MoveLocalX(GameObject _object, float _from, float _to, float _time, TweenPool _pool = null){

			TweenModel _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = new Vector3(_from, _object.transform.localPosition.y, _object.transform.localPosition.z);
			_tween.DiffValue = new Vector3(_to - _from, 0f, 0f);
			_tween.Time = _time;
			_tween.Type = TweenType.MoveLocal;
			_tween.UpdateFunc = UpdateFuncs.MoveLocal;

			return _tween;
		}


		public static TweenModel MoveLocalY(GameObject _object, float _to, float _time, TweenPool _pool = null){
			return MoveLocalY(_object, _object.transform.localPosition.y, _to, _time, _pool);
		}

		public static TweenModel MoveLocalY(GameObject _object, float _from, float _to, float _time, TweenPool _pool = null){

			TweenModel _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = new Vector3(_object.transform.localPosition.x, _from, _object.transform.localPosition.z);
			_tween.DiffValue = new Vector3(0f, _to - _from, 0f);
			_tween.Time = _time;
			_tween.Type = TweenType.MoveLocal;
			_tween.UpdateFunc = UpdateFuncs.MoveLocal;

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
			_tween.UpdateFunc = UpdateFuncs.Bezier;
			_tween.childDataList = BezierToChildData(_to);
			return _tween;
		}

		public static TweenModel MoveByBezierWithSpeed(GameObject _object, Vector3[] _to, float _distancePerSecond, TweenPool _pool = null){
			var _bezier = new BezierPath(BicTween.ChildDataToBezier(BezierToChildData(_to)).ToArray());
			var _time = _bezier.distance / _distancePerSecond;
			var _tween = MoveByBezier(_object, _to, _time, _pool);
			_tween.Data = _bezier;
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

		public static TweenModel Alpha(GameObject _object, float _to, float _time, TweenPool _pool = null){
			float _from = 0;
			var _uigraphic = _object.GetComponent<UnityEngine.UI.Graphic>();
			if(_uigraphic != null){
				_from = _uigraphic.color.a;
				return alpha(_object, _from, _to, _time, TweenType.AlphaUIGraphic, _pool);
			}else{
				var _sprite = _object.GetComponent<SpriteRenderer>();
				if(_sprite != null){
					_from = _sprite.color.a;
					return alpha(_object, _from, _to, _time, TweenType.AlphaUIGraphic, _pool);
				}
			}

			throw new SystemException("[BICTWEEN] Did not support this object " + _object.name);
		}

		public static TweenModel Alpha(GameObject _object, float _from, float _to, float _time, TweenPool _pool = null){
			var _uigraphic = _object.GetComponent<UnityEngine.UI.Graphic>();
			if(_uigraphic != null){
				return alpha(_object, _from, _to, _time, TweenType.AlphaUIGraphic, _pool);
			}else{
				var _sprite = _object.GetComponent<SpriteRenderer>();
				if(_sprite != null){
					return alpha(_object, _from, _to, _time, TweenType.AlphaUIGraphic, _pool);
				}
			}

			throw new SystemException("[BICTWEEN] Did not support this object " + _object.name);
		}

		public static TweenModel Alpha(UnityEngine.UI.Graphic _uigraphic, float _from, float _to, float _time, TweenPool _pool = null){
			return alpha(_uigraphic.gameObject, _from, _to, _time, TweenType.AlphaUIGraphic, _pool);
		}

		public static TweenModel Alpha(SpriteRenderer _sprite, float _from, float _to, float _time, TweenPool _pool = null){
			return alpha(_sprite.gameObject, _from, _to, _time, TweenType.AlphaSprite, _pool);
		}

		public static TweenModel Alpha(UnityEngine.UI.Graphic _uigraphic, float _to, float _time, TweenPool _pool = null){
			return alpha(_uigraphic.gameObject, _uigraphic.color.a, _to, _time, TweenType.AlphaUIGraphic, _pool);
		}

		public static TweenModel Alpha(SpriteRenderer _sprite, float _to, float _time, TweenPool _pool = null){
			return alpha(_sprite.gameObject, _sprite.color.a, _to, _time, TweenType.AlphaSprite, _pool);
		}

		private static TweenModel alpha(GameObject _object, float _from, float _to, float _time, TweenType _alphaType, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = new Vector4(0, 0, 0, _from);
			_tween.DiffValue = new Vector4(0, 0, 0, _to) - _tween.OriginValue;
			_tween.Time = _time;
			_tween.Type = _alphaType;
			UpdateFuncs.SetUpdateFunc(_tween);
			return _tween;
		}

		public static TweenModel Size(GameObject _object, Vector3 _from, Vector3 _to, float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.OriginValue = _from;
			_tween.DiffValue = _to - _from;
			_tween.Time = _time;
			_tween.Type = TweenType.Size;
			_tween.UpdateFunc = UpdateFuncs.Size;

			return _tween;
		}


		public static TweenModel Shake(GameObject _object, Vector3 _range, float _time, TweenPool _pool = null){
			var _tween = CreateModel(_pool);
			_tween.TargetObject = _object;
			_tween.DiffValue = _range;
			_tween.Time = _time;
			_tween.Type = TweenType.Size;
			_tween.UpdateFunc = UpdateFuncs.Size;

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

		
		#if BICUTIL_SPINE
		public static TweenModel SpineAnimation(SkeletonAnimation _spine, string _animationName, TweenPool _pool = null){
			var _spineData = _spine.AnimationState.Data.skeletonData.FindAnimation(_animationName);
			float _time = _spineData.Duration;
			
			var _tween = CreateModel(_pool);
			_tween.TargetObject = null;
			_tween.Time = _time;
			_tween.Data = _spine;
			_tween.Type = TweenType.Delay;
			_tween.UpdateFunc = (_updateData)=>{
				_spine.AnimationState.SetAnimation(0, _spineData, false);
				_tween.UpdateFunc = null;
			};
			return _tween;
		}
		#endif
		
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

		public static void Cancel(GameObject _targetObject, TweenPool _pool = null){
			if(_pool != null){
				_pool.Cancel(_targetObject);
			}else{
				defaultPool.Cancel(_targetObject);
			}
		}
		#endregion


		public static List<Vector3> ChildDataToBezier(List<int> _childDataList){
			List<Vector3> _list = new List<Vector3>();
			
			if(_childDataList == null){
				return _list;
			}

			for(int i = 0; i < _childDataList.Count; i+=3){
				_list.Add(new Vector3(_childDataList[i] / 1000f, _childDataList[i+1] / 1000f, _childDataList[i+2] / 1000f));
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

	public interface IEaseData{
		Vector4 OriginValue {get;}
		Vector4 DiffValue {get;}
		Vector4 CurrentValue {get;set;}
		float Rate {get;}
	}

	public interface IUpdateData{
		Vector4 OriginValue{get;}
		Vector4 CurrentValue{get;}
		GameObject TargetObject{get;}
		object Data{get;set;}
		float Rate {get;}
		List<int> ChildDataList{get;}
		string StringData{get;}
		Vector4 DiffValue{get;set;}
	}

	public enum TimeType{
		Scaled,
		Unscaled,
		Fixed,
		Real
	}

	public class TimeFuncs{
		public static Func<float> GetFunc(TimeType _type){
			switch(_type){
				case TimeType.Fixed: return FixedTime;
				case TimeType.Scaled: return ScaledTime;
				case TimeType.Unscaled: return UnscaledTime;
				case TimeType.Real: return RealTime;
			}

			return null;
		}
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

	public class TweenCancelObject{
		public TweenModel Tween = null;
		public int Id = -1;

		public bool IsComplete{
			get{
				if(Tween == null){
					return true;
				}
				
				if(Tween.IsDestroyed == true){
					return true;
				}

				if(Tween.Id != this.Id){
					return true;
				}

				return false;
			}
		}

		public TweenCancelObject(TweenModel _tween = null){
			SetTween(_tween);
		}

		public void SetTween(TweenModel _tween){
			Tween = _tween;

			if(_tween != null){
				Id = _tween.Id;
			}
		}

		public void Cancel(){
			if(Tween == null){
				return;
			}

			if(Tween.IsPlaying == true){
				Tween.Cancel(Id);
			}

			Tween = null;
			Id = -1;
		}
	}


}
