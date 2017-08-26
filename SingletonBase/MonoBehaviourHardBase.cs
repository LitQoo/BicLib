using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BicUtil.SingletonBase
{
	public class MonoBehaviourHardBase<T> : MonoBehaviour where T: class {
		private static T instance;  
		public static T Instance  
		{  
			get{
				if(instance == null)  
				{  
					var _container = new GameObject();  
					_container.name = "Singleton";  
					instance = _container.AddComponent(typeof(T)) as T;  
					DontDestroyOnLoad(_container);
				}  

				return instance;  
			}
		}  
	}
}