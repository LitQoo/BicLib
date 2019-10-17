using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BicUtil.ClassInitializer
{
	public class ClassInitializer : MonoBehaviour {
		#region  Static
		static public ClassInitializer Instance = null;
		#endregion

		#region LinkingObject
		[SerializeField]
		private Transform initObjectParent;
		[SerializeField]
		private List<ClassInitializerObjectInfo> initObjects;
		private bool isDeinitialize = false;
		#endregion

		#region LifeCycle
		private void Awake(){
			initialize ();
		}

		private void OnDestroy() {
			if(isDeinitialize == false){
				throw new System.Exception("did not Deinitialize");
			}
		}

		private void OnApplicationQuit() {
			Deinitialize();
		}
		#endregion

		#region logic
		private void initialize(){
			ClassInitializer.Instance = this;

			if(initObjectParent != null){
				var _initCount = initObjects.Count;
				for(int i = 0; i < initObjectParent.childCount; i++){
					var _child = initObjectParent.GetChild(i);
					var _isExists = initObjects.Exists(_row=>_row.Target == _child.gameObject);
					if(_isExists == false){
						initObjects.Add(new ClassInitializerObjectInfo(_child.gameObject, _initCount + i));
					}
				}
			}

			var _orderedList = initObjects.OrderBy (_object => _object.Order);

			foreach (var _item in _orderedList) {
				//if (_item.Target != null) {
					IClassInitializerObject[] _initObjects = _item.Target.GetComponents<IClassInitializerObject> ();
					for (int i = 0; i < _initObjects.Length; i++){
						_initObjects[i].Initialize ();
					}
				//}
			}
		}

		public void Deinitialize(){
			ClassInitializer.Instance = null;
			if( isDeinitialize == false){
				var _orderedList = initObjects.OrderBy (_object => _object.Order);

				foreach (var _item in _orderedList) {
					//if (_item.Target != null) {
						IClassInitializerObject[] _initObjects = _item.Target.GetComponents<IClassInitializerObject> ();
						for (int i = 0; i < _initObjects.Length; i++){
							_initObjects[i].Deinitialize ();
						}
					//}
				}

				isDeinitialize = true;
			}
		}
		#endregion
	}

	public interface IClassInitializerObject
	{
		void Initialize ();
		void Deinitialize();
	}

	[System.Serializable]
	public class ClassInitializerObjectInfo{
		public GameObject Target;
		public int Order;

		public ClassInitializerObjectInfo(GameObject _target, int _order){
			this.Target = _target;
			this.Order = _order;
		}
	}
}