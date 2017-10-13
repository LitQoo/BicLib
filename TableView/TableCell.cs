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
		public IRecordContainer Model{
			set{ 
				if (model != null) {
					model.ClearNotifyAndBinding ();
				}

				model = value;
				model.OnChangedValueActions += SetDataFunction.Invoke;
				model.NotifyChanged();
			}

			get{ 
				return model;
			}
		}


		public virtual string reuseIdentifier { 
			get { 
				return this.GetType().Name; 
			} 
		}

		[HideInInspector]
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
			OnClickedActions [_buttonName] (model);
		}
		#endregion

		#region Logic
		private IRecordContainer model = null;

		private void removeBindCell(){
			if (model != null) {
				model.ClearNotifyAndBinding ();
			}

			model = null;
		}
		#endregion
    }
}