using UnityEngine;
using System.Collections;
using System;

namespace BicDB.Storage
{
	public class WebStorage : MonoBehaviour, IStorage {
		#region Static
		static public string LOAD_URL_KEY = "webstorageLoadURL";
		#endregion

		#region singleton
		private static IStorage instance = null;  
		private static GameObject container;  
		public static IStorage GetInstance()  
		{  
			if(instance == null)  
			{  
				container = new GameObject();  
				container.name = "BicDBWebStorage";  
				instance = container.AddComponent(typeof(WebStorage)) as IStorage;  
				DontDestroyOnLoad(container);
			}  

			return instance;  
		}  
		#endregion

		#region IStorage
		public void Save<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {

			if (_callback != null) {
				_callback(true);
			}
		}

		private Action<bool> loadCallback = null;
		public void Load<T>(ITable<T> _table, Action<bool> _callback = null, object _parameter = null) where T : IModel, new() {
			loadCallback = _callback;
			StartCoroutine(getTextFromWWW(_table));

		}

		private IEnumerator getTextFromWWW<T> (ITable<T> _table) where T : IModel, new()
		{
			if (!_table.ContainsHeader (LOAD_URL_KEY)) {
				throw new SystemException ("not found Header " + LOAD_URL_KEY);
			}

			WWW www = new WWW(_table.GetHeader(LOAD_URL_KEY).AsString);
			yield return www;

			bool _isSuccess = false;

			if (www.error != null)
			{
				_isSuccess = false;
			}
			else
			{
				_isSuccess = true;

				try {
					JsonConvertor.ConvertJsonDictionaryToTable(www.text, _table);
				} catch (Exception) {
					_isSuccess = false;
				}
			}

			if (loadCallback != null) {
				loadCallback (_isSuccess);
			}
		}
		#endregion

	}
}