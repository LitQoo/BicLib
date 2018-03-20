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

		public void SetData(int _startCellIndex, Func<int, IRecordContainer> _cellDataFunc){
			for(int i = 0; i < Cells.Count; i++){
				var _model = _cellDataFunc(_startCellIndex + i);
				if(_model != null){
					Cells[i].gameObject.SetActive(true);
					Cells[i].Model = _model;
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

		#region Logic

		#endregion
    }
}