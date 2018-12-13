using System;
using System.Collections.Generic;
using BicDB.Container;

namespace BicUtil.TableView
{
    internal class TableDataSourceAuto<T> : ITableViewDataSource  where T : IRecordContainer, new(){
        public Func<TableView, IList<T>, int, float> getRowHeightFunc = null;

		private IList<T> table;
        public TableDataSourceAuto(IList<T> _table, Func<TableView, IList<T>, int, float> _getRowHeightFunc = null){
            table = _table;
            getRowHeightFunc = _getRowHeightFunc;
        }
        
        public int GetNumberOfCellsForTableView(TableView _tableView)
		{
			return _tableView.GetTableSize();
		}

		public float GetHeightForRowInTableView(TableView _tableView, int _rowIndex)
		{
            if(getRowHeightFunc != null){
                return getRowHeightFunc(_tableView, table, _rowIndex);
            }
			return _tableView.GetRowHeight();
		}

		public TableRow GetCellForRowInTableView(TableView _tableView, int _rowIndex)
		{
			var _tableRow = _tableView.CreateTableRow(); // 셀 리턴
			_tableRow.RowIndex = _rowIndex;
			return _tableRow;
		}
	}

}
