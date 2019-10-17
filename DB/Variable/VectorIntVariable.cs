using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;

namespace BicDB.Variable
{
	public class VectorIntVariable : DictionaryContainer<IntVariable>, IBindRmover
	{
		#region Event
		public event Action<VectorIntVariable> OnChangedValueActions;

		public void Subscribe(Action<VectorIntVariable> _callback, bool _needFirstCall = false){
			OnChangedValueActions += _callback;
			if(_needFirstCall == true){
				OnChangedValueActions(this);
			}
		}

		public void Unsubscribe(Action<VectorIntVariable> _callback){
			OnChangedValueActions -= _callback;
		}
		#endregion

		#region LifeCycle
		public VectorIntVariable(int x, int y) : base(){
			this ["x"] = new IntVariable (x);
			this ["y"] = new IntVariable (y);
		}

        public VectorIntVariable() : this(0, 0){
            
        }
		#endregion

		#region Member
		public int X {
			get{ 
				return this ["x"].AsInt;
			}

            set{
                this["x"].AsInt = value;
                NotifyChanged();
            }
		}

		public int Y {
			get{ 
				return this ["y"].AsInt;
			}

            set{
                this["y"].AsInt = value;
                NotifyChanged();
            }
		}

		public Vector2Int AsVector{
			get{
				return new Vector2Int (this ["x"].AsInt, this ["y"].AsInt);
			}

			set{ 
				this ["x"].AsInt = value.x;
				this ["y"].AsInt = value.y;
				NotifyChanged ();

			}
		}
		#endregion

		#region Logic
		public void NotifyChanged(){
			if (OnChangedValueActions != null) {
				OnChangedValueActions (this);
			}
		}

		public void ClearNotifyAndBinding (){
			OnChangedValueActions = null;
		}
		#endregion
	}
}