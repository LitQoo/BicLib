using System;
using BicDB;
using System.Linq;
using System.Collections.Generic;
using BicDB.Utility;
using BicDB.Container;
using UnityEngine;

namespace BicDB.Variable
{
	public class VectorVariable : DictionaryContainer<FloatVariable>
	{
		#region Event
		public event Action<VectorVariable, string> OnChangedValueActions = delegate{};
		#endregion

		#region LifeCycle
		public VectorVariable(float x, float y) : base(){
			this ["x"] = new FloatVariable (x);
			this ["y"] = new FloatVariable (y);
		}
		#endregion

		#region Member
		public FloatVariable X {
			get{ 
				return this ["x"];
			}
		}

		public FloatVariable Y {
			get{ 
				return this ["y"];
			}
		}

		public Vector2 AsVector{
			get{
				return new Vector2 (this ["x"].AsFloat, this ["y"].AsFloat);
			}

			set{ 
				this ["x"].AsFloat = value.x;
				this ["y"].AsFloat = value.y;
				NotifyChanged ();

			}
		}
		#endregion

		#region Logic
		public void NotifyChanged(string _message = ""){
			OnChangedValueActions (this, _message);
		}
		#endregion
	}
//
//	public class VirtualVectorVariable : DictionaryContainer<FloatVariable>
//	{
//		public FloatVariable X {
//			get{ 
//				return this ["x"].AsFloat;
//			}
//			set{ 
//				this ["x"].AsFloat = value;
//			}
//		}
//
//		public FloatVariable Y {
//			get{ 
//				return this ["y"].AsFloat;
//			}
//
//			set{ 
//				this ["y"].AsFloat = value;
//			}
//		}
//
//		public Vector2 AsVector{
//			get{
//				return new Vector2 (this ["x"].AsFloat, this ["y"].AsFloat);
//			}
//
//			set{ 
//				this ["x"].AsFloat = value.x;
//				this ["y"].AsFloat = value.y;
//			}
//		}
//
//		public VirtualVectorVariable(Action<float> _setter, Func<float> _getter) : base(){
//			//this ["x"] = new VirtualFloatVariable(
//			//this ["y"] = new FloatVariable (y);
//		}
//	}
}