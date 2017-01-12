using System;
using System.Collections.Generic;
using BigjamLibrary.BicDB.Column;

namespace BigjamLibrary.BicDB
{
	public interface IRow
	{
		Dictionary<string, IColumn> Columns { get; }

		event Action<IRow> OnChangedColumnActions;
		void OnChangedColumn (IColumn _Column);
	}

	public class Row : IRow{
		public event Action<IRow> OnChangedColumnActions = delegate{};
		private Dictionary<string, IColumn> columns = new Dictionary<string, IColumn>();
		public Dictionary<string, IColumn> Columns {
			get{ 
				return columns;
			}
		}

		public void AddManagedColumn(string key, IColumn _column){
			columns.Add (key, _column);
			_column.OnChangedValueActions += OnChangedColumn;
		}

		public void OnChangedColumn(IColumn _column){
			OnChangedColumnActions (this);
		}
	}
}


