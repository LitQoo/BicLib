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
					Debug.LogWarning("Need Config Script Execution Order for Singleton");

					var _container = new GameObject();  
					_container.name = "TemporarySingleton";  
					instance = _container.AddComponent(typeof(T)) as T;  
					DontDestroyOnLoad(_container);
				}

				return instance;
			}
		}  

		protected void Awake(){
			instance = this as T;
		}
	}

	public class SingletonBase<T> where T : class, new()
	{
		private static T instance = null;
		public static T Instance{
			get{
				if(instance == null){
					instance = new T();
				}

				return instance;
			}
		}
	}
}