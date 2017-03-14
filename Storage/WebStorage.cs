using UnityEngine;
using System.Collections;
using System;
using BicDB.Utility;
using BicDB.Variable;

namespace BicDB.Storage
{
	public class WebStorage : MonoBehaviour, IStorage {
		#region Static
		static public string LOAD_URL_KEY = "webstorageLoadURL";
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1,
			ErrorNetwork = 2
		}

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
		public void Save<T>(ITable<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IModelVariable, new() {

			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success));
			}
		}

		private Action<Result> loadCallback = null;
		public void Load<T>(ITable<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IModelVariable, new() {
			loadCallback = _callback;
			StartCoroutine(getTextFromWWW(_table));

		}

		private IEnumerator getTextFromWWW<T> (ITable<T> _table) where T : IModelVariable, new()
		{
			if (!_table.Header.ContainsKey (LOAD_URL_KEY)) {
				throw new SystemException ("not found Header " + LOAD_URL_KEY);
			}

			WWW www = new WWW((_table.Header[LOAD_URL_KEY] as IVariable).AsString);
			yield return www;

			var _result = new Result ((int)ResultCode.Success);
			var _json = www.text;
			int _counter = 0;
			if (www.error != null)
			{
				_result.Code = (int)ResultCode.ErrorNetwork;
				_result.Message = www.error;
			}
			else
			{
				try {
					JsonConvertor.GetInstance().BuildTableVariable(_table, ref _json, ref _counter);
				} catch (Exception) {
					_result.Code = (int)ResultCode.FailedConvertJson;
					_result.Message = ResultCode.FailedConvertJson.ToString ();
				}
			}

			if (loadCallback != null) {
				loadCallback (_result);
			}
		}
		#endregion

	}
}