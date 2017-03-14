using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB.Container
{
	public interface IModelContainer : IDataBase{
		IDataBase this [string _key] { get; }
		bool Contains(string _key);
		void AddManagedColumn(string _key, IDataBase _value);
		List<string> GetColumnNameList();

		event Action<IModelContainer, string> OnChangedValueActions;
		void NotifyChanged(string _message = "");
	}

	public class ModelContainer : IModelContainer{
		private Dictionary<string, IDataBase> data = new Dictionary<string, IDataBase>();

		public event Action<IModelContainer, string> OnChangedValueActions;
		public void NotifyChanged(string _message = ""){
			OnChangedValueActions(this, _message);
		}

		#region AsValue
		public string AsFormattedString{get;set;}
		public DataType Type { get { return DataType.Model; }}
		#endregion

		#region IDictionaryVariable
		public IDataBase this [string _key] { 
			get{ 
				return data [_key];
			} 

			protected set{ 
				data [_key] = value;
			} 
		}


		public void AddManagedColumn(string _key, IDataBase _value){
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

		public void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.BuildModelVariable(this, ref _json, ref _counter);
		}

		public void BuildFormattedString(ref string _json, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this, ref _json);
		}

		#endregion
	}
}