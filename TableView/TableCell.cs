using UnityEngine;
using System.Collections;
using System;
using BicDB.Container;
using BicDB;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BicUtil.TableView
{
    public class TableCell : MonoBehaviour
    {
		#region Type
		[System.Serializable]
		public class CellDataChanged : UnityEvent<IRecordContainer, string> { }
		#endregion

		#region LinkingObject
		[SerializeField]
		public CellDataChanged SetDataFunction;
		#endregion

		#region Member
		public IRecordContainer CellData{
			set{ 
				if (cellData != null) {
					cellData.ClearOnChangedValueActions ();
				}

				cellData = value;
				cellData.OnChangedValueActions += SetDataFunction.Invoke;
				cellData.NotifyChanged();
			}

			get{ 
				return cellData;
			}
		}


		public virtual string reuseIdentifier { 
			get { 
				return this.GetType().Name; 
			} 
		}

		public int CellIndex = -1;
		#endregion

		#region LifeCycle
		public void OnDestory(){
			removeBindCell();
		}

		public void OnRemove(){
			removeBindCell();
		}
		#endregion

		#region Event
		public Dictionary<string, Action<IRecordContainer>> OnClickedActions{ get; set;}

		public void OnClicked(string _buttonName){
			OnClickedActions [_buttonName] (cellData);
		}
		#endregion

		#region Logic
		private IRecordContainer cellData = null;

		private void removeBindCell(){
			if (cellData != null) {
				cellData.ClearOnChangedValueActions ();
			}

			cellData = null;
		}
		#endregion
    }
}