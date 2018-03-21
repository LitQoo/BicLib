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
	}
}