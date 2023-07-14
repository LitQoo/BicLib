using System;
using BicDB;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using BicDB.Variable;
using BicDB.Storage;
using BicUtil.Json;

namespace BicDB.Container
{
	public class MutableDictionaryContainer : IMutableDictionaryContainer
	{

		private IDictionary<string, IDataBase> data = new Dictionary<string, IDataBase>();

		#region IDictionaryContainer
		public Action<string, IDataBase> OnAddedRowActions { get; set; }
		public Action<string, IDataBase> OnRemovedRowActions { get; set; }
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.Dictionary; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildMutableDictionaryContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, _stringBuilder);
		}

		public IVariable AsVariable{ 
			get{ 
				return null;	
			} 
		}

		public D As<D>() where D : class, IDataBase{
			return this as D;
		}
		#endregion

		#region IDictionary
		public void Add(string _key, IDataBase _value)
		{
			data.Add(_key, _value);

			if (OnAddedRowActions != null) {
				OnAddedRowActions(_key, _value);
			}
		}

		public void Add(KeyValuePair<string, IDataBase> _item)
		{
			Add(_item.Key, _item.Value);
		}
			
		public bool ContainsKey(string _key)
		{
			return data.ContainsKey(_key);
		}

		public bool Remove(string _key)
		{

			if (!ContainsKey(_key)) {
				return false;
			}

			if (OnRemovedRowActions != null) {
				OnRemovedRowActions(_key, data[_key]);
			}

			return data.Remove(_key);
		}

		public bool Remove(KeyValuePair<string, IDataBase> _item)
		{
			return Remove(_item.Key);
		}

		public void Clear()
		{
			foreach (var _item in data) {
				if (OnRemovedRowActions != null) {
					OnRemovedRowActions(_item.Key, _item.Value);
				}
			}

			data.Clear();
		}

		public bool TryGetValue(string _key, out IDataBase _value)
		{
			return data.TryGetValue(_key, out _value);
		}

		public bool Contains(KeyValuePair<string, IDataBase> _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(KeyValuePair<string, IDataBase>[] _array, int _arrayIndex){
			data.CopyTo(_array, _arrayIndex);
		} 

		public IEnumerator<KeyValuePair<string, IDataBase>> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return data.GetEnumerator();
		}

		public IDataBase this[string _key] {
			get {
				return data[_key];
			}
			set {
				data[_key] = value;
			}
		}

		public ICollection<string> Keys {
			get {
				return data.Keys;
			}
		}

		public ICollection<IDataBase> Values {
			get {
				return data.Values;
			}
		}

		public int Count {
			get {
				return data.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return data.IsReadOnly;
			}
		}

		public void NotifyChangedWithChild(){
			foreach(var _info in this.data){
				switch(_info.Value){
					case INotifyWithChild _parent: _parent.NotifyChangedWithChild(); break;
					case IVariable _varaible:_varaible.NotifyChanged(); break;
					default:
						
					break;
				}
			}
		}
		
		public override string ToString(){
			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			BuildFormattedString (_stringBuilder, JsonConvertor.GetInstance ());
			return _stringBuilder.ToString();
		}

		public void MergeCopyBy(IMutableDictionaryContainer _model){
			string _json = _model.ToString();
			int _counter = 0;
			this.BuildVariable (ref _json, ref _counter, JsonConvertor.GetInstance ());
		}

		public bool ParseJson(string _json, bool _merge){
			if(_merge == false){
				this.data.Clear();
			}

			int _count = 0;
			try{
				BuildVariable(ref _json, ref _count, JsonConvertor.GetInstance());
				return true;
			}catch{
				return false;
			}
		}

		static public MutableDictionaryContainer CreateFromJson(string _jsonString){
			var _result = new MutableDictionaryContainer();
			_result.ParseJson(_jsonString, false);
			return _result;
		}

		static public MutableDictionaryContainer CreatePathAndValue(string[] _path, IDataBase _value){
			var _result = new MutableDictionaryContainer();
			_result.SetValueByKeyPath<MutableDictionaryContainer>(_value, _path);
			return _result;
		}
		#endregion
	}

	public static class DictionaryUtil{
		static public void updateExistingFields(IDictionary<string, IDataBase> _origin, IDictionary<string, IDataBase> _newValue){
			foreach(var _key in _origin.Keys.ToArray()){
				if(_newValue.ContainsKey(_key) == true){
					if(_origin[_key].Type == _newValue[_key].Type && _origin[_key] is IDictionary<string, IDataBase>){
						updateExistingFields(_origin[_key] as IDictionary<string, IDataBase>, _newValue[_key] as IDictionary<string, IDataBase>);
					}else{
						_origin[_key] = _newValue[_key];
					}
				}
			}
		}

		static public (T Diff, int DepthMax) getDiff<T>(IDictionary<string, IDataBase> _origin, IDictionary<string, IDataBase> _target, int _targetDepth = int.MaxValue, int _depth = 0) where T : IDictionary<string, IDataBase>, new(){
			var _diff = new T();
			var _childDepthMax = 0; 
			foreach(var _key in _target.Keys){
				if(_origin.ContainsKey(_key) == true){
					var _targetValue = _target[_key]; 
					if(_targetValue is IDictionary<string, IDataBase>){

						var _nested = getDiff<T>(_origin[_key] as IDictionary<string, IDataBase>, _targetValue as IDictionary<string, IDataBase>, _targetDepth, _depth + 1);
						_childDepthMax = UnityEngine.Mathf.Max(_childDepthMax, _nested.DepthMax + 1);

						if(_nested.Diff.Keys.Count <= 0){
						}else if(_targetDepth > 0){
							if(_depth < _targetDepth){
								_diff[_key] = _nested.Diff as IDataBase;
							}else{
								_diff[_key] = _targetValue;
							}
						}else if(_targetDepth <= 0){
							if(-_targetDepth >= _childDepthMax && _depth > 0){
								_diff[_key] = _targetValue;
							}else{
								_diff[_key] = _nested.Diff as IDataBase;;
							}
						}else{
							_diff[_key] = _nested.Diff as IDataBase;;
						}
					}else if(_targetValue is IListContainer<IDataBase>){
						var _targetList = _targetValue as IListContainer<IDataBase>;
						var _originList = _origin[_key] as IListContainer<IDataBase>;
						if(_targetList.ToString() != _originList.ToString()){
							_diff[_key] = _targetValue;
						}
					}else if(_targetValue.AsVariable.AsString != _origin[_key].AsVariable.AsString){
						_diff[_key] = _targetValue;
					}
				}else{
					_diff[_key] = _target[_key];
				}
			}

			return (_diff, _childDepthMax);
		}

		static public void removeAllExceptAt(IDictionary<string, IDataBase> _dict, string[] _paths){
            var _target = _dict;
            for(int i = 0; i < _paths.Length; i++){
                var _fieldName = _paths[i];
                foreach(var _key in _target.Keys.ToArray()){
                    if(_key != _fieldName){
                        _target.Remove(_key);
                    }
                }
                
                _target = _target[_fieldName] as MutableDictionaryContainer;
                if(_target == null){
                    break;
                }
            }
        }

		static public bool areKeysEqual(IDictionary<string, IDataBase> dict1, IDictionary<string, IDataBase> dict2)
        {
            if (dict1 == null || dict2 == null)
            {
                return false;
            }

            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            foreach (string key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                {
                    return false;
                }
            }

            return true;
        }

		static public void getKeyPathList(IDictionary<string, IDataBase> _target, List<string> _result, string _head, char _separator){
			foreach(var _key in _target.Keys){
				var _next = _target[_key] as IDictionary<string, IDataBase>;
				var _path = (string.IsNullOrEmpty(_head) ? ""  : _head + _separator) + _key;
				_result.Add(_path);
				if(_next != null){
					getKeyPathList(_next, _result, _path, _separator);
				}
			}
		}

		static public bool removeByKeyPath(IDictionary<string, IDataBase> _target, string[] _keyPath){
			for(int i = 0; i < _keyPath.Length; i++){
				var _key = _keyPath[i];
				if(_keyPath.Length - 1 == i){
					return _target.Remove(_key);
				}

				if(_target.ContainsKey(_key) == false){
					return false;
				}

				_target = _target[_key] as IDictionary<string, IDataBase>;
				if(_target == null){
					return false;
				}
			}

			return false;
		}

		public static void setValueByKeyPath<T>(IDictionary<string, IDataBase> _target, IDataBase _value, string[] _keyPath) where T : IDataBase, IDictionary<string, IDataBase>, new(){
			for(int i = 0; i < _keyPath.Length; i++){
				var _key = _keyPath[i];
				if(i == _keyPath.Length - 1){
					_target[_key] = _value;
				}else if(_target.ContainsKey(_key) == true){
					_target = _target[_key] as IDictionary<string, IDataBase>;
				}else{
					_target[_key] = new T();
					_target = _target[_key] as IDictionary<string, IDataBase>;
				}
			}
		}

		public static IDataBase findValue(IDictionary<string, IDataBase> _target, string[] _pathList){
			 for(int i = 0; i < _pathList.Length; i++){
                foreach(var _key in _target.Keys.ToArray()){
                    if(_key != _pathList[i]){
                        _target.Remove(_key);
                    } 
                }

                if(i == _pathList.Length - 1){
                    return _target[_pathList[i]];
                }

                _target = _target[_pathList[i]] as IDictionary<string, IDataBase>;
            }

			return null;
		}


		#region Extension

		static public IDataBase FindValue(this IDictionary<string, IDataBase> _this, string[] _pathList){
			return DictionaryUtil.findValue(_this, _pathList);
		}

		static public bool RemoveByKeyPath(this IDictionary<string, IDataBase> _this, string[] _keyPath){
			return DictionaryUtil.removeByKeyPath(_this, _keyPath);
		}

		static public void SetValueByKeyPath<T>(this IDictionary<string, IDataBase> _this, IDataBase _value, string[] _keyPath) where T : IDataBase, IDictionary<string, IDataBase>, new(){
			DictionaryUtil.setValueByKeyPath<T>(_this, _value, _keyPath);
		}

		static public List<string> GetKeyPathList(this IDictionary<string, IDataBase> _this, char _separator = '.'){
			var _result = new List<string>();
			DictionaryUtil.getKeyPathList(_this, _result, "", _separator);
			return _result;
		}

		static public void UpdateExistingFields(this IDictionary<string, IDataBase> _this, MutableDictionaryContainer _newValue){
			DictionaryUtil.updateExistingFields(_this, _newValue);
		}

		static public T GetDiff<T>(this T _this, IDictionary<string, IDataBase> _target, int _targetDepth = int.MaxValue) where T : IDictionary<string, IDataBase>, IDataBase, new(){
			var _diff = DictionaryUtil.getDiff<T>(_this, _target, _targetDepth, 0).Diff;
			var _json = _diff.ToString();
			
			int _count = 0;
			try{
				_diff.BuildVariable(ref _json, ref _count, JsonConvertor.GetInstance());
			}catch{
				
			}

			return _diff;
		}

		static public bool AreKeysEqual(this IDictionary<string, IDataBase> _this, IDictionary<string, IDataBase> targetDict)
		{
			return DictionaryUtil.areKeysEqual(_this, targetDict);
		}

		static public void RemoveAllExceptAt(this IDictionary<string, IDataBase> _this, string[] _paths){
			DictionaryUtil.removeAllExceptAt(_this, _paths);
		}
		#endregion
	}

}
