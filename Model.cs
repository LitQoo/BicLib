using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB
{
//	public interface IModel
//	{
//		//event
//		event Action<IModel, string> OnNotifyActions;
//
//		//indexer
//		IVariable this [string _key] { get; }
//
//		//method
//		void NotifyChanged(string _message = "");
//		void AddManagedColumn(string key, IVariable _column);
//		bool ContainsKey(string _key);
//		List<string> GetColumnNameList();
//		bool IsChanged();
//	}
//
//	public class Model : IModel{
//		public event Action<IModel, string> OnNotifyActions = delegate{};
//
//		private Dictionary<string, IVariable> columns = new Dictionary<string, IVariable>();
//
//		public IVariable this[string _key]
//		{
//			get{return columns [_key];}
//		}
//
//		public void AddManagedColumn(string key, IVariable _column){
//			columns.Add (key, _column);
//		}
//
//		public void NotifyChanged(string _message = ""){
//			OnNotifyActions(this, _message);
//		}
//
//		public bool ContainsKey(string _key){
//			return columns.ContainsKey (_key);
//		}
//
//		public List<string> GetColumnNameList(){
//			return new List<string>(columns.Keys);
//		}
//
//		public bool IsChanged(){
//			foreach (var _item in columns) {
//				if (_item.Value.IsChanged) {
//					return true;
//				}
//			}
//
//			return false;
//		}
//	}

	public interface IModelVariable : IVariable{
		IVariable this [string _key] { get; }
		bool Contains(string _key);
		void AddManagedColumn(string _key, IVariable _value);
		List<string> GetColumnNameList();
	}

	public class ModelVariable : VariableBase, IModelVariable{
		private Dictionary<string, IVariable> data = new Dictionary<string, IVariable>();

		#region AsValue
		public int AsInt{ get{ return 0; } set{} }
		public string AsString{ get{ return Storage.JsonConvertor.ConvertDictionaryToJsonString<IVariable>(data); } set{ parse(value);  NotifyChanged ();} }
		public float AsFloat{ get{ return  0; } set{ } }
		public bool AsBool{ get{ return false; } set{ } }
		public VariableType Type { get { return VariableType.List; }}
		#endregion

		#region IDictionaryVariable
		public IVariable this [string _key] { 
			get{ 
				return data [_key];
			} 

			protected set{ 
				data [_key] = value;
			} 
		}


		public void AddManagedColumn(string _key, IVariable _value){
			data.Add (_key, _value);
		}

		public bool Contains(string _key){
			return data.ContainsKey (_key);
		}

		public List<string> GetColumnNameList(){
			return new List<string>(data.Keys);
		}
		#endregion


		#region Logic
		public void LoadValue(string _value){
			parse(_value);
			IsChanged = false;
		}

		private void parse(string _jsonString){
			Storage.JsonConvertor.ConvertJsonToModel(this, _jsonString);
		}
		#endregion
	}
}