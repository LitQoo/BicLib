using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BicUtil.ClassInitializer
{
	public class ClassInitializer : MonoBehaviour {
		#region LinkingObject
		[SerializeField]
		private List<ClassInitializerObjectInfo> initObjects;
		#endregion

		#region LifeCycle
		private void Awake(){
			initialize ();
		}

		private void OnDestroy() {
			deinitialize ();
		}
		#endregion

		#region logic
		private void initialize(){
			var _orderedList = initObjects.OrderBy (_object => _object.Order);

			foreach (var _item in _orderedList) {
				if (_item.Target != null) {
					IClassInitializerObject[] _initObjects = _item.Target.GetComponents<IClassInitializerObject> ();
					for (int i = 0; i < _initObjects.Length; i++){
						_initObjects[i].Initialize ();
					}
				}
			}
		}

		private void deinitialize(){
			var _orderedList = initObjects.OrderBy (_object => _object.Order);

			foreach (var _item in _orderedList) {
				if (_item.Target != null) {
					IClassInitializerObject[] _initObjects = _item.Target.GetComponents<IClassInitializerObject> ();
					for (int i = 0; i < _initObjects.Length; i++){
						_initObjects[i].Deinitialize ();
					}
				}
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