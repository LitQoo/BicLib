using System;
using System.Collections.Generic;
using BicDB.Variable;

namespace BicDB
{
	public interface IModelVariable : IVariable{
		new event Action<IModelVariable, string> OnChangedValueActions;
		IVariable this [string _key] { get; }
		bool Contains(string _key);
		void AddManagedColumn(string _key, IVariable _value);
		List<string> GetColumnNameList();
	}

	public class ModelVariable : VariableBase, IModelVariable{
		private Dictionary<string, IVariable> data = new Dictionary<string, IVariable>();

		public new event Action<IModelVariable, string> OnChangedValueActions = delegate{};


		#region AsValue
		public int AsInt{ get{ return 0; } set{} }
		public string AsString{ get{ return string.Empty; } set{ } }
		public float AsFloat{ get{ return  0; } set{ } }
		public bool AsBool{ get{ return false; } set{ } }
		public VariableType Type { get { return VariableType.Model; }}
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

		public void LoadFormatString(ref string _json, ref int _counter, IStringParser _parser)
		{
			_parser.ToModel(this, ref _json, ref _counter);
			IsChanged = false;
		}

		public void GetFormatString(ref string _json, IStringFormatter _formatter){
			_formatter.ToFormattedString(this, ref _json);
		}

		public new void NotifyChanged(string _message = ""){
			IsChanged = true;
			OnChangedValueActions (this, _message);
		}
		#endregion
	}
}