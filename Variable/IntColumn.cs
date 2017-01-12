using System;
using BigjamLibrary.BicDB;

namespace BigjamLibrary.BicDB.Column
{
	public class IntColumn : IColumn{
		#region AsValue
		private int data;
		public int AsInt{ get{ return data; } set{ data = value; OnChangedValue ();} }
		public string AsString{ get{ return data.ToString (); } set{ data = Int32.Parse (value);  OnChangedValue ();} }
		public float AsFloat{ get{ return (float)data; } set{ data = (int)value;  OnChangedValue ();} }
		public ColumnType Type { get { return ColumnType.Int; }}
		#endregion

		public event Action<IColumn> OnChangedValueActions = delegate{};
		public IntColumn(int _value){
			data = _value;
		}

		public void OnChangedValue(){
			OnChangedValueActions (this);
		}
	}
}

