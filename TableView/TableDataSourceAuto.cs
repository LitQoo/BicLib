using System;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;

namespace BicUtil.TableView
{
    public class TableDataSourceAuto<T> : ITableViewDataSource  where T : IRecordContainer, new(){
        protected Func<TableView, IList<T>, int, float> getRowHeightFunc = null;
		protected IList<T> table;
		protected TableView tableView;

        public TableDataSourceAuto(TableView _tableView, IList<T> _table, Func<TableView, IList<T>, int, float> _getRowHeightFunc = null){
            table = _table;
			tableView = _tableView;
            getRowHeightFunc = _getRowHeightFunc;
        }
        
        public int GetNumberOfCellsForTableView()
		{
			return table.Count + (HasHeadRow == true ? 1 : 0) +  + (HasFootRow == true ? 1 : 0);
		}

		public float GetHeightForRowInTableView(int _rowIndex)
		{
            if(getRowHeightFunc != null){
                return getRowHeightFunc(tableView, table, _rowIndex);
            }

			if(HasHeadRow == true && _rowIndex == 0){
				return tableView.GetRowHeight(headRowName);
			}

			if(HasFootRow == true && _rowIndex == GetRowCount() - 1){
				return tableView.GetRowHeight(footRowName);
			}

			return tableView.GetDefaultRowHeight() * GetHeightUnit(_rowIndex);
		}

		public CollectionRowData GetArrangeInfoInRow(int _rowIndex){
			if(this.arrangeInfo != null){
				if(HasHeadRow == true){
					_rowIndex--;
				}

				try{
					return this.arrangeInfo[_rowIndex];
				}catch{
					Debug.Log("leng : " + this.arrangeInfo.Length.ToString() + "/" + _rowIndex.ToString());
					throw new System.Exception("asdf");
				}
			}else{
				return new CollectionRowData(this.tableView.CellCountInRowDefault);
			}
		}

		private CollectionRowData[] arrangeInfo = null;
		public Func<CollectionRowData[]> ArrangeInfoBuilder{get;set;} = null;

		public void UpdateArrangeInfo(){
			this.arrangeInfo = this.ArrangeInfoBuilder(); 
		}

		public int GetHeightUnit(int _rowIndex){
			var _info = GetArrangeInfoInRow(_rowIndex);
			int _heightUnit = 1;
			for(int v = 0; v < _info.VerticalCount; v++){
				var _unit = 0;
				for(int h = 0; h < _info.Data[v].Length; h++){
					_unit += _info.Data[v][h].y;
				}

				_heightUnit = Mathf.Max(_heightUnit, _unit);
			}

			return _heightUnit;
		}

		public TableRow GetCellForRowInTableView(int _rowIndex)
		{
			TableRow _tableRow = null;
			if(HasHeadRow == true && _rowIndex == 0){
				_tableRow = tableView.CreateTableRow(headRowName);
			}else if(HasFootRow == true && _rowIndex == GetRowCount() - 1){
				_tableRow = tableView.CreateTableRow(footRowName);
			}else{
				_tableRow = tableView.CreateTableRow(tableView.defaultReusableRowId); // 셀 리턴
			}

			_tableRow.RowIndex = _rowIndex;	
			return _tableRow;
		}

		public IRecordContainer GetCellData(int _index, int _rowIndex, int _cellOrder){
			if((HasHeadRow == true && _rowIndex == 0) || (HasFootRow == true && _rowIndex == GetRowCount() - 1)){
				return null;
			}

			try{
				return table[_index];
			}catch{
				return null;
			}
		}

		public int GetCellCountInRow(int _rowIndex){
			if(HasHeadRow == true && _rowIndex == 0){
				return 0;
			}

			if(HasFootRow == true && _rowIndex == GetRowCount() - 1){
				return 0;
			}
			
			if(this.arrangeInfo != null){
				if(HasHeadRow == true){
					_rowIndex--;
				}

				return arrangeInfo[_rowIndex].CellCount;
			}

			return tableView.CellCountInRowDefault;
		}

		public int GetRowCount(){
			int _offset = 0;
			if(HasHeadRow == true){
				_offset++;
			}

			if(HasFootRow == true){
				_offset++;
			}

			if(this.arrangeInfo != null){
				return arrangeInfo.Length + _offset;
			}

			return (int)Math.Ceiling((float)table.Count / (float)tableView.CellCountInRowDefault) + _offset;
		}

		public int GetStartDataIndex(int _rowIndex){
			if((HasHeadRow == true && _rowIndex == 0) || (HasFootRow == true && _rowIndex == GetRowCount() - 1)){
				return -1;
			}

			if(HasHeadRow == true & _rowIndex != 0){
				_rowIndex--;
			}

			if(this.ArrangeInfoBuilder != null){ 
				int _cellCount = 0;
				int _start = 0;

				for(int i = _start; i < _rowIndex; i++){
					_cellCount += this.arrangeInfo[i].CellCount;
				}
				
				return _cellCount;
			}

			return _rowIndex * tableView.CellCountInRowDefault;
		}

		public void ReloadData(){
			if(this.ArrangeInfoBuilder != null){
				UpdateArrangeInfo();
			}
		}

		public int GetRowIndex(int _cellIndex){
			int _cellCount = 0;
            int _rowCount = GetRowCount();
			var _rowIndex = _rowCount;

            for(int i = 0; i < _rowCount; i++){
                _cellCount += GetCellCountInRow(i);
                if(_cellIndex < _cellCount){
					_rowIndex = i;
                    break;
                }
            }

            return _rowIndex;
		}

		public int GetRowIndex<U>(Func<U, bool> _find) where U : class, IRecordContainer{
			var _cellIndex = -1;

			for(int i = 0; i < this.table.Count; i++){
				if(_find(table[i] as U) == true){
					_cellIndex = i;
					break;
				}
			}

			return GetRowIndex(_cellIndex);
		}

		#region Head and Foot
		public bool HasHeadRow{get=>headRowName != string.Empty;}
		private string headRowName = "";

		public void SetHeadRow(string _rowName){
			headRowName = _rowName;
		}


		public bool HasFootRow{get=>footRowName != string.Empty;}
        private string footRowName = "";

		public void SetFootRow(string _rowName){
			footRowName = _rowName;
		}
		#endregion
	}

	public class CollectionRowData{
		public Vector2Int[][] Data;
		public int CellCount = 0;
		public int VerticalCount = 0;

		public CollectionRowData(int _verticalCount){
			var _v = new Vector2Int[_verticalCount][];

			for(int i = 0; i < _verticalCount; i++){
				_v[i] = new Vector2Int[1];
				_v[i][0] = new Vector2Int(1, 1);
			}

			this.Data = _v;
		}

		public CollectionRowData(Vector2Int[] _vertical1, Vector2Int[] _vertical2, Vector2Int[] _vertical3){
			var _verticalCount = 1 + (_vertical2 != null ? 1 : 0) + (_vertical3 != null ? 1 : 0);
			Data = new Vector2Int[_verticalCount][];
			Data[0] = _vertical1;
			this.CellCount = _vertical1.Length;
			this.VerticalCount = 1;

			if(_vertical2 != null){
				Data[1] = _vertical2;
				this.CellCount += _vertical2.Length;
				this.VerticalCount++;
			} 

			if(_vertical3 != null){
				Data[2] = _vertical3;
				this.CellCount += _vertical3.Length;
				this.VerticalCount++;
			}
		} 
	}

}
