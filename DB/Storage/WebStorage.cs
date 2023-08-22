#if BICUTIL_WWW
using UnityEngine;
using System.Collections;
using System;
using BicDB.Container;
using BicDB.Variable;
using BicUtil.Json;
using UnityEngine.Networking;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.IO;

namespace BicDB.Storage
{
	public class WebStorage : MonoBehaviour, ITableStorage {
		#region Static
		// static public string LOAD_URL_KEY = "webstorageLoadURL";
		// static public string SEND_RECORD_URL = "sendRecordURL";
		// static public string SEND_RECORDS_URL = "sendRecordsURL";
		// static public string DELETE_RECORDS_URL = "deleteRecordsURL";
		// static public string ENCRYPT = "encrypt";
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

		public UniTask<Result> LoadAsync<T>(ITableContainer<T> _table, object _parameter) where T : IRecordContainer, new()
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
			
            
            var _formData = new RecordContainer();
			_formData.AddManagedColumn("primaryKey", new StringVariable(_table.PrimaryKey));
			
			this.sendWebRequestWithCache(_webParam, _formData, _downloadText=>{
				var _storageResult = buildTable(_downloadText, _table, _webParam);

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

		private static (Result, string) buildJson(string _json, WebStorageParameter _webParam)
		{
			var _storageResult = new Result((int)ResultCode.Success);

            do
            {
                // if (_result.result != UnityWebRequest.Result.Success)
                // {
                //     _storageResult.Code = (int)ResultCode.ErrorNetwork;
                //     _storageResult.Message = _result.error;
                //     break;
                // }

                try
                {
                    if (_webParam.ShouldEncrypt == true)
                    {
                        _json = BicUtil.Crypto.AES256.Decrypt(_json, _webParam.EncryptKey);
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

        private static Result buildTable<T>(string _text, ITableContainer<T> _table, WebStorageParameter _webParam) where T : IRecordContainer, new()
        {
			(var _storageResult, var _json) = buildJson(_text, _webParam);
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
                _formDataString = BicUtil.Crypto.AES256.Encrypt(_formDataString, _webParam.EncryptKey);
            }
            _form.AddField("data", _formDataString);
            return _form;
        }

        public async UniTask<Result> PullAsync<T>(ITableContainer<T> _targetTable, object _parameter) where T : IRecordContainer, new (){
			var _table = _targetTable;
			var _webParam = _parameter as WebStorageParameter;
			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}


			#if UNITY_EDITOR
			var _pullId = UnityEngine.Random.Range(0, 100000); 
			Debug.Log("[WebStorage] PullAsync("+_pullId+") " + _targetTable.Name + "\n" + _webParam.ToString());
			#endif

			(var _type, var _downloadText) = await getWebRequestWithCache(_webParam); 
			var _storageResult = buildTable(_downloadText, _table, _webParam);

			#if UNITY_EDITOR
			Debug.Log("[WebStorage] PullAsync("+_pullId+") Result Cached by " + _type.ToString() + " & " + _targetTable.Count.ToString());
			#endif

			return _storageResult;
		}

		private IEnumerator sendWebRequestCoroutine(UnityWebRequest _request, Action<UnityWebRequest> _callback){
			yield return _request.SendWebRequest();	
			_callback(_request);
		}

		public void SendWebRequest(UnityWebRequest _request, Action<UnityWebRequest> _callback){
			if(_request == null){
				_callback(null);
			}else{
				StartCoroutine(this.sendWebRequestCoroutine(_request, _callback));
			}
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

			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _record);
			_formData.AddManagedColumn(_webParam.TargetKey, _record[_webParam.TargetKey].AsVariable);
			_formData.AddManagedColumn("primaryKey", new StringVariable(_webParam.TargetKey));
			
			this.sendWebRequestWithCache(_webParam, _formData, _downloadText=>{
				(var _storageResult, var _json) = buildJson(_downloadText, _webParam);

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

		public async UniTask<T> GetRecordAsync<T>(WebStorageParameter _param) where T : class, IRecordContainer, new (){
			var _result = await GetRecordWithCachingTypeAsync<T>(_param);
			return _result.Result;
		}

		public T GetRecordOnMemoryCache<T>(WebStorageParameter _param) where T : class, IRecordContainer, new (){
			var _webParam = _param as WebStorageParameter;
			
			if(memoryCache.ContainsKey(_webParam.CacheId) == true){

				var _resultRecord = new T();
				_resultRecord.AddManagedColumn("result", new IntVariable());

				var _json = memoryCache[_webParam.CacheId].text;
				if(_webParam.ShouldEncrypt == true){
					_json = BicUtil.Crypto.AES256.Decrypt(_json, _param.EncryptKey);
				}

				
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

		public (CachingLevel CachingType, T Result) GetRecordOnCache<T>(WebStorageParameter _param) where T : class, IRecordContainer, new (){
			var _webParam = _param as WebStorageParameter;
			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}
			
			var _result = getCache(_param);

			return buildRecord<T>(_webParam, _result);

		}

		public async UniTask<(CachingLevel CachingType, T Result)> GetRecordWithCachingTypeAsync<T>(WebStorageParameter _param) where T : class, IRecordContainer, new ()
        {
            var _webParam = _param as WebStorageParameter;
            if (_webParam == null)
            {
                _webParam = new WebStorageParameter();
            }

            var _result = await getWebRequestWithCache(_webParam);

            return buildRecord<T>(_webParam, _result);
        }

        private (CachingLevel CachingType, T Result) buildRecord<T>(WebStorageParameter _webParam, (CachingLevel CachingType, string Text) _result) where T : class, IRecordContainer, new()
        {
            if (string.IsNullOrEmpty(_result.Text) == true)
            {
                return (_result.CachingType, null);
            }

            try
            {
                if (_webParam.ShouldEncrypt == true)
                {
                    _result.Text = BicUtil.Crypto.AES256.Decrypt(_result.Text, _webParam.EncryptKey);
                }
            }
            catch
            {
                return (_result.CachingType, null);
            }

            var _resultRecord = new T();
            _resultRecord.AddManagedColumn("result", new IntVariable());

            if (_resultRecord.ParseJson(_result.Text) == true)
            {
                if (_resultRecord["result"].AsVariable.AsInt != 0)
                {
                    lock (memoryCache)
                    {
                        if (memoryCache.ContainsKey(_webParam.CacheId) == true)
                        {
                            memoryCache.Remove(_webParam.CacheId);
                        }
                    }

                    FileStorageUtil.RemoveFile(Path.Combine(CACHE_DIRECTORY, _webParam.CacheId + ".txt"));

                    return (_result.CachingType, null);
                }

                _resultRecord.Remove("result");
                return (_result.CachingType, _resultRecord);
            }
            else
            {
                Debug.Log("parse error");
                return (_result.CachingType, null);
            }
        }

        public async UniTask<(Result Result, RecordContainer Data)> SendRecordAsync<T>(T _record, WebStorageParameter _param) where T : IRecordContainer, new (){
			var _webParam = _param;
			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _record);

			var _downloadText = await postWebRequestWithCache(_webParam, _formData);

			(var _storageResult, var _json) = buildJson(_downloadText, _webParam);

			if(_storageResult.IsSuccess == false){
				return (_storageResult, null);
			}

			var _resultRecord = new RecordContainer();
			_resultRecord.AddManagedColumn("result", new IntVariable());

			if(_resultRecord.ParseJson(_json) == false){
				return (new Result((int)ResultCode.FailedConvertJson, "", 0, "FailedConvertJson error"), null);
			}

			if(_resultRecord["result"].AsVariable.AsInt == 0){
				return (new Result((int)ResultCode.Success), _resultRecord);
			}else{
				return (new Result(_resultRecord["result"].AsVariable.AsInt, "", 0, _resultRecord.ToString()), _resultRecord);
			}
		}

		public void SendRecord<T>(T _targetRecord, WebStorageParameter _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _record = _targetRecord;
			var _webParam = _param;

			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}

			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _record);

			this.sendWebRequestWithCache(_webParam, _formData, _downloadText=>{
				(var _storageResult, var _json) = buildJson(_downloadText, _webParam);

				if(_storageResult.IsSuccess == false){
					_resultCallback(_storageResult);
					return;
				}

				var _resultRecord = new RecordContainer();
				_resultRecord.AddManagedColumn("result", new IntVariable());

				if(_resultRecord.ParseJson(_json) == false){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.FailedConvertJson));
					}
					return;
				}

				if(_resultRecord["result"].AsVariable.AsInt != 0){
					if(_resultCallback != null){
						_resultCallback(new Result(_resultRecord["result"].AsVariable.AsInt, "", 0, _resultRecord.ToString()));
					}
					return;
				}

				if(_resultCallback != null){
					_resultCallback(new Result((int)ResultCode.Success));
				}
			});
		}

		public void SendRecords<T>(ITableContainer<T> _targetTable, ListContainer<T> _targetRecords, WebStorageParameter _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _table = _targetTable;
			var _records = _targetRecords;
			var _webParam = _param;

			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}

			var _formData = new RecordContainer();
			_formData.AddManagedColumn("data", _records);
			_formData.AddManagedColumn("primaryKey", new StringVariable(_webParam.TargetKey));
			
			this.sendWebRequestWithCache(_webParam, _formData, _downloadText=>{
				(var _storageResult, var _json) = buildJson(_downloadText, _webParam);

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
						_resultCallback(new Result((int)ResultCode.FailedConvertJson, _message:_json));
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

					if(_table != null){
						_table.AddWithoutDuplication(_record);
					}
				}

				if(_table != null && _table.OnSave != null){
					_table.OnSave(new Result((int)ResultCode.Success));
				}

				if(_resultCallback != null){
					_resultCallback(new Result((int)ResultCode.Success));
				}
			});
		}

		public void DeleteRecords<T>(ITableContainer<T> _targetTable, ListContainer<T> _targetRecords, WebStorageParameter _param, Action<Result> _callback = null) where T : IRecordContainer, new (){
			var _resultCallback = _callback;
			var _table = _targetTable;
			var _records = _targetRecords;
			var _webParam = _param;
			
			if(_webParam == null){
				_webParam = new WebStorageParameter();
			}

			var _formData = new RecordContainer();
			var _ids = new ListContainer<StringVariable>();
			for(int i = 0; i < _records.Count; i++){
				_ids.Add(new StringVariable(_records[i][_table.PrimaryKey].AsVariable.AsString));
			}

			_formData.AddManagedColumn("data", _ids);
			_formData.AddManagedColumn("primaryKey", new StringVariable(_table.PrimaryKey));

			if(_param != null){
				foreach(var _value in _webParam.Param){
					_formData.AddManagedColumn(_value.Key, new StringVariable(_value.Value));
				}
			}


			this.sendWebRequestWithCache(_webParam, _formData, _downloadText=>{
				// if(_result.result != UnityWebRequest.Result.Success){
				// 	if(_resultCallback != null){
				// 		_resultCallback(new Result((int)ResultCode.ErrorNetwork, "", 0, _result.error));
				// 	}
				// 	return;
				// }

				string _json = _downloadText;
				try{
					if(_webParam.ShouldEncrypt == true){
						_json = BicUtil.Crypto.AES256.Decrypt(_json, _param.EncryptKey);
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

				if(_resultRecord.ParseJson(_json) == false){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.FailedConvertJson));
					}
					return;
				}

				if(_resultRecord["result"].AsVariable.AsInt != 0){
					if(_resultCallback != null){
						_resultCallback(new Result((int)ResultCode.ServerRequestError));
					}
					return;
				}

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
			});
		}
        #endregion

		#region Cache
		static private string CACHE_DIRECTORY{ get=>Path.Combine(Application.persistentDataPath, "webcache");}
	
		private Dictionary<string, (long timestamp, string text)> memoryCache = new Dictionary<string, (long, string)>();
		private bool isExistsDirectory = false;
		
		private bool hasCacheFoced(WebStorageParameter _param){
			if((_param.CachingLevel & CachingLevel.File) != 0 && File.Exists(Path.Combine(CACHE_DIRECTORY, _param.CacheId + ".txt")) == true){
				return true;
			}

			if((_param.CachingLevel & CachingLevel.Resource) != 0){
				var _result = ResourceStorage.ReadAsset(Path.Combine(_param.ResourceCacheDirectoryPath,_param.CacheId));
				if(string.IsNullOrEmpty(_result) == false){
					return true;
				}
			}

			return false;
		}
		public void RemoveCacheAll(){
			System.IO.Directory.Delete(CACHE_DIRECTORY, true);
			isExistsDirectory = false;
		}

		private (CachingLevel CachingType, string Result) getCache(WebStorageParameter _param){
			if(_param.IsEnabledCache == false){
				// Debug.Log("cache is disable");
				return (CachingLevel.None, string.Empty);
			}

			//캐시타임 체크해서 지났으면 캐시로드하지 말고 그냥로드 캐시타임은 메모리, 파일 따로 조사해야할듯? 그럼 파일 json파싱을 해야하는데? -0-
			if((_param.CachingLevel & CachingLevel.Memory) != 0){
				lock(memoryCache){
					if(memoryCache.ContainsKey(_param.CacheId) == true){
						// Debug.Log("memory caching");
						//캐시타임체크
						if(getTimestamp() - memoryCache[_param.CacheId].timestamp < _param.CacheTime){
							return (CachingLevel.Memory, memoryCache[_param.CacheId].text);
						}else{
							//Debug.Log("cache timeout");
						}
					}
				}
			}

			if(_param.ResourceCacheEnableBeforeWeb == true){
				string _result = loadCacheFromResrouceAndCaching(_param);

				if(string.IsNullOrEmpty(_result) == false){
					if((_param.CachingLevel & CachingLevel.Memory) != 0){
						lock(memoryCache){
							memoryCache[_param.CacheId] = (getTimestamp(), _result);
						}
					}

					return (CachingLevel.Resource, _result);
				}
			}
			
			if((_param.CachingLevel & CachingLevel.File) != 0){
				// Debug.Log("file caching");
				//파일에서 읽어오고
				
				(var _head, var _data) = FileStorageUtil.ReadFileHeadLineAndData(Path.Combine(CACHE_DIRECTORY, _param.CacheId + ".txt"));
				if(_data != null){

					// Debug.Log("file founded");
					//파싱, 캐시타임 체크
					var _timestamp = long.Parse(_head);
					//문제없으면 메모리캐시에 등록 후 리턴
					if(getTimestamp() - _timestamp < _param.CacheTime){
						// Debug.Log("file cace done");
						if((_param.CachingLevel & CachingLevel.Memory) != 0){
							lock(memoryCache){
								memoryCache[_param.CacheId] = (_timestamp, _data);
							}
						}
						
						return (CachingLevel.File, _data);
					}else{
						// Debug.Log("file cache timeout");
					}
				}else{
					// Debug.Log("file cache not found");
				}
			}
			
			// Debug.Log("cache not work");
			return (CachingLevel.None, string.Empty);
		}

		private void setCache(WebStorageParameter _param, string _text){
			if((_param.CachingLevel & CachingLevel.Memory) != 0){
				// Debug.Log("meorycache enable set-");
				lock(memoryCache){
					this.memoryCache[_param.CacheId] = (getTimestamp(), _text);
				}
			}

			if((_param.CachingLevel & CachingLevel.File) != 0){
				//파일에도 저장, 저장시 캐싱타임도 저장해야할듯 {timestamp}\n{_text} 으로?
				AddCacheToFile(_param.CacheId, _text);
			}
		}

		public void AddCacheToFile(string _id, string _data){
			// Debug.Log("write file cache");

			if(_data == null || _data.Length <= 100){
				return;
			}

			if(isExistsDirectory == false && Directory.Exists(CACHE_DIRECTORY) == false){
				System.IO.Directory.CreateDirectory(CACHE_DIRECTORY);
				isExistsDirectory = true;
			}

			FileStorageUtil.WriteFile(getTimestamp().ToString()+'\n'+_data, Path.Combine(CACHE_DIRECTORY,_id + ".txt"));
		}
		

		private long getTimestamp(){
			return System.DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
		}


		private async UniTask<(CachingLevel CachingType, string Text)> getWebRequestWithCache(WebStorageParameter _param){
			var _cached = getCache(_param);
			if(_cached.CachingType != CachingLevel.None){
				return _cached;
			}

			
			UnityWebRequest _request = null;
			if(string.IsNullOrEmpty(_param.Url) == false){
				_request = UnityWebRequest.Get(_param.UrlWithParam);
				setTimeout(_param, _request);
				await _request.SendWebRequest().ToUniTask();
			}

			if(_request != null && _request.result == UnityWebRequest.Result.Success){
				setCache(_param, _request.downloadHandler.text);
				#if UNITY_EDITOR
				var _msg = _request.downloadHandler.text;
				if(_param.ShouldEncrypt == true){
					 _msg = BicUtil.Crypto.AES256.Decrypt(_msg, _param.EncryptKey);
				}
				DebugForEditor.Log("[WebStorage] Success Download by web " + _param.ToString() + "/" + _msg);
				#endif
				return (CachingLevel.None, _request.downloadHandler.text);
			}else{
				

				if((_param.CachingLevel & CachingLevel.File) != 0){
					(var _head, var _data) = FileStorageUtil.ReadFileHeadLineAndData(Path.Combine(CACHE_DIRECTORY,_param.CacheId + ".txt"));
					if(_data != null){
						#if UNITY_EDITOR
						DebugForEditor.Log("[WebStorage] Failed Download by web, Use File " + _param.ToString());
						#endif
						return (CachingLevel.File, _data);
					}
				}
				
				if(string.IsNullOrEmpty(_param.ResourceCacheDirectoryPath) == false){
					try
					{

						string _result = loadCacheFromResrouceAndCaching(_param);
						if(string.IsNullOrEmpty(_result) == false){
							#if UNITY_EDITOR
							DebugForEditor.Log("[WebStorage] Failed Download by web, Use Resource " + _param.ToString());
							#endif
							return (CachingLevel.Resource, _result);
						}
					}
					catch
					{
						DebugForEditor.Log("[WebStorage] Error loadCacheFromResrouceAndCachingAsync");
						return (CachingLevel.None, string.Empty);
					}
				}

				#if UNITY_EDITOR
				DebugForEditor.Log("[WebStorage] Failed Download by web " + _param.ToString() + "/ request : " + (_request == null?"null":_request.result.ToString()));
				#endif
				return (CachingLevel.None, string.Empty);
			}

		}

		private string loadCacheFromResrouceAndCaching(WebStorageParameter _param)
        {
            var _result = ResourceStorage.ReadAsset(Path.Combine(_param.ResourceCacheDirectoryPath, _param.CacheId));
            _result = removeFirstLineAndCaching(_param, _result);
            return _result;
        }

		private string removeFirstLineAndCaching(WebStorageParameter _param, string _string){
			if (string.IsNullOrEmpty(_string) == false)
            {
				var _startIndex = _string.IndexOf('\n', 0) + 1;
				if(_startIndex > 0 && _startIndex < 100){
					_string = _string.Substring(_startIndex);
				}

                _param.CachingLevel = (_param.CachingLevel & ~CachingLevel.File);

                setCache(_param, _string);
            }

			return _string;
		}

        private async UniTask<string> postWebRequestWithCache(WebStorageParameter _param, RecordContainer _formData){
			var _cached = getCache(_param);
			if(_cached.CachingType != CachingLevel.None){
				return _cached.Result;
			}

			UnityWebRequest _request = null;
			if(string.IsNullOrEmpty(_param.Url) == false)
            {
                WWWForm _form = createForm(_param, _formData);
                _request = UnityWebRequest.Post(_param.Url, _form);

                setTimeout(_param, _request);

                await _request.SendWebRequest().ToUniTask();
            }

            if (_request != null && _request.result == UnityWebRequest.Result.Success){
				setCache(_param, _request.downloadHandler.text);
				return _request.downloadHandler.text;
			}else if(string.IsNullOrEmpty(_param.ResourceCacheDirectoryPath) == false){
				try{
					string _result = loadCacheFromResrouceAndCaching(_param);
					return _result;
				}catch{
					return string.Empty;
				}
			}else{
				return string.Empty;
			}
		}

        private void setTimeout(WebStorageParameter _param, UnityWebRequest _request)
        {
            if (_param.ForcedCacheWebTimeout > 0)
            {
                var _hasCache = hasCacheFoced(_param);
                if (_hasCache == true)
                {
                    _request.timeout = _param.ForcedCacheWebTimeout;
                }
                else
                {
					_request.timeout = _param.DefaultWebTimeout;
                }
            }
            else
            {
				_request.timeout = _param.DefaultWebTimeout;
            }
        }

        private void sendWebRequestWithCache(WebStorageParameter _param, RecordContainer _formData, Action<string> _callback){
			
			var _cached = getCache(_param);
			if(_cached.CachingType != CachingLevel.None){
				_callback(_cached.Result);
				return;
			}
			
			
			UnityWebRequest _request = null;
			if(string.IsNullOrEmpty(_param.Url) == false){
				WWWForm _form = createForm(_param, _formData);
				_request = UnityWebRequest.Post(_param.Url, _form);
			}


			this.SendWebRequest(_request, _result=>{
				if(_request != null && _request.result == UnityWebRequest.Result.Success){
					setCache(_param, _request.downloadHandler.text);
					_callback(_request.downloadHandler.text);
				}else if(string.IsNullOrEmpty(_param.ResourceCacheDirectoryPath) == false){
					try{
						string _cached = loadCacheFromResrouceAndCaching(_param);
						_callback(_cached);
					}catch{
						_callback(string.Empty);
					}
				}else{
					_callback(string.Empty);
				}

				_request.Dispose();
			});
		}

		#endregion

    }

	[Flags]
	public enum CachingLevel{
		None = 0,
		Resource = 1 << 0,
		File = 1 << 1,
		Memory = 1 << 2,

		FileAndMemory = File | Memory,
		FileAndResource = File | Resource,
		All = Resource | File | Memory,
	}

	public class WebStorageParameter{
		public string Url = string.Empty;
		public bool ShouldEncrypt = false;
		public string EncryptKey = string.Empty;
		public string TargetKey = string.Empty;
		public Func<string, string> RequestConvertor = null;
		public Dictionary<string, string> Param = null;
		public string ResourceCacheDirectoryPath = string.Empty;
		public bool ResourceCacheEnableBeforeWeb = false; 

		public CachingLevel CachingLevel = CachingLevel.None;
		public string CacheId = string.Empty;
		public long CacheTime = 0;
		public int ForcedCacheWebTimeout = 0;
		public int DefaultWebTimeout = 0;
		
		
		public bool IsEnabledCache{
			get{
				return CachingLevel != CachingLevel.None;
			}
		}

		public string UrlWithParam{
			get{
				var _result = Url;
				if(Param != null){
					bool _isFirst = true;
					foreach(var _param in this.Param){
						if(_isFirst == true){
							_result += "?";
							_isFirst = false; 
						}else{
							_result += "&";
						}

						_result += _param.Key + "=" + UnityWebRequest.EscapeURL(_param.Value);
					}
				}

				return _result;
			}
		}


		public override string ToString(){
			return "Webparam : " + Url + " & cache : " + CacheId + " & " + CachingLevel.ToString(); 
		}
	}
}
#endif