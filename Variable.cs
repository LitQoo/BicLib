using System;

namespace BicDB
{
	public interface IVariable
	{

		event Action<IVariable> OnChangedValueActions;

		int AsInt{ get; set; }
		string AsString{ get; set; }
		float AsFloat{ get; set; }
		bool AsBool{ get; set; }
		VariableType Type { get; }

		void OnChangedValue();
	}

	public enum VariableType
	{
		Int,
		Float,
		String,
		Bool
	}


}

