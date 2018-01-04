using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BicUtil.Tween{
	public class TweenPool : MonoBehaviour {
		//public TweenPool pool = new TweenPool();
		[SerializeField]
		public List<int> GroupIdList;
		[SerializeField]
		public List<TweenModel> TweenList;
		
		private void Awake(){
			//setPool(TweenList);
		}

		public TweenModel GetGroup(int _groupIndex){
			return GetTween(GroupIdList[_groupIndex]);
		}

		public TweenModel GetTween(string _name){
			for(int i = 0; i < TweenList.Count; i++){
				if(TweenList[i].Name == _name){
					return TweenList[i];
				}
			}

			throw new SystemException("not found tween. id = " + _name.ToString());
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
			_group.Remove();
		}

		public void AddChildTween(TweenModel _parent, TweenModel _child){
			_parent.AddChild(_child);
		}

		public void AddGroup(){
			var _newTween = BicTween.Sequance(this);
			_newTween.Name = "Group" + _newTween.Id;
			GroupIdList.Add(_newTween.Id);
		}

		//[NonSerialized]
		//public TweenModel[] Pool = new TweenModel[100];
		[SerializeField]
		public int MaxPlayingIndex = 0;
		private float previousRealTime;
		private float realDeltaTime = 0;
		public bool IsLocked = false;

		public TweenModel CreateModel(){
			if(TweenList == null){
				TweenList = new List<TweenModel>(100);
			}

			var _count = TweenList.Count;
			int _maxId = 0;
			for(int i = 0; i < _count; i++){
				if(TweenList[i] != null){
					var __model = TweenList[i];

					if(IsLocked == false){
						if(__model.destoryCount == 1){
							__model.Clear();
							__model.Play();
							Debug.Log("reuse tween!" + i.ToString());
							return __model;
						}else if(__model.destoryCount > 1){
							__model.destoryCount--;
						}
					}

					_maxId = Math.Max(_maxId, TweenList[i].Id);
				}
			}

			var _result = new TweenModel();
			_result.pool = this;
			_result.Clear();
			_result.Id = _maxId + 1;
			_result.Name = _result.Id.ToString();
			TweenList.Add(_result);
			_result.Play();
			Debug.Log("new tween create! " + _result.Id.ToString());
			return _result;


			throw new SystemException("[BicTween] Pool is full");
		}

		private bool isUpdatedPlayingMax = false;

		public void UpdateMaxPlayingIndex(int _index){
			if(MaxPlayingIndex < _index){
				MaxPlayingIndex = Math.Min(_index, TweenList.Count - 1);
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
				TweenList = new List<TweenModel>(100);
			}

			updateDeltaTime();
			int _lastPlayingIndex = -1;
			if(MaxPlayingIndex >= 0){
				//var _count = TweenList.Count;
				for(int i = 0; i <= MaxPlayingIndex; i++){
					var _tween = TweenList[i];
					if(_tween != null && _tween.IsPlaying == true){
						if(_tween.IsDestroyed == false){
							_lastPlayingIndex = i;
						}else{
							continue;
						}

						if(_tween.Update == null){
							_tween.SetUpdate();
						}
						
						if(_tween.Update != null){
							_tween.Update();
						}
					}
				}
			}

			if(isUpdatedPlayingMax == false){
				MaxPlayingIndex = _lastPlayingIndex;
			}

			isUpdatedPlayingMax = false;
		}

		private void updateDeltaTime(){
			#if UNITY_EDITOR
			if(Application.isPlaying){
				realDeltaTime = Time.deltaTime;
			}else{
				realDeltaTime = Time.realtimeSinceStartup - previousRealTime;
				previousRealTime = Time.realtimeSinceStartup;
			}
			#else
				deltaTime = time.deltaTime;
			#endif

		}

		// public void setPool(List<TweenModel> _list){
		// 	if(_list == null){
		// 		return;
		// 	}

		// 	TweenList = new TweenModel[Math.Max(_list.Count, 100)];
		// 	for(int i = 0; i < _list.Count; i++){
		// 		if(_list[i].Id != i){
		// 			TweenList[i] = _list[i];
		// 			Debug.LogWarning("[BicTween] somthing wrong");
		// 		}else{
		// 			TweenList[i] = _list[i];
		// 		}
		// 	}
		// }

		public void DontDestroy(){
			#if UNITY_EDITOR
			if(Application.isPlaying == true){
				DontDestroyOnLoad(this);
			}
			#else
				DontDestroyOnLoad(this);
			#endif
		}

		// public void reflashPool(){
		// 	setPool(TweenList);
		// }
    }
}
