using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace BicUtil.Tween
{
	[Obsolete("Chagne TweenModel to Tween")]
    public class TweenModel : Tween{

	}
	
	[Serializable]
	public class Tween : IEaseData, IUpdateData{       
		#region Static
		public const int DESTORY_WAIT_3FRAME = 3;
		public const int DESTORY_READY_TO_RECYCLE = 1;
		public const int DESTORY_NOT = 0;

		public const string SPAWN_OPTION_ALL = "A";
		public const string SPAWN_OPTION_ANY_SKIP = "S";
		public const string SPAWN_OPTION_ANY_CANCEL = "C";
		public const string SPAWN_OPTION_ANY_KEEP = "K";
		#endregion

		#region properties
		public GameObject TargetObject{get{return targetObject;} set{targetObject = value;}}
		public Vector4 OriginValue{get{return originValue;}set{originValue = value;}}
		public Vector4 DiffValue{get{return diffValue;}set{diffValue = value;}}
		public List<int> ChildDataList{get{return childDataList;} set{childDataList = value;}}
		public Vector4 CurrentValue{get;set;}
		public Action<Tween> LateSetValueFunc{get;set;}
        public object Data{get;set;}
        public float Rate{ get;set;}
        public int PlayingIndex{get;set;}
        public bool IsPlaying{get;set;}
        public bool IsLockedComplete{get;set;}
		public int CreatedFrameCount{get;set;}
		public string CallerInfo{get;set;}
		public Action Update{get;set;}
		public string StringData{get{ return stringData;} set{stringData = value;}}
        public bool IsDestroyed{get{ return destoryCount >= DESTORY_READY_TO_RECYCLE; }}
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
		//public Tween Parent;
        [NonSerialized]
		public int destoryCount = DESTORY_NOT;
        [NonSerialized]
		public int CurrentRepeatCount;
		#endregion
        
		#region Func
		private Func<float> timeFunc;
        private Action<IUpdateData> updateFunc;
		private Func<IEaseData, Vector4> easeFunc;

		#endregion 

		#region  Events
		public Action OnStartCallback;
        public Action OnCompleteCallback;
        public Action<Vector4> OnUpdateCallback;
        public Action<Tween, int> OnRepeatCallback;
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
			OnStartCallback = null;
			OnUpdateCallback = null;
			OnRepeatCallback = null;
			LateSetValueFunc = null;
			EaseFunc = EaseFuncs.Linear;
			EaseType = EaseType.Linear;
			UpdateFunc = null;
			TimeFunc = TimeFuncs.ScaledTime;
			timeType = TimeType.Scaled;
			IsPlaying = false;
			destoryCount = DESTORY_NOT;
			RepeatCount = 0;
			Data = null;
			CurrentRepeatCount = 0;
            childDataList = null;
			Type = TweenType.None;
			TargetObject = null;
			IsLockedComplete = false;
			CreatedFrameCount = 0;
			CallerInfo = string.Empty;

			if(this.childDataList != null){
				this.childDataList.Clear();
			}
		}

		public Tween Copy(TweenPool _pool = null){
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
			_tween.OnStartCallback = this.OnStartCallback;
			_tween.OnUpdateCallback = this.OnUpdateCallback;
			_tween.LateSetValueFunc = this.LateSetValueFunc;
			_tween.EaseFunc = this.EaseFunc;
			_tween.EaseType = this.EaseType;
			_tween.UpdateFunc = this.UpdateFunc;
			_tween.TimeFunc = this.TimeFunc;
			_tween.TimeType = this.TimeType;
			_tween.IsPlaying = false;
			_tween.destoryCount = DESTORY_NOT;
			_tween.RepeatCount = 0;
			_tween.Data = null;
			_tween.CurrentRepeatCount = 0;
			_tween.Type = this.Type;
			_tween.TargetObject = this.TargetObject;
			_tween.OriginValue = this.OriginValue;
			_tween.DiffValue = this.DiffValue;
			_tween.Time = this.Time;
			_tween.stringData = this.stringData;
			_tween.IsLockedComplete = this.IsLockedComplete;
			_tween.CreatedFrameCount = this.CreatedFrameCount;
			_tween.CallerInfo = this.CallerInfo + "(copy)";
			
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

		private void setValuesByFunc(){
			if(LateSetValueFunc != null){
				LateSetValueFunc(this);
			}
		}

		public void SetUpdate(){
			if(type == TweenType.Sequance || type == TweenType.Virtual){
				Update = updateForSequance;
			}else if(type == TweenType.Spawn){
				switch(this.stringData){
					case SPAWN_OPTION_ALL:
						Update = updateForSpawn;
					break;
					case SPAWN_OPTION_ANY_CANCEL:
					case SPAWN_OPTION_ANY_SKIP:
					case SPAWN_OPTION_ANY_KEEP:
						Update = updateForSpawnAny;
					break;
					default:
						Update = updateForSpawn;
						Debug.LogError("Spawn Option ERROR");
					break;
				}
			}else{
                Update = updateForSingle;
			}
		}

		private int sequanceIndex = 0;
		
		public Tween GetPlayingChild(){
			if(this.Type == TweenType.Sequance){
				if(this.Data == null){
					return null;
				}else{
					var _list = Data as List<Tween>;
					return _list[sequanceIndex];
				}
			}else{
				return null;
			}
		}

		private void updateForSequance(){
			
			if(Rate == 0f){
				setValuesByFunc();
				if(OnStartCallback != null && this.CurrentRepeatCount == 0){
					OnStartCallback();
				}

				Rate = 0.5f;

				var _childList = GetChildList();
				Data = _childList;
				sequanceIndex = 0;
				if(_childList.Count > 0){	
                	_childList[sequanceIndex].Play(false);
				}else{
					complete();
				}
				return;
			}
			
			var _list = Data as List<Tween>;
			
			if(_list.Count <= sequanceIndex){
				if(RepeatCount == CurrentRepeatCount){		
					Rate = 1f;
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

			if(_list[sequanceIndex].IsPlaying == false){
				if(_list.Count > sequanceIndex){
					sequanceIndex++;
					if(sequanceIndex < _list.Count){
						_list[sequanceIndex].Play(false);
					}
				}
			}

			if(OnUpdateCallback != null){
				OnUpdateCallback(CurrentValue);
			}
		}

		private void updateForSpawn(){
			if(Rate == 0f){
				setValuesByFunc();

				if(OnStartCallback != null && this.CurrentRepeatCount == 0){
					OnStartCallback();
				}

				Rate = 0.5f;

				var _childList = GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					_childList[i].Play(false);
				}
				
				Data = _childList;
			}


			var _list = Data as List<Tween>;
			
			if(_list.Count == 0){
				if(RepeatCount == CurrentRepeatCount){	
					Rate = 1f;
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
				if(_list[i].IsPlaying == false){	
					_list.RemoveAt(i);
				}
			}

			if(OnUpdateCallback != null){
				OnUpdateCallback(CurrentValue);
			}
		}


		private void updateForSpawnAny(){
			if(Rate == 0f){
				setValuesByFunc();

				if(OnStartCallback != null && this.CurrentRepeatCount == 0){
					OnStartCallback();
				}

				Rate = 0.5f;

				var _childList = GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					_childList[i].Play(false);
				}

				Data = _childList;
			}


			var _list = Data as List<Tween>;

			if(_list.Count == 0){
				if(RepeatCount == CurrentRepeatCount){	
					Rate = 1f;
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

			bool _isCompleted = false;
			for(int i = _list.Count - 1; i >= 0; i--){
				if(_list[i].IsPlaying == false){	
					_list.RemoveAt(i);
					_isCompleted = true;
				}
			}

			if(_isCompleted == true){
				for(int i = _list.Count - 1; i >= 0; i--){
					var _child = _list[i];
					if(this.stringData == SPAWN_OPTION_ANY_SKIP){
						_child.Skip(_child.Id);
					}else if(this.stringData == SPAWN_OPTION_ANY_CANCEL){
						_child.Cancel(_child.Id);
					}

					_list.RemoveAt(i);
					
				}
			}

			if(OnUpdateCallback != null){
				OnUpdateCallback(CurrentValue);
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
			float _deltaTime = TimeFunc();
			#endif
			
			

			if(Rate == 0f && CurrentRepeatCount == 0){
				setValuesByFunc();
				if(OnStartCallback != null){
					OnStartCallback();
				}
			}
			
			Rate = Mathf.Min(1f, Rate +  _deltaTime / Time);
			CurrentValue = EaseFunc(this);

			if(UpdateFunc != null){
				UpdateFunc(this);
			}

			if(OnUpdateCallback != null){
				OnUpdateCallback(CurrentValue);
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
			}
		}

		private void complete(){
			this.IsPlaying = false;

			if(OnCompleteCallback != null){
				OnCompleteCallback();
			}

			
			if(IsLockedComplete == false){
				destoryAndClear();
			}
		}

		private void destoryAndClear(){
			var _childList = GetChildList();
			for(int i = 0; i < _childList.Count; i++){
				_childList[i].destoryAndClear();
			}
			
			if(this.pool.IsLocked == false){
				Clear();
			}

			this.destoryCount = DESTORY_WAIT_3FRAME;
		}

		public List<Tween> GetChildList(){
			List<Tween> _result = new List<Tween>();
			if(childDataList != null && this.type != TweenType.Bezier && this.type != TweenType.BezierWorld){
				for(int i = 0; i < childDataList.Count; i++){
					_result.Add(pool.GetTween(childDataList[i]));
				}
			}
            return _result;
		}

		public int GetChildCount(){
			return childDataList.Count;
		}

		public Tween Play(bool _needApplyInitialInformations = true){
			this.Rate = 0;
			this.IsPlaying = true;
			this.destoryCount = DESTORY_NOT;
			this.CurrentRepeatCount = 0;
			this.Data = null;


			if(_needApplyInitialInformations == true){
				pool.ApplyInitialInformation(this.Id);
			}

			pool.UpdateMaxPlayingIndex(this.PoolIndex);
			return this;
		}

		public Tween InitialUpdate(){
			if(this.Rate == 0){
				this.Update();
			}

			return this;
		}

		public Tween Pause(){
			IsPlaying = false;
			return this;
		}

		public void Cancel(int _id){
			if(_id != this.Id || this.IsDestroyed == true){
				return;
			}

			this.IsPlaying = false;

			if(IsGrouped == true){
				var _childList = GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					_childList[i].Cancel(_childList[i].Id);
				}
			}

			if(IsLockedComplete == false){
				destoryAndClear();
			}
		}

		public void Skip(int _id){
			if(_id != this.Id || this.IsDestroyed == true || Rate == 1f){
				return;
			}

			

			if(IsGrouped == true){
				if(this.Rate == 0f && OnStartCallback != null && this.CurrentRepeatCount == 0){
					OnStartCallback();
				}

				var _childList = GetChildList();
				for(int i = 0; i < _childList.Count; i++){
					_childList[i].Skip(_childList[i].Id);
				}

				if(OnRepeatCallback != null){
					OnRepeatCallback(this, this.RepeatCount);
				}

				Rate = 1f;
				complete();
			}else{
				if(Rate == 0f){
					setValuesByFunc();

					if(OnStartCallback != null && this.CurrentRepeatCount == 0){
						OnStartCallback();
					}
				}

				Rate = 1f;
				this.CurrentRepeatCount = this.RepeatCount;
				updateForSingle();
			}
		}

		public Tween SubscribeStart(Action _callback, bool _clearSubscribe = false){
			if(_clearSubscribe == true){
				ClearSubscribeStart();
			}

			if(_callback != null){
				OnStartCallback += _callback;
			}

			return this;
		}

		public Tween ClearSubscribeStart(){
			OnStartCallback = null;
			return this;
		}

		public Tween SubscribeComplete(Action _callback, bool _clearSubscribe = false){
			if(_clearSubscribe == true){
				ClearSubscribeComplete();
			}

			if(_callback != null){
				OnCompleteCallback += _callback;
			}

			return this;
		}

		public Tween ClearSubscribeComplete(){
			OnCompleteCallback = null;
			return this;
		}

		public Tween SubscribeUpdate(Action<Vector4> _callback){
			OnUpdateCallback += _callback;
			return this;
		}

		public Tween SubscribeRepeat(Action<Tween, int> _callback){
			OnRepeatCallback += _callback;
			return this;
		}

		public Tween SetEase(EaseType _easeType){
			EaseType = _easeType;
			return this;
		}

		public Tween SetCaller(
			[CallerMemberName] string _memberName = "",
			[CallerFilePath] string _sourceFilePath = "",
			[CallerLineNumber] int _sourceLineNumber = 0){
			CallerInfo = _memberName + "," + _sourceFilePath + "," + _sourceLineNumber.ToString();
			return this;
		}

		// public Tween SetTimeType(Func<float> _timeType){
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

		public Tween SetRepeat(int _repeat){
			RepeatCount = _repeat;
			return this;
		}

		public Tween SetRepeatForever(){
			RepeatCount = -1;
			return this;
		}

		public Tween AddChild(Tween _tween){
			#if UNITY_EDITOR
			if(this.PlayingIndex > _tween.PlayingIndex){
				Debug.LogWarning($"[BicTween] Child tween Playing Index({_tween.PlayingIndex}) is bigger then parent({PlayingIndex})");
			}
			#endif

			_tween.Pause();
			_tween.IsLockedComplete = true;
			
			if(this.Type == TweenType.Spawn){
				this.Time = Mathf.Max(_tween.Time, this.Time);
			}else if(this.Type == TweenType.Sequance){
				this.Time += _tween.Time;
			}
			
			childDataList.Add(_tween.Id);
			return this;
		}

		public void SetParent(Tween _parent){
			_parent.AddChild(this);
		}

		public Tween AddTo(Tween _tween){
			_tween.AddChild(this);
			return this;
		}

		public Tween AddTo(MultiTween _maker){
			_maker.AddChild(this);
			return this;
		}

		public Tween InsertChild(Tween _tween, int _index){
			_tween.Pause();

			childDataList.Insert(_index, _tween.Id);
			return this;
		}

		public void RemoveChild(Tween _tween){
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

		public Tween SetTargetObject(GameObject _target){
			TargetObject = _target;
			return this;
		}

		#region Tracker
		[Obsolete("Use Tracker")]
		public TweenCancelObject CancelObject{
			get{return new TweenCancelObject(this);}
		}

		[Obsolete("Use SetTracker")]
		public Tween SetCancelObject(TweenCancelObject _cancelObject){
			_cancelObject.SetTween(this);

			return this;
		}

		public TweenTracker Tracker{
			get{return new TweenTracker(this);}
		}

		public Tween SetTracker(TweenTracker _tracker){
			_tracker.SetTween(this);
			return this;
		}

		public Tween SetTracker(string _id){
			var _tracker = this.pool.GetTracker(_id);
			this.SetTracker(_tracker);
			return this;
		}
		#endregion

		#region Task
		public TweenAwaiter GetAwaiter () {
			return new TweenAwaiter(this);
		}

		public async Task<Tween> GetTask(){
			return await this;
		}

		public void SetParent(List<Task<Tween>> _taskList){
			_taskList.Add(this.GetTask());
		}
		#endregion

        #if UNITY_EDITOR
        [NonSerialized]
        public Rect editor_rect;
		[NonSerialized]
		public Tween editor_parent;
		[NonSerialized]
		public string editor_targetPath;
        #endif
	}


// make the interface task-like
	public class TweenAwaiter : INotifyCompletion {
		private readonly Tween tween;
		int tweenIndex = 0;
		// wrap the async operation
		public TweenAwaiter (Tween _tween) {
			tweenIndex = _tween.Id;
			tween = _tween;
		}

		// is task already done (yes)
		public bool IsCompleted {
			get{
				return tween.Rate >= 1f && tween.RepeatCount == tween.CurrentRepeatCount || 
				tween.IsDestroyed == true ||
				tweenIndex != tween.Id;
			}
		}

		// wait until task is done (never called)
		public void OnCompleted (Action continuation){
			tween.SubscribeComplete(continuation);
		}

		// return the result
		public Tween GetResult () {
			return this.tween;
		}
	}

	public enum SpawnType{
		WaitAll,
		WaitAnyKeepPlayOtherChilds,
		WaitAnyCancelOtherChilds,
		WaitAnySkipOtherChilds
		
	}

}

