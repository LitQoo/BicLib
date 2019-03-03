using UnityEngine;
using System.Collections;
using System;
using BicDB.Container;
using BicDB;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BicUtil.TableView
{
    public class TableRow : MonoBehaviour
    {
		#region Member
		[SerializeField]
		private int cellCount = 1;

		[HideInInspector]
		public List<TableCell> Cells = new List<TableCell>();

        public virtual string reuseIdentifier { 
			get { 
				return "tableRow"; 
			} 
		}

		[HideInInspector]
		public int RowIndex = -1;
		#endregion

		#region LifeCycle
		private void Awake(){
			
		}

		public void OnRemove(){
			for(int i = 0; i < Cells.Count; i++){
				Cells[i].OnRemove();
			}
		}

		#endregion

		#region Logic
		public void InitializeCells(){
			if(cellCount > 0){
				var _tableCell = transform.GetComponentInChildren<TableCell>();
				var _childs = transform.GetComponentsInChildren<TableCell>();
				var _cellCount = _childs.Length;
				for(int i = _cellCount; i < cellCount; i++){
					var _cell = Instantiate(_tableCell.gameObject);
					_cell.transform.SetParent(this.gameObject.transform);
					_cell.transform.SetSiblingIndex(_tableCell.transform.GetSiblingIndex());
				}

				Cells.Clear();
			}
			
			if(Cells.Count <= 0){
				var _childs = transform.GetComponentsInChildren<TableCell>();
				for(int i = 0; i < _childs.Length; i++){
					var _tableCell = _childs[i];
					if(_tableCell != null){
						Cells.Add(_tableCell);
					}
				}
			}

			cellCount = Cells.Count;
		}

		public void SetData(int _startCellIndex, Func<int, IRecordContainer> _cellDataFunc, int _cellCount){
			for(int i = 0; i < Cells.Count; i++){
				if(_cellCount > i){
					var _model = _cellDataFunc(_startCellIndex + i);
					if(_model != null){
						Cells[i].gameObject.SetActive(true);
						Cells[i].Model = _model;
					}else{
						Cells[i].gameObject.SetActive(false);
					}
				}else{
					Cells[i].gameObject.SetActive(false);
				}
			}
		}

		public void BindOnClickedEvent(Dictionary<string, Action<IRecordContainer>> _onClickedEvent){
			for(int i = 0; i < Cells.Count; i++){
				Cells[i].OnClickedActions = _onClickedEvent;
			}
		}
		#endregion
    }
}