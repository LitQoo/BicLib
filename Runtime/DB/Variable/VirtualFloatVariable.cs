using System;
using BicDB;

namespace BicDB.Variable
{
	public class VirtualFloatVariable : FloatVariable
	{
		private float _data;
		override protected float data {
			get { 
				_data = Getter ();
				return _data;
			}

			set { 
				_data = value;
				Setter (_data);
			}
		}

		public Func<float> Getter;
		public Action<float> Setter;

		public VirtualFloatVariable(Func<float> _getter = null, Action<float> _setter = null) : base(){
			Getter = _getter;
			Setter = _setter;
		}

		public new void ClearNotifyAndBinding(){
			base.UnsubscribeAll();
			Getter = null;
			Setter = null;
		}
	}
}