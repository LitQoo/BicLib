using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB
{
	public interface IModel
	{
		//event
		event Action<IModel, string> OnNotifyActions;

		//indexer
		IVariable this [string _key] { get; }

		//method
		void NotifyChanged(string _message = "");
		void AddManagedColumn(string key, IVariable _column);
		bool ContainsKey(string _key);
		List<string> GetColumnNameList();
		bool IsChanged();
	}

	public class Model : IModel{
		public event Action<IModel, string> OnNotifyActions = delegate{};

		private Dictionary<string, IVariable> columns = new Dictionary<string, IVariable>();

		public IVariable this[string _key]
		{
			get{return columns [_key];}
		}

		public void AddManagedColumn(string key, IVariable _column){
			columns.Add (key, _column);
		}

		public void NotifyChanged(string _message = ""){
			OnNotifyActions(this, _message);
		}

		public bool ContainsKey(string _key){
			return columns.ContainsKey (_key);
		}

		public List<string> GetColumnNameList(){
			return new List<string>(columns.Keys);
		}

		public bool IsChanged(){
			foreach (var _item in columns) {
				if (_item.Value.IsChanged) {
					return true;
				}
			}

			return false;
		}
	}
}