using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BicUtil.SingletonBase
{
	public class MonoBehaviourHardBase<T> : MonoBehaviour where T: class {
		protected static string SingletonName = "Singleton";
		private static T instance;  
		public static T Instance  
		{  
			get{
				if(instance == null)  
				{  
					var _container = new GameObject();  
					_container.name = SingletonName;
					instance = _container.AddComponent(typeof(T)) as T;  
					DontDestroyOnLoad(_container);
				}  

				return instance;  
			}
		}  
	}

	public interface ISingleton{
		void OnCreatedSingleton();

	}	

	
	public class MonoBehaviourSingleton<T> : MonoBehaviour where T: class, ISingleton {
		protected static string SingletonName = "Singleton";
		private static T instance;  
		public static T Instance  
		{  
			get{
				if(instance == null)  
				{  
					var _container = new GameObject();  
					_container.name = SingletonName;
					instance = _container.AddComponent(typeof(T)) as T;
					instance.OnCreatedSingleton();  
					DontDestroyOnLoad(_container);
				}  

				return instance;  
			}
		}  
	}


	// public interface ISingletonInjection{

	// }
	// static public class MonoBehaviourSingletonExtensions{
    //     public static T GetSingleton<T>(this ISingletonInjection _page) where T : MonoBehaviourSingleton<T>, ISingleton{
    //         return MonoBehaviourSingleton<T>.Instance;
    //     }
    // }
}