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
		
		// private void Update(){
		// 	pool.Update();
		// }
		public TweenModel GetGroup(int _groupIndex){
			return GetTween(GroupIdList[_groupIndex]);
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
		private int maxPlayingIndex = 0;
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
							__model.IsPlaying = true;
							__model.destoryCount = 0;
							UpdateMaxPlayingIndex(i);
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
			_result.IsPlaying = true;
			UpdateMaxPlayingIndex(_count);
			TweenList.Add(_result);
			return _result;


			throw new SystemException("[BicTween] Pool is full");
		}

		public void UpdateMaxPlayingIndex(int _index){
			if(maxPlayingIndex < _index){
				maxPlayingIndex = _index;
			}
		}

		public void Update(){

			if(TweenList == null){
				TweenList = new List<TweenModel>(100);
			}

			updateDeltaTime();
			int _lastPlayingIndex = 0;
			var _count = TweenList.Count;
			for(int i = 0; i < _count; i++){
				if(TweenList[i] != null && TweenList[i].IsPlaying == true){
					if(TweenList[i].Update == null){
						TweenList[i].SetUpdate();
					}
					
					if(TweenList[i].Update != null){
						TweenList[i].Update();
					}

					if(TweenList[i].IsDestroyed == false){
						_lastPlayingIndex = i;
					}
				}
			}

			maxPlayingIndex = _lastPlayingIndex;
			
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
