#if BICUTIL_WWW
using UnityEngine;
using System.Collections;
using System;
using BicDB.Container;
using BicDB.Variable;
using System.Runtime.CompilerServices;
using BicUtil.Json;
using UnityEngine.Networking;

namespace BicDB.Storage
{
	public class WebStorage : MonoBehaviour, ITableStorage {
		#region Static
		static public string LOAD_URL_KEY = "webstorageLoadURL";
		static public string SEND_RECORD_URL = "sendRecordURL";
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1,
			ErrorNetwork = 2
		}

		#region singleton
		private static WebStorage instance = null;  
		private static GameObject container;  
		public static WebStorage Instance{
			get{
				if(instance == null){
					GetInstance();
				}

				return instance;
			}
		}
		public static ITableStorage GetInstance()  
		{  
			if(instance == null)  
			{  
				container = new GameObject();  
				container.name = "BicDBWebStorage";  
				instance = container.AddComponent(typeof(WebStorage)) as WebStorage;  
				DontDestroyOnLoad(container);
			}  

			return instance;  
		}  
		#endregion

		#region IStorage
		public string StorageType{get{ return "WebStorage"; }}
		
		public void Save<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success));
			}
		}

		public void Push<T>(ITableContainer<T> _table, Action<Result> _callback) where T : IRecordContainer, new()
		{
			if (_callback != null) {
				_callback(new Result((int)ResultCode.Success));
			}
		}

		private Action<Result> loadCallback = null;
		public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			_table.Clear();
			this.Pull(_table, _callback, _parameter);
		}

		public void Pull<T>(ITableContainer<T> _table, Action<Result> _callback, object _parameter) where T : IRecordContainer, new (){
			loadCallback = _callback;

			if (!_table.Header.ContainsKey (LOAD_URL_KEY)) {
				throw new SystemException ("not found Header " + LOAD_URL_KEY);
			}

			var _form = new WWWForm();
			var _request = UnityWebRequest.Post(_table.Header[LOAD_URL_KEY].AsVariable.AsString, _form);
			this.SendWebRequest(_request, _result=>{
				var _storageResult = new Result ((int)ResultCode.Success);
				var _webParam = _parameter as WebStorageParameter;
				string _json = string.Empty;
				if(_webParam != null && _webParam.RequestConvertor != null){
					_json = _webParam.RequestConvertor(_result.downloadHandler.text);
				}else{
					_json = _result.downloadHandler.text;
				}

				int _counter = 0;
				if (string.IsNullOrEmpty(_result.error) == false)
				{
					_storageResult.Code = (int)ResultCode.ErrorNetwork;
					_storageResult.Message = _result.error;
				}
				else
				{
					try {
						JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);
					} catch (Exception) {
						_storageResult.Code = (int)ResultCode.FailedConvertJson;
						_storageResult.Message = ResultCode.FailedConvertJson.ToString ();
					}
				}

				if (loadCallback != null) {
					loadCallback (_storageResult);
				}
			});
		}

		private IEnumerator sendWebRequestCoroutine(UnityWebRequest _request, Action<UnityWebRequest> _callback){
			yield return _request.SendWebRequest();
			_callback(_request);
		}

		public void SendWebRequest(UnityWebRequest _request, Action<UnityWebRequest> _callback){
			StartCoroutine(this.sendWebRequestCoroutine(_request, _callback));
		}

		public void SendRecord<T>(ITableContainer<T> _table, T _record, Action<Result> _callback = null) where T : IRecordContainer, new (){
			if(string.IsNullOrEmpty(_table.PrimaryKey) == true){
				throw new SystemException("Need to set PrimaryKey");
			}

			if (!_table.Header.ContainsKey (SEND_RECORD_URL)) {
				throw new SystemException ("not found Header " + SEND_RECORD_URL);
			}

			var _form = new WWWForm();
			_form.AddField("data", _record.ToString());
			_form.AddField(_table.PrimaryKey, _record[_table.PrimaryKey].AsVariable.AsString);

			var _request = UnityWebRequest.Post(_table.Header[SEND_RECORD_URL].AsVariable.AsString, _form);

			WebStorage.Instance.SendWebRequest(_request, _result=>{
				if(_result.isHttpError == true || _result.isNetworkError == true){
					if(_callback != null){
						_callback(new Result((int)ResultCode.ErrorNetwork));
					}
					return;
				}

				var _resultRecord = new RecordContainer();
				_resultRecord.AddManagedColumn("result", new IntVariable());
				_resultRecord.AddManagedColumn(_table.PrimaryKey, new StringVariable());
				if(_resultRecord.ParseJson(_result.downloadHandler.text) == true){
					if(_resultRecord["result"].AsVariable.AsInt == 0){
						if(string.IsNullOrEmpty(_record[_table.PrimaryKey].AsVariable.AsString) == true){
							_record[_table.PrimaryKey].AsVariable.AsString = _resultRecord[_table.PrimaryKey].AsVariable.AsString;
						}
						
						_table.AddWithoutDuplication(_record);
						if(_callback != null){
							_callback(new Result((int)ResultCode.Success));
						}
						return;
					}else{
						if(_callback != null){
							_callback(new Result((int)ResultCode.ErrorNetwork));
						}
						return;
					}
				}else{
					if(_callback != null){
						_callback(new Result((int)ResultCode.FailedConvertJson));
					}
					return;
				}
			});
		}
		#endregion

	}

	public class WebStorageParameter{
		public Func<string, string> RequestConvertor = null;

		public WebStorageParameter(Func<string, string> _requestConvertor){
			RequestConvertor = _requestConvertor;
		}
	}
}
#endif