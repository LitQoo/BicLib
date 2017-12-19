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
        public bool IsDestroyed{get{ return destoryCount >= 1; }}
        public TweenType Type{get{return type;}set{type = value; SetUpdate();}}
        public Action<IUpdateData> UpdateFunc{
            get{
                if(updateFunc == null){
                    updateFunc = UpdateFuncs.GetFunc(Type);
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

		public Action<IEaseData> EaseFunc{
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
		private TweenType type;
		[SerializeField]
		private EaseType easeType;
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
		public int destoryCount;
        [NonSerialized]
		public int CurrentRepeatCount;
		#endregion
        
		#region Func
        private Action<IUpdateData> updateFunc;
		private Action<IEaseData> easeFunc;
		public Func<float> DeltaTime;

		#endregion 

		#region  Events
        public Action OnCompleteCallback;
        public Action<Vector4> OnUpdateCallback;
        public Action<int> OnRepeatCallback;
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
			EaseFunc = EaseFuncs.Linear;
			EaseType = EaseType.Linear;
			UpdateFunc = null;
			DeltaTime = TimeType.ScaledTime;
			IsPlaying = false;
			destoryCount = 0;
			RepeatCount = 0;
			Data = null;
			CurrentRepeatCount = 0;
            childDataList = null;
			Type = TweenType.None;
		}

		public void SetUpdate(){
			if(type == TweenType.Sequance){
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
			
			if(_list.Count - 1 <= sequanceIndex){
				complete();
                Data = null;
				
				return;
			}

			if(_list[sequanceIndex].IsDestroyed){
				if(_list.Count > sequanceIndex){
					sequanceIndex++;
					_list[sequanceIndex].Play();
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
				complete();
                Data = null;
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
				_deltaTime = DeltaTime();
			}else{
				_deltaTime = BicTween.realDeltaTime;
			}
			#else
			float _deltaTime = DeltaTime();
			#endif
			
			Rate = Mathf.Min(1f, Rate +  _deltaTime / Time);
			EaseFunc(this);

			if(UpdateFunc != null){
				UpdateFunc(this);
			}

			if(Rate == 1f){
				if(RepeatCount == CurrentRepeatCount){	
					complete();
				}else{
					CurrentRepeatCount++;
					Rate = 0;
					if(OnRepeatCallback !=null){
						OnRepeatCallback(CurrentRepeatCount);
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
			this.destoryCount = 5;
		}

		public List<TweenModel> GetChildList(){
			List<TweenModel> _result = new List<TweenModel>();
			if(childDataList != null){
				for(int i = 0; i < childDataList.Count; i++){
					_result.Add(pool.TweenList[childDataList[i]]);
				}
			}
            return _result;
		}

		public TweenModel Play(){
			this.Rate = 0;
			this.IsPlaying = true;
			this.destoryCount = 0;
			pool.UpdateMaxPlayingIndex(this.Id);
			return this;
		}

		public TweenModel Pause(){
			IsPlaying = false;
			return this;
		}

		public TweenModel SubscribeComplete(Action _callback){
			OnCompleteCallback += _callback;
			return this;
		}

		public TweenModel SubscribeUpdate(Action<Vector4> _callback){
			OnUpdateCallback += _callback;
			return this;
		}

		public TweenModel SubscribeRepeat(Action<int> _callback){
			OnRepeatCallback += _callback;
			return this;
		}

		public TweenModel SetEase(Action<IEaseData> _easeFunc){
			EaseFunc = _easeFunc;
			return this;
		}

		public TweenModel SetTimeType(Func<float> _timeType){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				DeltaTime = _timeType;
			}else{
				DeltaTime = TimeType.RealTime;
			}
			#else
			DeltaTime = _timeType;
			#endif
			return this;
		}

		public TweenModel SetRepeat(int _repeat){
			RepeatCount = _repeat;
			return this;
		}

		public TweenModel SetRepeatForever(){
			RepeatCount = -1;
			return this;
		}

		public TweenModel AddTween(TweenModel _tween){
			_tween.Pause();
			childDataList.Add(_tween.Id);
			return this;
		}

        #if UNITY_EDITOR
        [NonSerialized]
        public Rect editor_rect;
        #endif
	}

}
