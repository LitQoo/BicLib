using System;
using BicDB;
using System.Collections.Generic;
using UnityEditor;
using System.Security.Cryptography;
using System.Linq;

namespace BicDB.Container
{
	public interface IDictionaryContainer<T> : IDataBase where T : IDataBase, new()
	{
		T this [string _key] { get; }
		string[] Keys{ get; }

		int GetSize();
		void Add(string _key, T _value);
		void RemoveAt(string _key);
		bool Contains(string _key);
		void Clear();
	}

	public class DictionaryContainer<T> : IDictionaryContainer<T> where T : IDataBase, new()
	{

		private Dictionary<string, T> data = new Dictionary<string, T>();

		#region AsValue
		public int AsInt{ get{ return 0; } set{} }
		public string AsString{ get{ return string.Empty; } set{ } }
		public float AsFloat{ get{ return  0; } set{ } }
		public bool AsBool{ get{ return false; } set{ } }
		public DataType Type { get { return DataType.Dictionary; }}
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
		public DictionaryContainer() : base(){

		}
		#endregion

		#region Logic

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildDictionaryVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}
		#endregion
	}

}
