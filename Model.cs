using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB
{
	public interface IModelVariable : IVariableBase{
		IVariableBase this [string _key] { get; }
		bool Contains(string _key);
		void AddManagedColumn(string _key, IVariableBase _value);
		List<string> GetColumnNameList();

		event Action<IModelVariable, string> OnChangedValueActions;
		void NotifyChanged(string _message = "");
	}

	public class ModelVariable : IModelVariable{
		private Dictionary<string, IVariableBase> data = new Dictionary<string, IVariableBase>();

		public event Action<IModelVariable, string> OnChangedValueActions;
		public void NotifyChanged(string _message = ""){
			OnChangedValueActions(this, _message);
		}

		#region AsValue
		public string AsFormattedString{get;set;}
		public VariableType Type { get { return VariableType.Model; }}
		#endregion

		#region IDictionaryVariable
		public IVariableBase this [string _key] { 
			get{ 
				return data [_key];
			} 

			protected set{ 
				data [_key] = value;
			} 
		}


		public void AddManagedColumn(string _key, IVariableBase _value){
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