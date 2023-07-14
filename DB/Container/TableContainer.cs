using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Container;
using BicDB.Variable;
using BicDB.Storage;
using BicDB.Core;
using UnityEngine;
using BicUtil.Json;
using System.Threading.Tasks;

namespace BicDB.Container
{
	public class TableContainer<T> : ITableContainer<T>, IQueryTable where T : class, IRecordContainer, new(){
		private IList<T> rows = new List<T>();

		#region IRecordContainerParent
		public string Name{ get; set;}

		public string PrimaryKey{ 
			get{ 
				if (Header.ContainsKey(HeaderKey.PrimaryKey)) {
					return (Header[HeaderKey.PrimaryKey] as IVariable).AsString;
				} else {
					return string.Empty;
				}
			} 

			set{ 
				Header [HeaderKey.PrimaryKey] = new StringVariable (value);
			} 
		}

		private MutableDictionaryContainer header = new MutableDictionaryContainer();
		public MutableDictionaryContainer Header {get{ return header;}}

		private MutableDictionaryContainer property = new MutableDictionaryContainer();
		public MutableDictionaryContainer Property{get{ return property;}}

		public IVariable GetRecordKey(IRecordContainer _record){
			return new IntVariable(IndexOf(_record as T));
		}

		public T GetRowByPrimaryKey(string _key){
			for(int i = 0; i < this.Count; i++){
				if(this[i][this.PrimaryKey].AsVariable.AsString == _key){
					return this[i];
				}
			}

			return null;
		}
		#endregion

		#region IListContainerReadObject

		public object GetObject(int _index){
			return this[_index];
		}

		public int ObjectCount=>this.Count;
		#endregion


		#region ITableContainer
		public virtual Action<T> OnAddedRowActions { get; set; }
		public virtual Action<T> OnRemovedRowActions { get; set; }
		public virtual Action OnChangeCountActions { get; set; }
		public virtual Action<Result> OnSave { get; set; }

		public event Action OnSetup;
		public event Action<string, string> OnMigration;
		public event Action OnHashCodeError;

		public void SubscribeOnSave(Action<Result> _callback, bool _needFirstCall = false){
			this.OnSave += _callback;
			
			if(_needFirstCall == true){
				_callback(new Result(0));
			}
		}

		public void SubscribeOnAdded(Action<T> _callback){
			this.OnAddedRowActions += _callback;
		}
		#endregion

		#region LifeCycle
		public TableContainer(string _name){
			TableService.Init();
			#if UNITY_EDITOR
			TableService.Inspector.Track("Table."+_name, this);
			#endif
			Name = _name;
		}
		#endregion

		#region IListVariable
		public int IndexOf(Func<T,bool> _find){
			for(int i = 0; i < this.Count; i++){
				if(_find(rows[i]) == true){
					return i;
				}
			}

			return -1;
		}
		
		public int IndexOf(T _item)
		{
			return rows.IndexOf(_item);
		}

		public void Insert(int _index, T _item)
		{
			_item.Parent = this;
			rows.Insert(_index, _item);
			if (OnAddedRowActions != null) {
				OnAddedRowActions(_item);
			}
			
			if(OnChangeCountActions != null){
				OnChangeCountActions();
			}
		}

		public void RemoveAt(int _index)
		{
			rows [_index].Parent = null;
			if (OnRemovedRowActions != null) {
				OnRemovedRowActions(rows [_index]);
			}

			if(OnChangeCountActions != null){
				OnChangeCountActions();
			}
			rows.RemoveAt(_index);
		}

		public void Add(T _item)
		{
			_item.Parent = this;
			rows.Add(_item);
			if (OnAddedRowActions != null) {
				OnAddedRowActions(_item);
			}
			
			if(OnChangeCountActions != null){
				OnChangeCountActions();
			}
		}

		public void AddWithoutDuplication(T _item)
		{
			if(String.IsNullOrEmpty(this.PrimaryKey)){
				throw new Exception("[BicDB] Not Set PrimaryKey");
			}

			if(this.Contains(_item) == true){
				return;
			}

			var _itemPrimaryValue = _item[PrimaryKey].AsVariable.AsString;
			T _foundRow = rows.FirstOrDefault(_row=>_row[PrimaryKey].AsVariable.AsString == _itemPrimaryValue);

			if(_foundRow == null){
				this.Add(_item);
			}else{
				_foundRow.MergeCopyBy(_item);
			}
		}	

		public void Clear()
		{
			foreach (var _item in rows) {
				_item.Parent = null;
				if (OnRemovedRowActions != null) {
					OnRemovedRowActions(_item);
				}
			}

			rows.Clear();

			if(OnChangeCountActions != null){
				OnChangeCountActions();
			}
		}

		public bool Contains(T _item)
		{
			return rows.Contains(_item);
		}

		public void CopyTo(T[] _array, int _arrayIndex)
		{
			rows.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(T _item)
		{
			
			if (rows.Remove(_item)) {
				_item.Parent = null;
				if (OnRemovedRowActions != null) {
					OnRemovedRowActions(_item);
				}

				if(OnChangeCountActions != null){
					OnChangeCountActions();
				}
				return true;
			}

			return false;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return rows.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return rows.GetEnumerator();
		}

		public T this[int _index] {
			get {
				return rows[_index];
			}
			set {
				if (rows[_index] != null) {
					rows[_index].Parent = null;
				}

				value.Parent = this;
				rows[_index] = value;
			}
		}

		public int Count {
			get {
				return rows.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return rows.IsReadOnly;
			}
		}

		public void NotifyChangedWithChild(){
			foreach(var _info in this.rows){
				_info.NotifyChangedWithChild();
			}
		}
		#endregion

		#region IDatabase
		public DataType Type { get { return DataType.Table; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildTableContainer(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter)
		{
			_formatter.BuildFormattedString(this, _stringBuilder, null);
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


		#region Storage
		private ITableStorage storage;
		public void SetStorage(ITableStorage _storage){
			storage = _storage;
		}

		public void Save(Action<Result> _callback = null, object _parameter = null){
			storage.Save(this, _result=>{
				if(_result.Code == 0){
					if(Name != TableService.TABLENAME){
						var _tableInfo = TableService.GetTableInfo(Name, true);
						_tableInfo.Table = this;
						_tableInfo.AddPath(_result.Path);
						_tableInfo.StorageType.AsString = storage.StorageType;
						_tableInfo.HashCode.AsInt = _result.HashCode;
						_tableInfo.SaveCount.AsInt++;
						TableService.Save();
					}else{
						var _currentSavingPath = PlayerPrefs.GetString(TableService.TABLENAME, string.Empty);
						if(_currentSavingPath != _result.Path){
							PlayerPrefs.SetString(TableService.TABLENAME, _result.Path);
							PlayerPrefs.Save();
						}
					}
				}

				if(_callback !=null){
					_callback(_result);
				}

				if(OnSave != null){
					OnSave(_result);
				}
			}, _parameter);
		}

		public void Load(Action<Result> _callback = null, object _parameter = null){
			storage.Load(this, _result=>{
				if(_result.Code == 0){
					if(Name != TableService.TABLENAME){
						bool _saveTableInfomaiton = false;
						var _tableInfo = TableService.GetTableInfo(Name, true);
						_tableInfo.Table = this;
						
						if(_tableInfo.IsFirstSetup == true){
							if(this.OnSetup != null){
								this.OnSetup();
							}
						}else if(_tableInfo.StorageType.AsString != storage.StorageType){
							//onmigration
							if(this.OnMigration != null){
								this.OnMigration(_tableInfo.StorageType.AsString, storage.StorageType);
							}

							_tableInfo.StorageType.AsString = storage.StorageType;
							_saveTableInfomaiton = true;
						}

						if(_tableInfo != null && _tableInfo.HashCode.AsInt != _result.HashCode){
							if(this.OnHashCodeError != null){
								this.OnHashCodeError();
							}
							
							_tableInfo.ErrorCount.AsInt++;
							_tableInfo.HashCode.AsInt = _result.HashCode;
							_saveTableInfomaiton = true;
						}

						if(_saveTableInfomaiton == true){
							_tableInfo.SaveCount.AsInt++;
							TableService.Save();
						}
					}
				}

				if(_callback !=null){
					_callback(_result);
				}
			}, _parameter);
		}

		public void Pull(Action<Result> _callback = null, object _parameter = null){
			storage.Pull(this, _callback, _parameter);
		}

		private IList<T> commitRows = new List<T>();
		public IList<T> CommitRow{get{return commitRows;}}
		
		public void Commit(IRecordContainer _record){
			if(commitRows.Contains(_record as T) == false){
				commitRows.Add(_record as T);
			}
		}

		public void Push(Action<Result> _callback = null){
			if(commitRows.Count > 0){
				storage.Push(this, _callback);
				commitRows.Clear();
			}
		}

		public async Task<Result> LoadAsync(object _parameter = null){
			var _result = await storage.LoadAsync(this, _parameter);
			return _result;
		}

        public IRecordContainer RecordAt(int _index)
        {
            return this.ElementAt(_index) as IRecordContainer;
        }
        #endregion

        #region Autoincrease
        private const string AUTO_INCREASE_NUMBER = "__AutoIncreaseNumber__";
		public int AutoIncreaseNumber{
			get{
				if(this.Property.ContainsKey(AUTO_INCREASE_NUMBER) == false){
					this.Property.Add(AUTO_INCREASE_NUMBER, new IntVariable(1));
				}

				var _result = this.Property[AUTO_INCREASE_NUMBER].AsVariable.AsInt;
				this.Property[AUTO_INCREASE_NUMBER].AsVariable.AsInt++;
				return _result;
			}
		}
		#endregion

		#region override
		public override string ToString(){
			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			BuildFormattedString (_stringBuilder, JsonConvertor.GetInstance ());
			return _stringBuilder.ToString();
		}
		#endregion
	}

}