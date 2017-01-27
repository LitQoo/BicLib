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
		bool IsChanged{ get; set;}

		void NotifyChanged();
		void LoadValue(string _value);
	}

	public enum VariableType
	{
		Int,
		Float,
		String,
		Bool
	}

	public class VariableBase{
		public event Action<IVariable> OnChangedValueActions = delegate{};

		public bool IsChanged{ get; set;}

		public VariableBase(){
			IsChanged = false;
		}

		public void NotifyChanged(){
			IsChanged = true;
			OnChangedValueActions (this as IVariable);
		}
	}
}

