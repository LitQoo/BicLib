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
		#endregion

		#region logic
		private void initialize(){

			var _orderedList = initObjects.OrderBy (_object => _object.Order);

			foreach (var _item in _orderedList) {
				if (_item.Target != null) {
					_item.Target.Initialize ();
				}
			}
		}
		#endregion
	}

	public abstract class ClassInitializerObject : MonoBehaviour, IClassInitializerObject
	{
		public abstract void Initialize ();
	}

	public interface IClassInitializerObject
	{
		void Initialize ();
	}

	[System.Serializable]
	public class ClassInitializerObjectInfo{
		public ClassInitializerObject Target;
		public int Order;
	}
}

