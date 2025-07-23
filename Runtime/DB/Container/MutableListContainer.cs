using System;
using BicDB;
using System.Collections.Generic;
using System.Linq;
using BicDB.Storage;
using BicDB.Container;
using BicDB.Variable;
using System.Collections;

namespace BicDB.Container
{
	public class MutableListContainer : IMutableListContainer
	{
		private IList<IDataBase> data = new List<IDataBase>();

		#region IListContainer
		public event Action<IDataBase> OnAddedValueActions = delegate{};
		public event Action OnClearedValueActions = delegate{};
		public OnChangedElementDelegator<int, IDataBase> OnChangedElementActions{ get; set;}
		#endregion

		#region IDataBase
		public DataType Type { get { return DataType.List; }}

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser){
			_parser.BuildMutableListContainer(this, ref _json, ref _counter);
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

		#region IList
		public int IndexOf(Func<IDataBase, bool> _find){
			for(int i = 0; i < this.Count; i++){
				if(_find(data[i]) == true){
					return i;
				}
			}

			return -1;
		}

		public int IndexOf(IDataBase _item)
		{
			return data.IndexOf(_item);
		}

		public void Insert(int _index, IDataBase _item)
		{
			data.Insert(_index, _item);
			OnAddedValueActions(_item);
		}

		public void RemoveAt(int _index)
		{
			data.RemoveAt(_index);
		}

		public void Add(IDataBase _item)
		{
			data.Add(_item);
			OnAddedValueActions(_item);
		}
			
		public void Clear()
		{
			data.Clear();
			OnClearedValueActions();
		}

		public bool Contains(IDataBase _item)
		{
			return data.Contains(_item);
		}

		public void CopyTo(IDataBase[] _array, int _arrayIndex)
		{
			data.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(IDataBase _item)
		{
			return data.Remove(_item);
		}

		public IEnumerator<IDataBase> GetEnumerator()
		{
			return data.GetEnumerator ();
			//return new ListContainerEnumerator<T> (data.ToArray());
		}

		public void NotifyChangedWithChild(){
			foreach(var _info in this.data){
				switch(_info){
					case INotifyWithChild _parent: _parent.NotifyChangedWithChild(); break;
					case IVariable _varaible:_varaible.NotifyChanged(); break;
					default:
						
					break;
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return data.GetEnumerator();
		}

		public IDataBase this[int _index] {
			get {
				return data[_index];
			}
			set {
				data[_index] = value;
				OnChangedElementActions[_index](_index, data[_index]);
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

		public object GetObject(int _index){
			return data[_index];
		}

		public int ObjectCount=>this.Count;
		#endregion

		public MutableListContainer() : base(){
			OnChangedElementActions = new OnChangedElementDelegator<int, IDataBase>();

		}

		public override string ToString(){
			System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
			BuildFormattedString (_stringBuilder, BicUtil.Json.JsonConvertor.GetInstance ());
			return _stringBuilder.ToString();
		}

	}
}

