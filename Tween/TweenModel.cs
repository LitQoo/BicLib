using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BicUtil.Tween
{
    [Serializable]
	public class TweenModel : IEaseData, IUpdateData{        
		#region properties
		public GameObject TargetObject{get{return targetObject;} set{targetObject = value;}}
		public Vector4 OriginValue{get{return originValue;}set{originValue = value;}}
		public Vector4 DiffValue{get{return diffValue;}set{diffValue = value;}}
		public List<int> ChildDataList{get{return childDataList;} set{childDataList = value;}}
		public Vector4 CurrentValue{get;set;}
        public object Data{get;set;}
        public float Rate{ get;set;}
        public int PlayingIndex{get;set;}
        public bool IsPlaying{get;set;}
		public Action Update{get;set;}
		public string StringData{get{ return stringData;} set{stringData = value;}}
        public bool IsDestroyed{get{ return destoryCount >= 1; }}
        public TweenType Type{get{return type;}set{type = value; SetUpdate();}}
		public int PoolIndex{get{ return pool.GetIndex(this);}}
        public Action<IUpdateData> UpdateFunc{
            get{
                if(updateFunc == null){
                    UpdateFuncs.SetUpdateFunc(this);
                }
				
                return updateFunc;
            }

            set{
                updateFunc = value;
            }
        }

		public EaseType EaseType{
			get{ return easeType;}
			set{ 
				easeType = value;
				easeFunc = EaseFuncs.GetFunc(easeType);
			}
		}

		public Func<IEaseData, Vector4> EaseFunc{
			get{
				if(easeFunc == null){
					easeFunc = EaseFuncs.GetFunc(EaseType);
				}
				return easeFunc;
			}

			set{
				easeFunc = value;
			}
		}

		public TimeType TimeType{
			get{return timeType;}
			set{
				timeType = value;
				timeFunc = TimeFuncs.GetFunc(timeType);
			}
		}

		public Func<float> TimeFunc{
			get{
				if(timeFunc == null){
					timeFunc = TimeFuncs.GetFunc(timeType);
				}
				return timeFunc;
			}

			set{
				timeFunc = value;
			}
		}

		public bool IsGrouped{
			get{
				return Type == TweenType.Sequance || Type == TweenType.Spawn;
			}
		}
		#endregion
       
		#region Serialized Members
        public string Name; 
		public float Time;
		public int Id;
		public int RepeatCount;
		public TweenPool pool;
		[SerializeField]
		public List<int> childDataList;
		[SerializeField]
		private string stringData;
		[SerializeField]
		private TweenType type;
		[SerializeField]
		private EaseType easeType;
		[SerializeField]
		private TimeType timeType;
		[SerializeField]
		private GameObject targetObject;
		[SerializeField]
		private Vector4 originValue;
		[SerializeField]
		private Vector4 diffValue;
		[SerializeField]
		private Vector4 targetValue;
		#endregion

		#region  NoneSerialized Members (just use in playmode)
		//public TweenModel Parent;
        [NonSerialized]
		public int destoryCount = 0;
        [NonSerialized]
		public int CurrentRepeatCount;
		#endregion
        
		#region Func
		private Func<float> timeFunc;
        private Action<IUpdateData> updateFunc;
		private Func<IEaseData, Vector4> easeFunc;

		#endregion 

		#region  Events
        public Action OnCompleteCallback;
        public Action<Vector4> OnUpdateCallback;
        public Action<TweenModel, int> OnRepeatCallback;
		#endregion

		public override string ToString(){
			string _result = "";
			_result += "Name:"+Name+"\n";
			_result += "Hash:"+GetHashCode().ToString()+"\n";
			_result += "Type:"+type.ToString()+"\n";
			_result += "Id : " + Id.ToString() + "\n";
			//_result += "ParentId:"+(Parent != null ? Parent.Id.ToString() : "null")+"\n";
			_result += "TargetObject:" + (TargetObject == null ? "null":TargetObject.name)+"\n";
			_result += "OriginValue:" + OriginValue.ToString()+"\n";
			_result += "DiffValue:" + DiffValue.ToString()+"\n";
			_result += "CurrentValue:" + DiffValue.ToString()+"\n";
			_result += "Time:"+Time.ToString()+"\n";
			_result += "IsPlaying:"+IsPlaying.ToString()+"\n";
			_result += "destoryCount:"+destoryCount.ToString()+"\n";
			_result += "RepeatCount:"+RepeatCount.ToString()+"\n";
			_result += "CurrentRepeatCount:"+CurrentRepeatCount.ToString()+"\n";
            _result += "ChildCount:"+(childDataList != null ? childDataList.Count.ToString() : "0");

			return _result;

		}

        public void PrintChilds(){
             Debug.Log("this->" + ToString()); 
            Debug.Log("print childs-------------------"); 
			var _childList = GetChildList();
            for(int i = 0; i < _childList.Count; i++){
                Debug.Log(_childList[i].ToString());
            }
            Debug.Log("print childs end-------------------");
        }

        public void Clear(){
			Rate = 0;
			OnCompleteCallback = null;
			OnUpdateCallback = null;
			OnRepeatCallback = null;
			EaseFunc = EaseFuncs.Linear;
			EaseType = EaseType.Linear;
			UpdateFunc = null;
			TimeFunc = TimeFuncs.ScaledTime;
			timeType = TimeType.Scaled;
			IsPlaying = false;
			destoryCount = 0;
			RepeatCount = 0;
			Data = null;
			CurrentRepeatCount = 0;
            childDataList = null;
			Type = TweenType.None;
		}

		public TweenModel Copy(TweenPool _pool = null){
			if(_pool == null){
				_pool = this.pool;
			}

			if(_pool == null){
				throw new SystemException("[BicTween] TweenPool is null");
			}

			var _tween = BicTween.CreateModel(_pool);
			_tween.Rate = 0;
			_tween.Name = this.Name + "_copy";
			_tween.OnCompleteCallback = this.OnCompleteCallback;
			_tween.OnUpdateCallback = this.OnUpdateCallback;
			_tween.EaseFunc = this.EaseFunc;
			_tween.EaseType = this.EaseType;
			_tween.UpdateFunc = this.UpdateFunc;
			_tween.TimeFunc = this.TimeFunc;
			_tween.TimeType = this.TimeType;
			_tween.IsPlaying = false;
			_tween.destoryCount = 0;
			_tween.RepeatCount = 0;
			_tween.Data = null;
			_tween.CurrentRepeatCount = 0;
			_tween.Type = this.Type;
			_tween.TargetObject = this.TargetObject;
			_tween.OriginValue = this.OriginValue;
			_tween.DiffValue = this.DiffValue;
			_tween.Time = this.Time;
			_tween.stringData = this.stringData;
			
			#if UNITY_EDITOR
			_tween.editor_targetPath = this.pool.GetTargetPath(_tween.targetObject);
			#endif
			
			if(this.IsGrouped == true){

				if(_tween.childDataList == null){
					_tween.childDataList = new List<int>();
				}

				var _childs = this.GetChildList();
				for(int i = 0; i < _childs.Count; i++){
					var _copy = _childs[i].Copy(_pool);
					if(_copy == null){
						throw new SystemException("[BicTween] Failed Copy");
					}

					_tween.AddChild(_copy);
				}
			}else if(type == TweenType.Bezier){
				_tween.childDataList = new List<int>(this.childDataList.ToArray());
			}
			
			return _tween;
		}

		public void SetUpdate(){
			if(type == TweenType.Sequance || type == TweenType.Virtual){
				Update = updateForSequance;
			}else if(type == TweenType.Spawn){
				Update = updateForSpawn;
			}else{
                Update = updateForSingle;
			}
		}

		private int sequanceIndex = 0;
		private void updateForSequance(){
			if(Data == null){
				var _childList = GetChildList();
				Data = _childList;
				sequanceIndex = 0;
                _childList[sequanceIndex].Play();
			}

			var _list = Data as List<TweenModel>;
			
			if(_list.Count <= sequanceIndex){
				if(RepeatCount == CurrentRepeatCount){	
					complete();
					Data = null;
				}else{
					CurrentRepeatCount++;
					Rate = 0;
					Data = null;
					if(OnRepeatCallback != null){
						OnRepeatCallback(this, CurrentRepeatCount);
					}
				}

				return;
			}

			if(_list[sequanceIndex].IsDestroyed){
				if(_list.Count > sequanceIndex){
					sequanceIndex++;
					if(sequanceIndex < _list.Count){
						_list[sequanceIndex].Play();
					}
				}
			}
		}

		private void updateForSpawn(){
			if(Data == null){
				var _childList = GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					_childList[i].Play();
				}
				
				Data = _childList;
			}

			var _list = Data as List<TweenModel>;
			
			if(_list.Count == 0){
				if(RepeatCount == CurrentRepeatCount){	
					complete();
					Data = null;
				}else{
					CurrentRepeatCount++;
					Rate = 0;
					Data = null;
					if(OnRepeatCallback != null){
						OnRepeatCallback(this, CurrentRepeatCount);
					}
				}

				return;
			}
			
			
			for(int i = _list.Count - 1; i >= 0; i--){
				if(_list[i].IsDestroyed){
					_list.RemoveAt(i);
				}
			}
		}

		private void updateForSingle(){
			#if UNITY_EDITOR
			float _deltaTime = 0;
			if(Application.isPlaying){
				_deltaTime = TimeFunc();
			}else{
				_deltaTime = BicTween.realDeltaTime;
			}
			#else
			float _deltaTime = DeltaTime();
			#endif
			
			Rate = Mathf.Min(1f, Rate +  _deltaTime / Time);
			CurrentValue = EaseFunc(this);

			if(UpdateFunc != null){
				UpdateFunc(this);
			}

			if(Rate == 1f){
				if(RepeatCount == CurrentRepeatCount){	
					complete();
					Data = null;
				}else{
					CurrentRepeatCount++;
					Rate = 0;
					if(OnRepeatCallback != null){
						OnRepeatCallback(this, CurrentRepeatCount);
					}
				}
			}else{
				if(OnUpdateCallback != null){
					OnUpdateCallback(CurrentValue);
				}
			}
		}

		private void complete(){
			if(OnCompleteCallback != null){
				OnCompleteCallback();
			}

            this.IsPlaying = false;
			this.destoryCount = 1;
		}

		public List<TweenModel> GetChildList(){
			List<TweenModel> _result = new List<TweenModel>();
			if(childDataList != null){
				for(int i = 0; i < childDataList.Count; i++){
					_result.Add(pool.GetTween(childDataList[i]));
				}
			}
            return _result;
		}

		public TweenModel Play(){
			this.Rate = 0;
			this.IsPlaying = true;
			this.destoryCount = 0;
			this.CurrentRepeatCount = 0;
			this.Data = null;

			pool.UpdateMaxPlayingIndex(this.PoolIndex);
			return this;
		}

		public TweenModel Pause(){
			IsPlaying = false;
			return this;
		}

		public void Cancel(){
			this.IsPlaying = false;
			this.destoryCount = 1;

			if(IsGrouped == true){
				var _childList = GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					_childList[i].Cancel();
				}
			}
		}

		public TweenModel SubscribeComplete(Action _callback, bool _clearSubscribe = false){
			if(_clearSubscribe == true){
				OnCompleteCallback = null;
			}

			OnCompleteCallback += _callback;
			return this;
		}

		public TweenModel SubscribeUpdate(Action<Vector4> _callback){
			OnUpdateCallback += _callback;
			return this;
		}

		public TweenModel SubscribeRepeat(Action<TweenModel, int> _callback){
			OnRepeatCallback += _callback;
			return this;
		}

		public TweenModel SetEase(EaseType _easeType){
			EaseType = _easeType;
			return this;
		}

		// public TweenModel SetTimeType(Func<float> _timeType){
		// 	#if UNITY_EDITOR
		// 	if(Application.isPlaying){
		// 		TimeFunc = _timeType;
		// 	}else{
		// 		TimeFunc = TimeFuncs.RealTime;
		// 	}
		// 	#else
		// 	DeltaTime = _timeType;
		// 	#endif
		// 	return this;
		// }

		public TweenModel SetRepeat(int _repeat){
			RepeatCount = _repeat;
			return this;
		}

		public TweenModel SetRepeatForever(){
			RepeatCount = -1;
			return this;
		}

		public TweenModel AddChild(TweenModel _tween){
			_tween.Pause();

			childDataList.Add(_tween.Id);
			return this;
		}

		public TweenModel InsertChild(TweenModel _tween, int _index){
			_tween.Pause();

			childDataList.Insert(_index, _tween.Id);
			return this;
		}

		public void RemoveChild(TweenModel _tween){
			if(this.IsGrouped){
				var _childs = this.GetChildList();
				for(int i = 0; i < _childs.Count; i++){
					_childs[i].RemoveChild(_tween);
				}

				childDataList.Remove(_tween.Id);
			}
		}

		public void Remove(){
			pool.RemoveTween(this);

			if(this.IsGrouped == true){
				var _childs = this.GetChildList();
				for(int i = 0; i < _childs.Count; i++){
					_childs[i].Remove();
				}
			}
		}

		public TweenModel SetTargetObject(GameObject _target){
			TargetObject = _target;
			return this;
		}

        #if UNITY_EDITOR
        [NonSerialized]
        public Rect editor_rect;
		[NonSerialized]
		public TweenModel editor_parent;
		[NonSerialized]
		public string editor_targetPath;
        #endif
	}

}
