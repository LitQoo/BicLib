#if BICUTIL_WWW
using UnityEngine;
using System.Collections;
using System;
using BicDB.Container;
using BicDB.Variable;
using System.Runtime.CompilerServices;
using BicUtil.Json;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BicDB.Storage
{
	public class WebStorage : MonoBehaviour, ITableStorage {
		#region Static
		static public string LOAD_URL_KEY = "webstorageLoadURL";
		static public string SEND_RECORD_URL = "sendRecordURL";
		static public string DELETE_RECORD_URL = "deleteRecordURL";
		static public string ENCRYPT = "encrypt";
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1,
			ErrorNetwork = 2,
			ServerRequestError = 3,
			Crypto = 3
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

		public void Load<T>(ITableContainer<T> _table, Action<Result> _callback = null, object _parameter = null) where T : IRecordContainer, new() {
			_table.Clear();
			this.Pull(_table, _callback, _parameter);
		}

		public Task<Result> LoadAsync<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new()
		{
			throw new NotImplementedException();
			// _table.Clear();
			//  this.PullAsync(_table, _callback, _parameter);
		}

		public void Pull<T>(ITableContainer<T> _targetTable, Action<Result> _callback, object _parameter) where T : IRecordContainer, new (){
			var _loadCallback = _callback;
			var _table = _targetTable;

			if (!_table.Header.ContainsKey (LOAD_URL_KEY)) {
				throw new SystemException ("not found Header " + LOAD_URL_KEY);
			}

			if (!_table.Header.ContainsKey (ENCRYPT)) {
				throw new SystemException ("not found Header " + ENCRYPT);
			}

			var _webParam = _parameter as WebStorageParameter;
			var _formData = new RecordContainer();

			if(_webParam != null && _webParam.Param != null){
				foreach(var _value in _webParam.Param){
					_formData.AddManagedColumn(_value.Key, new StringVariable(_value.Value));
				}
			}

			var _form = new WWWForm();
			var _formDataString = _formData.ToString();
			if(_table.Header[ENCRYPT].AsVariable.AsBool == true){
				_formDataString = BicUtil.Crypto.AES256.Encrypt(_formDataString);
			}
			_form.AddField("data", _formDataString);

			var _request = UnityWebRequest.Post(_table.Header[LOAD_URL_KEY].AsVariable.AsString, _form);
			this.SendWebRequest(_request, _result=>{
				var _storageResult = new Result ((int)ResultCode.Success);
				string _json = _result.downloadHandler.text;
				

				int _counter = 0;
				if (string.IsNullOrEmpty(_result.error) == false)
				{
					_storageResult.Code = (int)ResultCode.ErrorNetwork;
					_storageResult.Message = _result.error;
				}
				else
				{
					try{
						if(_table.Header[ENCRYPT].AsVariable.AsBool == true){
							_json = BicUtil.Crypto.AES256.Decrypt(_json);
						}
					}catch{
						if(_loadCallback != null){
							_storageResult.Code = (int)ResultCode.Crypto;
							_loadCallback(_storageResult);
						}
						return;
					}

					if(_webParam != null && _webParam.RequestConvertor != null){
						_json = _webParam.RequestConvertor(_json);
					}

					try {
						JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);
					} catch (Exception) {
						_storageResult.Code = (int)ResultCode.FailedConvertJson;
						_storageResult.Message = ResultCode.FailedConvertJson.ToString ();
					}
				}

				if (_loadCallback != null) {
					_loadCallback (_storageResult);
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
			SendRecord(_table, _record, null, _callback);
		}

		public void DeleteRecord<T>(ITableContainer<T> _targetTable, IVariable _primaryValue, Dictionary<string, string> _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _table = _targetTable;
			if(string.IsNullOrEmpty(_table.PrimaryKey) == true){
				throw new SystemException("Need to set PrimaryKey");
			}

			if (!_table.Header.ContainsKey (SEND_RECORD_URL)) {
				throw new SystemException ("not found Header " + SEND_RECORD_URL);
			}

			if (!_table.Header.ContainsKey (ENCRYPT)) {
				throw new SystemException ("not found Header " + ENCRYPT);
			}

			throw new NotImplementedException();


			var _formData = new RecordContainer();
			_formData.AddManagedColumn("primaryValue", _primaryValue);
			_formData.AddManagedColumn("primaryKey", new StringVariable(_table.PrimaryKey));

			if(_param != null){
				foreach(var _value in _param){
					_formData.AddManagedColumn(_value.Key, new StringVariable(_value.Value));
				}
			}

			var _form = new WWWForm();
			var _formDataString = _formData.ToString();

			if(_table.Header[ENCRYPT].AsVariable.AsBool == true){
				_formDataString = BicUtil.Crypto.AES256.Encrypt(_formDataString);
			}

			_form.AddField("data", _formDataString);

			var _request = UnityWebRequest.Post(_table.Header[DELETE_RECORD_URL].AsVariable.AsString, _form);


			WebStorage.Instance.SendWebRequest(_request, _result=>{

				if(_result.isHttpError == true || _result.isNetworkError == true){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.ErrorNetwork, "", 0, _result.error));
					}
					return;
				}

				string _json = _result.downloadHandler.text;
				try{
					if(_table.Header[ENCRYPT].AsVariable.AsBool == true){
						_json = BicUtil.Crypto.AES256.Decrypt(_json);
					}
				}catch{
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.Crypto));
					}
				}

				var _resultRecord = new RecordContainer();
				_resultRecord.AddManagedColumn("result", new IntVariable());

				if(_resultRecord.ParseJson(_json) == true){
					if(_resultRecord["result"].AsVariable.AsInt == 0){
						for(int i = _table.Count - 1; i >= 0; i--){
							if(_table[i][_table.PrimaryKey].AsVariable.AsString == _primaryValue.AsString){
								_table.RemoveAt(i);
							}
						}

						if(_table.OnSave != null){
							_table.OnSave(new Result((int)ResultCode.Success));
						}

						if(_resultCallback != null){
							_resultCallback(new Result((int)ResultCode.Success));
						}
						return;
					}else{
						if(_resultCallback != null){
							_resultCallback(new Result((int)ResultCode.ServerRequestError));
						}
						return;
					}
				}else{
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.FailedConvertJson));
					}
					return;
				}
			});

		}

		public void SendRecord<T>(ITableContainer<T> _targetTable, T _targetRecord, Dictionary<string, string> _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _table = _targetTable;
			var _record = _targetRecord;
			if(string.IsNullOrEmpty(_table.PrimaryKey) == true){
				throw new SystemException("Need to set PrimaryKey");
			}

			if (!_table.Header.ContainsKey (SEND_RECORD_URL)) {
				throw new SystemException ("not found Header " + SEND_RECORD_URL);
			}

			if (!_table.Header.ContainsKey (ENCRYPT)) {
				throw new SystemException ("not found Header " + ENCRYPT);
			}

			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _record);
			_formData.AddManagedColumn(_table.PrimaryKey, _record[_table.PrimaryKey].AsVariable);
			
			if(_param != null){
				foreach(var _value in _param){
					_formData.AddManagedColumn(_value.Key, new StringVariable(_value.Value));
				}
			}

			var _form = new WWWForm();
			var _formDataString = _formData.ToString();

			if(_table.Header[ENCRYPT].AsVariable.AsBool == true){
				_formDataString = BicUtil.Crypto.AES256.Encrypt(_formDataString);
			}
			_form.AddField("data", _formDataString);


			var _request = UnityWebRequest.Post(_table.Header[SEND_RECORD_URL].AsVariable.AsString, _form);

			WebStorage.Instance.SendWebRequest(_request, _result=>{
				
				if(_result.isHttpError == true || _result.isNetworkError == true){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.ErrorNetwork, "", 0, _result.error));
					}
					return;
				}

				string _json = _result.downloadHandler.text;
				try{
					if(_table.Header[ENCRYPT].AsVariable.AsBool == true){
						_json = BicUtil.Crypto.AES256.Decrypt(_json);
					}
				}catch{
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.Crypto));
					}
				}

				var _resultRecord = new RecordContainer();
				_resultRecord.AddManagedColumn("result", new IntVariable());
				_resultRecord.AddManagedColumn(_table.PrimaryKey, new StringVariable());

				if(_resultRecord.ParseJson(_json) == true){
					if(_resultRecord["result"].AsVariable.AsInt == 0){
						if(string.IsNullOrEmpty(_record[_table.PrimaryKey].AsVariable.AsString) == true){
							_record[_table.PrimaryKey].AsVariable.AsString = _resultRecord[_table.PrimaryKey].AsVariable.AsString;
						}
						
						_table.AddWithoutDuplication(_record);

						if(_table.OnSave != null){
							_table.OnSave(new Result((int)ResultCode.Success));
						}

						if(_resultCallback != null){
							_resultCallback(new Result((int)ResultCode.Success));
						}

						return;
					}else{
						if(_resultCallback != null){
							_resultCallback(new Result((int)ResultCode.ServerRequestError));
						}
						return;
					}
				}else{
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.FailedConvertJson));
					}
					return;
				}
			});
		}
        #endregion

    }

	public class WebStorageParameter{
		public Func<string, string> RequestConvertor = null;
		public Dictionary<string, string> Param = null;
		public WebStorageParameter(Func<string, string> _requestConvertor, Dictionary<string, string> _param){
			RequestConvertor = _requestConvertor;
			Param = _param;
		}
	}
}
#endif