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


		public bool RemoveByKeyPath(string[] _keyPath){
			return removeByKeyPath(this, _keyPath);
		}

		public void SetValueByKeyPath(IDataBase _value, string[] _keyPath){
			var _target = this as IDictionary<string, IDataBase>;
			for(int i = 0; i < _keyPath.Length; i++){
				var _key = _keyPath[i];
				if(i == _keyPath.Length - 1){
					_target[_key] = _value;
				}else if(_target.ContainsKey(_key) == true){
					_target = _target[_key] as IDictionary<string, IDataBase>;
				}else{
					_target[_key] = new MutableDictionaryContainer();
					_target = _target[_key] as IDictionary<string, IDataBase>;
				}
			}
		}

		static internal bool removeByKeyPath(IDictionary<string, IDataBase> _target, string[] _keyPath){
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


		public List<string> GetKeyPath(char _separator = '.'){
			var _result = new List<string>();
			getKeyPath(this, _result, "", _separator);
			return _result;
		}

		static internal void getKeyPath(IDictionary<string, IDataBase> _target, List<string> _result, string _head, char _separator){
			foreach(var _key in _target.Keys){
				var _next = _target[_key] as IDictionary<string, IDataBase>;
				var _path = (string.IsNullOrEmpty(_head) ? ""  : _head + _separator) + _key;
				_result.Add(_path);
				if(_next != null){
					getKeyPath(_next, _result, _path, _separator);
				}
			}
		}
		
		public MutableDictionaryContainer GetDiff(IDictionary<string, IDataBase> _target, int _targetDepth = int.MaxValue){
			var _diff = getDiff<MutableDictionaryContainer>(this, _target, _targetDepth, 0).Diff;
			var _string = _diff.ToString();
			_diff.ParseJson(_string, false);
			return _diff;
		}

		static internal (T Diff, int DepthMax)  getDiff<T>(IDictionary<string, IDataBase> _origin, IDictionary<string, IDataBase> _target, int _targetDepth = int.MaxValue, int _depth = 0) where T : IDictionary<string, IDataBase>, new(){
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
		#endregion
	}

}
