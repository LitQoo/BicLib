using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BicUtil.SingletonBase
{
	public abstract class MonoBehaviourSoftBase<T> : MonoBehaviour where T: class {
		private static T instance = null;
		private static bool isInit = false;
		public static T Instance{
			get{ 
				if (instance == null && isInit == false) {
					Debug.LogWarning("Need Config Script Execution Order for Singleton");
					isInit = true;
					var _container = new GameObject();  
					_container.name = "TemporarySingleton";  
					instance = _container.AddComponent(typeof(T)) as T;  
					DontDestroyOnLoad(_container);
				}

				return instance;
			}

			protected set{
				instance = value; 
			}
		}  

		protected void Awake(){
			instance = this as T;
		}
	}

	public abstract class SingletonBase<T> : ISingletonObject where T : class, ISingletonObject, new()
	{
		private static T instance = null;
		private static bool isInit = false;
		public static T Instance{
			get{
				if(instance == null && isInit == false){
					isInit = true;
					instance = new T();
					instance.Initialize();
				}

				return instance;
			}
		}

		public abstract void Initialize();
	}

	public interface ISingletonObject{
		void Initialize();
	}
}