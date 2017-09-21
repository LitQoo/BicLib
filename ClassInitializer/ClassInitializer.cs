using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BicUtil.ClassInitializer
{
	public class ClassInitializer : MonoBehaviour {
		#region LinkingObject
		[SerializeField]
		private ClassInitializerObject[] initObjects;
		#endregion

		#region LifeCycle
		private void Awake(){
			initialize ();
		}
		#endregion

		#region logic
		private void initialize(){
			for (int i = 0; i < initObjects.Length; i++) {
				if (initObjects [i] != null) {
					initObjects [i].Initialize ();
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
}

