using System;

namespace BigjamLibrary.BicDB.Column
{
	public interface IColumn
	{
		int AsInt{ get; set; }
		string AsString{ get; set; }
		float AsFloat{ get; set; }
		ColumnType Type { get; }
		void OnChangedValue();
		event Action<IColumn> OnChangedValueActions;
	}

	public enum ColumnType
	{
		Int,
		Float,
		String
	}
}

