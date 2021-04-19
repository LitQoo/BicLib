#if BICUTIL_WWW
using UnityEngine;
using System.Collections;
using System;
using BicDB.Container;
using BicDB.Variable;
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
		static public string SEND_RECORDS_URL = "sendRecordsURL";
		static public string DELETE_RECORDS_URL = "deleteRecordsURL";
		static public string ENCRYPT = "encrypt";
		#endregion

		public enum ResultCode
		{
			Success = 0,
			FailedConvertJson = 1,
			ErrorNetwork = 2,
			ServerRequestError = 3,
			Crypto = 4
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
			_table.Clear();
			return this.PullAsync(_table, _parameter);
		}

		public void Pull<T>(ITableContainer<T> _targetTable, Action<Result> _callback, object _parameter) where T : IRecordContainer, new ()
        {
            var _loadCallback = _callback;
            var _table = _targetTable;
			var _webParam = _parameter as WebStorageParameter;
			
			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}

			if(string.IsNullOrEmpty(_webParam.Url) == true){
				assertByTableHeader(_table, new string[]{LOAD_URL_KEY, ENCRYPT});
				_webParam.Url = _table.Header[LOAD_URL_KEY].AsVariable.AsString;
				_webParam.ShouldEncrypt = _table.Header[ENCRYPT].AsVariable.AsBool;
			}
			
            
            var _formData = new RecordContainer();
			_formData.AddManagedColumn("primaryKey", new StringVariable(_table.PrimaryKey));
			WWWForm _form = createForm(_webParam, _formData);
            var _request = UnityWebRequest.Post(_webParam.Url, _form);

            this.SendWebRequest(_request, _result =>
            {
                var _storageResult = buildTable(_result, _table, _webParam);

                if (_loadCallback != null)
                {
                    _loadCallback(_storageResult);
                }
            });
        }

        private static void assertByTableHeader<T>(ITableContainer<T> _table, string[] _checkList) where T : IRecordContainer, new()
        {
			foreach(var _key in _checkList){
				if (!_table.Header.ContainsKey(_key))
				{
					throw new SystemException("not found Header " + _key);
				}
			}
        }

		private static (Result, string) buildJson(UnityWebRequest _result, WebStorageParameter _webParam)
		{
			var _storageResult = new Result((int)ResultCode.Success);
			var _json = _result.downloadHandler.text; 

            do
            {
                if (_result.result != UnityWebRequest.Result.Success)
                {
                    _storageResult.Code = (int)ResultCode.ErrorNetwork;
                    _storageResult.Message = _result.error;
                    break;
                }

                try
                {
                    if (_webParam.ShouldEncrypt == true)
                    {
                        _json = BicUtil.Crypto.AES256.Decrypt(_json);
                    }
                }
                catch
                {
                    _storageResult.Code = (int)ResultCode.Crypto;
                    _storageResult.Message = "crypto error";
                    break;
                }

                if (_webParam != null && _webParam.RequestConvertor != null)
                {
                    _json = _webParam.RequestConvertor(_json);
                }

            } while (false);

            return (_storageResult, _json);
		}

        private static Result buildTable<T>(UnityWebRequest _result, ITableContainer<T> _table, WebStorageParameter _webParam) where T : IRecordContainer, new()
        {
			(var _storageResult, var _json) = buildJson(_result, _webParam);
			if(_storageResult.IsSuccess == true){
				try
				{
					int _counter = 0;
					JsonConvertor.GetInstance().BuildTableContainer(_table, ref _json, ref _counter);
					_storageResult.Code = (int)ResultCode.Success;
				}
				catch (Exception)
				{
					_storageResult.Code = (int)ResultCode.FailedConvertJson;
					_storageResult.Message = ResultCode.FailedConvertJson.ToString();
				}
			}

            return _storageResult;
        }

        private static WWWForm createForm(WebStorageParameter _webParam, RecordContainer _formData = null)
        {
			if(_formData == null){
            	_formData = new RecordContainer();
			}

            if (_webParam != null && _webParam.Param != null)
            {
                foreach (var _value in _webParam.Param)
                {
                    _formData.AddManagedColumn(_value.Key, new StringVariable(_value.Value));
                }
            }

            var _form = new WWWForm();
            var _formDataString = _formData.ToString();
            if (_webParam.ShouldEncrypt == true)
            {
                _formDataString = BicUtil.Crypto.AES256.Encrypt(_formDataString);
            }
            _form.AddField("data", _formDataString);
            return _form;
        }

        public async Task<Result> PullAsync<T>(ITableContainer<T> _targetTable, object _parameter) where T : IRecordContainer, new (){
			var _table = _targetTable;

			var _webParam = _parameter as WebStorageParameter;
			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}

			if(string.IsNullOrEmpty(_webParam.Url) == true){
				assertByTableHeader(_table, new string[]{LOAD_URL_KEY, ENCRYPT});
				_webParam.Url = _table.Header[LOAD_URL_KEY].AsVariable.AsString;
				_webParam.ShouldEncrypt = _table.Header[ENCRYPT].AsVariable.AsBool;
			}

			var _formData = new RecordContainer();
			_formData.AddManagedColumn("primaryKey", new StringVariable(_table.PrimaryKey));
			WWWForm _form = createForm(_webParam, _formData);
			var _request = UnityWebRequest.Post(_webParam.Url, _form);
			
			await _request.SendWebRequest();
			
			var _storageResult = buildTable(_request, _table, _webParam);
			return _storageResult;
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

		public void SendRecord<T>(ITableContainer<T> _targetTable, T _targetRecord, WebStorageParameter _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _table = _targetTable;
			var _record = _targetRecord;
			var _webParam = _param;

			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}

			if(string.IsNullOrEmpty(_webParam.Url) == true){
				assertByTableHeader(_table, new string[]{SEND_RECORD_URL, ENCRYPT, HeaderKey.PrimaryKey});
				_webParam.Url = _table.Header[SEND_RECORD_URL].AsVariable.AsString;
				_webParam.ShouldEncrypt = _table.Header[ENCRYPT].AsVariable.AsBool;
				_webParam.TargetKey = _table.PrimaryKey;
			}

			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _record);
			_formData.AddManagedColumn(_webParam.TargetKey, _record[_webParam.TargetKey].AsVariable);
			_formData.AddManagedColumn("primaryKey", new StringVariable(_webParam.TargetKey));
			var _form = createForm(_webParam, _formData);
			var _request = UnityWebRequest.Post(_webParam.Url, _form);

			WebStorage.Instance.SendWebRequest(_request, _result=>{
				
				(var _storageResult, var _json) = buildJson(_result, _webParam);

				if(_storageResult.IsSuccess == false){
					_resultCallback(_storageResult);
					return;
				}

				var _resultRecord = new RecordContainer();
				_resultRecord.AddManagedColumn("result", new IntVariable());
				_resultRecord.AddManagedColumn(_webParam.TargetKey, new StringVariable());

				if(_resultRecord.ParseJson(_json) == false){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.FailedConvertJson));
					}
					return;
				}

				if(_resultRecord["result"].AsVariable.AsInt != 0){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.ServerRequestError, "", 0, _resultRecord.ToString()));
					}
					return;
				}

				if(string.IsNullOrEmpty(_record[_webParam.TargetKey].AsVariable.AsString) == true){
					_record[_webParam.TargetKey].AsVariable.AsString = _resultRecord[_webParam.TargetKey].AsVariable.AsString;
				}
				
				_table.AddWithoutDuplication(_record);

				if(_table.OnSave != null){
					_table.OnSave(new Result((int)ResultCode.Success));
				}

				if(_resultCallback != null){
					_resultCallback(new Result((int)ResultCode.Success));
				}
			});
		}

		public static async Task<T> GetRecordAsync<T>(WebStorageParameter _param) where T : class, IRecordContainer, new (){
			// bool _isEncrypt = true;
			var _webParam = _param;
			var _request = UnityWebRequest.Get(_webParam.Url);
			await _request.SendWebRequest();

			if(_request.result == UnityWebRequest.Result.Success){
				string _json = _request.downloadHandler.text;

				try{
					if(_webParam.ShouldEncrypt == true){
						_json = BicUtil.Crypto.AES256.Decrypt(_json);
					}
				}catch{
					return null;
				}

				var _resultRecord = new T();
				_resultRecord.AddManagedColumn("result", new IntVariable());

				if(_resultRecord.ParseJson(_json) == true){
					_resultRecord.Remove("result");
					return _resultRecord;
				}else{
					return null;
				}
			}else{
				return null;
			}
		}

		public static async Task<Result> SendRecordAsync<T>(T _record, WebStorageParameter _param) where T : IRecordContainer, new (){
			var _webParam = _param;
			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _record);

			var _form = createForm(_webParam, _formData);
			var _request = UnityWebRequest.Post(_webParam.Url, _form);

			await _request.SendWebRequest();

			(var _storageResult, var _json) = buildJson(_request, _webParam);

			if(_storageResult.IsSuccess == false){
				return _storageResult;
			}

			Debug.Log("_json : " + _json);

			var _resultRecord = new RecordContainer();
			_resultRecord.AddManagedColumn("result", new IntVariable());

			if(_resultRecord.ParseJson(_json) == false){
				return new Result((int)ResultCode.FailedConvertJson, "", 0, "FailedConvertJson error");
			}

			if(_resultRecord["result"].AsVariable.AsInt == 0){
				return new Result((int)ResultCode.Success);
			}else{
				return new Result((int)ResultCode.ServerRequestError, "", 0, _resultRecord.ToString());
			}
		}

		public void SendRecords<T>(ITableContainer<T> _targetTable, ListContainer<T> _targetRecords, WebStorageParameter _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _table = _targetTable;
			var _records = _targetRecords;
			var _webParam = _param;

			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}

			if(string.IsNullOrEmpty(_webParam.Url) == true){
				assertByTableHeader(_table, new string[]{SEND_RECORDS_URL, ENCRYPT, HeaderKey.PrimaryKey});
				_webParam.Url = _table.Header[SEND_RECORDS_URL].AsVariable.AsString;
				_webParam.ShouldEncrypt = _table.Header[ENCRYPT].AsVariable.AsBool;
				_webParam.TargetKey = _table.PrimaryKey;
			}

			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _records);
			_formData.AddManagedColumn("primaryKey", new StringVariable(_webParam.TargetKey));
			var _form = createForm(_webParam, _formData);
			var _request = UnityWebRequest.Post(_webParam.Url, _form);

			this.SendWebRequest(_request, _result=>{
				
				(var _storageResult, var _json) = buildJson(_result, _webParam);

				if(_storageResult.IsSuccess == false){
					_resultCallback(_storageResult);
					return;
				}
				
				var _resultRecord = new RecordContainer();
				_resultRecord.AddManagedColumn("result", new IntVariable());
				var _primaryKeys = new ListContainer<StringVariable>();
				_resultRecord.AddManagedColumn(_webParam.TargetKey, _primaryKeys);


				if(_resultRecord.ParseJson(_json) == false){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.FailedConvertJson));
					}
					return;
				}

				if(_resultRecord["result"].AsVariable.AsInt != 0){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.ServerRequestError, "", 0, _resultRecord.ToString()));
					}
					return;
				}
				
				for(int i = 0; i < _records.Count; i++){
					var _record = _records[i];
					if(string.IsNullOrEmpty(_record[_webParam.TargetKey].AsVariable.AsString) == true){
						_record[_webParam.TargetKey].AsVariable.AsString = _primaryKeys[i].AsString;
					}

					_table.AddWithoutDuplication(_record);
				}

				if(_table.OnSave != null){
					_table.OnSave(new Result((int)ResultCode.Success));
				}

				if(_resultCallback != null){
					_resultCallback(new Result((int)ResultCode.Success));
				}
			});
		}

		public void DeleteRecords<T>(ITableContainer<T> _targetTable, ListContainer<T> _targetRecords, Dictionary<string, string> _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _table = _targetTable;
			var _records = _targetRecords;
			if(string.IsNullOrEmpty(_table.PrimaryKey) == true){
				throw new SystemException("Need to set PrimaryKey");
			}

			if (!_table.Header.ContainsKey (DELETE_RECORDS_URL)) {
				throw new SystemException ("not found Header " + DELETE_RECORDS_URL);
			}

			if (!_table.Header.ContainsKey (ENCRYPT)) {
				throw new SystemException ("not found Header " + ENCRYPT);
			}

			var _formData = new RecordContainer();
			var _ids = new ListContainer<StringVariable>();
			for(int i = 0; i < _records.Count; i++){
				_ids.Add(new StringVariable(_records[i][_table.PrimaryKey].AsVariable.AsString));
			}

			_formData.AddManagedColumn("data", _ids);
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


			var _request = UnityWebRequest.Post(_table.Header[DELETE_RECORDS_URL].AsVariable.AsString, _form);

			WebStorage.Instance.SendWebRequest(_request, _result=>{

				if(_result.result != UnityWebRequest.Result.Success){
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
					return;
				}

				var _resultRecord = new RecordContainer();
				_resultRecord.AddManagedColumn("result", new IntVariable());
				var _primaryKeys = new ListContainer<StringVariable>();
				_resultRecord.AddManagedColumn(_table.PrimaryKey, _primaryKeys);

				if(_resultRecord.ParseJson(_json) == true){
					if(_resultRecord["result"].AsVariable.AsInt == 0){

						for(int i = 0; i < _primaryKeys.Count; i++){
							var _target = _table.FirstOrDefault(_row=>_row[_table.PrimaryKey].AsVariable.AsString == _primaryKeys[i].AsString);
							if(_target != null){
								_table.Remove(_target);
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
        #endregion

    }

	public class WebStorageParameter{
		public string Url = string.Empty;
		public bool ShouldEncrypt = false;
		public string TargetKey = string.Empty;
		public Func<string, string> RequestConvertor = null;
		public Dictionary<string, string> Param = null;

		public WebStorageParameter(){
		}
	}
}
#endif