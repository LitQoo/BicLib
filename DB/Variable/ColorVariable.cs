using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Container;
using UnityEngine;
using BicDB.Storage;

namespace BicDB.Variable
{
	public class ColorVariable : DictionaryContainer<FloatVariable>, IBindRmover, IDataBase, IVariable
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

        event Action<IVariableReadOnly> IVariableReadOnly.OnChangedValueActions
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public void Subscribe(Action<ColorVariable> _callback, bool _needFirstCall = false){
			onChangedValueActions += _callback;
			if(_needFirstCall == true){
				_callback(this as ColorVariable);
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

		public float RGBAverage{
			get{
				return (this.R + this.G + this.B) / 3f;
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
		

		public string AsString { 
			get{
				return "#"+ColorUtility.ToHtmlStringRGBA(this.AsColor);
			} 

			set{
				Color color;
				if(ColorUtility.TryParseHtmlString(value, out color) == true){
					AsColor = color;
				}
			} 
		}

        public int AsInt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float AsFloat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AsBool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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

		#region IDataBase
		public new void BuildVariable(ref string _json, ref int _counter, IStringParser _parser)
		{
			if(_json[_counter] == '"'){
				_parser.BuildStringVariable(this, ref _json, ref _counter);
			}else{
				_parser.BuildDictionaryContainer(this, ref _json, ref _counter);
			}
		}

		public new void BuildFormattedString(System.Text.StringBuilder _stringBuilder, IStringFormatter _formatter){
			_formatter.BuildFormattedString(this as IVariable, _stringBuilder);
		}

        public void Subscribe(Action<IVariableReadOnly> _callback, bool _needFirstCall = false)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Action<IVariableReadOnly> _callback)
        {
            throw new NotImplementedException();
        }

        public void NotifyChanged(IVariableReadOnly _value)
        {
            throw new NotImplementedException();
        }

        public bool IsEqual(IVariable _variable)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}