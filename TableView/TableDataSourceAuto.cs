using System;
using System.Collections.Generic;
using BicDB.Container;

namespace BicUtil.TableView
{
    internal class TableDataSourceAuto<T> : ITableViewDataSource  where T : IRecordContainer, new(){
        private Func<TableView, IList<T>, int, float> getRowHeightFunc = null;
		private IList<T> table;
		private TableView tableView;

        public TableDataSourceAuto(TableView _tableView, IList<T> _table, Func<TableView, IList<T>, int, float> _getRowHeightFunc = null){
            table = _table;
			tableView = _tableView;
            getRowHeightFunc = _getRowHeightFunc;
        }
        
        public int GetNumberOfCellsForTableView()
		{
			return table.Count;
		}

		public float GetHeightForRowInTableView(int _rowIndex)
		{
            if(getRowHeightFunc != null){
                return getRowHeightFunc(tableView, table, _rowIndex);
            }

			return tableView.GetRowHeight();
		}

		public TableRow GetCellForRowInTableView(int _rowIndex)
		{
			var _tableRow = tableView.CreateTableRow(); // 셀 리턴
			_tableRow.RowIndex = _rowIndex;
			return _tableRow;
		}

		public IRecordContainer GetCellData(int _index){
			if(table.Count <= _index){
				return null;
			}

			return table[_index];	
		}

		public int GetCellCountInRow(int _rowIndex){
			return tableView.CellCountInRowDefault;
		}

		public int GetRowCount(){
			return (int)Math.Ceiling((float)GetNumberOfCellsForTableView() / (float)tableView.CellCountInRowDefault);
		}

		public int GetStartDataIndex(int _rowIndex){
			 return _rowIndex * tableView.CellCountInRowDefault;
		}
	}

}
