using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB
{
	public interface IModel
	{
		//event
		event Action<IModel, string> OnNotifyActions;

		//member
		Dictionary<string, IVariable> Columns { get; }

		//method
		void Notify(string _message = "");
		void AddManagedColumn(string key, IVariable _column);
	}

	public class Model : IModel{
		public event Action<IModel, string> OnNotifyActions = delegate{};

		private Dictionary<string, IVariable> columns = new Dictionary<string, IVariable>();
		public Dictionary<string, IVariable> Columns {
			get{ 
				return columns;
			}
		}

		public void AddManagedColumn(string key, IVariable _column){
			columns.Add (key, _column);
		}

		public void Notify(string _message = ""){
			OnNotifyActions(this, _message);
		}
	}
}


