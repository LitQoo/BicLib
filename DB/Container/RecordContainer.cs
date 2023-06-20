using System;
using System.Collections.Generic;
using BicDB.Container;
using BicDB.Variable;
using BicUtil.Json;
using BicDB.Storage;

namespace BicDB.Container
{
	public class RecordContainer : IRecordContainer{

		#region Logic
		private IDictionary<string, IDataBase> data = new Dictionary<string, IDataBase>();
		#endregion

		#region LifeCycle
		public RecordContainer(){

		}

		public RecordContainer(Dictionary<string, IDataBase> _default) : this(){
			foreach(var _info in _default){
				AddManagedColumn(_info.Key, _info.Value);
			}
		}
		#endregion

		#region IRecordContainer
		public IRecordContainerParent Parent{ get; set; }
		public virtual Action<IRecordContainer, string> OnChangedValueActions{ get; set;}
		Func<IRecordContainer, IVariable> GetRecordKey{ get; set;}

		public void NotifyChanged(string _message = ""){
			if (OnChangedValueActions != null) {
				OnChangedValueActions(this, _message);
			}
		}

		public void AddManagedColumn(string _key, IDataBase _value){
			if(data.ContainsKey(_key) == false){
				data.Add (_key, _value);
			}else{
				data[_key] = _value; 
			}
		}

		public T GetValue<T>(string _key) where T : class, IDataBase{
			if (!data.ContainsKey(_key)) {
				return default(T);
			}

			return (data[_key] as T);
		}

		public void ClearNotifyAndBinding(){
			OnChangedValueActions = delegate{};
		}

		public void Commit(){
			Parent.Commit(this);
		}
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.Record; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser){
			_parser.BuildModelContainer(this, ref _json, ref _counter);

			this.NotifyChanged();
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

		public override string ToString(){
			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			BuildFormattedString (_stringBuilder, JsonConvertor.GetInstance ());
			return _stringBuilder.ToString();
		}

		public bool ParseJson(string _json){
			int _count = 0;
			try{
				BuildVariable(ref _json, ref _count, JsonConvertor.GetInstance());
				return true;
			}catch{
				return false;
			}
		}
		#endregion

		#region IDictionary
		public void Add(string _key, IDataBase _value)
		{
			data.Add(_key, _value);
		}

		public bool ContainsKey(string _key)
		{
			return data.ContainsKey(_key);
		}

		public bool Remove(string _key)
		{
			return data.Remove(_key);
		}

		public bool TryGetValue(string _key, out IDataBase _value)
		{
			return data.TryGetValue(_key, out _value);
		}

		public void Add(KeyValuePair<string, IDataBase> _item)
		{
			data.Add(_item);
		}

		public void Clear()
		{
			data.Clear();
		}

		public bool Contains(KeyValuePair<string, IDataBase> _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(KeyValuePair<string, IDataBase>[] _array, int _arrayIndex){
			data.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(KeyValuePair<string, IDataBase> _item)
		{
			return data.Remove(_item);
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

		public void MergeCopyBy(IRecordContainer _model){
			string _json = _model.ToString();
			int _counter = 0;
			this.BuildVariable (ref _json, ref _counter, JsonConvertor.GetInstance ());
		}

		static public RecordContainer GetDiff(IDictionary<string, IDataBase> _origin, IDictionary<string, IDataBase> _target, int _depth = int.MaxValue){
			var _diff = new RecordContainer();
			
			foreach(var _key in _target.Keys){
				if(_origin.ContainsKey(_key) == true){
					var _targetValue = _target[_key]; 
					if(_targetValue is IDictionary<string, IDataBase>){
						
						var _nestedDiff = GetDiff(_origin[_key] as IDictionary<string, IDataBase>, _targetValue as IDictionary<string, IDataBase>, _depth - 1);
						if(_nestedDiff.Keys.Count <= 0){

						}else if(_depth <= 0){
							_diff[_key] = _targetValue;
						}else{
							_diff[_key] = _nestedDiff;
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

			var _string = _diff.ToString();
			_diff.ParseJson(_string);
			return _diff;
		}
		#endregion
	}
}