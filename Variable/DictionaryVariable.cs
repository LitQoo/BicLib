using System;
using BicDB;
using System.Collections.Generic;

namespace BicDB.Variable
{
	public class DictionaryVariable<T> : VariableBase, IDictionaryVariable<T> where T : IVariable, new()
	{

		private Dictionary<string, T> data = new Dictionary<string, T>();

		#region AsValue
		public int AsInt{ get{ return 0; } set{} }
		public string AsString{ get{ return Storage.JsonConvertor.ConvertDictionaryToJsonString<T>(data); } set{ parse(value);  NotifyChanged ();} }
		public float AsFloat{ get{ return  0; } set{ } }
		public bool AsBool{ get{ return false; } set{ } }
		public VariableType Type { get { return VariableType.List; }}
		#endregion

		#region IDictionaryVariable
		public T this [string _key] { 
			get{ 
				return data [_key];
			} 

			protected set{ 
				data [_key] = value;
			} 
		}

		public int GetSize(){
			return data.Count;
		}

		public void Add(string _key, T _value){
			data.Add (_key, _value);
		}

		public void RemoveAt(string _key){
			data.Remove (_key);	
		}

		public bool Contains(T _value){
			foreach (var _item in data) {
				if (_value.AsString == _item.Value.AsString) {
					return true;
				}
			}

			return false;	
		}

		public bool Contains(string _key){
			return data.ContainsKey (_key);
		}

		public void Clear(){
			data.Clear ();
		}
		#endregion

		#region LifeCycle
		public DictionaryVariable() : base(){

		}
		#endregion

		#region Logic
		public void LoadValue(string _value){
			parse(_value);
			IsChanged = false;
		}

		private void parse(string _jsonString){
			data = Storage.JsonConvertor.ConvertJsonToDictionary<T>(_jsonString);
		}
		#endregion
	}
}
