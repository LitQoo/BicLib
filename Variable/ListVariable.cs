using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;

namespace BicDB.Variable
{

	public interface IListVariable<T> : IVariable, ILinqSupporter<T> where T : IVariable, new(){
		new event Action<IListVariable<T>, string> OnChangedValueActions;
		event Action<T> OnAddedValueActions;
		event Action OnClearedValueActions;
		OnChangedElementDelegator<int, T> OnChangedElementActions { get; set;}

		T this [int _index] { get; set;}
		int GetSize();
		void Add(T _value);
		void RemoveAt(int _index);
		bool Contains(T _value);
		void Clear();
	}


	public class ListVariable<T> : VariableBase, IListVariable<T> where T : IVariable, new()
	{
		public new event Action<IListVariable<T>, string> OnChangedValueActions;
		public event Action<T> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};
		public OnChangedElementDelegator<int, T> OnChangedElementActions{ get; set;}

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

			set{ 
				data[_index] = value;
				OnChangedElementActions[_index](_index, data[_index]);
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

		#region Linq
		public IEnumerable<T> Where(Func<T, bool> _func){
			return data.Where(_func);
		}

		public T FirstOrDefault(Func<T, bool> _func){
			return data.FirstOrDefault(_func);
		}

		public IEnumerable<U> Select<U>(Func<T, U> _func){
			return data.Select(_func);
		}

		public IOrderedEnumerable<T> OrderBy<U>(Func<T, U> _func){
			return data.OrderBy(_func);
		}

		public IOrderedEnumerable<T> OrderByDescending<U>(Func<T, U> _func){
			return data.OrderByDescending(_func);
		}
		#endregion

		private List<T> data = new List<T>();

		public ListVariable() : base(){
			OnChangedElementActions = new OnChangedElementDelegator<int, T>();
		}

		public void LoadValue(string _value){
			parse(_value);
			IsChanged = false;
		}

		private void parse(string _jsonString){
			data = Storage.JsonConvertor.ConvertJsonToList<T>(_jsonString);
		}

		public new void NotifyChanged(string _message = ""){
			IsChanged = true;
			OnChangedValueActions (this, _message);
		}
	}

}

