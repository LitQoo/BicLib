using System;
using BicDB;
using System.Collections.Generic;
using UnityEditor;
using System.Security.Cryptography;
using System.Linq;

namespace BicDB.Variable
{
	public interface IDictionaryVariable<T> : IVariableBase where T : IVariableBase, new()
	{
		T this [string _key] { get; }
		string[] Keys{ get; }

		int GetSize();
		void Add(string _key, T _value);
		void RemoveAt(string _key);
		bool Contains(T _value);
		bool Contains(string _key);
		void Clear();
	}

	public class DictionaryVariable<T> : VariableBase, IDictionaryVariable<T> where T : IVariableBase, new()
	{

		private Dictionary<string, T> data = new Dictionary<string, T>();

		#region AsValue
		public int AsInt{ get{ return 0; } set{} }
		public string AsString{ get{ return string.Empty; } set{ } }
		public float AsFloat{ get{ return  0; } set{ } }
		public bool AsBool{ get{ return false; } set{ } }
		public VariableType Type { get { return VariableType.Dictionary; }}
		public string AsFormattedString { get; set; }
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
				if (_value.AsFormattedString == _item.Value.AsFormattedString) {
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

		public string[] Keys{
			get{ 
				return data.Keys.ToArray(); 
			}
		} 
		#endregion

		#region LifeCycle
		public DictionaryVariable() : base(){

		}
		#endregion

		#region Logic

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildDictionaryVariable(this, ref _json, ref _counter);
			IsChanged = false;
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion
	}

}
