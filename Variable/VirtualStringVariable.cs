using System;
using BicDB;
using BicDB.Utility;

namespace BicDB.Variable
{
	public class VirtualStringVariable : StringVariable
	{
		private string _data;
		override protected string data {
			get { 
				_data = Getter ();
				return _data;
			}

			set { 
				_data = value;
				Setter (_data);
			}
		}

		public Func<string> Getter;
		public Action<string> Setter;

		public VirtualStringVariable(Func<string> _getter = null, Action<string> _setter = null) : base(){
			Getter = _getter;
			Setter = _setter;
		}

		public new void ClearNotifyAndBinding(){
			base.ClearNotifyAndBinding ();
			Getter = null;
			Setter = null;
		}

	}
}