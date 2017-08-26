using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BicUtil.SingletonBase
{
	public class MonoBehaviourSoftBase<T> : MonoBehaviour where T: class {
		private static T instance = null;
		public static T Instance{
			get{ 
				if (instance == null) {
					throw new System.Exception ("Need Config Script Execution Order for Singleton");
				}

				return instance;
			}
		}  

		protected void Awake(){
			instance = this as T;
		}
	}
}