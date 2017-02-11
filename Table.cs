using System;
using System.Collections.Generic;
using System.Linq;
using BicDB.Variable;

namespace BicDB
{

	public interface ITable<T> : ILinqSupporter<T>
	{
		string Name{ get; set;}
		string PrimaryKey{ get; set; }

		T this [int _index] { get; }

		void AddRow(T _row);
		void Save(Action<bool> _callaback = null, object _parameter = null);
		void Load(Action<bool> _callaback = null, object _parameter = null);
		void SetStorage(IStorage _storage);
		int GetSize();
		void Clear();

		void SetHeader (string _key, string _value);
		void SetHeader (string _key, float _value);
		void SetHeader (string _key, int _value);
		void SetHeader (string _key, bool _value);
		IVariable GetHeader (string _key);
		bool ContainsHeader(string _key);
		string[] GetHeaderKeyList();

	}

	static public class HeaderKey{
		static public string PrimaryKey = "primaryKey";
	}

	public interface ILinqSupporter<T>
	{

		IEnumerable<T> Where(Func<T, bool> _func);
		T FirstOrDefault(Func<T, bool> _func);
		IEnumerable<U> Select<U>(Func<T, U> _func);
	}


	public class Table<T> : ITable<T> where T : class, IModel, new(){
		private List<T> rows = new List<T>();

		#region LifeCycle
		public Table(string _name){
			Name = _name;
		}
		#endregion

		#region ITable
		public string Name{ get; set;}

		public int GetSize(){
			return rows.Count;
		}

		public T this[int _index]
		{
			get{return rows [_index] as T;}
		}

		public void AddRow(T _row){
			rows.Add(_row);
		}

		public void Clear(){
			rows.Clear();
		}

		public string PrimaryKey{ 
			get{ 
				return GetHeader (HeaderKey.PrimaryKey).AsString;
			} 

			set{ 
				SetHeader (HeaderKey.PrimaryKey, value);
			} 
		}
		#endregion

		#region Linq
		public IEnumerable<T> Where(Func<T, bool> _func){
			return rows.Where(_func);
		}

		public T FirstOrDefault(Func<T, bool> _func){
			return rows.FirstOrDefault(_func);
		}

		public IEnumerable<U> Select<U>(Func<T, U> _func){
			return rows.Select<T,U>(_func);
		}

		public IEnumerable<T> GetChangedRows(){
			return Where ((T _row)=> _row.IsChanged());
		}
		#endregion

		#region Storage
		private IStorage storage;
		public void SetStorage(IStorage _storage){
			storage = _storage;
		}

		public void Save(Action<bool> _callaback = null, object _parameter = null){
			storage.Save(this, _callaback, _parameter);
		}


		public void Load(Action<bool> _callaback = null, object _parameter = null){
			storage.Load(this, _callaback, _parameter);
		}
		#endregion

		#region Header
		private Dictionary<string, IVariable> header = new Dictionary<string, IVariable> ();

		public void SetHeader (string _key, string _value){
			if (!ContainsHeader (_key)) {
				header.Add(_key, new StringVariable(_value));
			}else{
				header [_key].AsString = _value;
			}
		}

		public void SetHeader (string _key, float _value){
			if (!ContainsHeader (_key)) {
				header.Add(_key, new FloatVariable(_value));
			}else{
				header [_key].AsFloat = _value;
			}
		}

		public void SetHeader (string _key, int _value){
			if (!ContainsHeader (_key)) {
				header.Add(_key, new IntVariable(_value));
			}else{
				header [_key].AsInt = _value;
			}
		}

		public void SetHeader (string _key, bool _value){
			if (!ContainsHeader (_key)) {
				header.Add(_key, new BoolVariable(_value));
			}else{
				header [_key].AsBool = _value;
			}
		}

		public IVariable GetHeader (string _key){
			return header [_key];
		}

		public bool ContainsHeader(string _key){
			return header.ContainsKey (_key);
		}

		public string[] GetHeaderKeyList(){
			return header.Keys.ToArray ();
		}

		#endregion
	}

}

