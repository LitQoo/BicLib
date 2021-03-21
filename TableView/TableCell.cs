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
		[SerializeField]
		public CellDataChanged OnRemoveFunction;
		[SerializeField]
		public RectTransform RectTransform;

		#endregion

		#region Member
		public IRecordContainer Model{
			set{ 
				removeBindCell();

				model = value;
				if(model != null){
					model.OnChangedValueActions += callToSetDataFunction;
					model.NotifyChanged();
				}
			}

			get{ 
				return model;
			}
		}

        private void callToSetDataFunction(IRecordContainer _record, string _msg)
        {
            SetDataFunction.Invoke(_record, _msg);
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
		public void Awake(){
			if(SetDataFunction.GetPersistentEventCount() <= 0){
				var _cell = GetComponent<ITableCellWithBinder>();
				if(_cell != null){
					SetDataFunction.AddListener(_cell.Bind);
					OnRemoveFunction.AddListener(_cell.Unbind);
				}
			}

			#region UNITY_EDITOR
			if(RectTransform == null){
				Debug.LogError("[TableView] Set Recttrasform in TableCell");
			}
			#endregion
		}

		public void OnDestory(){
			removeBindCell();
		}

		public void OnRemove(){
			OnRemoveFunction.Invoke(model, "");
			removeBindCell();
		}
		#endregion

		#region Event
		public Dictionary<string, Action<IRecordContainer>> OnClickedActions{ get; set;}

		public void OnClicked(string _buttonName){
			if(OnClickedActions == null){
				return;
			}
			
			if(OnClickedActions.ContainsKey(_buttonName)){
				OnClickedActions [_buttonName] (model);
			}
		}
		#endregion

		#region Logic
		private IRecordContainer model = null;

		private void removeBindCell(){
			if (model != null) {
				model.OnChangedValueActions -= callToSetDataFunction;
			}

			model = null;
		}
		#endregion
    }

	public interface ITableCellWithBinder{
		void Bind(IRecordContainer _data, string _msg);
		void Unbind(IRecordContainer _data, string _msg);
	}
}