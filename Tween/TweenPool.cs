using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.Tween{
	public class TweenPool : MonoBehaviour {
		[SerializeField]
		public List<int> GroupIdList;
		[SerializeField]
		public List<TweenModel> TweenList;
		[SerializeField]
		public List<InitialInformations> InitialList;
		
		public TweenModel GetGroup(int _groupIndex){
			return GetTween(GroupIdList[_groupIndex]);
		}

		public TweenModel GetTween(string _name){
			string _names = "";
			for(int i = 0; i < TweenList.Count; i++){
				_names += TweenList[i].Name + ",";
				if(TweenList[i].Name == _name){
					return TweenList[i];
				}
			}

			throw new SystemException("not found tween. Name = " + _name.ToString() + "/" + this.name + "/" + _names);
		}

		public void ApplyInitialInformation(int _groupId){
			var _informations = GetInitialInformations(_groupId);

			if(_informations != null){
				_informations.Apply();
			}else{
				#if UNITY_EDITOR
				//Debug.Log("[BicTween] Not found IntialInformations");
				#endif
			}
		}

		public InitialInformations GetInitialInformations(int _groupId){
			if(InitialList != null){
				for(int i = 0; i < InitialList.Count; i++){
					if(InitialList[i].TargetTweenId == _groupId){
						return InitialList[i];
					}
				}
			}

			return null;
		}

		public TweenModel GetTween(int _id){
			for(int i = 0; i < TweenList.Count; i++){
				if(TweenList[i].Id == _id){
					return TweenList[i];
				}
			}

			throw new SystemException("not found tween. id = " + _id.ToString());
		}

		public int GetTweenIndex(int _id){
			for(int i = 0; i < TweenList.Count; i++){
				if(TweenList[i].Id == _id){
					return i;
				}
			}

			throw new SystemException("not found tween. id = " + _id.ToString());
		}

		public void AddTween(TweenModel _group, TweenModel _tween){
			_group.AddChild(_tween);
		}

		public void RemoveTween(TweenModel _group, TweenModel _tween){
			_group.RemoveChild(_tween);
			_tween.Remove();
		}

		public void RemoveTween(TweenModel _tween){
			TweenList.Remove(_tween);
		}

		public void RemoveGroup(TweenModel _group){
			GroupIdList.Remove(_group.Id);
			InitialList.RemoveAll(_initial=>_initial.TargetTweenId == _group.Id);
			_group.Remove();
		}

		public void AddChildTween(TweenModel _parent, TweenModel _child){
			_parent.AddChild(_child);
		}

		public void AddGroup(){
			var _newTween = BicTween.Sequance(this);
			_newTween.Name = "Group" + _newTween.Id;
			
			if(GroupIdList == null){
				GroupIdList = new List<int>();
			}
			
			GroupIdList.Add(_newTween.Id);
		}

		public string GetTargetPath(GameObject _target){
			return findTargetPath(gameObject, _target);
		}

		private string findTargetPath(GameObject _parent, GameObject _target){
			if(_target == null){
				return string.Empty;
			}

			if(_target == _parent){
				return _parent.name;
			}

			for(int i = 0; i < _parent.transform.childCount; i++){
				Transform _child = _parent.transform.GetChild(i);
				if(_child.gameObject == _target){
					return _parent.name + "/" + _child.name;
				}else{
					var _find = findTargetPath(_child.gameObject, _target);
					if(_find != string.Empty){
						return _parent.name + "/" + _find;
					}
				}
			}

			return string.Empty;
		}

		public GameObject GetTargetObject(string _path){
			var _paths = _path.Split('/');
			
			var _parent = gameObject.transform;
			for(int i = 1; i < _paths.Length; i++){
				if(_parent == null){
					return null;
				}

				_parent = _parent.Find(_paths[i]);
			}

			return _parent.gameObject;
		}

		public void RefindAllTargetObject(){
			#if UNITY_EDITOR
			for(int i = 0; i < TweenList.Count; i++){
				if(string.IsNullOrEmpty(TweenList[i].editor_targetPath) == false){
					var _findTarget = GetTargetObject(TweenList[i].editor_targetPath);
					if(_findTarget != null){
						TweenList[i].TargetObject = _findTarget;
					}
				}
			}
			#else
			throw new SystemException("[BicTween] for Editor method");
			#endif
		}

		public void RefindTargetObject(TweenModel _group){
			#if UNITY_EDITOR
			var _childList = _group.GetChildList();

			for(int i = 0; i < _childList.Count; i++){
				var _child = _childList[i];
				if(_child.IsGrouped){
					RefindTargetObject(_child);
				}else if(string.IsNullOrEmpty(_child.editor_targetPath) == false){
					var _findTarget = GetTargetObject(_child.editor_targetPath);
					if(_findTarget != null){
						_child.TargetObject = _findTarget;
					}
				}
			}
			#else
			throw new SystemException("[BicTween] for Editor method");
			#endif
		}

		[NonSerialized]
		public int MaxPlayingIndex = 0;
		public bool IsLocked = false;
		[SerializeField]
		private int nextId = 0;
		public int NextId{ get{ return nextId++;} }

		public TweenModel CreateModel(){
			if(TweenList == null){
				TweenList = new List<TweenModel>(100);
			}
			
			var _count = TweenList.Count;
			for(int i = 0; i < _count; i++){
				if(TweenList[i] != null){
					var __model = TweenList[i];

					if(IsLocked == false){
						if(__model.destoryCount == TweenModel.DESTORY_READY_TO_RECYCLE){
							__model.Clear();
							__model.Id = NextId;
							__model.Name = __model.Id.ToString();
							__model.CreatedFrameCount = Time.frameCount;
							__model.Play();
							return __model;
						}
					}
				}
			}

			var _result = new TweenModel();
			_result.pool = this;
			_result.Clear();
			_result.Id = NextId;
			_result.Name = _result.Id.ToString();
			_result.CreatedFrameCount = Time.frameCount;
			TweenList.Add(_result);
			_result.Play();
			return _result;
		}

		private bool isUpdatedPlayingMax = false;

		public void UpdateMaxPlayingIndex(int _index){
			if(MaxPlayingIndex < _index){
				MaxPlayingIndex = Mathf.Min(_index, TweenList.Count - 1);
			}

			isUpdatedPlayingMax = true;
		}

		public int GetIndex(TweenModel _tween){
			for(int i = 0; i < TweenList.Count; i++){
				if(TweenList[i] == _tween){
					return i;
				}
			}

			throw new SystemException("[BicTween] Does not find tween " + _tween.Name + "(" + _tween.Id.ToString() + ")");
		}

		public void Update(){

			if(TweenList == null){
				TweenList = new List<TweenModel>();
			}

			int _lastPlayingIndex = -1;
			int _frameCount = Time.frameCount;
			if(MaxPlayingIndex >= 0){
				var _count = Mathf.Min(MaxPlayingIndex, TweenList.Count - 1);
				for(int i = 0; i <= _count; i++){
					var _tween = TweenList[i];
					try{
						if(_tween != null && _tween.IsPlaying == true){
							if(_tween.IsDestroyed == false){
								_lastPlayingIndex = i;
							}else{
								continue;
							}
							

							if(_tween.Update == null){
								_tween.SetUpdate();
							}

							if(_tween.CreatedFrameCount == _frameCount){
								continue;
							}
							
							if(_tween.Update != null){
								_tween.Update();
							}
						}else if(_tween.destoryCount > TweenModel.DESTORY_READY_TO_RECYCLE){
							_tween.destoryCount--;
							_lastPlayingIndex = i;
						}
					}catch(SystemException _e){
						Debug.LogWarning("[BicTween] Error In Update /" + _tween.Type.ToString() + "/" + _tween.CallerInfo);
						throw _e;
					}
				}
			}

			if(isUpdatedPlayingMax == false){
				MaxPlayingIndex = _lastPlayingIndex;
			}

			isUpdatedPlayingMax = false;
		}

		public void DontDestroy(){
			#if UNITY_EDITOR
			if(Application.isPlaying == true){
				DontDestroyOnLoad(this);
			}
			#else
				DontDestroyOnLoad(this);
			#endif
		}

		public void Cancel(GameObject _object){
			if(_object == null){
				return;
			}
			
			var _count = TweenList.Count;
			for(int i = 0; i < TweenList.Count; i++){
				var _tween = TweenList[i];
				if(_tween.TargetObject == _object && _tween.IsPlaying == true){
					_tween.Cancel(_tween.Id);
				}
			}
		}

		public void CancelAll(){
			var _count = TweenList.Count;
			for(int i = 0; i < TweenList.Count; i++){
				var _tween = TweenList[i];
				if(_tween.IsPlaying == true){
					_tween.Cancel(_tween.Id);
				}
			}
		}

		#region List
		public Dictionary<string, TweenTracker> trackers = new Dictionary<string, TweenTracker>();
		public TweenTracker GetTracker(string _id){
			if(this.trackers.ContainsKey(_id) == false){
				var _tracker = new TweenTracker();
				this.trackers[_id] = _tracker;
				return _tracker;
			}else{
				return this.trackers[_id];
			}
		}
		#endregion
  	}
}
