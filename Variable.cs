using System;

namespace BigjamLibrary.BicDB.Variable
{
	public interface IVariable
	{

		event Action<IVariable> OnChangedValueActions;

		int AsInt{ get; set; }
		string AsString{ get; set; }
		float AsFloat{ get; set; }
		VariableType Type { get; }

		void OnChangedValue(IVariable _variable);
	}

	public enum VariableType
	{
		Int,
		Float,
		String
	}
}

