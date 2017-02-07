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

	public interface IListVariable : IVariable{
		IVariable this [int _index] { get; }
		int GetSize();
		void Add(IVariable _value);
		void RemoveAt(int _index);
		bool Contains(IVariable _value);
	}

	public enum VariableType
	{
		Int,
		Float,
		String,
		Bool,
		List,
		Dictionary
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

