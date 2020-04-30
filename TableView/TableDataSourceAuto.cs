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
			return table.Count + (hasHeadRow == true ? 1 : 0) +  + (hasFootRow == true ? 1 : 0);
		}

		public float GetHeightForRowInTableView(int _rowIndex)
		{
            if(getRowHeightFunc != null){
                return getRowHeightFunc(tableView, table, _rowIndex);
            }

			if(hasHeadRow == true && _rowIndex == 0){
				return tableView.GetRowHeight(headRowName);
			}

			if(hasFootRow == true && _rowIndex == GetRowCount() - 1){
				return tableView.GetRowHeight(footRowName);
			}

			return tableView.GetDefaultRowHeight();
		}

		public TableRow GetCellForRowInTableView(int _rowIndex)
		{
			TableRow _tableRow = null;
			if(hasHeadRow == true && _rowIndex == 0){
				_tableRow = tableView.CreateTableRow(headRowName);
			}else if(hasFootRow == true && _rowIndex == GetRowCount() - 1){
				_tableRow = tableView.CreateTableRow(footRowName);
			}else{
				_tableRow = tableView.CreateTableRow(tableView.defaultReusableRowId); // 셀 리턴
			}

			_tableRow.RowIndex = _rowIndex;	
			return _tableRow;
		}

		public IRecordContainer GetCellData(int _index, int _rowIndex, int _cellOrder){
			if(hasHeadRow == true){
				if(_rowIndex == 0){
					return null;
				}else{
					_index--;
				}
			}

			if(table.Count <= _index){
				return null;
			}

			return table[_index];	
		}

		public int GetCellCountInRow(int _rowIndex){
			if(hasHeadRow == true && _rowIndex == 0){
				return 1;
			}

			if(hasFootRow == true && _rowIndex == GetRowCount() - 1){
				return 1;
			}

			return tableView.CellCountInRowDefault;
		}

		public int GetRowCount(){
			int _offset = 0;
			if(hasHeadRow == true){
				_offset++;
			}

			if(hasFootRow == true){
				_offset++;
			}

			return (int)Math.Ceiling((float)table.Count / (float)tableView.CellCountInRowDefault) + _offset;
		}

		public int GetStartDataIndex(int _rowIndex){
			if(hasHeadRow == true){
				if(_rowIndex == 0){
					return 0;
				}else{
					return (_rowIndex - 1) * tableView.CellCountInRowDefault + 1;		
				}
			}

			return _rowIndex * tableView.CellCountInRowDefault;
		}

		public void ReloadData(){
			
		}

		public int GetRowIndex(int _cellIndex){
			int _cellCount = 0;
            int _rowCount = GetRowCount();
			var _rowIndex = _rowCount;
            for(int i = 0; i < _rowCount; i++){
                _cellCount += GetCellCountInRow(i);

                if(_cellIndex <= _cellCount){
                    _rowIndex = i;
                    break;
                }
            }

            return _rowIndex;
		}

		#region Head and Foot
		private bool hasHeadRow{get=>headRowName != string.Empty;}
		private string headRowName = "";

		public void SetHeadRow(string _rowName){
			headRowName = _rowName;
		}


		private bool hasFootRow{get=>footRowName != string.Empty;}
		private string footRowName = "";

		public void SetFootRow(string _rowName){
			footRowName = _rowName;
		}
		#endregion
	}

}
