using System;
using BicDB;
using System.Collections.Generic;

namespace BicDB.Variable
{
	public class ListVariable<T> : VariableBase, IListVariable<T> where T : IVariable, new()
	{
		public event Action<T> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};

		#region AsValue
		public int AsInt{ get{ return 0; } set{} }
		public string AsString{ get{ return Storage.JsonConvertor.ConvertListToJsonString<T>(data); } set{ parse(value);  NotifyChanged ();} }
		public float AsFloat{ get{ return  0; } set{ } }
		public bool AsBool{ get{ return false; } set{ } }
		public VariableType Type { get { return VariableType.List; }}
		#endregion

		#region IListVariable
		public T this [int _index] { 
			get{ 
				return data[_index];
			} 
		}

		public int GetSize(){
			return data.Count;
		}

		public void Add(T _value){
			data.Add(_value);
			OnAddedValueActions(_value);
		}

		public void RemoveAt(int _index){
			data.RemoveAt(_index);
		}

		public bool Contains(T _value){
			foreach (var _item in data) {
				if (_value.AsString == _item.AsString) {
					return true;
				}
			}

			return false;
		}

		public void Clear(){
			data.Clear ();
			OnClearedValueActions();
		}
		#endregion



		private List<T> data = new List<T>();

		public ListVariable() : base(){

		}

		public void LoadValue(string _value){
			parse(_value);
			IsChanged = false;
		}

		private void parse(string _jsonString){
			data = Storage.JsonConvertor.ConvertJsonToList<T>(_jsonString);
		}
	}
}

