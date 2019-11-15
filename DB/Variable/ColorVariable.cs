using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;

namespace BicDB.Variable
{
	public class ColorVariable : DictionaryContainer<FloatVariable>, IBindRmover
	{
		#region Event
		private event Action<ColorVariable> onChangedValueActions = delegate{};

		[Obsolete("use Subscribe")]
		public event Action<ColorVariable> OnChangedValueActions{
			add{
				onChangedValueActions += value;
			}

			remove{
				onChangedValueActions -= value;
			}
		}

		public void Subscribe(Action<ColorVariable> _callback, bool _needFirstCall = false){
			onChangedValueActions += _callback;
			if(_needFirstCall == true){
				onChangedValueActions(this as ColorVariable);
			}
		}

		public void Unsubscribe(Action<ColorVariable> _callback){
			onChangedValueActions -= _callback;
		}

		public void UnsubscribeAll(){
			onChangedValueActions = delegate{};
		}
		#endregion

		#region LifeCycle
        public ColorVariable() : base(){
            this ["r"] = new FloatVariable (1f);
            this ["g"] = new FloatVariable (1f);
            this ["b"] = new FloatVariable (1f);
            this ["a"] = new FloatVariable (1f);
        }

		public ColorVariable(float r, float g, float b, float a) : base(){
			this ["r"] = new FloatVariable (r);
			this ["g"] = new FloatVariable (g);
            this ["b"] = new FloatVariable (b);
			this ["a"] = new FloatVariable (a);
		}

		public ColorVariable(Color _color) : this(_color.r, _color.g, _color.b, _color.a){
			
		}
		#endregion

		#region Member
		public float R {
			get{ 
				return this ["r"].AsFloat;
			}
            set{
                this["r"].AsFloat = value;
                NotifyChanged();
            }
		}

		public float G {
			get{ 
				return this ["g"].AsFloat;
			}
            set{
                this["g"].AsFloat = value;
                NotifyChanged();
            }
		}

        public float B {
			get{ 
				return this ["b"].AsFloat;
			}
            set{
                this["b"].AsFloat = value;
                NotifyChanged();
            }
        }

        public float A {
			get{ 
				return this ["a"].AsFloat;
			}
            set{
                this["a"].AsFloat = value;
                NotifyChanged();
            }
        }

		public Color AsColor{
			get{
				return new Color(this.R, this.G, this.B, this.A);
			}

			set{ 
				this ["r"].AsFloat = value.r;
				this ["g"].AsFloat = value.g;
				this ["b"].AsFloat = value.b;
				this ["a"].AsFloat = value.a;
				NotifyChanged ();
			}
		}
		#endregion

		#region Logic
		public void NotifyChanged(){
			if (onChangedValueActions != null) {
				onChangedValueActions (this);
			}
		}

		public void ClearNotifyAndBinding (){
			onChangedValueActions = null;
		}
		#endregion
	}
}